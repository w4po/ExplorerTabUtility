using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.UI.Views;

// ReSharper disable once RedundantExtendsListEntry
public partial class CustomMessageBox : Window
{
    private MessageBoxResult _result = MessageBoxResult.None;

    public CustomMessageBox()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    /// <summary>
    /// Shows a message box with the specified text, title, buttons, and icon
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="title">The title of the message box</param>
    /// <param name="buttons">The buttons to display</param>
    /// <param name="icon">The icon to display</param>
    /// <param name="defaultButton">The button that should be focused by default</param>
    /// <returns>The result of the message box</returns>
    public static MessageBoxResult Show(string message, string title,
        MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultButton = MessageBoxResult.None)
    {
        return Show(null, message, title, buttons, icon, defaultButton);
    }

    /// <summary>
    /// Shows a message box with the specified owner, text, title, buttons, and icon
    /// </summary>
    /// <param name="owner">The owner window</param>
    /// <param name="message">The message to display</param>
    /// <param name="title">The title of the message box</param>
    /// <param name="buttons">The buttons to display</param>
    /// <param name="icon">The icon to display</param>
    /// <param name="defaultButton">The button that should be focused by default</param>
    /// <returns>The result of the message box</returns>
    public static MessageBoxResult Show(Window? owner, string message, string title,
        MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultButton = MessageBoxResult.None)
    {
        var messageBox = new CustomMessageBox
        {
            Title = string.IsNullOrEmpty(title) ? Constants.AppName : title,
            MessageText =
            {
                Text = message
            },
            Owner = owner,
            WindowStartupLocation = owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen
        };

        messageBox.SetIcon(icon);
        messageBox.AddButtons(buttons, defaultButton);
        messageBox.ShowDialog();
        return messageBox._result;
    }

    private void SetIcon(MessageBoxImage icon)
    {
        if (icon == MessageBoxImage.None)
        {
            IconPath.Visibility = Visibility.Collapsed;
            return;
        }

        IconPath.Visibility = Visibility.Visible;

        switch (icon)
        {
            case MessageBoxImage.Error:
                IconPath.Data =
                    Geometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z");
                IconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
                break;
            case MessageBoxImage.Warning:
                IconPath.Data = Geometry.Parse("M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z");
                IconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"));
                break;
            case MessageBoxImage.Information:
                IconPath.Data =
                    Geometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z");
                IconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
                break;
            case MessageBoxImage.Question:
                IconPath.Data = Geometry.Parse(
                    "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 17h-2v-2h2v2zm2.07-7.75l-.9.92C13.45 12.9 13 13.5 13 15h-2v-.5c0-1.1.45-2.1 1.17-2.83l1.24-1.26c.37-.36.59-.86.59-1.41 0-1.1-.9-2-2-2s-2 .9-2 2H8c0-2.21 1.79-4 4-4s4 1.79 4 4c0 .88-.36 1.68-.93 2.25z");
                IconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00838F"));
                break;
        }
    }

    private void AddButtons(MessageBoxButton buttons, MessageBoxResult defaultButton = MessageBoxResult.None)
    {
        ButtonPanel.Children.Clear();

        switch (buttons)
        {
            case MessageBoxButton.OK:
                AddButton("OK", MessageBoxResult.OK, defaultButton is MessageBoxResult.None or MessageBoxResult.OK);
                break;
            case MessageBoxButton.OKCancel:
                AddButton("OK", MessageBoxResult.OK, defaultButton is MessageBoxResult.None or MessageBoxResult.OK);
                AddButton("Cancel", MessageBoxResult.Cancel, defaultButton is MessageBoxResult.Cancel);
                break;
            case MessageBoxButton.YesNo:
                AddButton("Yes", MessageBoxResult.Yes, defaultButton is MessageBoxResult.None or MessageBoxResult.Yes);
                AddButton("No", MessageBoxResult.No, defaultButton is MessageBoxResult.No);
                break;
            case MessageBoxButton.YesNoCancel:
                AddButton("Yes", MessageBoxResult.Yes, defaultButton is MessageBoxResult.None or MessageBoxResult.Yes);
                AddButton("No", MessageBoxResult.No, defaultButton is MessageBoxResult.No);
                AddButton("Cancel", MessageBoxResult.Cancel, defaultButton is MessageBoxResult.Cancel);
                break;
        }
    }

    private void AddButton(string text, MessageBoxResult result, bool isDefault = false)
    {
        var button = new Button
        {
            Content = text,
            IsDefault = isDefault
        };

        button.Click += (_, _) =>
        {
            _result = result;
            Close();
        };

        ButtonPanel.Children.Add(button);

        if (isDefault)
            button.Focus();
    }
}