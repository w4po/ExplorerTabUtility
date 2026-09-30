using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Languages.Manager;
using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.Managers
{
    internal static class BookmarkManager
    {
        /// <summary>
        /// 书签集合
        /// </summary>
        public static IReadOnlyList<FolderInfo> Bookmarks { get; }

        /// <summary>
        /// 书签
        /// </summary>
        public static FolderInfo Folder => folderInfo;

        /// <summary>
        /// 其他书签
        /// </summary>
        public static FolderInfo OtherFolder => otherFolderInfo;

        /// <summary>
        /// 溢出书签
        /// </summary>
        public static FolderInfo OverflowFolder => overflowFolderInfo;

        /// <summary>
        /// 上次保存路径
        /// </summary>
        public static IReadOnlyList<SaveFolderInfo> LastSaveFolders => lastSaveFolders.ToList(true).AsReadOnly();

        /// <summary>
        /// 最后保存路径Id
        /// </summary>
        public static Guid LastSaveFolderId { get; private set; }

        /// <summary>
        /// 剪切板
        /// </summary>
        public static Clipboard ClipboardManager { get; } = new Clipboard();

        private static readonly FolderInfo bookmarks;
        private static readonly FolderInfo folderInfo;
        private static readonly FolderInfo otherFolderInfo;
        private static readonly FolderInfo overflowFolderInfo;
        private static readonly LRUCache<Guid, SaveFolderInfo> lastSaveFolders;
        private static readonly SaveFolderInfo saveFolderInfo;
        private static readonly SaveFolderInfo otherSaveFolderInfo;
        private static readonly BookmarkConfig config;
        private static readonly string configFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Constants.AppName,
            Constants.BookmarksFileName);

        static BookmarkManager()
        {
            folderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000001"), LangeuageHelper.Instance.LanguageFields.BookmarkBar);
            otherFolderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000002"), LangeuageHelper.Instance.LanguageFields.OtherBookmark);
            overflowFolderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000003"), ">>");
            bookmarks = new FolderInfo(Guid.Empty, string.Empty, folderInfo, otherFolderInfo);
            Bookmarks = new ReadOnlyCollection<FolderInfo>([folderInfo, otherFolderInfo]);
            saveFolderInfo = new SaveFolderInfo(folderInfo);
            otherSaveFolderInfo = new SaveFolderInfo(otherFolderInfo);
            lastSaveFolders = new LRUCache<Guid, SaveFolderInfo>(5, saveFolderInfo, otherSaveFolderInfo);
            config = InitBookmarkConfig();

            LoadBookmark();
            LangeuageHelper.Instance.OnLangeuageChanged += OnLangeuageChanged;
        }

        private static void OnLangeuageChanged()
        {
            folderInfo.Name = saveFolderInfo.Name = LangeuageHelper.Instance.LanguageFields.BookmarkBar;
            otherFolderInfo.Name = otherSaveFolderInfo.Name = LangeuageHelper.Instance.LanguageFields.OtherBookmark;
        }

        private static BookmarkConfig InitBookmarkConfig()
        {
            var directory = Path.GetDirectoryName(configFilePath);
            Directory.CreateDirectory(directory!);

            if (!File.Exists(configFilePath))
            {
                return new BookmarkConfig();
            }

            try
            {
                var json = File.ReadAllText(configFilePath);
                return JsonSerializer.Deserialize<BookmarkConfig>(json) ?? new BookmarkConfig();
            }
            catch (Exception)
            {
                return new BookmarkConfig();
            }
        }

        /// <summary>
        /// 判断是否书签栏或其他书签
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool IsMainOrOtherFolder(FolderInfo info)
        {
            return info.Id == folderInfo.Id || info.Id == otherFolderInfo.Id;
        }

        /// <summary>
        /// 判断是否其他书签或溢出书签
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool IsOtherOrOverflowFolder(FolderInfo info)
        {
            return info.Id == otherFolderInfo.Id || info.Id == overflowFolderInfo.Id;
        }

        /// <summary>
        /// 恢复配置
        /// </summary>
        public static void RecoverConfig()
        {
            lastSaveFolders.Clear();
            folderInfo.Items.Clear();
            otherFolderInfo.Items.Clear();

            LoadBookmark();
        }

        private static void LoadBookmark()
        {
            var folder = config.Bookmarks.FirstOrDefault(t => t.Id == folderInfo.Id);
            if (folder?.Items.Count > 0)
            {
                folderInfo.AddRange(folder.Items);
            }

            folder = config.Bookmarks.FirstOrDefault(t => t.Id == otherFolderInfo.Id);
            if (folder?.Items.Count > 0)
            {
                otherFolderInfo.AddRange(folder.Items);
            }

            for (int i = config.LastSaveFolders.Count - 1; i >= 0; i--)
            {
                var temp = config.LastSaveFolders[i];
                lastSaveFolders.Add(temp.Id, temp);
            }

            LastSaveFolderId = config.LastSaveFolderId;
        }

        /// <summary>
        /// 保存书签
        /// </summary>
        /// <param name="isEdit">是否是编辑</param>
        /// <param name="lastParentId">上次的父节点id</param>
        /// <param name="currentParentId">这次的父节点id</param>
        /// <param name="bookmark">书签</param>
        /// <param name="newName">新书签名称</param>
        /// <param name="newLocation">新书签路径</param>
        /// <returns></returns>
        public static bool Save(bool isEdit, Guid lastParentId, Guid currentParentId, BookmarkInfo bookmark, string newName, string newLocation)
        {
            if (GetTargetFolderInfoFault(currentParentId, out var currentParentFolder)) return false;

            bookmark.Name = newName;
            bookmark.Location = newLocation;
            if (isEdit)
            {
                if (lastParentId != currentParentId)
                {
                    if (GetTargetFolderInfoFault(lastParentId, out var lastParentFolder) == false)
                    {
                        lastParentFolder.Remove(bookmark.Id);
                    }
                    currentParentFolder.Add(bookmark);
                }
            }
            else
            {
                bookmark.Id = Guid.NewGuid();
                currentParentFolder.Add(bookmark);
            }

            UpdateLastSaveFolders(currentParentFolder);
            SaveConfig();
            return true;
        }

        /// <summary>
        /// 保存文件夹，返回是否成功
        /// </summary>
        /// <param name="parentId">父节点id</param>
        /// <param name="saveFolder">要保存的文件夹</param>
        /// <param name="newName">新文件夹名</param>
        /// <param name="saveConfig">是否保存配置</param>
        /// <returns></returns>
        public static bool Save(Guid parentId, FolderInfo saveFolder, string newName, bool saveConfig)
        {
            if (saveFolder.Id == Guid.Empty)
            {
                if (GetTargetFolderInfoFault(parentId, out var parentFolder))
                {
                    return false;
                }

                saveFolder.Id = Guid.NewGuid();
                saveFolder.Name = newName;
                parentFolder.Add(saveFolder);
            }
            else
            {
                saveFolder.Name = newName;
                UpdateLastSaveFolderName(saveFolder);
            }

            if (saveConfig)
            {
                SaveConfig();
            }

            return true;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="folders"></param>
        public static void Save(List<FolderInfo> folders)
        {
            foreach (var item in folders)
            {
                if (item.Id == folderInfo.Id)
                {
                    folderInfo.Items = item.Items;
                }
                else if (item.Id == otherFolderInfo.Id)
                {
                    otherFolderInfo.Items = item.Items;
                }
            }

            SaveConfig();
        }

        /// <summary>
        /// 删除书签
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="current"></param>
        public static void Delete(FolderInfo parent, BookmarkInfo current)
        {
            parent.Remove(current.Id);
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="current"></param>
        public static void Delete(FolderInfo parent, FolderInfo current)
        {
            if (parent == overflowFolderInfo)
            {
                folderInfo.Remove(current.Id);
            }
            else
            {
                parent.Remove(current.Id);
            }

            var deleteIds = current.GetFolderIds().ToList();
            lastSaveFolders.Delete(deleteIds.Contains);
        }

        /// <summary>
        /// 上次保存路径删除文件夹
        /// </summary>
        /// <param name="deleteFolderIds">要删除的文件夹Id</param>
        public static void LastSaveFoldersDelete(List<Guid> deleteFolderIds)
        {
            lastSaveFolders.Delete(deleteFolderIds.Contains);
        }

        /// <summary>
        /// 查找目标目录是否失败
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static bool GetTargetFolderInfoFault(Guid folderId, out FolderInfo folder)
        {
            return bookmarks.Search(folderId, out folder) == false;
        }

        private static void UpdateLastSaveFolderName(FolderInfo folder)
        {
            var temp = lastSaveFolders.Get(folder.Id);
            if (temp != null)
            {
                temp.Name = folder.Name;
            }
        }

        private static void UpdateLastSaveFolders(FolderInfo folder)
        {
            LastSaveFolderId = folder.Id;

            if (folder.Id == folderInfo.Id || folder.Id == otherFolderInfo.Id) return;

            var info = lastSaveFolders.Get(folder.Id) ?? new SaveFolderInfo(folder);
            lastSaveFolders.Add(folder.Id, info);
        }

        public static string Export()
        {
            return JsonSerializer.Serialize(bookmarks);
        }

        public static bool Import(string json)
        {
            FolderInfo? data;
            try
            {
                data = JsonSerializer.Deserialize<FolderInfo>(json);
                if (data == null) return false;
            }
            catch (Exception)
            {
                return false;
            }

            UpdateId(data);
            data.Name = LangeuageHelper.Instance.LanguageFields.ImportBookmark;
            folderInfo.Items.Add(data);

            return true;
        }

        private static void UpdateId(FolderInfo data)
        {
            data.Id = Guid.NewGuid();
            foreach (var item in data.Items)
            {
                if (item is FolderInfo folder)
                {
                    UpdateId(folder);
                }
                else
                {
                    item.Id = Guid.NewGuid();
                }
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public static void SaveConfig()
        {
            try
            {
                config.Bookmarks = [.. Bookmarks];
                config.LastSaveFolders = lastSaveFolders.ToList(false);
                config.LastSaveFolderId = LastSaveFolderId;

                var json = JsonSerializer.Serialize(config);
                File.WriteAllText(configFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save bookmarks: {ex.Message}");
            }
        }

        internal class Clipboard : BindableBase
        {
            private bool treeViewCanPaste;
            /// <summary>
            /// TreeView是否可以粘贴
            /// </summary>
            public bool TreeViewCanPaste
            {
                get { return treeViewCanPaste; }
                set { SetProperty(ref treeViewCanPaste, value); }
            }

            private bool listBoxCanPaste;
            /// <summary>
            /// ListBox是否可以粘贴
            /// </summary>
            public bool ListBoxCanPaste
            {
                get { return listBoxCanPaste; }
                private set { SetProperty(ref listBoxCanPaste, value); }
            }

            private bool isSearch;
            /// <summary>
            /// 是否搜索
            /// </summary>
            public bool IsSearch
            {
                get { return isSearch; }
                set { SetProperty(ref isSearch, value); }
            }

            private string? manageHotKey;
            /// <summary>
            /// 书签管理热键
            /// </summary>
            public string? ManageHotKey
            {
                get { return manageHotKey; }
                set { SetProperty(ref manageHotKey, value); }
            }

            private bool isCopy;
            private List<BookmarkTreeViewInfo> infos = new List<BookmarkTreeViewInfo>();

            /// <summary>
            /// 剪切
            /// </summary>
            /// <param name="infos"></param>
            /// <param name="isNotSearch"></param>
            public void Cut(List<BookmarkTreeViewInfo> infos, bool isNotSearch)
            {
                this.infos = infos;
                ListBoxCanPaste = isNotSearch;
                IsSearch = isNotSearch == false;
                TreeViewCanPaste = true;
                isCopy = false;
            }

            /// <summary>
            /// 复制
            /// </summary>
            /// <param name="infos"></param>
            /// <param name="isNotSearch"></param>
            public void Copy(List<BookmarkTreeViewInfo> infos, bool isNotSearch)
            {
                this.infos = infos;
                ListBoxCanPaste = isNotSearch;
                IsSearch = isNotSearch == false;
                TreeViewCanPaste = true;
                isCopy = true;
            }

            /// <summary>
            /// 设置ListBox是否可以粘贴
            /// </summary>
            /// <param name="isNotSearch"></param>
            public void SetListBoxCanPaste(bool isNotSearch)
            {
                IsSearch = isNotSearch == false;
                ListBoxCanPaste = infos.Count > 0 && isNotSearch;
            }

            /// <summary>
            /// 获取剪切板信息
            /// </summary>
            /// <param name="parentLevel"></param>
            /// <returns></returns>
            public List<BookmarkTreeViewInfo> GetInfos(int parentLevel)
            {
                if (isCopy)
                {
                    var result = new List<BookmarkTreeViewInfo>(infos.Count);
                    foreach (var item in infos)
                    {
                        result.Add(item.Copy(Guid.NewGuid, parentLevel));
                    }
                    return result;
                }
                else
                {
                    isCopy = true;
                    var level = parentLevel + 1;
                    foreach (var item in infos)
                    {
                        SetLevel(item, level);
                    }
                    return infos;
                }
            }

            private void SetLevel(BookmarkTreeViewInfo info, int level)
            {
                info.Level = level;
                level++;
                foreach (var item in info.Children)
                {
                    SetLevel(item, level);
                }
            }
        }

        internal class BookmarkConfig
        {
            public List<FolderInfo> Bookmarks { get; set; } = [];
            public List<SaveFolderInfo> LastSaveFolders { get; set; } = [];
            public Guid LastSaveFolderId { get; set; }
        }

        internal class LRUCache<TKey, TValue>(int capacity, params TValue[] array) where TKey : notnull
        {
            private readonly Dictionary<TKey, LinkedListNode<TValue>> dict = new(capacity);
            private readonly LinkedList<TValue> linkedList = new();
            private readonly List<TValue> list = new(capacity + array.Length);
            private readonly TValue[] array = array;
            private readonly int capacity = capacity;

            public TValue? Get(TKey key)
            {
                return dict.TryGetValue(key, out var node) ? node.Value : default;
            }

            public void Add(TKey key, TValue value, bool update = false)
            {
                if (dict.TryGetValue(key, out var item))
                {
                    if (update) item.Value = value;
                    linkedList.Remove(item);
                    linkedList.AddFirst(item);
                }
                else
                {
                    if (linkedList.Count == capacity)
                    {
                        linkedList.RemoveLast();
                    }

                    item = new LinkedListNode<TValue>(value);
                    dict[key] = item;
                    linkedList.AddFirst(item);
                }
            }

            public void Delete(Func<TKey, bool> predicate)
            {
                var keys = dict.Keys.Where(predicate).ToList();
                foreach (var key in keys)
                {
                    if (dict.TryGetValue(key, out var item))
                    {
                        dict.Remove(key);
                        linkedList.Remove(item);
                    }
                }
            }

            public void Clear()
            {
                linkedList.Clear();
                dict.Clear();
            }

            public List<TValue> ToList(bool withArray)
            {
                list.Clear();
                list.AddRange(linkedList);
                if (withArray) list.AddRange(array);

                return list;
            }
        }
    }
}
