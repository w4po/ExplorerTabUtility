using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.UI.Views.Controls;

// ReSharper disable once RedundantExtendsListEntry
public partial class NumericInputControl : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(NumericInputControl),
            new PropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(NumericInputControl),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(NumericInputControl),
            new PropertyMetadata(double.MaxValue));

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(NumericInputControl),
            new PropertyMetadata(1.0));

    public static readonly DependencyProperty SuffixProperty =
        DependencyProperty.Register(
            nameof(Suffix),
            typeof(string),
            typeof(NumericInputControl),
            new PropertyMetadata(string.Empty, OnSuffixChanged));

    public static readonly DependencyProperty StringFormatProperty =
        DependencyProperty.Register(
            nameof(StringFormat),
            typeof(string),
            typeof(NumericInputControl),
            new PropertyMetadata("{0:N0}", OnStringFormatChanged));

    public static readonly DependencyProperty ValueChangedCommandProperty =
        DependencyProperty.Register(
            nameof(ValueChangedCommand),
            typeof(ICommand),
            typeof(NumericInputControl),
            new PropertyMetadata(null));

    #endregion

    #region Properties

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public string Suffix
    {
        get => (string)GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public string StringFormat
    {
        get => (string)GetValue(StringFormatProperty);
        set => SetValue(StringFormatProperty, value);
    }

    public ICommand? ValueChangedCommand
    {
        get => (ICommand)GetValue(ValueChangedCommandProperty);
        set => SetValue(ValueChangedCommandProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<RoutedPropertyChangedEventArgs<double>>? ValueChanged;

    #endregion

    #region Fields

    private readonly DispatcherTimer? _repeatTimer;
    private const double InitialDelay = 500; // ms before starting continuous increment/decrement
    private const double RepeatInterval = 50; // ms between increments/decrements
    private bool _isIncrementing;
    private System.Windows.Media.Brush? _originalCaretBrush;
    private bool _isCaretHidden;

    #endregion

    #region Constructor

    public NumericInputControl()
    {
        InitializeComponent();
        UpdateTextFromValue();

        // Add command binding for paste
        var pasteBinding = new CommandBinding(ApplicationCommands.Paste, PasteExecuted, PasteCanExecute);
        TxtValue.CommandBindings.Add(pasteBinding);

        // Initialize timer for button repeat
        _repeatTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(RepeatInterval)
        };
        _repeatTimer.Tick += RepeatTimer_Tick;

        // Add mouse events for the buttons
        BtnIncrement.PreviewMouseLeftButtonDown += BtnIncrement_PreviewMouseLeftButtonDown;
        BtnIncrement.PreviewMouseLeftButtonUp += Btn_PreviewMouseLeftButtonUp;
        BtnIncrement.MouseLeave += Btn_MouseLeave;
        BtnIncrement.Click += BtnIncrement_Click;

        BtnDecrement.PreviewMouseLeftButtonDown += BtnDecrement_PreviewMouseLeftButtonDown;
        BtnDecrement.PreviewMouseLeftButtonUp += Btn_PreviewMouseLeftButtonUp;
        BtnDecrement.MouseLeave += Btn_MouseLeave;
        BtnDecrement.Click += BtnDecrement_Click;
    }

    #endregion

    #region Property Change Handlers

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericInputControl control)
        {
            control.OnValueChanged((double)e.OldValue, (double)e.NewValue);
        }
    }

    private static void OnSuffixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericInputControl control)
        {
            control.SuffixLabel.Text = (string)e.NewValue;
        }
    }

    private static void OnStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericInputControl control)
        {
            control.UpdateTextFromValue();
        }
    }

    private void OnValueChanged(double oldValue, double newValue)
    {
        // Update the text box
        UpdateTextFromValue();

        // Raise the ValueChanged event
        ValueChanged?.Invoke(this, new RoutedPropertyChangedEventArgs<double>(oldValue, newValue));

        // Execute the command if provided
        if (ValueChangedCommand != null && ValueChangedCommand.CanExecute(newValue))
        {
            ValueChangedCommand.Execute(newValue);
        }
    }

    #endregion

    #region Input Validation

    private static bool IsValidNumericInput(string text)
    {
        // Allow digits, commas, and decimal points
        return text.All(c => char.IsDigit(c) || c == ',' || c == '.');
    }

    private bool HasLeadingZeros(string text)
    {
        return text.StartsWith("0") && text.Length > 1 && text[1] != ',' && text[1] != '.';
    }

    private void PasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Paste)
        {
            // Get the clipboard text
            var clipboardText = Clipboard.GetText();

            // Allow paste if the text contains valid numeric characters (digits, commas, decimal points)
            var isValidText = !string.IsNullOrEmpty(clipboardText) && IsValidNumericInput(clipboardText);

            // Check for leading zeros in paste content
            if (isValidText && sender is TextBox textBox)
            {
                // If pasting at the beginning and the clipboard starts with 0
                if (textBox.CaretIndex == 0 && HasLeadingZeros(clipboardText))
                {
                    isValidText = false;
                }
                // If the textbox is empty or "0" and the clipboard starts with 0
                else if ((string.IsNullOrEmpty(textBox.Text) || textBox.Text == "0") &&
                         HasLeadingZeros(clipboardText))
                {
                    isValidText = false;
                }
            }

            e.CanExecute = isValidText;
            e.Handled = true;
        }
    }

    private void PasteExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Paste && sender is TextBox textBox)
        {
            var clipboardText = Clipboard.GetText();
            if (!string.IsNullOrEmpty(clipboardText) && IsValidNumericInput(clipboardText))
            {
                // Get the current text and selection
                var selectionStart = textBox.SelectionStart;
                var selectionLength = textBox.SelectionLength;
                var currentText = textBox.Text;

                // Create the new text by replacing the selected portion
                var newText = currentText.Substring(0, selectionStart) +
                              clipboardText +
                              currentText.Substring(selectionStart + selectionLength);

                // Update the text
                textBox.Text = newText;

                // Set the caret position after the pasted text
                textBox.CaretIndex = selectionStart + clipboardText.Length;

                e.Handled = true;
            }
        }
    }

    private void TxtValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Only allow numeric input and decimal separators
        if (!IsValidNumericInput(e.Text))
        {
            e.Handled = true;
            return;
        }

        // If it's a decimal point or comma, check if there's already one in the text
        if (e.Text is "." or ",")
        {
            if (sender is TextBox textBox && (textBox.Text.Contains(".") || textBox.Text.Contains(",")))
            {
                e.Handled = true;
                return;
            }
        }

        // Prevent leading zeros
        if (e.Text == "0")
        {
            if (sender is not TextBox textBox) return;
            // If the textbox is empty or contains only "0", don't allow additional zeros
            if (string.IsNullOrEmpty(textBox.Text) || textBox.Text == "0")
            {
                // Allow a single zero
                if (textBox.Text == "0")
                {
                    e.Handled = true;
                }
            }
            // If the caret is at the start and the text doesn't start with a digit, don't allow leading zero
            else if (textBox.CaretIndex == 0 && !char.IsDigit(textBox.Text[0]))
            {
                e.Handled = true;
            }
        }
    }

    #endregion

    #region Caret Position Management

    private int GetCaretPositionAfterFormatting(int currentPosition, string formattedText)
    {
        // Adjust for suffix if present
        var suffixLength = string.IsNullOrEmpty(Suffix) ? 0 : Suffix.Length;

        // Ensure caret position is valid
        var maxPosition = Math.Max(0, formattedText.Length - suffixLength);
        return Math.Min(currentPosition, maxPosition);
    }

    private void SetCaretPositionAtEnd(TextBox textBox)
    {
        var suffixLength = string.IsNullOrEmpty(Suffix) ? 0 : Suffix.Length;
        var position = Math.Max(0, textBox.Text.Length - suffixLength);
        textBox.CaretIndex = position;
    }

    private void RestoreCaretPosition(TextBox textBox, int position, string newText)
    {
        if (position >= 0)
        {
            textBox.CaretIndex = GetCaretPositionAfterFormatting(position, newText);
        }
    }

    /// <summary>
    /// Calculates the appropriate caret position after a text change, accounting for commas
    /// </summary>
    private int CalculateCaretPositionAfterDelete(string oldText, string newText, int oldCaretPosition)
    {
        // If the texts are the same, don't change the position
        if (oldText == newText)
            return oldCaretPosition;

        // If we're at the beginning, stay at the beginning
        if (oldCaretPosition == 0)
            return 0;

        // If we're at the end, stay at the end
        if (oldCaretPosition >= oldText.Length)
            return newText.Length;

        // Count commas before the caret in the old text
        var oldCommaCount = CountCommasBeforePosition(oldText, oldCaretPosition);

        // Calculate the digit position (ignoring commas)
        var digitPosition = oldCaretPosition - oldCommaCount;

        // Count commas before the equivalent digit position in the new text
        var newCommaCount = CountCommasBeforeDigitPosition(newText, digitPosition);

        // Calculate the new caret position
        var newCaretPosition = digitPosition + newCommaCount;

        // Ensure the new position is within bounds
        return Math.Min(newCaretPosition, newText.Length);
    }

    private int CountCommasBeforePosition(string text, int position)
    {
        var count = 0;
        for (var i = 0; i < position && i < text.Length; i++)
        {
            if (text[i] == ',')
                count++;
        }

        return count;
    }

    private int CountCommasBeforeDigitPosition(string text, int digitPosition)
    {
        var count = 0;
        var currentDigitPos = 0;

        foreach (var c in text)
        {
            if (c == ',')
            {
                count++;
            }
            else
            {
                // This is a digit
                currentDigitPos++;
                if (currentDigitPos >= digitPosition)
                    break;
            }
        }

        return count;
    }

    #endregion

    #region Value Manipulation

    private void IncrementValue()
    {
        HideCaretDuringOperation();

        var newValue = Math.Min(Value + Step, Maximum);
        if (Math.Abs(Value - newValue) > double.Epsilon)
        {
            Value = newValue;
        }
    }

    private void DecrementValue()
    {
        HideCaretDuringOperation();

        var newValue = Math.Max(Value - Step, Minimum);
        if (Math.Abs(Value - newValue) > double.Epsilon)
        {
            Value = newValue;
        }
    }

    private void HideCaretDuringOperation()
    {
        if (!_isCaretHidden)
        {
            _originalCaretBrush = TxtValue.CaretBrush;
            TxtValue.CaretBrush = System.Windows.Media.Brushes.Transparent;
            _isCaretHidden = true;
        }
    }

    private void RestoreCaretVisibility()
    {
        if (_isCaretHidden)
        {
            TxtValue.CaretBrush = _originalCaretBrush;
            _isCaretHidden = false;

            // Set caret position at the end of the text (before the suffix)
            SetCaretPositionAtEnd(TxtValue);
        }
    }

    private void UpdateTextFromValue()
    {
        // Store the current caret position if the TextBox is focused
        var caretPosition = TxtValue.IsFocused ? TxtValue.CaretIndex : -1;

        // Format the value according to the string format
        var formattedValue = string.Format(StringFormat, Value);

        // Update the text box without triggering the TextChanged event
        if (TxtValue.Text != formattedValue)
        {
            TxtValue.Text = formattedValue;
            RestoreCaretPosition(TxtValue, caretPosition, formattedValue);
        }
    }

    private string NormalizeNumericText(string text)
    {
        // Remove non-numeric characters except commas and decimal points
        var cleanText = new string(text.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray());

        // Replace decimal points with commas for consistent parsing
        cleanText = cleanText.Replace('.', ',');

        // Handle comma at the beginning
        if (cleanText.StartsWith(","))
        {
            cleanText = "0" + cleanText;
        }

        // Try to parse and normalize
        if (double.TryParse(cleanText, out var value))
        {
            // If it's a whole number, remove leading zeros by converting to int and back to string
            if (Math.Abs(value - Math.Floor(value)) < double.Epsilon)
            {
                return ((int)value).ToString();
            }
            else
            {
                // For decimal values, return the parsed value as string to maintain decimal places
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        return cleanText;
    }

    private double ClampValue(double value)
    {
        return value.Clamp(Minimum, Maximum);
    }

    #endregion

    #region Button Repeat Logic

    private void RepeatTimer_Tick(object? sender, EventArgs e)
    {
        if (_repeatTimer == null) return;

        // After the first tick (initial delay), change to faster repeat interval
        if (Math.Abs(_repeatTimer.Interval.TotalMilliseconds - InitialDelay) < double.Epsilon)
        {
            _repeatTimer.Interval = TimeSpan.FromMilliseconds(RepeatInterval);
        }

        if (_isIncrementing)
        {
            IncrementValue();
        }
        else
        {
            DecrementValue();
        }
    }

    private void BtnIncrement_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_repeatTimer == null) return;

        _isIncrementing = true;
        IncrementValue();

        // Start timer with initial delay
        _repeatTimer.Interval = TimeSpan.FromMilliseconds(InitialDelay);
        _repeatTimer.Start();

        // Capture mouse to receive mouse up even if pointer leaves the button
        Mouse.Capture((UIElement)sender);

        // Set focus to the text input field
        TxtValue.Focus();
    }

    private void BtnDecrement_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_repeatTimer == null) return;

        _isIncrementing = false;
        DecrementValue();

        // Start timer with initial delay
        _repeatTimer.Interval = TimeSpan.FromMilliseconds(InitialDelay);
        _repeatTimer.Start();

        // Capture mouse to receive mouse up even if pointer leaves the button
        Mouse.Capture((UIElement)sender);

        // Set focus to the text input field
        TxtValue.Focus();
    }

    private void Btn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        StopRepeatTimer();
        Mouse.Capture(null);

        // Set focus to the text input field
        TxtValue.Focus();
    }

    private void Btn_MouseLeave(object sender, MouseEventArgs e)
    {
        // Only stop if mouse button is not pressed
        if (Mouse.Captured != sender)
        {
            StopRepeatTimer();
        }
    }

    private void StopRepeatTimer()
    {
        if (_repeatTimer is { IsEnabled: true })
        {
            _repeatTimer.Stop();
            RestoreCaretVisibility();
        }
    }

    #endregion

    #region Event Handlers

    private void TxtValue_TextChanged(object sender, TextChangedEventArgs e)
    {
        // If user is typing, ensure caret is visible
        if (_repeatTimer == null || !_repeatTimer.IsEnabled)
        {
            RestoreCaretVisibility();
        }

        if (sender is not TextBox textBox) return;

        // Store the current text and caret position
        var caretPosition = textBox.CaretIndex;

        // Handle empty or comma-starting text
        if (string.IsNullOrEmpty(textBox.Text) || textBox.Text.StartsWith(","))
        {
            double newValue = 0;
            if (textBox.Text.StartsWith(",") && textBox.Text.Length > 1)
            {
                // Try to parse the value after the comma
                if (double.TryParse("0" + textBox.Text, out var parsedValue))
                {
                    newValue = parsedValue;
                }
            }

            Value = newValue;

            // Don't update the text if it's empty - let the user continue typing
            if (string.IsNullOrEmpty(textBox.Text))
            {
                return;
            }

            var formattedValue = string.Format(StringFormat, newValue);

            // Only update if the text has changed to avoid resetting caret
            if (textBox.Text != formattedValue)
            {
                textBox.Text = formattedValue;
                RestoreCaretPosition(textBox, caretPosition == 0 ? 0 : -1, formattedValue);
            }

            return;
        }

        // Try to parse the current text as a number
        if (double.TryParse(textBox.Text, out var value))
        {
            // Normalize the text (remove leading zeros)
            var normalizedText = NormalizeNumericText(textBox.Text);

            // If the normalized text is different from the current text (ignoring commas)
            if (normalizedText != textBox.Text.Replace(",", ""))
            {
                var formattedValue = string.Format(StringFormat, value);

                // Only update if the text has changed to avoid resetting caret
                if (textBox.Text == formattedValue) return;
                // Save the old text before updating
                var originalText = textBox.Text;

                textBox.Text = formattedValue;

                // Use the special caret position calculation for comma handling
                textBox.CaretIndex = CalculateCaretPositionAfterDelete(originalText, formattedValue, caretPosition);
                return;
            }

            // Ensure value is within bounds
            var clampedValue = ClampValue(value);

            // If the value exceeds the maximum, revert to the maximum
            if (value > Maximum)
            {
                var formattedValue = string.Format(StringFormat, Maximum);

                // Only update if the text has changed to avoid resetting caret
                if (textBox.Text != formattedValue)
                {
                    textBox.Text = formattedValue;
                    SetCaretPositionAtEnd(textBox);
                }

                return;
            }

            // Update the value if it's different
            if (Math.Abs(Value - clampedValue) > double.Epsilon)
            {
                // Save the old text before updating the value
                var originalText = textBox.Text;

                Value = clampedValue;

                // If the text was changed due to the value update
                if (originalText != textBox.Text)
                {
                    // Use the special caret position calculation for comma handling
                    textBox.CaretIndex = CalculateCaretPositionAfterDelete(originalText, textBox.Text, caretPosition);
                }
            }

            // Don't reset the caret position if we didn't change the text
            // This is crucial for delete operations
        }
        else
        {
            // Text couldn't be parsed as a number, clean it up
            var cleanText = NormalizeNumericText(textBox.Text);

            // If we have a clean version with digits
            if (!string.IsNullOrEmpty(cleanText) && cleanText.Any(char.IsDigit))
            {
                // Try to parse the cleaned text
                if (double.TryParse(cleanText, out var cleanValue))
                {
                    // Format and update
                    var formattedValue = string.Format(StringFormat, cleanValue);

                    // Only update if the text has changed to avoid resetting caret
                    if (textBox.Text != formattedValue)
                    {
                        // Save the old text before updating
                        var originalText = textBox.Text;

                        textBox.Text = formattedValue;

                        // Update the value
                        Value = ClampValue(cleanValue);

                        // Use the special caret position calculation for comma handling
                        textBox.CaretIndex = CalculateCaretPositionAfterDelete(originalText, formattedValue, caretPosition);
                    }
                }
                else
                {
                    ResetToZero(textBox);
                }
            }
            else
            {
                ResetToZero(textBox);
            }
        }
    }

    private void ResetToZero(TextBox textBox)
    {
        // Reset to 0
        Value = 0;
        textBox.Text = string.Format(StringFormat, 0);
        textBox.CaretIndex = 1; // Position after the "0"
    }

    private void TxtValue_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Handle Up/Down arrow keys to increment/decrement the value
        if (e.Key == Key.Up)
        {
            IncrementValue();
            e.Handled = true;
            return;
        }
        else if (e.Key == Key.Down)
        {
            DecrementValue();
            e.Handled = true;
            return;
        }

        // If user starts typing, ensure caret is visible
        RestoreCaretVisibility();

        // Allow navigation keys and delete/backspace keys
        if (IsNavigationKey(e.Key) || IsDeleteKey(e.Key))
        {
            return;
        }

        // Allow keyboard shortcuts
        if (IsAllowedKeyboardShortcut(e.Key, Keyboard.Modifiers))
        {
            return;
        }

        // Allow numeric keys (both main keyboard and numpad)
        if (IsNumericKey(e.Key))
        {
            return;
        }

        // Block all other keys
        e.Handled = true;
    }

    private bool IsNavigationKey(Key key)
    {
        return key == Key.Left || key == Key.Right ||
               key == Key.Home || key == Key.End ||
               key == Key.Tab;
    }

    private bool IsDeleteKey(Key key)
    {
        return key == Key.Back || key == Key.Delete;
    }

    private bool IsNumericKey(Key key)
    {
        return key is >= Key.D0 and <= Key.D9 or >= Key.NumPad0 and <= Key.NumPad9;
    }

    private bool IsAllowedKeyboardShortcut(Key key, ModifierKeys modifiers)
    {
        // Allow Ctrl+A (select all)
        if (key == Key.A && (modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            return true;
        }

        // Allow Ctrl+C (copy)
        if (key == Key.C && (modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            return true;
        }

        // Allow Ctrl+X (cut)
        if (key == Key.X && (modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            return true;
        }

        // Allow Ctrl+V (paste) - will be handled by CommandBinding
        return key == Key.V && (modifiers & ModifierKeys.Control) == ModifierKeys.Control;
    }

    private void BtnIncrement_Click(object sender, RoutedEventArgs e)
    {
        // Handle single clicks for when PreviewMouseLeftButtonDown/Up are not triggered
        // (e.g., when clicking with touch or pen)
        IncrementValue();

        // Set focus to the text input field
        TxtValue.Focus();
    }

    private void BtnDecrement_Click(object sender, RoutedEventArgs e)
    {
        // Handle single clicks for when PreviewMouseLeftButtonDown/Up are not triggered
        // (e.g., when clicking with touch or pen)
        DecrementValue();

        // Set focus to the text input field
        TxtValue.Focus();
    }

    #endregion
}