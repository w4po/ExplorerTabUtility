using System;
using System.Linq;
using System.Text.Json;
using System.Windows.Controls;
using System.Collections.Generic;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.UI.Views;

namespace ExplorerTabUtility.Managers;

public class ProfileManager
{
    // Saved state (persistent)
    private readonly List<HotKeyProfile> _savedProfiles = [];

    // Temporary state (for editing)
    private readonly List<HotKeyProfile> _tempProfiles = [];
    private readonly Panel _profilePanel;

    public event Action? KeybindingsHookStarted;
    public event Action? KeybindingsHookStopped;

    public ProfileManager(Panel profilePanel)
    {
        _profilePanel = profilePanel;

        LoadSavedProfiles();
        RefreshPanel();
    }

    private void LoadSavedProfiles()
    {
        try
        {
            var profiles = JsonSerializer.Deserialize<List<HotKeyProfile>>(SettingsManager.HotKeyProfiles);
            if (profiles == null) return;

            _savedProfiles.Clear();
            _savedProfiles.AddRange(profiles);

            // Create temporary copies
            _tempProfiles.Clear();
            foreach (var p in _savedProfiles)
            {
                _tempProfiles.Add(p.Clone());
            }
        }
        catch
        {
            // Invalid JSON or deserialization error
        }
    }

    public void AddProfile(HotKeyProfile? profile = null)
    {
        var newProfile = profile?.Clone() ?? new HotKeyProfile();
        _tempProfiles.Add(newProfile);
        _profilePanel.Children.Add(new HotKeyProfileControl(newProfile, RemoveProfile, KeybindingHookStarted, KeybindingHookStopped));
    }

    private void RemoveProfile(HotKeyProfile profile)
    {
        _tempProfiles.Remove(profile);
        var control = FindControlByProfile(profile);
        if (control != null)
            _profilePanel.Children.Remove(control);
    }

    private void RefreshPanel()
    {
        _profilePanel.Children.Clear();

        foreach (var profile in _tempProfiles)
        {
            _profilePanel.Children.Add(new HotKeyProfileControl(profile, RemoveProfile, KeybindingHookStarted, KeybindingHookStopped));
        }
    }

    private void KeybindingHookStarted() => KeybindingsHookStarted?.Invoke();
    private void KeybindingHookStopped() => KeybindingsHookStopped?.Invoke();

    public void SetProfileEnabledFromTray(HotKeyProfile profile, bool enabled)
    {
        // Find and update in saved profiles (for tray menu)
        var savedProfile = _savedProfiles.First(p => p.Id == profile.Id);
        savedProfile.IsEnabled = enabled;

        // Find and update in temp profiles (for panel)
        var tempProfile = _tempProfiles.FirstOrDefault(p => p.Id == profile.Id);
        if (tempProfile == null) return;

        tempProfile.IsEnabled = enabled;
        var control = FindControlByProfile(tempProfile);
        if (control != null) control.IsEnabled = enabled;
    }

    public IReadOnlyList<HotKeyProfile> GetProfiles() => _savedProfiles.AsReadOnly();
    public IEnumerable<HotKeyProfile> GetKeyboardProfiles() => _savedProfiles.Where(p => !p.IsMouse);
    public IEnumerable<HotKeyProfile> GetMouseProfiles() => _savedProfiles.Where(p => p.IsMouse);

    public void SaveProfiles()
    {
        // Remove profiles that don't have any hotkeys
        _tempProfiles.Where(p => p.HotKeys == null || p.HotKeys.Length == 0).ToList().ForEach(RemoveProfile);

        // Update saved profiles
        _savedProfiles.Clear();
        foreach (var profile in _tempProfiles)
        {
            _savedProfiles.Add(profile.Clone());
        }

        // Save to settings
        SettingsManager.HotKeyProfiles = JsonSerializer.Serialize(_savedProfiles);
    }

    public void ImportProfiles(string jsonString)
    {
        try
        {
            var importedList = JsonSerializer.Deserialize<List<HotKeyProfile>>(jsonString);
            if (importedList == null) return;

            _tempProfiles.Clear();
            foreach (var profile in importedList)
            {
                _tempProfiles.Add(profile.Clone());
            }

            RefreshPanel();
        }
        catch
        {
            // Invalid JSON or deserialization error
        }
    }

    public string ExportProfiles() => JsonSerializer.Serialize(_savedProfiles);

    private HotKeyProfileControl? FindControlByProfile(HotKeyProfile profile)
    {
        return _profilePanel.Children
            .OfType<HotKeyProfileControl>()
            .FirstOrDefault(c => c.Tag?.Equals(profile) == true);
    }
}