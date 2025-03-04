using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ExplorerTabUtility.UI.Behaviors;

public static class HoverBackground
{
    #region Attached Properties

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(HoverBackground),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty HoverColorProperty =
        DependencyProperty.RegisterAttached(
            "HoverColor",
            typeof(Color),
            typeof(HoverBackground),
            new PropertyMetadata(Colors.DeepPink));

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.RegisterAttached(
            "AnimationDuration",
            typeof(TimeSpan),
            typeof(HoverBackground),
            new PropertyMetadata(TimeSpan.Zero));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);
    public static Color GetHoverColor(DependencyObject obj) => (Color)obj.GetValue(HoverColorProperty);
    public static void SetHoverColor(DependencyObject obj, Color value) => obj.SetValue(HoverColorProperty, value);
    public static TimeSpan GetAnimationDuration(DependencyObject obj) => (TimeSpan)obj.GetValue(AnimationDurationProperty);
    public static void SetAnimationDuration(DependencyObject obj, TimeSpan value) => obj.SetValue(AnimationDurationProperty, value);

    #endregion

    // Property to store the original background
    private static readonly DependencyProperty OriginalBackgroundProperty =
        DependencyProperty.RegisterAttached(
            "OriginalBackground",
            typeof(Brush),
            typeof(HoverBackground),
            new PropertyMetadata(null));

    private static Brush? GetOriginalBackground(DependencyObject obj) => (Brush?)obj.GetValue(OriginalBackgroundProperty);
    private static void SetOriginalBackground(DependencyObject obj, Brush? value) => obj.SetValue(OriginalBackgroundProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;

        if ((bool)e.NewValue)
        {
            element.MouseEnter += Element_MouseEnter;
            element.MouseLeave += Element_MouseLeave;

            // Store the original background if the element has a Background property
            if (element.IsLoaded)
                StoreOriginalBackground(element);
            else
                element.Loaded += (_, _) => StoreOriginalBackground(element);
        }
        else
        {
            element.MouseEnter -= Element_MouseEnter;
            element.MouseLeave -= Element_MouseLeave;

            // Restore original background if needed
            RestoreOriginalBackground(element);
        }
    }

    private static void Element_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not FrameworkElement element) return;

        var hoverColor = GetHoverColor(element);
        var duration = GetAnimationDuration(element);

        AnimateBackgroundChange(element, new SolidColorBrush(hoverColor), duration);
    }

    private static void Element_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not FrameworkElement element) return;

        var originalBrush = GetOriginalBackground(element);
        var duration = GetAnimationDuration(element);

        AnimateBackgroundChange(element, originalBrush, duration);
    }

    #region Helper Methods

    // Animate the background color change
    private static void AnimateBackgroundChange(FrameworkElement element, Brush? targetBrush, Duration duration)
    {
        // Get current background
        var currentBrush = GetBackgroundBrush(element);

        // If no current background or target is null, just set directly
        if (currentBrush == null || targetBrush == null)
        {
            ApplyBackground(element, targetBrush);
            return;
        }

        // Handle solid color brushes for animation
        if (currentBrush is SolidColorBrush currentSolidBrush && targetBrush is SolidColorBrush targetSolidBrush)
        {
            // Create animation
            var colorAnimation = new ColorAnimation
            {
                From = currentSolidBrush.Color,
                To = targetSolidBrush.Color,
                Duration = duration
            };

            // Create a new brush for animation (to avoid freezing issues)
            var animatingBrush = new SolidColorBrush(currentSolidBrush.Color);
            ApplyBackground(element, animatingBrush);

            // Start animation
            animatingBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }
        else
        {
            // For non-solid color brushes, just swap the brushes
            ApplyBackground(element, targetBrush);
        }
    }

    private static Brush? GetBackgroundBrush(FrameworkElement element)
    {
        // Handle different types of controls to get their background
        if (element is Panel panel)
            return panel.Background;

        if (element is Control control)
            return control.Background;

        if (element is Border border)
            return border.Background;

        if (element is Shape shape)
            return shape.Fill;

        // Use reflection for other types
        var propInfo = element.GetType().GetProperty("Background");
        if (propInfo != null && propInfo.PropertyType.IsAssignableFrom(typeof(Brush)))
        {
            return propInfo.GetValue(element) as Brush;
        }

        return null;
    }

    // Store the original background color
    private static void StoreOriginalBackground(FrameworkElement element)
    {
        var originalBackground = GetBackgroundBrush(element);

        // Store the original background
        SetOriginalBackground(element, originalBackground);
    }

    // Restore the original background
    private static void RestoreOriginalBackground(FrameworkElement element)
    {
        var originalBackground = GetOriginalBackground(element);

        if (originalBackground != null)
        {
            ApplyBackground(element, originalBackground);
        }
    }

    private static void ApplyBackground(FrameworkElement element, Brush? brush)
    {
        if (element is Control control)
            control.Background = brush;
        else if (element is Panel panel)
            panel.Background = brush;
        else if (element is Border border)
            border.Background = brush;
        else if (element is Shape shape)
            shape.Fill = brush;
        else
        {
            // Use reflection for other types
            var propInfo = element.GetType().GetProperty("Background");
            if (propInfo != null && propInfo.PropertyType.IsAssignableFrom(typeof(Brush)))
            {
                propInfo.SetValue(element, brush);
            }
        }
    }

    #endregion
}