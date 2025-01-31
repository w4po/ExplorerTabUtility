using System.Runtime.InteropServices;

namespace ExplorerTabUtility.Interop;

[ComImport]
[Guid("000214E6-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IShellFolder
{
    /// <summary>
    /// Translates the display name of a file object or a folder into an item identifier list.
    /// </summary>
    /// <param name="hwnd">A window handle. The client should provide a window handle if it displays a dialog or message box. Otherwise set hwnd to NULL.</param>
    /// <param name="pbc">Optional. A pointer to a bind context used to pass parameters as inputs and outputs to the parsing function.</param>
    /// <param name="pszDisplayName">A null-terminated Unicode string with the display name.</param>
    /// <param name="pchEaten">A pointer to a ULONG value that receives the number of characters of the display name that was parsed. If your application does not need this information, set pchEaten to NULL, and no value will be returned.</param>
    /// <param name="ppidl">When this method returns, contains a pointer to the PIDL for the object.</param>
    /// <param name="pdwAttributes">The value used to query for file attributes. If not used, it should be set to NULL.</param>
    /// <returns>If this method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
    [PreserveSig]
    int ParseDisplayName(nint hwnd, nint pbc, string pszDisplayName, out uint pchEaten, out nint ppidl, ref uint pdwAttributes);

    // Skip methods 1-3 (3 methods)
    void _VtblGap1_3();  // Covers EnumObjects, BindToObject, BindToStorage

    /// <summary>
    /// Determines the relative order of two file objects or folders, given 
    /// their item identifier lists. Return value: If this method is 
    /// successful, the CODE field of the HRESULT contains one of the 
    /// following values (the code can be retrived using the helper function
    /// GetHResultCode): Negative A negative return value indicates that the first item should precede the second (pidl1 &lt; pidl2). 
    ///Positive A positive return value indicates that the first item should
    ///follow the second (pidl1 > pidl2).  Zero A return value of zero
    ///indicates that the two items are the same (pidl1 = pidl2). 
    /// </summary>
    /// <param name="lParam">Value that specifies how the comparison  should be performed. The lower Sixteen bits of lParam define the sorting  rule. 
    ///  The upper sixteen bits of lParam are used for flags that modify the sorting rule. values can be from  the SHCIDS enum
    /// </param>
    /// <param name="pidl1">Pointer to the first item's ITEMIDLIST structure.</param>
    /// <param name="pidl2"> Pointer to the second item's ITEMIDLIST structure.</param>
    /// <returns></returns>
    /// <returns>If this method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
    [PreserveSig]
    int CompareIDs(nint lParam, nint pidl1, nint pidl2);
}