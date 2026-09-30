namespace ExplorerTabUtility.Models
{
    public class ComboBoxItemInfo<TKey> : BindableBase
    {
        private TKey key;
        /// <summary>
        /// 主键
        /// </summary>
        public TKey Key
        {
            get { return key; }
            set { SetProperty(ref key, value); }
        }

        private string display;
        /// <summary>
        /// 显示值
        /// </summary>
        public string Display
        {
            get { return display; }
            set { SetProperty(ref display, value); }
        }

        private string? toolTip;
        /// <summary>
        /// 提示
        /// </summary>
        public string? ToolTip
        {
            get { return toolTip; }
            set { SetProperty(ref toolTip, value); }
        }

        public ComboBoxItemInfo(TKey key, string display, string? toolTip = null)
        {
            this.key = key;
            this.display = display;
            this.toolTip = toolTip;
        }
    }

    internal class ComboBoxItemInfo : ComboBoxItemInfo<string>
    {
        public ComboBoxItemInfo(string key, string display, string? tooltip = null) : base(key, display, tooltip)
        {
        }
    }
}
