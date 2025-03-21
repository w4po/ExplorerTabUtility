using System;

namespace ExplorerTabUtility.Models;

public class WindowRecord(string location, nint handle = 0, string[]? selectedItems = null, string name = "")
{
    public nint Handle { get; set; } = handle;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public string[]? SelectedItems { get; set; } = selectedItems;
    public long CreatedAt { get; set; } = Environment.TickCount;

    public string DisplayLocation => Location.Replace(@"file:\\\", "");
}