using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.Core;
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
using System.Text.RegularExpressions;
using FirstLineTool.View.Database_Pages;

namespace FirstLineTool.View.Users_Management_Pages
{
    /// <summary>
    /// Interaction logic for ResetPasswordPage.xaml
    /// </summary>
    public partial class ResetPasswordPage : Window
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        private string _userId;
        private string _role;
   

        public ResetPasswordPage(string userId, string role)
        {
            InitializeComponent();
            _userId = userId;
            _role = role;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private async void btnResetPassword_ClickAsync(object sender, RoutedEventArgs e)
        {
            string newPassword = txtPassword.Password.Trim();
            string confirmPassword = txtConfirmationPassword.Password.Trim();

            // التحقق من عدم ترك الحقول فارغة
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                MyMessageBox.Show("Please enter password and confirm your new password.", "Warning",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            // التحقق من تطابق الباسوورد
            if (newPassword != confirmPassword)
            {
                MyMessageBox.Show("Passwords do not match!", "Warning",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            // التحقق من قوة الباسوورد باستخدام Regex
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).{8,}$";
            if (!Regex.IsMatch(newPassword, pattern))
            {
                MyMessageBox.Show("Password must be at least 8 characters long and include:\n- Uppercase letter\n- Lowercase letter\n- Number\n- Special character",
                    "Weak Password", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            // توليد salt وعمل hash
            string salt = PasswordHelper.GenerateSalt();
            string passwordHash = PasswordHelper.HashPassword(newPassword, salt);

            // تحديث قاعدة البيانات
            db.ExecuteData("update Users SET Password= '"+ passwordHash + "', Salt= '"+ salt + "' WHERE Id= "+ Convert.ToInt32(_userId) +"",useGlobal: true, "");

            // فتح MainWindow بعد تغيير الباسوورد
            PersonalInformationPage main = new PersonalInformationPage(_userId, _role);
            await WindowAnimator.FadeTransition(this, main);


        }

        private void txtConfirmationPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // نفّذ نفس كود زرار الـ Login
                btnResetPassword_ClickAsync(btnResetPassword_ClickAsync, null);
            }
        }
    }
}
