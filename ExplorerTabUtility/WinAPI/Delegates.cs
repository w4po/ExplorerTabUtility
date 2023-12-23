// ReSharper disable IdentifierTypo

namespace ExplorerTabUtility.WinAPI;

public delegate bool EnumWindowsProc(nint hWnd, nint lParam);
public delegate nint HookProc(int nCode, nint wParam, nint lParam);
public delegate void WinEventDelegate(nint hWinEventHook, uint eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);