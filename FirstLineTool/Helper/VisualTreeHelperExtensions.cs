using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace FirstLineTool.Helper
{
    public static class VisualTreeHelperExtensions
    {
        public static IEnumerable<T> FindChildren<T>(this DependencyObject d) where T : DependencyObject
        {
            if (d == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);

                if (child is T t)
                    yield return t;

                foreach (var sub in FindChildren<T>(child))
                    yield return sub;
            }
        }
    }
}
