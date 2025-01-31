using System.Runtime.InteropServices;

namespace ExplorerTabUtility.Interop;

[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("000214E2-0000-0000-C000-000000000046")]
[ComImport]
public interface IShellBrowser
{
    [PreserveSig]
    int GetWindow(out nint handle);
}