using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ExplorerTabUtility.WinAPI;

namespace ExplorerTabUtility.Helpers;

public static class KeyboardSimulator
{
    public static readonly VirtualKey[] ModifierKeys =
    [
        VirtualKey.LeftShift,
        VirtualKey.RightShift,
        VirtualKey.LeftControl,
        VirtualKey.RightControl,
        VirtualKey.LeftAlt,
        VirtualKey.RightAlt,
        VirtualKey.LWin,
        VirtualKey.RWin
    ];

    public static bool IsModifierKey(VirtualKey keyCode) => ModifierKeys.Contains(keyCode);
    public static bool IsExtendedKey(VirtualKey keyCode)
    {
        return keyCode
            is VirtualKey.RightAlt
            or VirtualKey.RightControl
            or VirtualKey.Insert
            or VirtualKey.Delete
            or VirtualKey.Home
            or VirtualKey.End
            or VirtualKey.PageUp
            or VirtualKey.PageDown
            or VirtualKey.Left
            or VirtualKey.Up
            or VirtualKey.Right
            or VirtualKey.Down
            or VirtualKey.NumLock
            or VirtualKey.Cancel
            or VirtualKey.Divide
            or VirtualKey.LWin
            or VirtualKey.RWin
            or VirtualKey.Apps;
    }
    public static bool IsKeyPressed(int keyCode)
    {
        return (WinApi.GetAsyncKeyState(keyCode) & 0x8000) != 0;
    }

    public static void ModifiedKeyStroke(VirtualKey modifierKeyCode, VirtualKey keyCode)
    {
        var inputs = new[]
        {
            CreateKeyDown(modifierKeyCode),
            CreateKeyDown(keyCode),
            CreateKeyUp(keyCode),
            CreateKeyUp(modifierKeyCode)
        };

        SendInputs(inputs);
    }
    public static void ModifiedKeyStroke(VirtualKey[] modifierKeyCodes, VirtualKey keyCode)
    {
        // 2 for each modifier (down and up) + 2 (for keyCode down and up)
        var totalEvents = 2 * modifierKeyCodes.Length + 2;
        var inputs = new INPUT[totalEvents];
        var index = 0;

        // Add modifier key down events in order.
        for (var i = 0; i < modifierKeyCodes.Length; i++)
            inputs[index++] = CreateKeyDown(modifierKeyCodes[i]);

        // Add key press events for the main key.
        inputs[index++] = CreateKeyDown(keyCode);
        inputs[index++] = CreateKeyUp(keyCode);

        // Add modifier key up events in reverse order.
        for (var i = modifierKeyCodes.Length - 1; i >= 0; i--)
            inputs[index++] = CreateKeyUp(modifierKeyCodes[i]);

        SendInputs(inputs);
    }
    public static void ModifiedKeyStroke(VirtualKey modifierKeyCode, params VirtualKey[] keyCodes)
    {
        // 2 for each key (down and up) + 2 (for modifier down and up)
        var totalEvents = 2 * keyCodes.Length + 2;
        var inputs = new INPUT[totalEvents];
        var index = 0;

        // Modifier key down
        inputs[index++] = CreateKeyDown(modifierKeyCode);

        // Key down for each key in order
        for (var i = 0; i < keyCodes.Length; i++)
            inputs[index++] = CreateKeyDown(keyCodes[i]);

        // Key up for each key in reverse order
        for (var i = keyCodes.Length - 1; i >= 0; i--)
            inputs[index++] = CreateKeyUp(keyCodes[i]);

        // Modifier key up
        inputs[index] = CreateKeyUp(modifierKeyCode);

        SendInputs(inputs);
    }
    public static void ModifiedKeyStroke(VirtualKey[] modifierKeyCodes, params VirtualKey[] keyCodes)
    {
        // Each key produces two inputs (down and up)
        var totalEvents = 2 * (modifierKeyCodes.Length + keyCodes.Length);
        var inputs = new INPUT[totalEvents];
        var index = 0;

        // Modifier key down events (in order)
        for (var i = 0; i < modifierKeyCodes.Length; i++)
            inputs[index++] = CreateKeyDown(modifierKeyCodes[i]);

        // Key down events for the main keys (in order)
        for (var i = 0; i < keyCodes.Length; i++)
            inputs[index++] = CreateKeyDown(keyCodes[i]);

        // Key up events for the main keys (in reverse order)
        for (var i = keyCodes.Length - 1; i >= 0; i--)
            inputs[index++] = CreateKeyUp(keyCodes[i]);

        // Modifier key up events (in reverse order)
        for (var i = modifierKeyCodes.Length - 1; i >= 0; i--)
            inputs[index++] = CreateKeyUp(modifierKeyCodes[i]);

        SendInputs(inputs);
    }

