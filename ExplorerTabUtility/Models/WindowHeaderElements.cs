using FlaUI.Core.AutomationElements;

namespace ExplorerTabUtility.Models;

public class WindowHeaderElements(
    AutomationElement? suggestBox = default,
    AutomationElement? addressBar = default)
{
    public AutomationElement? SuggestBox { get; set; } = suggestBox;
    public AutomationElement? AddressBar { get; set; } = addressBar;
}