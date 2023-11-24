// ReSharper disable IdentifierTypo

using System;

namespace ExplorerTabUtility.WinAPI;

public delegate nint EnumWindowsProc(IntPtr hWnd, nint lParam);
public delegate nint HookProc(int nCode, nint wParam, nint lParam);
public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);