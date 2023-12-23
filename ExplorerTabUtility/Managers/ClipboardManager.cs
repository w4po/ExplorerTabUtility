using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable InconsistentNaming

namespace ExplorerTabUtility.Managers;

public static class ClipboardManager
{
    // P/Invoke declarations
    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(nint hWndNewOwner);
    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();
    [DllImport("user32.dll")]
    private static extern nint GetClipboardData(uint uFormat);
    [DllImport("user32.dll")]
    private static extern nint SetClipboardData(uint uFormat, nint hMem);
    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();
    [DllImport("user32.dll")]
    private static extern uint EnumClipboardFormats(uint format);
    [DllImport("kernel32.dll")]
    private static extern nint GlobalLock(nint hMem);
    [DllImport("kernel32.dll")]
    private static extern uint GlobalSize(nint hMem);
    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(nint hMem);
    [DllImport("kernel32.dll")]
    private static extern nint GlobalAlloc(uint uFlags, nuint dwBytes);
    [DllImport("kernel32.dll")]
    private static extern nint GlobalFree(nint hMem);

    // Flags for GlobalAlloc
    private const uint GMEM_MOVEABLE = 0x0002;
    private const uint GMEM_ZEROINIT = 0x0040;
    private const uint GHND = GMEM_MOVEABLE | GMEM_ZEROINIT;

    // Clipboard formats
    private const uint CF_BITMAP = 2;
    private const uint CF_UNICODETEXT = 13;
    private const uint CF_ENHMETAFILE = 14;

    public static string GetClipboardText()
    {
        try
        {
            if (!OpenClipboard(default))
                return string.Empty;

            var handle = GetClipboardData(CF_UNICODETEXT);
            if (handle == default)
                return string.Empty;

            var lockedData = GlobalLock(handle);
            if (lockedData == default)
                return string.Empty;

            var size = GlobalSize(handle);
            if (size == 0)
            {
                GlobalUnlock(handle);
                return string.Empty;
            }

            var buffer = new byte[size];
            Marshal.Copy(lockedData, buffer, 0, (int)size);
            GlobalUnlock(handle);

            return Encoding.Unicode.GetString(buffer).TrimEnd('\0');
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error getting clipboard text: {e}");
            return string.Empty;
        }
        finally
        {
            CloseClipboard();
        }
    }

    public static void SetClipboardText(string text)
    {
        try
        {
            if (!OpenClipboard(default))
                return;

            EmptyClipboard();

            text = $"{text.TrimEnd('\0')}\0";
            var buffer = Encoding.Unicode.GetBytes(text);
            var size = (uint)buffer.Length;

            var handle = GlobalAlloc(GHND, size);
            if (handle == default)
                return;

            var pointer = GlobalLock(handle);
            if (pointer == default)
            {
                GlobalFree(handle);
                return;
            }

            Marshal.Copy(buffer, 0, pointer, (int)size);
            SetClipboardData(CF_UNICODETEXT, handle);
            GlobalUnlock(handle);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error setting clipboard text: {e}");
        }
        finally
        {
            CloseClipboard();
        }
    }

    // Get the current clipboard data as a dictionary of format and data pairs
    public static Dictionary<uint, byte[]> GetClipboardData()
    {
        var data = new Dictionary<uint, byte[]>();
        try
        {
            if (!OpenClipboard(default))
                return data;

            uint format = 0;
            while ((format = EnumClipboardFormats(format)) != 0)
            {
                if (format is CF_BITMAP or CF_ENHMETAFILE) continue; // Ignore these formats (might cause problems)

                var handle = GetClipboardData(format);
                if (handle == default)
                    continue;

                var lockedData = GlobalLock(handle);
                if (lockedData == default)
                    continue;

                var size = GlobalSize(handle);
                if (size == 0)
                {
                    GlobalUnlock(handle);
                    continue;
                }

                var buffer = new byte[size];
                Marshal.Copy(lockedData, buffer, 0, (int)size);
                data.Add(format, buffer);
                GlobalUnlock(handle);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error getting clipboard data: {e}");
        }
        finally
        {
            CloseClipboard();
        }
        return data;
    }

    // Set the clipboard data from a dictionary of format and data pairs
    public static void SetClipboardData(Dictionary<uint, byte[]> data)
    {
        if (data.Count == 0) return;
        
        try
        {
            if (!OpenClipboard(default))
                return;

            EmptyClipboard();
            foreach (var pair in data)
            {
                var format = pair.Key;
                var buffer = pair.Value;
                if (buffer == default || buffer.Length == 0) continue;

                var handle = GlobalAlloc(GHND, (nuint)buffer.Length);
                if (handle == default) continue;

                var pointer = GlobalLock(handle);
                if (pointer == default)
                {
                    GlobalFree(handle);
                    continue;
                }

                Marshal.Copy(buffer, 0, pointer, buffer.Length);
                SetClipboardData(format, handle);
                GlobalUnlock(handle);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error setting clipboard data: {e}");
        }
        finally
        {
            CloseClipboard();
        }
    }

    // Clear the clipboard
    public static void ClearClipboard()
    {
        try
        {
            if (!OpenClipboard(default)) return;

            EmptyClipboard();
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error clearing clipboard: {e}");
        }
        finally
        {
            CloseClipboard();
        }
    }
}