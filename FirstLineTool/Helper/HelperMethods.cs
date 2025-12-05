using FirstLineTool.View.Alert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FirstLineTool.Helper
{
    public class HelperMethods
    {
        public void SearchKeyPress(KeyEventArgs e, TextBox txt, Button btn, string msg)
        {
            // تحقق من الضغط على Enter
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    // عرض رسالة تحذير
                    MyMessageBox.Show(msg, "WARNING",
                        MyMessageBox.MyMessageBoxButtons.OK,
                        MyMessageBox.MyMessageBoxIcon.Warning);
                    return;
                }

                // تنفيذ كود الزرار (زي PerformClick)
                btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        public void OpenWindow<T>() where T : Window, new()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // نشوف إذا فيه Window مفتوح من نفس النوع
                foreach (Window win in Application.Current.Windows)
                {
                    if (win is T)
                    {
                        win.Activate(); // لو مفتوح، نفعّله
                        return;
                    }
                }

                // لو مش موجود، نفتح نسخة جديدة
                T window = new T();
                window.Show(); // لو عايز modal: window.ShowDialog();
            });
        }
    }
}
