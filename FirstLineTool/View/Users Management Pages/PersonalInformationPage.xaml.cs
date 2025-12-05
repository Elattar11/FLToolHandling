using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.Core;
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
using FirstLineTool.View.Alert;
using FirstLineTool.Helper;
using System.Data;

namespace FirstLineTool.View.Users_Management_Pages
{
    /// <summary>
    /// Interaction logic for PersonalInformationPage.xaml
    /// </summary>
    public partial class PersonalInformationPage : Window
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        private string _userId;
        private string _role;
        int row;

        public PersonalInformationPage(string userId, string role)
        {
            InitializeComponent();
            _userId = userId;
            _role = role;
        }

        private async void btnFinish_ClickAsync(object sender, RoutedEventArgs e)
        {
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string title = txtTitle.Text.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(title))
            {
                MyMessageBox.Show("Please enter your full information.", "Warning",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            string updateQuery = $"UPDATE Users SET Title = '{title}', FirstName = '{firstName}', LastName = '{lastName}' WHERE Id = {_userId}";

            db.ExecuteData(updateQuery, useGlobal: true, "");

            AllSetPage main = new AllSetPage(_userId, firstName, lastName, title, _role);
            await WindowAnimator.FadeTransition(this, main);


        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbl.Clear();
            tbl = db.ReadData(@"
                    SELECT Title, FirstName, LastName
                            FROM Users
                            WHERE Id = "+_userId+"", useGlobal: true,
                    "");

            txtFirstName.Text = tbl.Rows[row]["FirstName"].ToString();
            txtLastName.Text = tbl.Rows[row]["LastName"].ToString();
            txtTitle.Text = tbl.Rows[row]["Title"].ToString();



        }

        private void txtTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // نفّذ نفس كود زرار الـ Login
                btnFinish_ClickAsync(btnFinish_ClickAsync, null);
            }
        }
    }
}
