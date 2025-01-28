#pragma warning disable CS8714 // Nullability of type argument `TOptionalKey` doesn't match 'notnull' constraint.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ExplorerTabUtility.Models;

public readonly struct DualKeyEntry<TPrimaryKey, TOptionalKey, TValue>(TPrimaryKey primaryKey, TValue value, TOptionalKey? optionalKey = default)
{
    public TPrimaryKey PrimaryKey { get; } = primaryKey;
    public TOptionalKey? OptionalKey { get; } = optionalKey;
    public TValue Value { get; } = value;

    public void Deconstruct(out TPrimaryKey primaryKey, out TValue value) => (primaryKey, value) = (PrimaryKey, Value);
    public void Deconstruct(out TPrimaryKey primaryKey, out TValue value, out TOptionalKey? optionalKey)
    {
        (primaryKey, value, optionalKey) = (PrimaryKey, Value, OptionalKey);
    }
    public static implicit operator DualKeyEntry<TPrimaryKey, TOptionalKey, TValue>((TPrimaryKey primaryKey, TValue value) tuple)
    {
        return new DualKeyEntry<TPrimaryKey, TOptionalKey, TValue>(tuple.primaryKey, tuple.value);
    }
    public static implicit operator DualKeyEntry<TPrimaryKey, TOptionalKey, TValue>((TPrimaryKey primaryKey, TValue value, TOptionalKey? optionalKey) tuple)
    {
        return new DualKeyEntry<TPrimaryKey, TOptionalKey, TValue>(tuple.primaryKey, tuple.value, tuple.optionalKey);
    }
}

