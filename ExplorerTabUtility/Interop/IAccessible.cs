using System.Runtime.InteropServices;

namespace ExplorerTabUtility.Interop;

[ComImport]
[TypeLibType(TypeLibTypeFlags.FHidden | TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
[Guid("618736E0-3C3D-11CF-810C-00AA00389B71")]
public interface IAccessible
{
    [DispId(-5000)]
    [TypeLibFunc(64)]
    [return: MarshalAs(UnmanagedType.IDispatch)]
    object get_accParent();

    [DispId(-5001)]
    [TypeLibFunc(64)]
    int get_accChildCount();

    [TypeLibFunc(64)]
    [DispId(-5002)]
    [return: MarshalAs(UnmanagedType.IDispatch)]
    object get_accChild([In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [DispId(-5003)]
    [TypeLibFunc(64)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string get_accName([Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [TypeLibFunc(64)]
    [DispId(-5004)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string get_accValue([Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [DispId(-5005)]
    [TypeLibFunc(64)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string get_accDescription([Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [DispId(-5006)]
    [TypeLibFunc(64)]
    [return: MarshalAs(UnmanagedType.Struct)]
    object get_accRole([Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [TypeLibFunc(64)]
    [DispId(-5007)]
    [return: MarshalAs(UnmanagedType.Struct)]
    object get_accState([Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [TypeLibFunc(64)]
    [DispId(-5008)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string get_accHelp([Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [DispId(-5009)]
    [TypeLibFunc(64)]
    int get_accHelpTopic([MarshalAs(UnmanagedType.BStr)] out string pszHelpFile,
        [Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [DispId(-5010)]
    [TypeLibFunc(64)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string get_accKeyboardShortcut([Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [DispId(-5011)]
    [TypeLibFunc(64)]
    [return: MarshalAs(UnmanagedType.Struct)]
    object get_accFocus();

    [DispId(-5012)]
    [TypeLibFunc(64)]
    [return: MarshalAs(UnmanagedType.Struct)]
    object get_accSelection();

    [TypeLibFunc(64)]
    [DispId(-5013)]
    [return: MarshalAs(UnmanagedType.BStr)]
    string get_accDefaultAction([Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [DispId(-5014)]
    [TypeLibFunc(64)]
    void accSelect([In] int flagsSelect, [Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [DispId(-5015)]
    [TypeLibFunc(64)]
    void accLocation(out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight,
        [Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [TypeLibFunc(64)]
    [DispId(-5016)]
    [return: MarshalAs(UnmanagedType.Struct)]
    object accNavigate([In] int navDir, [Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varStart);

    [TypeLibFunc(64)]
    [DispId(-5017)]
    [return: MarshalAs(UnmanagedType.Struct)]
    object accHitTest([In] int xLeft, [In] int yTop);

    [TypeLibFunc(64)]
    [DispId(-5018)]
    void accDoDefaultAction([Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild);

    [TypeLibFunc(64)]
    [DispId(-5003)]
    void set_accName([Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild,
        [In] [MarshalAs(UnmanagedType.BStr)] string pszName);

    [TypeLibFunc(64)]
    [DispId(-5004)]
    void set_accValue([Optional] [In] [MarshalAs(UnmanagedType.Struct)] object varChild,
        [In] [MarshalAs(UnmanagedType.BStr)] string pszValue);
}