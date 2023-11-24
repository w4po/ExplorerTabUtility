using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ExplorerTabUtility.Models;

public class HWndCache
{
    private readonly Dictionary<IntPtr, long> _cache;
    private readonly object _lock;
    private readonly int _expireTimeMs;

    public HWndCache(int expireTimeMs)
    {
        _expireTimeMs = expireTimeMs;
        _lock = new object();
        _cache = new Dictionary<IntPtr, long>();
    }

    public void Add(IntPtr hWnd)
    {
        lock (_lock)
        {
            Cleanup();
            if (_cache.ContainsKey(hWnd)) return;

            _cache.Add(hWnd, Stopwatch.GetTimestamp());
        }
    }
    public bool Contains(IntPtr hWnd)
    {
        lock (_lock)
        {
            Cleanup();
            return _cache.ContainsKey(hWnd);
        }
    }

    private void Cleanup()
    {
        for (var i = _cache.Count - 1; i >= 0; i--)
        {
            var element = _cache.ElementAt(i);

            if (Helpers.Helper.IsTimeUp(element.Value, _expireTimeMs))
                _cache.Remove(element.Key);
        }
    }
}