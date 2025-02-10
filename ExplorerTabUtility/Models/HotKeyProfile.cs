using System;
using System.Linq;
using H.Hooks;

namespace ExplorerTabUtility.Models;

public class HotKeyProfile
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public Key[]? HotKeys { get; set; }
    public HotkeyScope Scope { get; set; }
    public HotKeyAction Action { get; set; }
    public string? Path { get; set; }
    public bool IsHandled { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool IsAsTab { get; set; } = true;
    public bool IsMouse { get; set; }
    public bool IsDoubleClick { get; set; }
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

    public HotKeyProfile Clone()
    {
        return new HotKeyProfile
        {
            Id = Id,
            Name = Name,
            HotKeys = HotKeys?.ToArray(),
            Scope = Scope,
            Action = Action,
            Path = Path,
            IsHandled = IsHandled,
            IsEnabled = IsEnabled,
            IsAsTab = IsAsTab,
            IsMouse = IsMouse,
            IsDoubleClick = IsDoubleClick,
            Delay = Delay
        };
    }
}