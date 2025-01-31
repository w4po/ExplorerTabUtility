using SHDocVw;
using System.Diagnostics;

namespace ExplorerTabUtility.Models;

public class WindowInfo
{
    public long CreatedAt { get; } = Stopwatch.GetTimestamp();
    public DWebBrowserEvents2_OnQuitEventHandler? OnQuitHandler { get; set; }
}