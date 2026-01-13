using FirstLineTool.Helper.NotificationCenter;
using FirstLineTool.View;
using FirstLineTool.View.Alert;
using FirstLineTool.View.Dashboard;
using FirstLineTool.View.Database_Pages;
using FirstLineTool.View.ExportQueue;
using FirstLineTool.View.Express_Senarios;
using FirstLineTool.View.IN_Senarios;
using FirstLineTool.View.Layer_Pages;
using FirstLineTool.View.Server_Pages;
using FirstLineTool.View.Users_Management_Pages;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FirstLineTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _firstname;
        private string _lastname;
        private string _title;
        private string _id;
        private string _role;

        #region Navigation

        private DatabaseLayerPages _databasePage;
        private ServerPage _serverPage;
        private UsersManagementPage _usersManagementPage;
        private UserCredentialsPage _userCredentialsPage;
        private UserInformationPage _userInformationPage;
        private ExportQueuePage _exportQueuePage;
        private DashboardPage _dashboardPage;
        private ExpressTabsPage _expressTabsPage;
        #endregion
        public MainWindow(string id, string firstname, string lastname, string title, string role)
        {
            InitializeComponent();

            
            

            _firstname = firstname;
            _lastname = lastname;
            _title = title;
            _id = id;
            _role = role;

            if (role == "User")
            {
                btnUsers.Visibility = Visibility.Collapsed;
               

            }
            else if (role == "Admin")
            {
                btnUsers.Visibility = Visibility.Visible;
                
            }

            txtName.Text = $"Welcome {_firstname}";
            txtTitle.Text = $"{_title}";
            _role = role;
        }

        
        

        public void UpdateUserInfo(string firstname, string lastname, string title)
        {
            _firstname = firstname;
            _lastname = lastname;
            _title = title;

            txtName.Text = $"Welcome {_firstname}";
            txtTitle.Text = $"{_title}";
        }


        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        
        

        //To active button when click on it to open it's page
        private void ResetMenuButtons()
        {
            btnDatabase.Tag = null;
            btnServer.Tag = null;
            btnUsers.Tag = null;
            btnUserInfo.Tag = null;
            btnCredentials.Tag = null;
            btnDashboard.Tag = null;
            btnExpress.Tag = null;
            btnIN.Tag = null;
            btnExport.Tag = null;

            // زود باقي الزراير هنا لو عندك
        }
        private void btnDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (_databasePage == null)
                _databasePage = new DatabaseLayerPages(_id, _role);

            MainFrame.Navigate(_databasePage);
            ResetMenuButtons();
            btnDatabase.Tag = "Active";
        }

        private void btnServer_Click(object sender, RoutedEventArgs e)
        {
            if (_serverPage == null)
                _serverPage = new ServerPage(_id, _role);

            MainFrame.Navigate(_serverPage);
            ResetMenuButtons();
            btnServer.Tag = "Active";
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private bool isMenuExpanded = true;
        private void btnSideMenu_Click(object sender, RoutedEventArgs e)
        {


            if (isMenuExpanded)
            {
                txtName.Visibility = Visibility.Hidden;
                txtTitle.Visibility = Visibility.Hidden;
                separatorofsidemenu.Visibility = Visibility.Collapsed;
                // اخفي النصوص
                HideTextBlocks(MenuPanel);
                MenuColumn.Width = new GridLength(60);   // تصغير العمود
            }
            else
            {
                txtName.Visibility = Visibility.Visible;
                txtTitle.Visibility = Visibility.Visible;
                separatorofsidemenu.Visibility = Visibility.Visible;
                // اظهر النصوص
                ShowTextBlocks(MenuPanel);
                MenuColumn.Width = new GridLength(250);  // إرجاع العمود للحجم الأصلي
            }

            isMenuExpanded = !isMenuExpanded;
        }


        private void HideTextBlocks(Panel parent)
        {
            foreach (var child in parent.Children)
            {
                if (child is Button btn && btn.Content is StackPanel sp)
                {
                    foreach (var item in sp.Children)
                    {
                        if (item is TextBlock tb)
                            tb.Visibility = Visibility.Collapsed;
                    }
                }
                else if (child is Panel p)
                {
                    HideTextBlocks(p); // Recursive عشان لو فيه Panels جوه بعض
                }
            }
        }

        private void ShowTextBlocks(Panel parent)
        {
            foreach (var child in parent.Children)
            {
                if (child is Button btn && btn.Content is StackPanel sp)
                {
                    foreach (var item in sp.Children)
                    {
                        if (item is TextBlock tb)
                            tb.Visibility = Visibility.Visible;
                    }
                }
                else if (child is Panel p)
                {
                    ShowTextBlocks(p);
                }
            }
        }

        private void btnUsers_Click(object sender, RoutedEventArgs e)
        {
            

            MainFrame.Navigate(new UsersManagementPage(_id));
            ResetMenuButtons();
            btnUsers.Tag = "Active";
        }


        private async void btnLogout_ClickAsync(object sender, RoutedEventArgs e)
        {
            var rs = MyMessageBox.Show("Are you sure you want to log out?", "Logout",
        MyMessageBox.MyMessageBoxButtons.YesNo, MyMessageBox.MyMessageBoxIcon.Warning);

            if (rs == MessageBoxResult.Yes)
            {

                new ToastWindow("Goodbye!", $"Bye Bye {_firstname}").Show();

                // أنيميشن اختفاء تدريجي للنافذة الحالية
                var fadeOut = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.8)),
                    FillBehavior = System.Windows.Media.Animation.FillBehavior.Stop
                };

                this.BeginAnimation(Window.OpacityProperty, fadeOut);

                // انتظار انتهاء الأنيميشن
                await Task.Delay(1000);

                // فتح نافذة تسجيل الدخول مع أنيميشن الدخول
                LoginView loginWindow = new LoginView();

                // نبدأها شفافة
                loginWindow.Opacity = 0;
                loginWindow.Show();

                // أنيميشن ظهور تدريجي
                var fadeIn = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.8))
                };
                loginWindow.BeginAnimation(Window.OpacityProperty, fadeIn);

                // إغلاق النافذة الحالية
                this.Close();
            }
        }

        private void btnUserInfo_Click(object sender, RoutedEventArgs e)
        {
            if (_userInformationPage == null)
                _userInformationPage = new UserInformationPage(_id, this);

            MainFrame.Navigate(_userInformationPage);
            ResetMenuButtons();
            btnUserInfo.Tag = "Active";
        }

        private void btnCredentials_Click(object sender, RoutedEventArgs e)
        {


            MainFrame.Navigate(new UserCredentialsPage(_id));
            ResetMenuButtons();
            btnCredentials.Tag = "Active";

        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            


            if (_dashboardPage == null)
                _dashboardPage = new DashboardPage(_id, _role);

            MainFrame.Navigate(_dashboardPage);
            ResetMenuButtons();
            btnDashboard.Tag = "Active";
        }

        private void btnExpress_Click(object sender, RoutedEventArgs e)
        {
            


            if (_expressTabsPage == null)
                _expressTabsPage = new ExpressTabsPage(_id, _role);

            MainFrame.Navigate(_expressTabsPage);
            ResetMenuButtons();
            btnExpress.Tag = "Active";
        }

        

        private void btnIN_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new INTemp());
            ResetMenuButtons();
            btnIN.Tag = "Active";
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            

            if (_exportQueuePage == null)
                _exportQueuePage = new ExportQueuePage(_id, _role);

            MainFrame.Navigate(_exportQueuePage);
            ResetMenuButtons();
            btnExport.Tag = "Active";
        }

        private bool isMaximized = false;

        private void btnMaxmize_Click(object sender, RoutedEventArgs e)
        {
            if (isMaximized)
            {
                this.WindowState = WindowState.Normal;
                this.Width = 1250;
                this.Height = 800;

                isMaximized = false;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                isMaximized = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MaxHeight = SystemParameters.WorkArea.Height;
        }

        

        
    }
}