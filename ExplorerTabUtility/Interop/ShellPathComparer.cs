using System;
using System.Runtime.InteropServices;
using ExplorerTabUtility.WinAPI;

namespace ExplorerTabUtility.Interop;

public sealed class ShellPathComparer : IDisposable
{
    private IShellFolder _desktopFolder;
    private bool _disposed;

    public ShellPathComparer()
    {
        Marshal.ThrowExceptionForHR(WinApi.SHGetDesktopFolder(out var pDesktopFolder));

        try
        {
            _desktopFolder = (IShellFolder)Marshal.GetObjectForIUnknown(pDesktopFolder);
        }
        finally
        {
            if (pDesktopFolder != 0)
                Marshal.Release(pDesktopFolder);
        }
    }

    /// <summary>
    /// Converts a path to a PIDL (Pointer to an Item ID List).
    /// </summary>
    /// <param name="path">The file system path to convert.</param>
    /// <returns>A PIDL representing the file system path. It is the caller's responsibility to release it.</returns>
    public nint GetPidlFromPath(string path)
    {
        uint pdwAttributes = 0;
        _desktopFolder.ParseDisplayName(0, 0, path, out _, out var pidl, ref pdwAttributes);

        return pidl;
    }
    public static string? GetPathFromPidl(nint pidl, uint sigdnName = WinApi.SIGDN_URL)
    {
        var hr = WinApi.SHGetNameFromIDList(pidl, sigdnName, out var path);
        return hr == 0 ? path : null;
    }
    public bool CompareIds(nint pidl1, nint pidl2)
    {
        return _desktopFolder.CompareIDs(0, pidl1, pidl2) == 0;
    }
    public bool CompareWithShGetName(nint pidl1, nint pidl2)
    {
        var fsPath1 = GetPathFromPidl(pidl1);
        if (fsPath1 == null) return false;

        var fsPath2 = GetPathFromPidl(pidl2);

        return StringComparer.OrdinalIgnoreCase.Equals(fsPath1, fsPath2);
    }
    public bool IsEquivalent(nint pidl1, nint pidl2)
    {
        try
        {
            return CompareIds(pidl1, pidl2) || CompareWithShGetName(pidl1, pidl2);
        }
        catch
        {
            return false;
        }
    }
    public bool IsEquivalent(nint pidl1, string path2)
    {
        nint pidl2 = 0;
        try
        {
            pidl2 = GetPidlFromPath(path2);

            return IsEquivalent(pidl1, pidl2);
        }
        catch
        {
            return false;
        }
        finally
        {
            if (pidl2 != 0)
                Marshal.FreeCoTaskMem(pidl2);
        }
    }
    public bool IsEquivalent(string path1, string path2)
    {
        if (StringComparer.OrdinalIgnoreCase.Equals(path1, path2))
            return true;

        nint pidl1 = 0;
        nint pidl2 = 0;
        try
        {
            pidl1 = GetPidlFromPath(path1);
            pidl2 = GetPidlFromPath(path2);

            return IsEquivalent(pidl1, pidl2);
        }
        catch
        {
            return false;
        }
        finally
        {
            if (pidl1 != 0)
                Marshal.FreeCoTaskMem(pidl1);

            if (pidl2 != 0)
                Marshal.FreeCoTaskMem(pidl2);
        }
    }
    public bool IsEquivalent(string targetPath, string comparePath, nint targetPidl)
    {
        if (StringComparer.OrdinalIgnoreCase.Equals(targetPath, comparePath))
            return true;

        nint comparePidl = 0;
        try
        {
            comparePidl = GetPidlFromPath(comparePath);
            return comparePidl != 0 && IsEquivalent(targetPidl, comparePidl);
        }
        catch
        {
            return false;
        }
        finally
        {
            if (comparePidl != 0)
                Marshal.FreeCoTaskMem(comparePidl);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        // release the RCW
        Marshal.ReleaseComObject(_desktopFolder);
        _desktopFolder = null!;

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~ShellPathComparer()
    {
        Dispose();
    }
}