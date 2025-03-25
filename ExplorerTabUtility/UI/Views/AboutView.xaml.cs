using System;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows;

namespace ExplorerTabUtility.UI.Views;

// ReSharper disable once RedundantExtendsListEntry
public partial class AboutView : UserControl
{
    private bool _isSupportersLoaded;

    public string Version
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v1.0.0";
        }
    }

    public AboutView()
    {
        InitializeComponent();
    }

    private void AboutView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isSupportersLoaded) return;
        _ = LoadSupporters();
    }

    private async Task LoadSupporters()
    {
        try
        {
            var supporters = await Helpers.Helper.GetSupporters().ConfigureAwait(true);
            if (supporters.Count > 0) _isSupportersLoaded = true;

            SupportersItemsControl.ItemsSource = supporters;
        }
        catch
        {
            // Ignored
        }
    }

    private void StarClickAnimation_Completed(object _, EventArgs __)
    {
        // Restart the continuous animation after the click animation is done
        var continuousStoryboard = FindName("ContinuousStarStoryboard") as BeginStoryboard;
        continuousStoryboard?.Storyboard?.Begin();
    }

    private void BtnGitHubSponsors_Click(object _, RoutedEventArgs __) => OpenUrl("https://github.com/sponsors/w4po");
    private void BtnPatreon_Click(object _, RoutedEventArgs __) => OpenUrl("https://www.patreon.com/w4po");
    private void BtnBuyMeACoffee_Click(object _, RoutedEventArgs __) => OpenUrl("https://www.buymeacoffee.com/w4po");
    private void BtnPayPal_Click(object _, RoutedEventArgs __) => OpenUrl("https://paypal.me/w4po77");
    private void OpenDeveloperPage(object _, RoutedEventArgs __) => OpenUrl("https://github.com/w4po");
    private void OpenProjectPage(object _, RoutedEventArgs __) => OpenUrl("https://github.com/w4po/ExplorerTabUtility");

    private void OpenSupportPage(object sender, RoutedEventArgs _)
    {
        if (sender is Border { Tag: string profileUrl } && !string.IsNullOrWhiteSpace(profileUrl))
            OpenUrl(profileUrl);
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore
        }
    }
}