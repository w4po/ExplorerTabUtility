using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Documents;
using System.Windows.Controls;

namespace ExplorerTabUtility.UI.Behaviors;

public static class HoverBrightness
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(HoverBrightness),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty BrightnessFactorProperty =
        DependencyProperty.RegisterAttached(
            "BrightnessFactor",
            typeof(double),
            typeof(HoverBrightness),
            new PropertyMetadata(0.05));

    public static readonly DependencyProperty OverlayColorProperty =
        DependencyProperty.RegisterAttached(
            "OverlayColor",
            typeof(Color),
            typeof(HoverBrightness),
            new PropertyMetadata(Colors.White));

    public static readonly DependencyProperty OverlayBrushProperty =
        DependencyProperty.RegisterAttached(
            "OverlayBrush",
            typeof(Brush),
            typeof(HoverBrightness),
            new PropertyMetadata(Brushes.Transparent));

    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.RegisterAttached(
            "Target",
            typeof(UIElement),
            typeof(HoverBrightness),
            new PropertyMetadata(null));

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
    public static double GetBrightnessFactor(DependencyObject obj) => (double)obj.GetValue(BrightnessFactorProperty);
    public static void SetBrightnessFactor(DependencyObject obj, double value) => obj.SetValue(BrightnessFactorProperty, value);
    public static void SetOverlayColor(DependencyObject element, Color value) => element.SetValue(OverlayColorProperty, value);
    public static Color GetOverlayColor(DependencyObject element) => (Color)element.GetValue(OverlayColorProperty);
    public static void SetOverlayBrush(DependencyObject element, Brush value) => element.SetValue(OverlayBrushProperty, value);
    public static Brush GetOverlayBrush(DependencyObject element) => (Brush)element.GetValue(OverlayBrushProperty);
    public static void SetTarget(DependencyObject element, UIElement value) => element.SetValue(TargetProperty, value);
    public static UIElement? GetTarget(DependencyObject element) => (UIElement?)element.GetValue(TargetProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;

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
        if (sender is not UIElement element) return;

        var adornerLayer = AdornerLayer.GetAdornerLayer(element);
        if (adornerLayer == null) return;

        // Remove existing adorners to avoid duplicates
        var existingAdorners = adornerLayer.GetAdorners(element);
        if (existingAdorners != null)
        {
            foreach (var adorner in existingAdorners)
            {
                if (adorner is BrightnessAdorner)
                {
                    adornerLayer.Remove(adorner);
                }
            }
        }

        // Add new brightness adorner
        adornerLayer.Add(new BrightnessAdorner(element));
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not UIElement element) return;

        var adornerLayer = AdornerLayer.GetAdornerLayer(element);
        if (adornerLayer == null) return;

        var adorners = adornerLayer.GetAdorners(element);
        if (adorners == null) return;

        foreach (var adorner in adorners)
        {
            if (adorner is BrightnessAdorner)
            {
                adornerLayer.Remove(adorner);
            }
        }
    }

    private class BrightnessAdorner : Adorner
    {
        private readonly UIElement _overlay;
        private readonly UIElement? _target;
        private readonly UIElement? _adornedElement;

        public BrightnessAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _adornedElement = adornedElement;

            var color = GetOverlayColor(adornedElement);
            var opacity = GetBrightnessFactor(adornedElement);
            var brush = GetOverlayBrush(adornedElement);
            if (brush == Brushes.Transparent)
                brush = new SolidColorBrush(color);

            // Get target element if specified, otherwise find border in template
            _target = GetTarget(adornedElement);
            _target = FindTemplateBorder(_target ?? adornedElement);
            if (_target is Border targetBorder)
            {
                // Create a border with matching corner radius
                _overlay = new Border
                {
                    Width = targetBorder.Width,
                    Height = targetBorder.Height,
                    CornerRadius = targetBorder.CornerRadius,
                    Opacity = opacity,
                    Background = brush,
                    IsHitTestVisible = false
                };
            }
            else
            {
                // Fallback to a rectangle
                _overlay = new Rectangle
                {
                    Fill = brush,
                    Opacity = opacity,
                    IsHitTestVisible = false
                };
            }

            AddVisualChild(_overlay);
        }

        private static Border? FindTemplateBorder(UIElement element)
        {
            // Direct border
            if (element is Border border)
                return border;

            // Control with template
            if (element is Control control)
            {
                control.ApplyTemplate();

                // Try to find border by name first
                if (control.Template?.FindName("border", control) is Border namedBorder)
                    return namedBorder;

                // Then try first border in visual tree
                return FindFirstVisualChild<Border>(control);
            }

            // Try visual tree for other elements
            return FindFirstVisualChild<Border>(element);
        }

        private static T? FindFirstVisualChild<T>(DependencyObject? parent) where T : DependencyObject
        {
            // Early exit if parent is null
            if (parent == null) return null;

            var childCount = VisualTreeHelper.GetChildrenCount(parent);

            // Direct children first (breadth-first approach)
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                    return typedChild;
            }

            // Then check children's children
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var result = FindFirstVisualChild<T>(child);

                if (result != null)
                    return result;
            }

            return null;
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index) => _overlay;

        protected override Size MeasureOverride(Size constraint)
        {
            _overlay.Measure(constraint);
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_target != null)
            {
                var targetPos = _target.TranslatePoint(new Point(0, 0), _adornedElement);
                _overlay.Arrange(new Rect(targetPos, _target.RenderSize));
                return finalSize;
            }

            _overlay.Arrange(new Rect(finalSize));
            return finalSize;
        }
    }
}