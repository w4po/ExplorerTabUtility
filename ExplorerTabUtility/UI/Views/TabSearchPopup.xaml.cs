using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.Hooks;
using Keyboard = System.Windows.Input.Keyboard;

namespace ExplorerTabUtility.UI.Views;

// ReSharper disable once RedundantExtendsListEntry
public partial class TabSearchPopup : Window
{
    private bool _isShowingDialog;
    private bool _isClosing;
    private readonly ExplorerWatcher _explorerWatcher;
    private IReadOnlyCollection<WindowRecord> _allWindows = null!;
    private IReadOnlyCollection<WindowRecord> _filteredWindows = null!;

    public TabSearchPopup(ExplorerWatcher explorerWatcher)
    {
        InitializeComponent();

        _explorerWatcher = explorerWatcher;

        LoadWindows();
        SearchBox.Focus();
    }

    private void LoadWindows()
    {
        _allWindows = _explorerWatcher.GetWindows();
        _filteredWindows = _allWindows;
        TabsList.ItemsSource = _filteredWindows;

        if (_filteredWindows.Count > 0)
            TabsList.SelectedIndex = 0;
    }

    private void FilterWindows(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            _filteredWindows = _allWindows;
        }
        else
        {
            const StringComparison sc = StringComparison.OrdinalIgnoreCase;
            _filteredWindows = _allWindows
                .Where(w => w.Name.IndexOf(searchText, sc) != -1 || w.Location.IndexOf(searchText, sc) != -1)
                .OrderByDescending(w => w.Name.IndexOf(searchText, sc) != -1) // Name matches first
                .ToList();
        }

        TabsList.ItemsSource = _filteredWindows;

        if (_filteredWindows.Count > 0)
            TabsList.SelectedIndex = 0;
    }

    private void SelectNext()
    {
        if (_filteredWindows.Count == 0)
            return;

        if (TabsList.SelectedIndex < _filteredWindows.Count - 1)
            TabsList.SelectedIndex++;
        else
            TabsList.SelectedIndex = 0; // Wrap around to the first item

        TabsList.ScrollIntoView(TabsList.SelectedItem);
    }

    private void SelectPrevious()
    {
        if (_filteredWindows.Count == 0)
            return;

        if (TabsList.SelectedIndex > 0)
            TabsList.SelectedIndex--;
        else
            TabsList.SelectedIndex = _filteredWindows.Count - 1; // Wrap around to the last item

        TabsList.ScrollIntoView(TabsList.SelectedItem);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterWindows(SearchBox.Text);
    }

    private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            SelectNext();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            SelectPrevious();
            e.Handled = true;
        }
        else if (e.Key == Key.Tab)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                SelectPrevious();
            else
                SelectNext();

            e.Handled = true;
        }
        else if (e.Key == Key.Enter && TabsList.SelectedItem != null)
        {
            SwitchToSelectedTab();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            CloseWindow();
            e.Handled = true;
        }
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        if (!_isShowingDialog)
            CloseWindow();
    }

    private void TabItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (TabsList.SelectedItem != null)
            SwitchToSelectedTab();
    }

    private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void SwitchToSelectedTab()
    {
        if (TabsList.SelectedItem is not WindowRecord selectedWindow)
            return;

        var asTab = true;
        var duplicate = false;

        // Check if SHIFT is pressed (open as new window)
        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            asTab = false;

        // Check if CTRL is pressed (duplicate)
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            duplicate = true;

        _ = _explorerWatcher.SwitchTo(
            selectedWindow.Location,
            selectedWindow.Handle,
            selectedWindow.SelectedItems,
            asTab,
            duplicate
        );

        CloseWindow();
    }

    private void CloseWindow()
    {
        if (_isClosing) return;
        _isClosing = true;
        Close();
    }

    private void ClearClosedWindows_Click(object sender, RoutedEventArgs e)
    {
        _isShowingDialog = true;
        var result = CustomMessageBox.Show(
            "Are you sure you want to clear the closed windows history?",
            "Confirm Clear History",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);
        _isShowingDialog = false;

        if (result == MessageBoxResult.Yes)
        {
            _explorerWatcher.ClearClosedWindows();
            LoadWindows();
        }

        // Re-activate the window and focus the search box
        Activate();
        SearchBox.Focus();
    }

    public new void Show()
    {
        base.Show();
        if (Activate()) return;

        Helper.BypassWinForegroundRestrictions();
        Activate();
    }
}