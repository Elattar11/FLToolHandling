using FirstLineTool.Core;
using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.Helper;
using FirstLineTool.View.Alert;
using FirstLineTool.View.Users_Management_Pages;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace FirstLineTool.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable dt = new DataTable();
        public LoginView()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void Button_Click_1Async(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password;

            if (pwdBox.Visibility == Visibility.Visible)
                password = pwdBox.Password.Trim();
            else
                password = txtVisiblePassword.Text.Trim();

            // التحقق من أن الحقول ليست فارغة
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MyMessageBox.Show("Please enter username and password.", "Warning",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            try
            {
                // جلب بيانات المستخدم من قاعدة البيانات
                DataTable dt = db.ReadData($"SELECT * FROM Users WHERE Username = '{username}'", useGlobal: true, "");

                if (dt.Rows.Count > 0)
                {
                    string storedHash = dt.Rows[0]["Password"].ToString();
                    string storedSalt = dt.Rows[0]["Salt"].ToString();

                    bool isValid = PasswordHelper.VerifyPassword(password, storedHash, storedSalt);

                    if (isValid)
                    {
                        string Id = dt.Rows[0]["Id"].ToString();
                        string firstname = dt.Rows[0]["FirstName"].ToString();
                        string lastname = dt.Rows[0]["LastName"].ToString();
                        string title = dt.Rows[0]["Title"].ToString();
                        string role = dt.Rows[0]["Role"].ToString();

                        bool isDefaultPassword = PasswordHelper.VerifyPassword("0000", storedHash, storedSalt);
                        if (isDefaultPassword)
                        {
                            // فتح نافذة تغيير الباسوورد أولاً
                            ResetPasswordPage setPasswordWindow = new ResetPasswordPage(Id, role);
                            await WindowAnimator.FadeTransition(this, setPasswordWindow);
                            return;
                        }

                        

                        LoadingScreen loading = new LoadingScreen();
                        await WindowAnimator.FadeTransition(this, loading);

                        //backup from global datapase in sharing to local
                        bool backupSuccess = await Task.Run(() => db.GetBackupFromGlobalToLocal());

                        

                        

                        if (!backupSuccess)
                        {
                            loading.Close();
                            MyMessageBox.Show("Failed to update local database from global. Please contact IT.",
                                "Backup Error",
                                MyMessageBox.MyMessageBoxButtons.OK,
                                MyMessageBox.MyMessageBoxIcon.Error);
                            return;
                        }

                        await Task.Delay(1000);

                        // بعد التحقق من اسم المستخدم والباسوورد
                        MainWindow main = new MainWindow(Id, firstname, lastname, title, role);

                        // استخدم الـ WindowAnimator لعمل fade in/out
                        await WindowAnimator.FadeTransition(loading, main);

                        // رسالة ترحيب بسيطة
                        new ToastWindow("Login Successful", $"Welcome back, {firstname}!").Show();
                    }
                    else
                    {
                        MyMessageBox.Show("Invalid username or password!", "Login Failed",
                            MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    }
                }
                else
                {
                    MyMessageBox.Show("Invalid username or password!", "Login Failed",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Error: " + ex.Message, "Login Error",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
            }
        }
        private bool isPasswordVisible = false;
        private void btnShowHide_Click(object sender, RoutedEventArgs e)
        {
            if (!isPasswordVisible)
            {
                txtVisiblePassword.Text = pwdBox.Password;
                txtVisiblePassword.Visibility = Visibility.Visible;
                pwdBox.Visibility = Visibility.Collapsed;

                btnShowHide.Content = new PackIcon
                {
                    Kind = PackIconKind.EyeOff,
                    Width = 20,
                    Height = 20,
                    Foreground = Brushes.Red
                };

                isPasswordVisible = true;
            }
            else
            {
                pwdBox.Password = txtVisiblePassword.Text;
                txtVisiblePassword.Visibility = Visibility.Collapsed;
                pwdBox.Visibility = Visibility.Visible;

                btnShowHide.Content = new PackIcon
                {
                    Kind = PackIconKind.Eye,
                    Width = 20,
                    Height = 20,
                    Foreground = Brushes.Red
                };

                isPasswordVisible = false;
            }
        }

        private void pwdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // نفّذ نفس كود زرار الـ Login
                Button_Click_1Async(Button_Click_1Async, null);
            }
        }
    }
}
