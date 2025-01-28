using H.Hooks;

namespace ExplorerTabUtility.Models;

public class HotKeyProfile
{
    public string? Name { get; set; }
    public Key[]? HotKeys { get; set; }
    public HotkeyScope Scope { get; set; }
    public HotKeyAction Action { get; set; }
    public string? Path { get; set; }
    public bool IsHandled { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public int Delay { get; set; }

    public HotKeyProfile() { }
    public HotKeyProfile(string name, Key[] hotKeys, HotKeyAction action, string? path = null, HotkeyScope scope = HotkeyScope.Global, int delay = 0)
    {
        Name = name;
        HotKeys = hotKeys;
        Action = action;
        Path = path;
        Scope = scope;
        Delay = delay;
    }
}