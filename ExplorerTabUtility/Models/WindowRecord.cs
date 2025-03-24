using System;
using System.Text.Json.Serialization;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.Models;

public class WindowRecord(string location, nint handle = 0, string[]? selectedItems = null, string name = "")
{
    [JsonConverter(typeof(IntPtrConverter))]
    public nint Handle { get; set; } = handle;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public string[]? SelectedItems { get; set; } = selectedItems;
    public long CreatedAt { get; set; } = Environment.TickCount;

    [JsonIgnore] public string DisplayLocation => Location.Replace(@"file:\\\", "");

    [JsonConstructor]
    private WindowRecord() : this(string.Empty)
    {
    }
}