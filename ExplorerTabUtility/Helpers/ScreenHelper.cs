using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ExplorerTabUtility.Helpers
{
    /// <summary>
    /// 屏幕辅助类
    /// </summary>
    static class ScreenHelper
    {
        /// <summary>
        /// 鼠标当前所在屏幕
        /// </summary>
        public static Screen MouseCurrentScreen
        {
            get
            {
                var mousePoint = Control.MousePosition;
                var allScreen = Screen.AllScreens;
                foreach (var item in allScreen)
                {
                    if (mousePoint.IsInScreen(item))
                    {
                        return item;
                    }
                }
                return allScreen.First();
            }
        }

        private static bool IsInScreen(this Point point, Screen screen)
        {
            var area = screen.WorkingArea;
            return point.X >= area.Left && point.X <= area.Right && point.Y >= area.Top && point.Y <= area.Bottom;
        }
    }
}
