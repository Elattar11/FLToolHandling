using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.Core;
using FirstLineTool.Helper;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;
using FirstLineTool.View.Alert;

namespace FirstLineTool.View.Users_Management_Pages
{
    /// <summary>
    /// Interaction logic for UserCredentialsPage.xaml
    /// </summary>
    public partial class UserCredentialsPage : Page
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        HelperMethods _helper = new HelperMethods();
        private int queryId;
        private string _userId;
        private bool _isInitialized = false;

        public UserCredentialsPage(string userId)
        {
            InitializeComponent();

            _userId = userId;

        }

        private void AutoNumber()
        {
            tbl.Clear();
            tbl = db.ReadData(@"
                SELECT 
                    Id,
                    SystemName AS 'System Name',
                    Username AS 'System Username'
                FROM UserAuth
                WHERE UserId = " + _userId+"", useGlobal: true
            , "");

            tbl.Constraints.Clear();
            dgSystems.ItemsSource = tbl.DefaultView;


            txtUsername.Clear();
            txtSearch.Clear();
            pwdBox.Clear();
            txtVisiblePassword.Clear();
            txtSystemName.Clear();

            

            btnAdd.IsEnabled = true;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            
        }


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            string password;
            if (pwdBox.Visibility == Visibility.Visible)
                password = pwdBox.Password.Trim();
            else
                password = txtVisiblePassword.Text.Trim();


            string encryptedPassword = EncryptionHelper.Encrypt(password);

            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                MyMessageBox.Show("Please enter Usernames of system!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtSystemName.Text))
            {
                MyMessageBox.Show("Please enter System Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MyMessageBox.Show("Please enter System Password!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }
            else
            {
                try
                {
                    db.ExecuteData(
                                    "insert into UserAuth (Username, Password, SystemName, UserId) " +
                                    "values ('" + txtUsername.Text.Trim() + "', '" + encryptedPassword + "', '" + txtSystemName.Text.Trim() + "' , " + _userId + ")",
                                    useGlobal: true,
                                    "New Auth System Added successfully"
                                );
                }
                catch (Exception ex)
                {

                    // التحقق من أن الخطأ بسبب UNIQUE constraint
                    if (ex.Message.Contains("Error 19"))
                    {
                        MyMessageBox.Show($"The system '{txtSystemName.Text.Trim()}' already exists!", "ERROR",
                            MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    }
                    else
                    {
                        // أي خطأ آخر
                        MyMessageBox.Show("Error: " + ex.Message, "ERROR",
                            MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    }
                }
                AutoNumber();
            }


        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            string password;
            if (pwdBox.Visibility == Visibility.Visible)
                password = pwdBox.Password.Trim();
            else
                password = txtVisiblePassword.Text.Trim();


            string encryptedPassword = EncryptionHelper.Encrypt(password);

            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                MyMessageBox.Show("Please enter Usernames of system!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtSystemName.Text))
            {
                MyMessageBox.Show("Please enter System Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MyMessageBox.Show("Please enter System Password!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }
            else
            {
                try
                {
                    db.ExecuteData("update UserAuth set Username='" + txtUsername.Text.ToString() + "', Password= '" + encryptedPassword + "', SystemName='" + txtSystemName.Text.Trim() + "' where Id=" + queryId + " ", useGlobal: true, "User system auth has been updated successfully");
                }
                catch (Exception ex)
                {

                    // التحقق من أن الخطأ بسبب UNIQUE constraint
                    if (ex.Message.Contains("Error 19"))
                    {
                        MyMessageBox.Show($"The system '{txtSystemName.Text.Trim()}' already exists!", "ERROR",
                            MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    }
                    else
                    {
                        // أي خطأ آخر
                        MyMessageBox.Show("Error: " + ex.Message, "ERROR",
                            MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    }
                }
                AutoNumber();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var rs = MyMessageBox.Show("Are you want to delete this Auth?!", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Warning);
            if (rs == MessageBoxResult.Yes)
            {
                
                db.ExecuteData("delete from UserAuth where Id=" + queryId + "", useGlobal: true, "User has been deleted successfully!");
                AutoNumber();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable tblSearch = new DataTable();
            tblSearch.Clear();
            tbl.Clear();
            tblSearch = db.ReadData("select * from UserAuth where SystemName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
            tbl = db.ReadData($@"
                                SELECT 
                                    Id,
                                    SystemName AS 'System Name',
                                    Username AS 'System Username'
                                FROM UserAuth
                                WHERE UserId = {_userId}
                                  AND SystemName LIKE '%{txtSearch.Text.Replace("'", "''")}%'
                            ", useGlobal: true, "");
            try
            {
                if (txtSearch.Text == "")
                {
                    MyMessageBox.Show("Please enter System Name what you want to search for", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    txtSearch.Clear();
                    AutoNumber();
                    return;
                }
                if (tblSearch.Rows.Count >= 1 && tbl.Rows.Count >= 1)
                {
                    queryId = Convert.ToInt32(tblSearch.Rows[0]["Id"]);
                    txtUsername.Text = tblSearch.Rows[0]["Username"].ToString();

                    txtSystemName.Text = tblSearch.Rows[0]["SystemName"].ToString();

                    string encryptedPassword = tblSearch.Rows[0]["Password"].ToString();
                    string realPassword = EncryptionHelper.Decrypt(encryptedPassword);

                    pwdBox.Password = realPassword;
                    txtVisiblePassword.Text = realPassword;

                    
                    dgSystems.ItemsSource = tbl.DefaultView;

                }
                else
                {
                    MyMessageBox.Show("System Name not found.", "ERROR", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    txtSearch.Clear();
                    AutoNumber();
                    return;
                }

            }
            catch (Exception)
            {

            }



            btnAdd.IsEnabled = false;
            btnUpdate.IsEnabled = true;
            btnDelete.IsEnabled = true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
            {
                AutoNumber();

                _isInitialized = true; // flag يمنع التحميل التاني
            }
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgSystems.SelectedItem == null) return;

                // Get the data of selected row
                DataRowView row = dgSystems.SelectedItem as DataRowView;
                if (row == null) return;

                //First column in DG is Id
                int selectedId = Convert.ToInt32(row["Id"]);

                DataTable tblShow = new DataTable();
                tblShow.Clear();
                tblShow = db.ReadData("SELECT * FROM UserAuth WHERE Id = " + selectedId, useGlobal: true, "");

                if (tblShow.Rows.Count > 0)
                {
                    queryId = Convert.ToInt32(tblShow.Rows[0]["Id"]);
                    txtUsername.Text = tblShow.Rows[0]["Username"].ToString();
                    txtSystemName.Text = tblShow.Rows[0]["SystemName"].ToString();

                    string encryptedPassword = tblShow.Rows[0]["Password"].ToString();
                    string realPassword = EncryptionHelper.Decrypt(encryptedPassword);

                    pwdBox.Password = realPassword;
                    txtVisiblePassword.Text = realPassword;

                    btnAdd.IsEnabled = false;
                    btnUpdate.IsEnabled = true;
                    btnDelete.IsEnabled = true;
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            _helper.SearchKeyPress(e, txtSearch, btnSearch, "Please enter System Name what you want to search for");
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
                pwdBox.Visibility = Visibility.Visible;
                txtVisiblePassword.Visibility = Visibility.Collapsed;

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
    }
}
