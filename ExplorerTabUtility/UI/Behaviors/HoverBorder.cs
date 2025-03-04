using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExplorerTabUtility.UI.Behaviors;

public static class HoverBorder
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(HoverBorder),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.RegisterAttached(
            "BorderBrush",
            typeof(Brush),
            typeof(HoverBorder),
            new PropertyMetadata(Brushes.DarkSlateBlue));

    private static readonly DependencyProperty OriginalBorderBrushProperty =
        DependencyProperty.RegisterAttached(
            "OriginalBorderBrush",
            typeof(Brush),
            typeof(HoverBorder),
            new PropertyMetadata(null));

    private static readonly DependencyProperty OriginalBorderThicknessProperty =
        DependencyProperty.RegisterAttached(
            "OriginalBorderThickness",
            typeof(Thickness),
            typeof(HoverBorder),
            new PropertyMetadata(new Thickness(0)));

    // Getter and setter methods
    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static Brush GetBorderBrush(DependencyObject element) => (Brush)element.GetValue(BorderBrushProperty);
    public static void SetBorderBrush(DependencyObject element, Brush? value) => element.SetValue(BorderBrushProperty, value);

    private static Brush GetOriginalBorderBrush(DependencyObject element) => (Brush)element.GetValue(OriginalBorderBrushProperty);
    private static void SetOriginalBorderBrush(DependencyObject element, Brush value) => element.SetValue(OriginalBorderBrushProperty, value);

    private static Thickness GetOriginalBorderThickness(DependencyObject element) =>
        (Thickness)element.GetValue(OriginalBorderThicknessProperty);

    private static void SetOriginalBorderThickness(DependencyObject element, Thickness value) =>
        element.SetValue(OriginalBorderThicknessProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Border element) return;

        if ((bool)e.NewValue)
        {
            element.MouseEnter += OnMouseEnter;
            element.MouseLeave += OnMouseLeave;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
        }
    }

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Border border) return;

        // Store original values
        SetOriginalBorderBrush(border, border.BorderBrush);
        SetOriginalBorderThickness(border, border.BorderThickness);

        // Apply hover effect
        var highlightBrush = GetBorderBrush(border);
        border.BorderBrush = highlightBrush;
        border.BorderThickness = new Thickness(1.25);
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Border border) return;

        // Restore original values
        border.BorderBrush = GetOriginalBorderBrush(border);
        border.BorderThickness = GetOriginalBorderThickness(border);
    }
}