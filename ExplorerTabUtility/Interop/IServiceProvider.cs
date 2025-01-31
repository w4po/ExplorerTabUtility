using System;
using System.Runtime.InteropServices;

namespace ExplorerTabUtility.Interop;

[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
[ComImport]
public interface IServiceProvider
{
    [PreserveSig]
    int QueryService(ref Guid guidService, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellBrowser? ppvObject);
}