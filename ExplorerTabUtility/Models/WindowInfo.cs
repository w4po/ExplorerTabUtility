using SHDocVw;
using System.Diagnostics;

namespace ExplorerTabUtility.Models;

public class WindowInfo
{
    public long CreatedAt { get; } = Stopwatch.GetTimestamp();
    public string? Location { get; set; }
    public string? Name { get; set; }
    public DWebBrowserEvents2_OnQuitEventHandler? OnQuitHandler { get; set; }
    public DWebBrowserEvents2_NavigateComplete2EventHandler? OnNavigateHandler { get; set; }
}