// ReSharper disable InconsistentNaming

using System.Runtime.InteropServices;

namespace ExplorerTabUtility.WinAPI;

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}