    public static void SendKeyDown(VirtualKey keyCode)
    {
        SendInputs([CreateKeyDown(keyCode)]);
    }
    public static void SendKeyDown(params VirtualKey[] keyCodes)
    {
        var inputs = new INPUT[keyCodes.Length];
        for (var i = 0; i < keyCodes.Length; i++)
            inputs[i] = CreateKeyDown(keyCodes[i]);

        SendInputs(inputs);
    }
    public static void SendKeyUp(VirtualKey keyCode)
    {
        SendInputs([CreateKeyUp(keyCode)]);
    }
    public static void SendKeyUp(params VirtualKey[] keyCodes)
    {
        var inputs = new INPUT[keyCodes.Length];
        for (var i = 0; i < keyCodes.Length; i++)
            inputs[i] = CreateKeyUp(keyCodes[i]);

        SendInputs(inputs);
    }
    public static void SendKeyPress(VirtualKey keyCode)
    {
        SendInputs([CreateKeyDown(keyCode), CreateKeyUp(keyCode)]);
    }
    public static void SendKeyPress(params VirtualKey[] keyCodes)
    {
        var inputs = new INPUT[keyCodes.Length * 2];

        for (var i = 0; i < keyCodes.Length; i++)
        {
            inputs[i * 2] = CreateKeyDown(keyCodes[i]);
            inputs[i * 2 + 1] = CreateKeyUp(keyCodes[i]);
        }

        SendInputs(inputs);
    }

    public static List<INPUT> AddUpEventsForCurrentlyPressedModifiers(this List<INPUT> inputs)
    {
        for (var i = 0; i < ModifierKeys.Length; i++)
        {
            if (IsKeyPressed((int)ModifierKeys[i]))
                inputs.Add(CreateKeyUp(ModifierKeys[i]));
        }

        return inputs;
    }
    public static List<INPUT> AddDownEventsForCurrentlyPressedModifiers(this List<INPUT> inputs)
    {
        for (var i = 0; i < ModifierKeys.Length; i++)
        {
            if (IsKeyPressed((int)ModifierKeys[i]))
                inputs.Add(CreateKeyDown(ModifierKeys[i]));
        }

        return inputs;
    }
    public static List<INPUT> AddKeyPress(this List<INPUT> inputs, params VirtualKey[] keyCode)
    {
        foreach (var key in keyCode)
            AddKeyPress(inputs, key);
        return inputs;
    }
    public static List<INPUT> AddKeyPress(this List<INPUT> inputs, VirtualKey keyCode)
    {
        AddKeyDown(inputs, keyCode);
        AddKeyUp(inputs, keyCode);
        return inputs;
    }
    public static List<INPUT> AddKeyDown(this List<INPUT> inputs, VirtualKey keyCode)
    {
        inputs.Add(CreateKeyDown(keyCode));
        return inputs;
    }
    public static List<INPUT> AddKeyUp(this List<INPUT> inputs, VirtualKey keyCode)
    {
        inputs.Add(CreateKeyUp(keyCode));
        return inputs;
    }

    public static INPUT CreateKeyDown(VirtualKey keyCode)
    {
        var wScan = (ushort)(WinApi.MapVirtualKey((uint)keyCode, 0) & 0xFFU);
        var dwFlags = IsExtendedKey(keyCode)
            ? KeyEventFlags.ExtendedKey
            : KeyEventFlags.KeyDown;

        return CreateKeyInput(keyCode, dwFlags, wScan);
    }
    public static INPUT CreateKeyUp(VirtualKey keyCode)
    {
        var wScan = (ushort)(WinApi.MapVirtualKey((uint)keyCode, 0) & 0xFFU);
        var dwFlags = IsExtendedKey(keyCode)
            ? KeyEventFlags.KeyUp | KeyEventFlags.ExtendedKey
            : KeyEventFlags.KeyUp;

        return CreateKeyInput(keyCode, dwFlags, wScan);
    }
    public static INPUT CreateKeyInput(VirtualKey keyCode, KeyEventFlags flags, ushort wScan = 0, uint dwExtraInfo = 0, uint time = 0)
    {
        return new INPUT
        {
            Type = InputType.Keyboard,
            Data = new InputUnion
            {
                Keyboard = new KEYBDINPUT
                {
                    wVk = keyCode,
                    wScan = wScan,
                    dwFlags = flags,
                    dwExtraInfo = dwExtraInfo,
                    time = time
                }
            }
        };
    }

    /// <summary>
    /// Sends the specified array of inputs to the current foreground window.
    /// </summary>
    /// <param name="inputs">The array of inputs to send.</param>
    /// <param name="throwOnError">If true, throws a Win32Exception if the function fails.</param>
    /// <exception cref="Win32Exception">Thrown if the input events cannot be sent successfully.</exception>
    public static void SendInputs(INPUT[] inputs, bool throwOnError = false)
    {
        if (inputs.Length == 0) return;

        var result = WinApi.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        if (throwOnError && result != inputs.Length)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}