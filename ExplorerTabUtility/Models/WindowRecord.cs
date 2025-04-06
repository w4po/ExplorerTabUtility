using System;
using System.Text.Json.Serialization;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.Models;

public class WindowRecord(string location, nint handle = 0, string[]? selectedItems = null, string name = "", bool restore = false)
{
    [JsonConverter(typeof(IntPtrConverter))]
    public nint Handle { get; set; } = handle;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public string[]? SelectedItems { get; set; } = selectedItems;
    public long CreatedAt { get; set; } = Environment.TickCount;
    public bool Restore { get; set; } = restore;

    [JsonIgnore] public string DisplayLocation => Uri.UnescapeDataString(Location.Replace(@"file:\\\", ""));

    [JsonConstructor]
    private WindowRecord() : this(string.Empty)
    {
    }
}