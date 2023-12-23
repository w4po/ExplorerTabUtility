using System.Collections.Generic;

namespace ExplorerTabUtility.Models;

public class Window(
    string path,
    IList<string>? selectedItems = default,
    nint oldWindowHandle = 0,
    nint oldTabHandle = 0)
{
    public nint OldWindowHandle { get; set; } = oldWindowHandle;
    public nint OldTabHandle { get; set; } = oldTabHandle;
    public string Path { get; set; } = path;
    public IList<string>? SelectedItems { get; set; } = selectedItems;
}