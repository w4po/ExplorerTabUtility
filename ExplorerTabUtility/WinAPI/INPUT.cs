// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

using System;
using System.Runtime.InteropServices;

namespace ExplorerTabUtility.WinAPI;

[StructLayout(LayoutKind.Sequential)]
public struct INPUT
{
    public InputType Type;
    public InputUnion Data;
}
public enum InputType : uint
{
    Mouse = 0,
    Keyboard = 1,
    Hardware = 2
}

[StructLayout(LayoutKind.Explicit)]
public struct InputUnion
{
    [FieldOffset(0)]
    public MOUSEINPUT Mouse;
    [FieldOffset(0)]
    public KEYBDINPUT Keyboard;
    [FieldOffset(0)]
    public HARDWAREINPUT Hardware;
}

public struct KEYBDINPUT
{
    public VirtualKey wVk;
    public ushort wScan;
    public KeyEventFlags dwFlags;
    public uint time;
    public nuint dwExtraInfo;
}

public struct MOUSEINPUT
{
    public int dx;
    public int dy;
    public uint mouseData;
    public uint dwFlags;
    public uint time;
    public nint dwExtraInfo;
}

public struct HARDWAREINPUT
{
    public uint uMsg;
    public ushort wParamL;
    public ushort wParamH;
}

[Flags]
public enum KeyEventFlags : uint
{
    KeyDown = 0,
    ExtendedKey = 1,
    KeyUp = 2,
    ScanCode = 8,
    Unicode = 4
}