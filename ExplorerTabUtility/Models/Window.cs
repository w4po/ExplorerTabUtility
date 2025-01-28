namespace ExplorerTabUtility.Models;

public class WindowRecord(string location, nint handle = 0, string[]? selectedItems = null)
{
    public nint Handle { get; set; } = handle;
    public string Location { get; set; } = location;
    public string[]? SelectedItems { get; set; } = selectedItems;
}