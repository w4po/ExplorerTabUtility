using System.Collections.Generic;

namespace ExplorerTabUtility.Models;

public class Window
{
    public nint OldWindowHandle { get; set; }
    public nint OldTabHandle { get; set; }
    public string Path { get; set; }
    public IList<string>? SelectedItems { get; set; }

    public Window(string path, IList<string>? selectedItems = default, nint oldWindowHandle = 0, nint oldTabHandle = 0)
    {
        Path = path;
        SelectedItems = selectedItems;
        OldWindowHandle = oldWindowHandle;
        OldTabHandle = oldTabHandle;
    }
}