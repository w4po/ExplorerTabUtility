using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.Languages.Manager
{
    /// <summary>
    /// 语言字段类
    /// </summary>
    internal class LanguageFields : BindableBase
    {
        class Field
        {
            public string DefaultValue { get; private set; }
            public string? Value { get; set; }

            public Field(string defaultValue)
            {
                DefaultValue = defaultValue;
            }
        }

        private readonly Dictionary<string, Field> dictCurrentLanguageFields = new Dictionary<string, Field>();

        #region 语言字段属性
        #region 语言信息
        /// <summary>
        /// 语言名称
        /// </summary>
        public string LanguageName { get => Get(); set => Set(value); }
        private string languageNameDefaultValue = "English";
        #endregion

        #region 文件筛选
        /// <summary>
        /// Json文件
        /// </summary>
        public string JsonFiles { get => Get(); set => Set(value); }
        private string jsonFilesDefaultValue = "JSON files (*.json)";

        /// <summary>
        /// 所有文件
        /// </summary>
        public string AllFiles { get => Get(); set => Set(value); }
        private string allFilesDefaultValue = "All Files";
        #endregion

        #region 快捷键菜单
        /// <summary>
        /// 快捷键
        /// </summary>
        public string Shortcuts { get => Get(); set => Set(value); }
        private string shortcutsDefaultValue = "Shortcuts";

        /// <summary>
        /// 快捷键提示
        /// </summary>
        public string ShortcutsToolTip { get => Get(); set => Set(value); }
        private string shortcutsToolTipDefaultValue = "Configure keyboard and mouse shortcuts";

        /// <summary>
        /// 打开程序菜单
        /// </summary>
        public string OpenApplicationMenu { get => Get(); set => Set(value); }
        private string openApplicationMenuDefaultValue = "Open application menu";

        /// <summary>
        /// 添加
        /// </summary>
        public string New { get => Get(); set => Set(value); }
        private string newDefaultValue = "NEW";

        /// <summary>
        /// 添加提示
        /// </summary>
        public string NewToolTip { get => Get(); set => Set(value); }
        private string newToolTipDefaultValue = "Create a new profile";

        /// <summary>
        /// 导入
        /// </summary>
        public string Import { get => Get(); set => Set(value); }
        private string importDefaultValue = "IMPORT";

        /// <summary>
        /// 导入提示
        /// </summary>
        public string ImportToolTip { get => Get(); set => Set(value); }
        private string importToolTipDefaultValue = "Import profiles from a file";

        /// <summary>
        /// 导出
        /// </summary>
        public string Export { get => Get(); set => Set(value); }
        private string exportDefaultValue = "EXPORT";

        /// <summary>
        /// 导出提示
        /// </summary>
        public string ExportToolTip { get => Get(); set => Set(value); }
        private string exportToolTipDefaultValue = "Export profiles to a file";

        /// <summary>
        /// 保存
        /// </summary>
        public string Save { get => Get(); set => Set(value); }
        private string saveDefaultValue = "SAVE";

        /// <summary>
        /// 保存提示
        /// </summary>
        public string SaveToolTip { get => Get(); set => Set(value); }
        private string saveToolTipDefaultValue = "Persist all profile changes";

        /// <summary>
        /// 自动保存
        /// </summary>
        public string AutoSave { get => Get(); set => Set(value); }
        private string autoSaveDefaultValue = "Auto save";

        /// <summary>
        /// 自动保存提示
        /// </summary>
        public string AutoSaveToolTip { get => Get(); set => Set(value); }
        private string autoSaveToolTipDefaultValue = "Automatically save profiles when closing the window";
        #endregion

        #region 偏好菜单
        /// <summary>
        /// 偏好
        /// </summary>
        public string Preferences { get => Get(); set => Set(value); }
        private string preferencesDefaultValue = "Preferences";

        /// <summary>
        /// 偏好提示
        /// </summary>
        public string PreferencesToolTip { get => Get(); set => Set(value); }
        private string preferencesToolTipDefaultValue = "Configure application settings";

        /// <summary>
        /// 程序设置
        /// </summary>
        public string ApplicationSettings { get => Get(); set => Set(value); }
        private string applicationSettingsDefaultValue = "Application Settings";

        /// <summary>
        /// 自动更新
        /// </summary>
        public string AutoUpdate { get => Get(); set => Set(value); }
        private string autoUpdateDefaultValue = "Auto update";

        /// <summary>
        /// 自动更新提示
        /// </summary>
        public string AutoUpdateToolTip { get => Get(); set => Set(value); }
        private string autoUpdateToolTipDefaultValue = "Automatically check for updates on startup";

        /// <summary>
        /// 我有主题问题
        /// </summary>
        public string ThemeIssue { get => Get(); set => Set(value); }
        private string themeIssueDefaultValue = "I have theme issues";

        /// <summary>
        /// 我有主题问题提示
        /// </summary>
        public string ThemeIssueToolTip { get => Get(); set => Set(value); }
        private string themeIssueToolTipDefaultValue = "Use alternative window hiding method that preserves your custom File-Explorer theme.&#x0a;Enable this if you have theme-related issues";

        /// <summary>
        /// 保存关闭历史
        /// </summary>
        public string SaveClosedHistory { get => Get(); set => Set(value); }
        private string saveClosedHistoryDefaultValue = "Save closed history";

        /// <summary>
        /// 保存关闭历史提示
        /// </summary>
        public string SaveClosedHistoryToolTip { get => Get(); set => Set(value); }
        private string saveClosedHistoryToolTipDefaultValue = "Save closed windows history so you can reopen them later (ReopenClosed, Tab search)";

        /// <summary>
        /// 恢复窗口
        /// </summary>
        public string RestorePreviousWindows { get => Get(); set => Set(value); }
        private string restorePreviousWindowsDefaultValue = "Restore previous windows";

        /// <summary>
        /// 恢复窗口提示
        /// </summary>
        public string RestorePreviousWindowsToolTip { get => Get(); set => Set(value); }
        private string restorePreviousWindowsToolTipDefaultValue = "Restore previously opened windows after restart or crash";

        /// <summary>
        /// 隐藏托盘图标
        /// </summary>
        public string HideTrayIcon { get => Get(); set => Set(value); }
        private string hideTrayIconDefaultValue = "Hide tray icon";

        /// <summary>
        /// 隐藏托盘图标提示
        /// </summary>
        public string HideTrayIconToolTip { get => Get(); set => Set(value); }
        private string hideTrayIconToolTipDefaultValue = "Hide the system tray icon";

        /// <summary>
        /// 选择语言
        /// </summary>
        public string SelectLanguage { get => Get(); set => Set(value); }
        private string selectLanguageDefaultValue = "Language";

        /// <summary>
        /// 选择语言提示
        /// </summary>
        public string SelectLanguageToolTip { get => Get(); set => Set(value); }
        private string selectLanguageToolTipDefaultValue = "Select Language";
        #endregion

        #region 关于菜单
        /// <summary>
        /// 关于
        /// </summary>
        public string About { get => Get(); set => Set(value); }
        private string aboutDefaultValue = "About";

        /// <summary>
        /// 关于提示
        /// </summary>
        public string AboutToolTip { get => Get(); set => Set(value); }
        private string aboutToolTipDefaultValue = "About the application and support options";

        /// <summary>
        /// 简介
        /// </summary>
        public string Profile { get => Get(); set => Set(value); }
        private string profileDefaultValue = "Enhance your Windows File Explorer experience";

        /// <summary>
        /// 星标
        /// </summary>
        public string Star { get => Get(); set => Set(value); }
        private string starDefaultValue = "Star on GitHub";

        /// <summary>
        /// 支持项目
        /// </summary>
        public string SupportProject { get => Get(); set => Set(value); }
        private string supportProjectDefaultValue = "Support the Project";

        /// <summary>
        /// 支持项目文本
        /// </summary>
        public string SupportProjectText { get => Get(); set => Set(value); }
        private string supportProjectTextDefaultValue = "If you find this utility helpful, consider supporting its development through one of these options:";

        /// <summary>
        /// 赞助
        /// </summary>
        public string GitHubSponsors { get => Get(); set => Set(value); }
        private string gitHubSponsorsDefaultValue = "GitHub Sponsors";

        /// <summary>
        /// 众筹
        /// </summary>
        public string Patreon { get => Get(); set => Set(value); }
        private string patreonDefaultValue = "Patreon";

        /// <summary>
        /// 请我喝杯咖啡
        /// </summary>
        public string Coffee { get => Get(); set => Set(value); }
        private string coffeeDefaultValue = "Buy Me A Coffee";

        /// <summary>
        /// 付款
        /// </summary>
        public string PayPal { get => Get(); set => Set(value); }
        private string payPalDefaultValue = "PayPal";

        /// <summary>
        /// 开发者
        /// </summary>
        public string Developer { get => Get(); set => Set(value); }
        private string developerDefaultValue = "Developer";

        /// <summary>
        /// 支持者
        /// </summary>
        public string Supporters { get => Get(); set => Set(value); }
        private string supportersDefaultValue = "Current Supporters";

        /// <summary>
        /// 支持者文本
        /// </summary>
        public string SupportersText { get => Get(); set => Set(value); }
        private string supportersTextDefaultValue = "Thank you to all the amazing people who support this project!";

        /// <summary>
        /// 成为第一个支持者
        /// </summary>
        public string FirstSupport { get => Get(); set => Set(value); }
        private string firstSupportDefaultValue = "Be the first to support this project!";

        /// <summary>
        /// 支持的意义
        /// </summary>
        public string SupportMeaning { get => Get(); set => Set(value); }
        private string supportMeaningDefaultValue = "Your support helps keep this project alive";
        #endregion

        #region 任务栏菜单
        /// <summary>
        /// 通知图标提示
        /// </summary>
        public string NotifyIconText { get => Get(); set => Set(value); }
        private string notifyIconTextDefaultValue = "Explorer Tab Utility: Force new windows to tabs.";

        /// <summary>
        /// 键盘钩子
        /// </summary>
        public string KeyboardHook { get => Get(); set => Set(value); }
        private string keyboardHookDefaultValue = "Keyboard Hook";

        /// <summary>
        /// 键盘钩子提示
        /// </summary>
        public string KeyboardHookToolTip { get => Get(); set => Set(value); }
        private string keyboardHookToolTipDefaultValue = "Enable or disable keyboard shortcuts";

        /// <summary>
        /// 鼠标钩子
        /// </summary>
        public string MouseHook { get => Get(); set => Set(value); }
        private string mouseHookDefaultValue = "Mouse Hook";

        /// <summary>
        /// 鼠标钩子提示
        /// </summary>
        public string MouseHookToolTip { get => Get(); set => Set(value); }
        private string mouseHookToolTipDefaultValue = "Enable or disable mouse shortcuts";

        /// <summary>
        /// 资源管理器钩子
        /// </summary>
        public string WindowHook { get => Get(); set => Set(value); }
        private string windowHookDefaultValue = "Window Hook";

        /// <summary>
        /// 资源管理器钩子提示
        /// </summary>
        public string WindowHookToolTip { get => Get(); set => Set(value); }
        private string windowHookToolTipDefaultValue = "Toggle automatic redirection of new Explorer windows to tabs";

        /// <summary>
        /// 重用标签页
        /// </summary>
        public string ReuseTabs { get => Get(); set => Set(value); }
        private string reuseTabsDefaultValue = "Reuse Tabs";

        /// <summary>
        /// 重用标签页提示
        /// </summary>
        public string ReuseTabsToolTip { get => Get(); set => Set(value); }
        private string reuseTabsToolTipDefaultValue = "When enabled, navigates to existing tabs instead of opening duplicate tabs";

        /// <summary>
        /// 开机启动
        /// </summary>
        public string AddToStartup { get => Get(); set => Set(value); }
        private string addToStartupDefaultValue = "Add to startup";

        /// <summary>
        /// 开机启动提示
        /// </summary>
        public string AddToStartupToolTip { get => Get(); set => Set(value); }
        private string addToStartupToolTipDefaultValue = "Automatically start Explorer Tab Utility when Windows starts";

        /// <summary>
        /// 检查更新
        /// </summary>
        public string CheckForUpdates { get => Get(); set => Set(value); }
        private string checkForUpdatesDefaultValue = "Check for updates";

        /// <summary>
        /// 检查更新提示
        /// </summary>
        public string CheckForUpdatesToolTip { get => Get(); set => Set(value); }
        private string checkForUpdatesToolTipDefaultValue = "Check if a newer version of Explorer Tab Utility is available";

        /// <summary>
        /// 设置
        /// </summary>
        public string Settings { get => Get(); set => Set(value); }
        private string settingsDefaultValue = "Settings";

        /// <summary>
        /// 设置提示
        /// </summary>
        public string SettingsToolTip { get => Get(); set => Set(value); }
        private string settingsToolTipDefaultValue = "Open the settings window to configure the application";

        /// <summary>
        /// 退出
        /// </summary>
        public string Exit { get => Get(); set => Set(value); }
        private string exitDefaultValue = "Exit";

        /// <summary>
        /// 退出提示
        /// </summary>
        public string ExitToolTip { get => Get(); set => Set(value); }
        private string exitToolTipDefaultValue = "Close Explorer Tab Utility and stop all hooks";
        #endregion

        #region 快捷键配置控件
        /// <summary>
        /// 启用提示
        /// </summary>
        public string EnabledToolTip { get => Get(); set => Set(value); }
        private string enabledToolTipDefaultValue = "Enable or disable this profile";

        /// <summary>
        /// 配置名称
        /// </summary>
        public string Name { get => Get(); set => Set(value); }
        private string nameDefaultValue = "Name";

        /// <summary>
        /// 配置名称提示
        /// </summary>
        public string NameToolTip { get => Get(); set => Set(value); }
        private string nameToolTipDefaultValue = "Name of the profile";

        /// <summary>
        /// 快捷键
        /// </summary>
        public string Hotkeys { get => Get(); set => Set(value); }
        private string hotkeysDefaultValue = "Hotkeys";

        /// <summary>
        /// 快捷键提示
        /// </summary>
        public string HotkeysToolTip { get => Get(); set => Set(value); }
        private string hotkeysToolTipDefaultValue = "Keyboard or mouse keys to listen for. Click to record a new key combination.";

        /// <summary>
        /// 快捷键作用范围提示
        /// </summary>
        public string ScopeToolTip { get => Get(); set => Set(value); }
        private string scopeToolTipDefaultValue = "Scope of the hotkeys: Global (anywhere) or only when File Explorer is focused";

        /// <summary>
        /// 快捷键操作提示
        /// </summary>
        public string ActionToolTip { get => Get(); set => Set(value); }
        private string actionToolTipDefaultValue = "Action to perform when the hotkeys are pressed";

        /// <summary>
        /// 显示更多配置提示
        /// </summary>
        public string ExpanderToggleToolTip { get => Get(); set => Set(value); }
        private string expanderToggleToolTipDefaultValue = "Show more options";

        /// <summary>
        /// 删除配置提示
        /// </summary>
        public string DeleteToolTip { get => Get(); set => Set(value); }
        private string deleteToolTipDefaultValue = "Delete this profile";

        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get => Get(); set => Set(value); }
        private string pathDefaultValue = "Location";

        /// <summary>
        /// 路径提示
        /// </summary>
        public string PathToolTip { get => Get(); set => Set(value); }
        private string pathToolTipDefaultValue = "Path to open when the hotkeys are pressed. (folder, file, website, CLSID)";

        /// <summary>
        /// 延迟
        /// </summary>
        public string Delay { get => Get(); set => Set(value); }
        private string delayDefaultValue = "Delay";

        /// <summary>
        /// 延迟提示
        /// </summary>
        public string DelayToolTip { get => Get(); set => Set(value); }
        private string delayToolTipDefaultValue = "Delay in milliseconds before performing the action";

        /// <summary>
        /// 已处理
        /// </summary>
        public string Handled { get => Get(); set => Set(value); }
        private string handledDefaultValue = "Handled";

        /// <summary>
        /// 已处理提示
        /// </summary>
        public string HandledToolTip { get => Get(); set => Set(value); }
        private string handledToolTipDefaultValue = "Prevent further processing of the hotkeys in other applications";

        /// <summary>
        /// 标签页
        /// </summary>
        public string OpenAsTab { get => Get(); set => Set(value); }
        private string openAsTabDefaultValue = "Tab";

        /// <summary>
        /// 标签页提示
        /// </summary>
        public string OpenAsTabToolTip { get => Get(); set => Set(value); }
        private string openAsTabToolTipDefaultValue = "Open as tab instead of a new window";
        #endregion

        #region 自定义消息弹窗控件
        /// <summary>
        /// 确定
        /// </summary>
        public string Ok { get => Get(); set => Set(value); }
        private string okDefaultValue = "OK";

        /// <summary>
        /// 取消
        /// </summary>
        public string Cancel { get => Get(); set => Set(value); }
        private string cancelDefaultValue = "Cancel";

        /// <summary>
        /// 是
        /// </summary>
        public string Yes { get => Get(); set => Set(value); }
        private string yesDefaultValue = "Yes";

        /// <summary>
        /// 否
        /// </summary>
        public string No { get => Get(); set => Set(value); }
        private string noDefaultValue = "No";
        #endregion

        #region 标签页搜索控件
        /// <summary>
        /// 清除提示
        /// </summary>
        public string ClearToolTip { get => Get(); set => Set(value); }
        private string clearToolTipDefaultValue = "Clear closed windows history";
        #endregion

        #region 提示消息
        /// <summary>
        /// 程序已运行时的提示消息
        /// </summary>
        public string AlreadyRunning { get => Get(); set => Set(value); }
        private string alreadyRunningDefaultValue = $"Another instance is already running.{Environment.NewLine}Check in System Tray Icons.";

        /// <summary>
        /// 恢复窗口时的提示消息
        /// </summary>
        public string RestorePreviouslyOpenedWindows { get => Get(); set => Set(value); }
        private string restorePreviouslyOpenedWindowsDefaultValue = "Do you want to restore previously opened windows?";

        /// <summary>
        /// 已设置快捷键时隐藏托盘图标的提示消息
        /// </summary>
        public string HideTrayIconWithHotkeys { get => Get(); set => Set(value); }
        private string hideTrayIconWithHotkeysDefaultValue = "You can show the app again by pressing {0}";

        /// <summary>
        /// 未设置快捷键时隐藏托盘图标的提示消息
        /// </summary>
        public string HideTrayIconWithOutHotkeys { get => Get(); set => Set(value); }
        private string hideTrayIconWithOutHotkeysDefaultValue = "Cannot hide tray icon if no hotkey is configured to toggle visibility.";

        /// <summary>
        /// 清除历史记录的提示消息
        /// </summary>
        public string ConfirmClearHistory { get => Get(); set => Set(value); }
        private string confirmClearHistoryDefaultValue = "Are you sure you want to clear the closed windows history?";

        /// <summary>
        /// 清除历史记录的消息标题
        /// </summary>
        public string ConfirmClearHistoryTitle { get => Get(); set => Set(value); }
        private string confirmClearHistoryTitleDefaultValue = "Confirm Clear History";
        #endregion

        #region HotkeyScope枚举
        /// <summary>
        /// 全局
        /// </summary>
        public string Global { get => Get(); set => Set(value); }
        private string globalDefaultValue = "Global";

        /// <summary>
        /// 资源管理器
        /// </summary>
        public string FileExplorer { get => Get(); set => Set(value); }
        private string fileExplorerDefaultValue = "FileExplorer";
        #endregion

        #region HotKeyAction枚举
        /// <summary>
        /// 打开
        /// </summary>
        public string Open { get => Get(); set => Set(value); }
        private string openDefaultValue = "Open";

        /// <summary>
        /// 打开提示
        /// </summary>
        public string OpenToolTip { get => Get(); set => Set(value); }
        private string openToolTipDefaultValue = "Open a new tab/window with the specified location.";

        /// <summary>
        /// 重复
        /// </summary>
        public string Duplicate { get => Get(); set => Set(value); }
        private string duplicateDefaultValue = "Duplicate";

        /// <summary>
        /// 重复提示
        /// </summary>
        public string DuplicateToolTip { get => Get(); set => Set(value); }
        private string duplicateToolTipDefaultValue = "Duplicate the current tab.";

        /// <summary>
        /// 重新打开
        /// </summary>
        public string ReopenClosed { get => Get(); set => Set(value); }
        private string reopenClosedDefaultValue = "ReopenClosed";

        /// <summary>
        /// 重新打开提示
        /// </summary>
        public string ReopenClosedToolTip { get => Get(); set => Set(value); }
        private string reopenClosedToolTipDefaultValue = "Reopen the last closed location.";

        /// <summary>
        /// 标签页搜索
        /// </summary>
        public string TabSearch { get => Get(); set => Set(value); }
        private string tabSearchDefaultValue = "TabSearch";

        /// <summary>
        /// 标签页搜索提示
        /// </summary>
        public string TabSearchToolTip { get => Get(); set => Set(value); }
        private string tabSearchToolTipDefaultValue = "Open tab search popup to find and switch between tabs.";

        /// <summary>
        /// 后退
        /// </summary>
        public string NavigateBack { get => Get(); set => Set(value); }
        private string navigateBackDefaultValue = "NavigateBack";

        /// <summary>
        /// 后退提示
        /// </summary>
        public string NavigateBackToolTip { get => Get(); set => Set(value); }
        private string navigateBackToolTipDefaultValue = "Navigate back.";

        /// <summary>
        /// 父目录
        /// </summary>
        public string NavigateUp { get => Get(); set => Set(value); }
        private string navigateUpDefaultValue = "NavigateUp";

        /// <summary>
        /// 父目录提示
        /// </summary>
        public string NavigateUpToolTip { get => Get(); set => Set(value); }
        private string navigateUpToolTipDefaultValue = "Navigate up.";

        /// <summary>
        /// 前进
        /// </summary>
        public string NavigateForward { get => Get(); set => Set(value); }
        private string navigateForwardDefaultValue = "NavigateForward";

        /// <summary>
        /// 前进提示
        /// </summary>
        public string NavigateForwardToolTip { get => Get(); set => Set(value); }
        private string navigateForwardToolTipDefaultValue = "Navigate forward.";

        /// <summary>
        /// 标记窗口
        /// </summary>
        public string SetTargetWindow { get => Get(); set => Set(value); }
        private string setTargetWindowDefaultValue = "SetTargetWindow";

        /// <summary>
        /// 标记窗口提示
        /// </summary>
        public string SetTargetWindowToolTip { get => Get(); set => Set(value); }
        private string setTargetWindowToolTipDefaultValue = "Mark the window that will receive the new tabs.";

        /// <summary>
        /// 资源管理器
        /// </summary>
        public string ToggleWinHook { get => Get(); set => Set(value); }
        private string toggleWinHookDefaultValue = "ToggleWinHook";

        /// <summary>
        /// 资源管理器提示
        /// </summary>
        public string ToggleWinHookToolTip { get => Get(); set => Set(value); }
        private string toggleWinHookToolTipDefaultValue = "Toggle the window hook.";

        /// <summary>
        /// 重用标签页
        /// </summary>
        public string ToggleReuseTabs { get => Get(); set => Set(value); }
        private string toggleReuseTabsDefaultValue = "ToggleReuseTabs";

        /// <summary>
        /// 重用标签页提示
        /// </summary>
        public string ToggleReuseTabsToolTip { get => Get(); set => Set(value); }
        private string toggleReuseTabsToolTipDefaultValue = "Toggle the reuse tabs option.";

        /// <summary>
        /// 程序显隐
        /// </summary>
        public string ToggleVisibility { get => Get(); set => Set(value); }
        private string toggleVisibilityDefaultValue = "ToggleVisibility";

        /// <summary>
        /// 程序显隐提示
        /// </summary>
        public string ToggleVisibilityToolTip { get => Get(); set => Set(value); }
        private string toggleVisibilityToolTipDefaultValue = "Show/Hide the app.";

        /// <summary>
        /// 分离标签
        /// </summary>
        public string DetachTab { get => Get(); set => Set(value); }
        private string detachTabDefaultValue = "DetachTab";

        /// <summary>
        /// 分离标签提示
        /// </summary>
        public string DetachTabToolTip { get => Get(); set => Set(value); }
        private string detachTabToolTipDefaultValue = "Detach the current tab.";

        /// <summary>
        /// 右贴靠
        /// </summary>
        public string SnapRight { get => Get(); set => Set(value); }
        private string snapRightDefaultValue = "SnapRight";

        /// <summary>
        /// 右贴靠提示
        /// </summary>
        public string SnapRightToolTip { get => Get(); set => Set(value); }
        private string snapRightToolTipDefaultValue = "Snap the current window to the right.";

        /// <summary>
        /// 左贴靠
        /// </summary>
        public string SnapLeft { get => Get(); set => Set(value); }
        private string snapLeftDefaultValue = "SnapLeft";

        /// <summary>
        /// 左贴靠提示
        /// </summary>
        public string SnapLeftToolTip { get => Get(); set => Set(value); }
        private string snapLeftToolTipDefaultValue = "Snap the current window to the left.";

        /// <summary>
        /// 上贴靠
        /// </summary>
        public string SnapUp { get => Get(); set => Set(value); }
        private string snapUpDefaultValue = "SnapUp";

        /// <summary>
        /// 上贴靠提示
        /// </summary>
        public string SnapUpToolTip { get => Get(); set => Set(value); }
        private string snapUpToolTipDefaultValue = "Snap the current window to the top.";

        /// <summary>
        /// 底贴靠
        /// </summary>
        public string SnapDown { get => Get(); set => Set(value); }
        private string snapDownDefaultValue = "SnapDown";

        /// <summary>
        /// 底贴靠提示
        /// </summary>
        public string SnapDownToolTip { get => Get(); set => Set(value); }
        private string snapDownToolTipDefaultValue = "Snap the current window to the bottom.";
        #endregion
        #endregion

        public LanguageFields()
        {
            InitLanguageFields();
        }

        #region 语言字典相关方法
        /// <summary>
        /// 重置语言字段
        /// </summary>
        public void ResetLanguageFields()
        {
            foreach (var item in dictCurrentLanguageFields.Values)
            {
                item.Value = null;
            }
        }

        /// <summary>
        /// 初始化语言字段
        /// </summary>
        private void InitLanguageFields()
        {
            //语言信息
            dictCurrentLanguageFields[nameof(LanguageName)] = new Field(languageNameDefaultValue);

            //文件筛选
            dictCurrentLanguageFields[nameof(JsonFiles)] = new Field(jsonFilesDefaultValue);
            dictCurrentLanguageFields[nameof(AllFiles)] = new Field(allFilesDefaultValue);

            //快捷键菜单
            dictCurrentLanguageFields[nameof(Shortcuts)] = new Field(shortcutsDefaultValue);
            dictCurrentLanguageFields[nameof(ShortcutsToolTip)] = new Field(shortcutsToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(OpenApplicationMenu)] = new Field(openApplicationMenuDefaultValue);
            dictCurrentLanguageFields[nameof(New)] = new Field(newDefaultValue);
            dictCurrentLanguageFields[nameof(NewToolTip)] = new Field(newToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(Import)] = new Field(importDefaultValue);
            dictCurrentLanguageFields[nameof(ImportToolTip)] = new Field(importToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(Export)] = new Field(exportDefaultValue);
            dictCurrentLanguageFields[nameof(ExportToolTip)] = new Field(exportToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(Save)] = new Field(saveDefaultValue);
            dictCurrentLanguageFields[nameof(SaveToolTip)] = new Field(saveToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(AutoSave)] = new Field(autoSaveDefaultValue);
            dictCurrentLanguageFields[nameof(AutoSaveToolTip)] = new Field(autoSaveToolTipDefaultValue);

            //偏好菜单
            dictCurrentLanguageFields[nameof(Preferences)] = new Field(preferencesDefaultValue);
            dictCurrentLanguageFields[nameof(PreferencesToolTip)] = new Field(preferencesToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(ApplicationSettings)] = new Field(applicationSettingsDefaultValue);
            dictCurrentLanguageFields[nameof(AutoUpdate)] = new Field(autoUpdateDefaultValue);
            dictCurrentLanguageFields[nameof(AutoUpdateToolTip)] = new Field(autoUpdateToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(ThemeIssue)] = new Field(themeIssueDefaultValue);
            dictCurrentLanguageFields[nameof(ThemeIssueToolTip)] = new Field(themeIssueToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(SaveClosedHistory)] = new Field(saveClosedHistoryDefaultValue);
            dictCurrentLanguageFields[nameof(SaveClosedHistoryToolTip)] = new Field(saveClosedHistoryToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(RestorePreviousWindows)] = new Field(restorePreviousWindowsDefaultValue);
            dictCurrentLanguageFields[nameof(RestorePreviousWindowsToolTip)] = new Field(restorePreviousWindowsToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(HideTrayIcon)] = new Field(hideTrayIconDefaultValue);
            dictCurrentLanguageFields[nameof(HideTrayIconToolTip)] = new Field(hideTrayIconToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(SelectLanguage)] = new Field(selectLanguageDefaultValue);
            dictCurrentLanguageFields[nameof(SelectLanguageToolTip)] = new Field(selectLanguageToolTipDefaultValue);

            //关于菜单
            dictCurrentLanguageFields[nameof(About)] = new Field(aboutDefaultValue);
            dictCurrentLanguageFields[nameof(AboutToolTip)] = new Field(aboutToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(Profile)] = new Field(profileDefaultValue);
            dictCurrentLanguageFields[nameof(Star)] = new Field(starDefaultValue);
            dictCurrentLanguageFields[nameof(SupportProject)] = new Field(supportProjectDefaultValue);
            dictCurrentLanguageFields[nameof(SupportProjectText)] = new Field(supportProjectTextDefaultValue);
            dictCurrentLanguageFields[nameof(GitHubSponsors)] = new Field(gitHubSponsorsDefaultValue);
            dictCurrentLanguageFields[nameof(Patreon)] = new Field(patreonDefaultValue);
            dictCurrentLanguageFields[nameof(Coffee)] = new Field(coffeeDefaultValue);
            dictCurrentLanguageFields[nameof(PayPal)] = new Field(payPalDefaultValue);
            dictCurrentLanguageFields[nameof(Developer)] = new Field(developerDefaultValue);
            dictCurrentLanguageFields[nameof(Supporters)] = new Field(supportersDefaultValue);
            dictCurrentLanguageFields[nameof(SupportersText)] = new Field(supportersTextDefaultValue);
            dictCurrentLanguageFields[nameof(FirstSupport)] = new Field(firstSupportDefaultValue);
            dictCurrentLanguageFields[nameof(SupportMeaning)] = new Field(supportMeaningDefaultValue);

            //任务栏菜单
            dictCurrentLanguageFields[nameof(NotifyIconText)] = new Field(notifyIconTextDefaultValue);
            dictCurrentLanguageFields[nameof(KeyboardHook)] = new Field(keyboardHookDefaultValue);
            dictCurrentLanguageFields[nameof(KeyboardHookToolTip)] = new Field(keyboardHookToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(MouseHook)] = new Field(mouseHookDefaultValue);
            dictCurrentLanguageFields[nameof(MouseHookToolTip)] = new Field(mouseHookToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(WindowHook)] = new Field(windowHookDefaultValue);
            dictCurrentLanguageFields[nameof(WindowHookToolTip)] = new Field(windowHookToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(ReuseTabs)] = new Field(reuseTabsDefaultValue);
            dictCurrentLanguageFields[nameof(ReuseTabsToolTip)] = new Field(reuseTabsToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(AddToStartup)] = new Field(addToStartupDefaultValue);
            dictCurrentLanguageFields[nameof(AddToStartupToolTip)] = new Field(addToStartupToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(CheckForUpdates)] = new Field(checkForUpdatesDefaultValue);
            dictCurrentLanguageFields[nameof(CheckForUpdatesToolTip)] = new Field(checkForUpdatesToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(Settings)] = new Field(settingsDefaultValue);
            dictCurrentLanguageFields[nameof(SettingsToolTip)] = new Field(settingsToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(Exit)] = new Field(exitDefaultValue);
            dictCurrentLanguageFields[nameof(ExitToolTip)] = new Field(exitToolTipDefaultValue);

            //快捷键配置控件
            dictCurrentLanguageFields[nameof(EnabledToolTip)] = new Field(enabledToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(Name)] = new Field(nameDefaultValue);
            dictCurrentLanguageFields[nameof(NameToolTip)] = new Field(nameToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(Hotkeys)] = new Field(hotkeysDefaultValue);
            dictCurrentLanguageFields[nameof(HotkeysToolTip)] = new Field(hotkeysToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(ScopeToolTip)] = new Field(scopeToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(ActionToolTip)] = new Field(actionToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(ExpanderToggleToolTip)] = new Field(expanderToggleToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(DeleteToolTip)] = new Field(deleteToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(Path)] = new Field(pathDefaultValue);
            dictCurrentLanguageFields[nameof(PathToolTip)] = new Field(pathToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(Delay)] = new Field(delayDefaultValue);
            dictCurrentLanguageFields[nameof(DelayToolTip)] = new Field(delayToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(Handled)] = new Field(handledDefaultValue);
            dictCurrentLanguageFields[nameof(HandledToolTip)] = new Field(handledToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(OpenAsTab)] = new Field(openAsTabDefaultValue);
            dictCurrentLanguageFields[nameof(OpenAsTabToolTip)] = new Field(openAsTabToolTipDefaultValue);

            //自定义消息弹窗控件
            dictCurrentLanguageFields[nameof(Ok)] = new Field(okDefaultValue);
            dictCurrentLanguageFields[nameof(Cancel)] = new Field(cancelDefaultValue);
            dictCurrentLanguageFields[nameof(Yes)] = new Field(yesDefaultValue);
            dictCurrentLanguageFields[nameof(No)] = new Field(noDefaultValue);

            //标签页搜索控件
            dictCurrentLanguageFields[nameof(ClearToolTip)] = new Field(clearToolTipDefaultValue);

            //消息
            dictCurrentLanguageFields[nameof(AlreadyRunning)] = new Field(alreadyRunningDefaultValue);
            dictCurrentLanguageFields[nameof(RestorePreviouslyOpenedWindows)] = new Field(restorePreviouslyOpenedWindowsDefaultValue);
            dictCurrentLanguageFields[nameof(HideTrayIconWithHotkeys)] = new Field(hideTrayIconWithHotkeysDefaultValue);
            dictCurrentLanguageFields[nameof(HideTrayIconWithOutHotkeys)] = new Field(hideTrayIconWithOutHotkeysDefaultValue);
            dictCurrentLanguageFields[nameof(ConfirmClearHistory)] = new Field(confirmClearHistoryDefaultValue);
            dictCurrentLanguageFields[nameof(ConfirmClearHistoryTitle)] = new Field(confirmClearHistoryTitleDefaultValue);

            //HotkeyScope枚举
            dictCurrentLanguageFields[nameof(Global)] = new Field(globalDefaultValue);
            dictCurrentLanguageFields[nameof(FileExplorer)] = new Field(fileExplorerDefaultValue);

            //HotKeyAction枚举
            dictCurrentLanguageFields[nameof(Open)] = new Field(openDefaultValue);
            dictCurrentLanguageFields[nameof(OpenToolTip)] = new Field(openToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(Duplicate)] = new Field(duplicateDefaultValue);
            dictCurrentLanguageFields[nameof(DuplicateToolTip)] = new Field(duplicateToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(ReopenClosed)] = new Field(reopenClosedDefaultValue);
            dictCurrentLanguageFields[nameof(ReopenClosedToolTip)] = new Field(reopenClosedToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(TabSearch)] = new Field(tabSearchDefaultValue);
            dictCurrentLanguageFields[nameof(TabSearchToolTip)] = new Field(tabSearchToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(NavigateBack)] = new Field(navigateBackDefaultValue);
            dictCurrentLanguageFields[nameof(NavigateBackToolTip)] = new Field(navigateBackToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(NavigateUp)] = new Field(navigateUpDefaultValue);
            dictCurrentLanguageFields[nameof(NavigateUpToolTip)] = new Field(navigateUpToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(NavigateForward)] = new Field(navigateForwardDefaultValue);
            dictCurrentLanguageFields[nameof(NavigateForwardToolTip)] = new Field(navigateForwardToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(SetTargetWindow)] = new Field(setTargetWindowDefaultValue);
            dictCurrentLanguageFields[nameof(SetTargetWindowToolTip)] = new Field(setTargetWindowToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(ToggleWinHook)] = new Field(toggleWinHookDefaultValue);
            dictCurrentLanguageFields[nameof(ToggleWinHookToolTip)] = new Field(toggleWinHookToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(ToggleReuseTabs)] = new Field(toggleReuseTabsDefaultValue);
            dictCurrentLanguageFields[nameof(ToggleReuseTabsToolTip)] = new Field(toggleReuseTabsToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(ToggleVisibility)] = new Field(toggleVisibilityDefaultValue);
            dictCurrentLanguageFields[nameof(ToggleVisibilityToolTip)] = new Field(toggleVisibilityToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(DetachTab)] = new Field(detachTabDefaultValue);
            dictCurrentLanguageFields[nameof(DetachTabToolTip)] = new Field(detachTabToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(SnapRight)] = new Field(snapRightDefaultValue);
            dictCurrentLanguageFields[nameof(SnapRightToolTip)] = new Field(snapRightToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(SnapLeft)] = new Field(snapLeftDefaultValue);
            dictCurrentLanguageFields[nameof(SnapLeftToolTip)] = new Field(snapLeftToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(SnapUp)] = new Field(snapUpDefaultValue);
            dictCurrentLanguageFields[nameof(SnapUpToolTip)] = new Field(snapUpToolTipDefaultValue);
            dictCurrentLanguageFields[nameof(SnapDown)] = new Field(snapDownDefaultValue);
            dictCurrentLanguageFields[nameof(SnapDownToolTip)] = new Field(snapDownToolTipDefaultValue);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="key"></param>
        /// 
        /// <returns></returns>
        public string GetValue(string key)
        {
            if (dictCurrentLanguageFields.TryGetValue(key, out var field))
            {
                return field.Value ?? field.DefaultValue;
            }
            return string.Empty;
        }

        /// <summary>
        /// 设置值，返回值是否更改
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetValue(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value) == false && dictCurrentLanguageFields.TryGetValue(key, out var field) && field.Value != value)
            {
                field.Value = value;
                return true;
            }
            return false;
        }

#pragma warning disable CS8604 // 引用类型参数可能为 null。
        private string Get([CallerMemberName] string? propertyName = null) => GetValue(propertyName);
#pragma warning restore CS8604 // 引用类型参数可能为 null。

        private void Set(string value, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName != null && SetValue(propertyName, value))
            {
                RaisePropertyChanged(propertyName);
            }
        }
        #endregion

        #region 通知所有属性已更改
        /// <summary>
        /// 通知所有属性已更改
        /// </summary>
        public void RaiseAllPropertyChanged()
        {
            foreach (var key in dictCurrentLanguageFields.Keys)
            {
                RaisePropertyChanged(key);
            }
        }
        #endregion
    }
}
