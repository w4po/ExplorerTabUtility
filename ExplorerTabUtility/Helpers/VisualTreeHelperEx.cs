using System.Windows;
using System.Windows.Media;

namespace ExplorerTabUtility.Helpers
{
    /// <summary>
    /// <see cref="VisualTreeHelper"/>的增强扩展
    /// </summary>
    class VisualTreeHelperEx
    {
        /// <summary>
        /// 查找指定类型的父节点
        /// </summary>
        /// <typeparam name="TParent">要查找的类型</typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TParent? GetParent<TParent>(DependencyObject obj) where TParent : DependencyObject
        {
            if (obj == null) return null;

            var parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is TParent item)
                {
                    return item;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
    }
}
