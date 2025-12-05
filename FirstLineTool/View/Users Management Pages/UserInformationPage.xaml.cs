using FirstLineTool.Core;
using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.View.Alert;
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

namespace FirstLineTool.View.Users_Management_Pages
{
    /// <summary>
    /// Interaction logic for UserInformationPage.xaml
    /// </summary>
    public partial class UserInformationPage : Page
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        private string _userId;
        private MainWindow _main;
        int row;
        private bool _isInitialized = false;


        public UserInformationPage(string userId , MainWindow main)
        {
            InitializeComponent();
            _userId = userId;
            _main = main;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
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

            db.ExecuteData(updateQuery, useGlobal: true, "User information updated successfully");

            _main.UpdateUserInfo(firstName, lastName, title);

        }

        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
            {
                tbl.Clear();
                tbl = db.ReadData(@"
                    SELECT Title, FirstName, LastName
                            FROM Users
                            WHERE Id = " + _userId + "", useGlobal: true,
                        "");

                txtFirstName.Text = tbl.Rows[row]["FirstName"].ToString();
                txtLastName.Text = tbl.Rows[row]["LastName"].ToString();
                txtTitle.Text = tbl.Rows[row]["Title"].ToString();

                _isInitialized = true;
            }
        }

        
    }
}