public class DualKeyDictionary<TPrimaryKey, TOptionalKey, TValue> :
    IDictionary<TPrimaryKey, TValue>,
    IEnumerable<TValue>,
    IEnumerable<DualKeyEntry<TPrimaryKey, TOptionalKey, TValue>>
    where TPrimaryKey : notnull
{
    // Primary key → (value + optional key)
    // Optional key → primary key
    private readonly Dictionary<TPrimaryKey, Entry> _primaryDict;
    private readonly Dictionary<TOptionalKey, TPrimaryKey> _optionalDict;

    private class Entry(TValue value, TOptionalKey? optionalKey)
    {
        public TValue Value { get; set; } = value;
        public TOptionalKey? OptionalKey { get; set; } = optionalKey;
    }
    private enum InsertionBehavior : byte
    {
        None,
        OverwriteExisting,
        ThrowOnExisting,
        ThrowOnOptionalKeyConflict
    }

    public DualKeyDictionary()
    {
        _primaryDict = new Dictionary<TPrimaryKey, Entry>();
        _optionalDict = new Dictionary<TOptionalKey, TPrimaryKey>();
    }

    public DualKeyDictionary(int capacity)
    {
        _primaryDict = new Dictionary<TPrimaryKey, Entry>(capacity);
        _optionalDict = new Dictionary<TOptionalKey, TPrimaryKey>(capacity);
    }

    TValue IDictionary<TPrimaryKey, TValue>.this[TPrimaryKey key]
    {
        get => TryGetValue(key, out TValue value) ? value : throw new KeyNotFoundException($"The primary key '{key}' was not found.");
        set => TryInsert(key, value, default, InsertionBehavior.OverwriteExisting);
    }
    public DualKeyEntry<TPrimaryKey, TOptionalKey, TValue> this[TPrimaryKey primaryKey]
    {
        get => TryGetValue(primaryKey, out DualKeyEntry<TPrimaryKey, TOptionalKey, TValue> entry) ? entry : throw new KeyNotFoundException($"The primary key '{primaryKey}' was not found.");
        set => TryInsert(primaryKey, value.Value, value.OptionalKey, InsertionBehavior.OverwriteExisting);
    }
    public DualKeyEntry<TPrimaryKey, TOptionalKey, TValue> this[TOptionalKey optionalKey] =>
        TryGetValue(optionalKey, out DualKeyEntry<TPrimaryKey, TOptionalKey, TValue> value) ? value : throw new KeyNotFoundException($"The optional key '{optionalKey}' was not found.");

    private bool TryInsert(TPrimaryKey primaryKey, TValue value, TOptionalKey? optionalKey, InsertionBehavior behavior)
    {
        var primaryExists = _primaryDict.TryGetValue(primaryKey, out var existingEntry);
        if (primaryExists)
        {
            if (behavior == InsertionBehavior.None) return false;
            if (behavior == InsertionBehavior.ThrowOnExisting)
                throw new ArgumentException(@"Primary key already exists", nameof(primaryKey));
        }

        var sameOptionalKey = primaryExists && EqualityComparer<TOptionalKey?>.Default.Equals(optionalKey, existingEntry!.OptionalKey);

        // Check for optional key conflict if the new optional key is different and not null
        if (!sameOptionalKey && optionalKey is not null && _optionalDict.TryGetValue(optionalKey, out var existingPrimaryForOptionalKey))
        {
            if (behavior == InsertionBehavior.None) return false;
            if (behavior == InsertionBehavior.ThrowOnExisting)
                throw new ArgumentException(@"Optional key already exists", nameof(optionalKey));

            // Check if the existing optional key belongs to a different primary key
            if (!EqualityComparer<TPrimaryKey>.Default.Equals(primaryKey, existingPrimaryForOptionalKey))
            {
                if (behavior == InsertionBehavior.ThrowOnOptionalKeyConflict)
                    throw new ArgumentException(@"Optional key is already used by another primary key", nameof(optionalKey));

                // Resolve the conflict by removing the optional key from the other primary key (will update _optionalDict below)
                _primaryDict[existingPrimaryForOptionalKey].OptionalKey = default;
            }
        }

        if (!primaryExists)
        {
            // Create the entry and store in both dictionaries
            var entry = new Entry(value, optionalKey);

            _primaryDict[primaryKey] = entry;

            if (optionalKey is not null)
                _optionalDict[optionalKey] = primaryKey;

            return true;
        }

        existingEntry!.Value = value;
        if (sameOptionalKey)
            return true;

        // Remove the old optional key from the dictionary if it exists
        if (existingEntry.OptionalKey is not null)
            _optionalDict.Remove(existingEntry.OptionalKey);

        existingEntry.OptionalKey = optionalKey;

        if (optionalKey is not null)
            _optionalDict[optionalKey] = primaryKey;

        return true;
    }

    /// <summary>
    /// Adds a new entry with the specified primaryKey, optionalKey, and value.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if primaryKey is null.</exception>
    /// <exception cref="ArgumentException">Thrown if primaryKey already exists, or optionalKey already exists and is not null.</exception>
    public void Add(TPrimaryKey primaryKey, TValue value, TOptionalKey? optionalKey)
    {
        TryInsert(primaryKey, value, optionalKey, InsertionBehavior.ThrowOnExisting);
    }

    /// <summary>
    /// Attempts to add a new entry with the specified primaryKey, optionalKey, and value.
    /// </summary>
    /// <returns>True if the entry was added successfully, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if primaryKey is null.</exception>
    public bool TryAdd(TPrimaryKey primaryKey, TValue value, TOptionalKey? optionalKey = default)
    {
        return TryInsert(primaryKey, value, optionalKey, InsertionBehavior.None);
    }

    /// <summary>
    /// Attempts to retrieve the value and optional key associated with the specified primary key.
    /// </summary>
    /// <returns>True if the primary key was found, false otherwise.</returns>
    public bool TryGetByPrimary(TPrimaryKey primaryKey, out TValue value, out TOptionalKey? optionalKey)
    {
        if (_primaryDict.TryGetValue(primaryKey, out var entry))
        {
            value = entry.Value;
            optionalKey = entry.OptionalKey;
            return true;
        }
        value = default!;
        optionalKey = default;
        return false;
    }

    /// <summary>
    /// Attempts to retrieve the value and primary key associated with the specified optional key.
    /// </summary>
    /// <returns>True if the optional key was found, false otherwise.</returns>
    public bool TryGetByOptional(TOptionalKey optionalKey, out TPrimaryKey primaryKey, out TValue value)
    {
        if (optionalKey is null)
        {
            primaryKey = default!;
            value = default!;
            return false;
        }

        if (_optionalDict.TryGetValue(optionalKey, out var primary) &&
            _primaryDict.TryGetValue(primary, out var entry))
        {
            primaryKey = primary;
            value = entry.Value;
            return true;
        }

        primaryKey = default!;
        value = default!;
        return false;
    }

    /// <summary>
    /// Updates the optional key of an existing entry identified by primaryKey.
    /// If the new optional key is null, the old one is removed.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if primaryKey is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the primaryKey does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown if the new optional key already exists.</exception>
    public void UpdateOptionalKey(TPrimaryKey primaryKey, TOptionalKey? newOptionalKey)
    {
        if (!_primaryDict.TryGetValue(primaryKey, out var entry)) throw new ArgumentException(@"Primary key not found", nameof(primaryKey));

        if (EqualityComparer<TOptionalKey?>.Default.Equals(newOptionalKey, entry.OptionalKey))
            return;

        // Check uniqueness of new optional key if not null
        if (newOptionalKey is not null && _optionalDict.ContainsKey(newOptionalKey))
            throw new ArgumentException(@"Optional key is already used by another primary key", nameof(newOptionalKey));

        // Remove old optional key mapping if it existed
        if (entry.OptionalKey is not null)
            _optionalDict.Remove(entry.OptionalKey);

        // Update the entry
        entry.OptionalKey = newOptionalKey;

        // Add new optional key mapping if not null
        if (newOptionalKey is not null)
            _optionalDict[newOptionalKey] = primaryKey;
    }

    /// <summary>
    /// Removes an entry by primary key if it exists.
    /// </summary>
    /// <returns>True if removal was successful, false otherwise.</returns>
    public bool RemoveByPrimary(TPrimaryKey primaryKey)
    {
        if (!_primaryDict.TryGetValue(primaryKey, out var entry)) return false;

        // Remove optional key if exists
        if (entry.OptionalKey is not null) _optionalDict.Remove(entry.OptionalKey);

        // Remove from primary dict
        _primaryDict.Remove(primaryKey);
        return true;
    }

    /// <summary>
    /// Removes an entry by optional key if it exists.
    /// </summary>
    /// <returns>True if removal was successful, false otherwise.</returns>
    public bool RemoveByOptional(TOptionalKey optionalKey)
    {
        if (!_optionalDict.TryGetValue(optionalKey, out var primaryKey)) return false;

        // Remove the entry from the primary dictionary
        _primaryDict.Remove(primaryKey);

        // Remove from optional dict
        _optionalDict.Remove(optionalKey);
        return true;
    }
    public void Clear()
    {
        _primaryDict.Clear();
        _optionalDict.Clear();
    }

    public IEnumerator<DualKeyEntry<TPrimaryKey, TOptionalKey, TValue>> GetEnumerator()
    {
        foreach (var kvp in _primaryDict)
            yield return new DualKeyEntry<TPrimaryKey, TOptionalKey, TValue>(kvp.Key, kvp.Value.Value, kvp.Value.OptionalKey);
    }
    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        foreach (var entry in _primaryDict.Values)
            yield return entry.Value;
    }
    IEnumerator<KeyValuePair<TPrimaryKey, TValue>> IEnumerable<KeyValuePair<TPrimaryKey, TValue>>.GetEnumerator()
    {
        foreach (var kvp in _primaryDict)
            yield return new KeyValuePair<TPrimaryKey, TValue>(kvp.Key, kvp.Value.Value);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Contains(KeyValuePair<TPrimaryKey, TValue> item) =>
        _primaryDict.TryGetValue(item.Key, out var value) && EqualityComparer<TValue?>.Default.Equals(value.Value, item.Value);
    public void CopyTo(KeyValuePair<TPrimaryKey, TValue>[] array, int index)
    {
        if (index > array.Length) throw new ArgumentOutOfRangeException(nameof(index));

        if (array.Length - index < Count) throw new ArgumentException("Not enough space in the array to copy the elements.");

        var i = index;
        foreach (var kvp in _primaryDict)
            array[i++] = new KeyValuePair<TPrimaryKey, TValue>(kvp.Key, kvp.Value.Value);
    }
    public bool TryGetValue(TPrimaryKey primaryKey, out DualKeyEntry<TPrimaryKey, TOptionalKey, TValue> value)
    {
        if (TryGetByPrimary(primaryKey, out var val, out var optionalKey))
        {
            value = new DualKeyEntry<TPrimaryKey, TOptionalKey, TValue>(primaryKey, val, optionalKey);
            return true;
        }
        value = default!;
        return false;
    }
    public bool TryGetValue(TOptionalKey optionalKey, out DualKeyEntry<TPrimaryKey, TOptionalKey, TValue> value)
    {
        if (TryGetByOptional(optionalKey, out var primaryKey, out var val))
        {
            value = new DualKeyEntry<TPrimaryKey, TOptionalKey, TValue>(primaryKey, val, optionalKey);
            return true;
        }
        value = default!;
        return false;
    }
    public bool TryGetValue(TPrimaryKey primaryKey, out TValue value) => TryGetByPrimary(primaryKey, out value, out _);
    public bool TryGetValue(TOptionalKey optionalKey, out TValue value) => TryGetByOptional(optionalKey, out _, out value);
    public bool TryGetValue(TOptionalKey optionalKey, out TPrimaryKey? primaryKey) => _optionalDict.TryGetValue(optionalKey, out primaryKey);
    public void Add(TPrimaryKey primaryKey, TValue value) => Add(primaryKey, value, default);
    public void Add(KeyValuePair<TPrimaryKey, TValue> item) => Add(item.Key, item.Value);
    public bool Remove(KeyValuePair<TPrimaryKey, TValue> item) => Remove(item.Key);
    public bool Remove(TPrimaryKey primaryKey) => RemoveByPrimary(primaryKey);
    public bool Remove(TOptionalKey optionalKey) => RemoveByOptional(optionalKey);
    public bool ContainsKey(TPrimaryKey primaryKey) => ContainsPrimary(primaryKey);
    public bool ContainsKey(TOptionalKey optionalKey) => ContainsOptional(optionalKey);
    public bool ContainsPrimary(TPrimaryKey primaryKey) => _primaryDict.ContainsKey(primaryKey);
    public bool ContainsOptional(TOptionalKey optionalKey) => optionalKey is not null && _optionalDict.ContainsKey(optionalKey);
    public int Count => _primaryDict.Count;
    public bool IsReadOnly => false;
    public ICollection<TPrimaryKey> Keys => _primaryDict.Keys;
    public ICollection<TValue> Values => new ValueCollection(this);
    public ICollection<TOptionalKey> OptionalKeys => _optionalDict.Keys;
    public ICollection<TValue> OptionalValues => new ValueCollection(this);

    private class ValueCollection(DualKeyDictionary<TPrimaryKey, TOptionalKey, TValue> dict) : ICollection<TValue>
    {
        public int Count => dict.Count;
        public bool IsReadOnly => true;
        public void Add(TValue item) => throw new NotSupportedException("Cannot add to a read-only collection.");
        public void Clear() => throw new NotSupportedException("Cannot clear a read-only collection.");
        public bool Contains(TValue item) => dict._primaryDict.Values.Any(e => EqualityComparer<TValue>.Default.Equals(e.Value, item));
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex), @"Index is out of range.");
            if (array.Length - arrayIndex < dict.Count)
                throw new ArgumentException("The number of elements in the collection is greater than the available space from arrayIndex to the end of the destination array.");

            foreach (var entry in dict._primaryDict.Values) array[arrayIndex++] = entry.Value;
        }
        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var entry in dict._primaryDict.Values)
                yield return entry.Value;
        }
        public bool Remove(TValue item) => throw new NotSupportedException("Cannot remove from a read-only collection.");
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}