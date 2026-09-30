namespace ExplorerTabUtility.Models
{
    /// <summary>
    /// 书签操作
    /// </summary>
    public enum BookmarkAction
    {
        /// <summary>
        /// 在当前标签页打开
        /// </summary>
        OpenInCurrentTab,

        /// <summary>
        /// 在新标签页打开
        /// </summary>
        OpenInNewTab,

        /// <summary>
        /// 在新窗口打开
        /// </summary>
        OpenInNewWindow,

        /// <summary>
        /// 重命名
        /// </summary>
        Rename,

        /// <summary>
        /// 编辑
        /// </summary>
        Edit,

        /// <summary>
        /// 删除
        /// </summary>
        Delete,

        /// <summary>
        /// 新建书签
        /// </summary>
        NewBookmark,

        /// <summary>
        /// 新建文件夹
        /// </summary>
        NewFolder,

        /// <summary>
        /// 管理书签
        /// </summary>
        ManageBookmarks,

        /// <summary>
        /// 剪切
        /// </summary>
        Cut,

        /// <summary>
        /// 复制
        /// </summary>
        Copy,

        /// <summary>
        /// 粘贴
        /// </summary>
        Paste,

        /// <summary>
        /// 在文件夹中显示
        /// </summary>
        ShowInFolder,
    }
}
