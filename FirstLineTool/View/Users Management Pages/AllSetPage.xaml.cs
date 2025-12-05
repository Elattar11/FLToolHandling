using FirstLineTool.Helper;
using FirstLineTool.View.Alert;
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

namespace FirstLineTool.View.Users_Management_Pages
{
    /// <summary>
    /// Interaction logic for AllSetPage.xaml
    /// </summary>
    public partial class AllSetPage : Window
    {
        private string _firstname;
        private string _lastname;
        private string _title;
        private string _id;
        private string _role;
        public AllSetPage(string id, string firstname, string lastname, string title, string role)
        {
            InitializeComponent();

            _firstname = firstname;
            _lastname = lastname;
            _title = title;
            _id = id;
            _role = role;

            txtWelcome.Text = $"Thank you, {firstname}, for completing your information! You can now use FL Handling Tool. Enjoy!";
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private async void Window_LoadedAsync(object sender, RoutedEventArgs e)
        {
            await Task.Delay(2500);
            new ToastWindow("Login Successful", $"Welcome back, {_firstname}!").Show();
            MainWindow main = new MainWindow(_id, _firstname, _lastname, _title, _role);
            await WindowAnimator.FadeTransition(this, main);

        }
    }
}
