using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.Languages.Manager
{
    internal class LangeuageHelper
    {
        #region 属性
        /// <summary>
        /// 单例
        /// </summary>
        public static LangeuageHelper Instance { get; } = new LangeuageHelper();

        /// <summary>
        /// 语言字段
        /// </summary>
        public LanguageFields LanguageFields { get; private set; }

        /// <summary>
        /// 语言集合
        /// </summary>
        public List<ComboBoxItemInfo> Languages { get; private set; }

        private ComboBoxItemInfo currentLanguage;
        /// <summary>
        /// 当前语言
        /// </summary>
        public ComboBoxItemInfo CurrentLanguage
        {
            get { return currentLanguage; }
            set
            {
                var change = LoadLanguage(value.Key);
                if (change)
                {
                    SettingsManager.Language = value.Key;
                    currentLanguage = value;
                }
                else
                {
                    change = LoadLanguage(currentLanguage.Key);
                }

                if (change) OnLangeuageChanged?.Invoke();
            }
        }
        #endregion

        #region 事件
        public event Action? OnLangeuageChanged;
        #endregion

        private readonly string languageDir = "Languages";

        private LangeuageHelper()
        {
            LanguageFields = new LanguageFields();
            Languages = GetLanguages();

            var language = SettingsManager.Language ?? "en-US";
            currentLanguage = Languages.FirstOrDefault(t => t.Key == language) ?? Languages.First();
            LoadLanguage(currentLanguage.Key);
        }

        #region 语言相关方法
        private List<ComboBoxItemInfo> GetLanguages()
        {
            if (Directory.Exists(languageDir) == false)
            {
                return GetDefaultLanguages();
            }

            try
            {
                var filePaths = Directory.GetFiles(languageDir, "*.txt");
                if (filePaths.Length == 0)
                {
                    return GetDefaultLanguages();
                }
                else
                {
                    var result = new List<ComboBoxItemInfo>(filePaths.Length);
                    foreach (var item in filePaths)
                    {
                        result.Add(GetLanguage(item));
                    }
                    return result;
                }
            }
            catch (Exception)
            {
                return GetDefaultLanguages();
            }
        }

        private List<ComboBoxItemInfo> GetDefaultLanguages()
        {
            return new List<ComboBoxItemInfo>
            {
                new ComboBoxItemInfo("en-US","English"),
            };
        }

        private ComboBoxItemInfo GetLanguage(string filePath)
        {
            var code = Path.GetFileNameWithoutExtension(filePath);
            var name = code;

            try
            {
                var languageName = nameof(LanguageFields.LanguageName);
                foreach (var line in File.ReadLines(filePath))
                {
                    if (GetKeyValue(line, out var key, out var value) && key == languageName)
                    {
                        name = value;
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }

            return new ComboBoxItemInfo(code, name);
        }

        private bool LoadLanguage(string langeuage)
        {
            try
            {
                LanguageFields.ResetLanguageFields();

                var filePath = Path.Combine(languageDir, $"{langeuage}.txt");
                if (File.Exists(filePath) == false) return false;

                foreach (var line in File.ReadLines(filePath))
                {
                    if (GetKeyValue(line, out var key, out var value))
                    {
                        LanguageFields.SetValue(key, value);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            LanguageFields.RaiseAllPropertyChanged();
            return true;
        }

        private bool GetKeyValue(string line, out string key, out string value)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
            {
                key = string.Empty;
                value = string.Empty;
                return false;
            }

            var index = line.IndexOf(':');
            if (index < 1)
            {
                key = string.Empty;
                value = string.Empty;
                return false;
            }

            key = line.Substring(0, index).Trim();
            value = line.Substring(index + 1).Replace("\\r\\n", "\\n").Replace("\\r", "\\n").Replace("\\n", Environment.NewLine);
            return true;
        }
        #endregion
    }
}
