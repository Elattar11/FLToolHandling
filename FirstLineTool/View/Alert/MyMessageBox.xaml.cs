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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FirstLineTool.View.Alert
{
    /// <summary>
    /// Interaction logic for MyMessageBox.xaml
    /// </summary>
    public partial class MyMessageBox : Window
    {
        public enum MyMessageBoxButtons
        {
            OK, YesNo, YesNoCancel
        }

        public enum MyMessageBoxIcon
        {
            Informative, Warning, Error, Accept, Question
        }

        public MyMessageBox()
        {
            InitializeComponent();
        }

        public MyMessageBox(string message, string title, MyMessageBoxButtons buttons, MyMessageBoxIcon icon)
        {
            InitializeComponent();
            lblTitle.Text = title;
            lblMessage.Text = message;

            // اخفاء كل الأزرار مبدئياً
            btnOk.Visibility = Visibility.Collapsed;
            btnYes.Visibility = Visibility.Collapsed;
            btnNo.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Collapsed;

            // إظهار الأزرار حسب النوع
            switch (buttons)
            {
                case MyMessageBoxButtons.OK:
                    btnOk.Visibility = Visibility.Visible;
                    break;
                case MyMessageBoxButtons.YesNo:
                    btnYes.Visibility = Visibility.Visible;
                    btnNo.Visibility = Visibility.Visible;
                    break;
                case MyMessageBoxButtons.YesNoCancel:
                    btnYes.Visibility = Visibility.Visible;
                    btnNo.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    break;
            }

            // الأيقونات (هنا تقدر تحط صور من Resources أو PackIcon من Material Design)
            switch (icon)
            {
                case MyMessageBoxIcon.Error:
                    iconMessage.Kind = MaterialDesignThemes.Wpf.PackIconKind.Error;
                    iconMessage.Foreground = Brushes.Red;
                    break;
                case MyMessageBoxIcon.Warning:
                    iconMessage.Kind = MaterialDesignThemes.Wpf.PackIconKind.Warning;
                    iconMessage.Foreground = Brushes.Orange;
                    break;
                case MyMessageBoxIcon.Informative:
                    iconMessage.Kind = MaterialDesignThemes.Wpf.PackIconKind.Information;
                    iconMessage.Foreground = Brushes.DodgerBlue;
                    break;
                case MyMessageBoxIcon.Accept:
                    iconMessage.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckCircle;
                    iconMessage.Foreground = Brushes.Green;
                    break;
                case MyMessageBoxIcon.Question:
                    iconMessage.Kind = MaterialDesignThemes.Wpf.PackIconKind.HelpCircle;
                    iconMessage.Foreground = Brushes.Purple;
                    break;
            }
        }

        public static MessageBoxResult Show(string msg, string title, MyMessageBoxButtons buttons, MyMessageBoxIcon icon)
        {
            var msgBox = new MyMessageBox(msg, title, buttons, icon);
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        public MessageBoxResult Result { get; private set; }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            this.Close();
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            this.Close();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            this.Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
