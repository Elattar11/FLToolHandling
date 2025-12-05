using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FirstLineTool.View.Alert
{
    /// <summary>
    /// Interaction logic for ToastWindow.xaml
    /// </summary>
    public partial class ToastWindow : Window
    {
        public ToastWindow(string title, string message)
        {
            InitializeComponent();

            txtTitle.Text = title;
            txtMessage.Text = message;

            Loaded += Window_LoadedAsync;
        }

        private async void Window_LoadedAsync(object sender, RoutedEventArgs e)
        {
            // تحديد مكان التوست (الزاوية السفلية اليمنى)
            this.Left = SystemParameters.WorkArea.Width - this.Width - 10;
            this.Top = SystemParameters.WorkArea.Height - this.Height - 10;

            // Fade In
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            BeginAnimation(OpacityProperty, fadeIn);

            // انتظر ثانيتين
            await Task.Delay(2000);

            // Fade Out
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            BeginAnimation(OpacityProperty, fadeOut);

            await Task.Delay(500);
            this.Close();
        }
    }
}
