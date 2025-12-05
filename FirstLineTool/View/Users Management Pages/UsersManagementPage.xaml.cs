using FirstLineTool.Core;
using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.Helper;
using FirstLineTool.View.Alert;
using FirstLineTool.View.Server_Pages;
using Microsoft.Data.Sqlite;
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

namespace FirstLineTool.View.Users_Management_Pages
{
    /// <summary>
    /// Interaction logic for UsersManagementPage.xaml
    /// </summary>
    public partial class UsersManagementPage : Page
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        HelperMethods _helper = new HelperMethods();
        private int queryId;
        private string _userId;

        private bool _isInitialized = false;

        public UsersManagementPage(string userId)
        {
            InitializeComponent();

            _userId = userId;
        }

        //List returned connections for dynamic search
        private List<dynamic> allTeams = new List<dynamic>();

        private void LoadConnections()
        {
            var options = new ComboBoxLoadOptions
            {
                TargetComboBox = cbxTeams,
                TableName = "Teams",
                DisplayMember = "TeamName",
                ValueMember = "Id",
                Db = db,
                UseGlobal = true,
            };

            ComboBoxHelper.LoadComboBox(options);


            allTeams = cbxTeams.ItemsSource.Cast<dynamic>().ToList();

            if (cbxTeams.Items.Count > 0)
                cbxTeams.SelectedIndex = 0;

        }

        private void AutoNumber()
        {
            tbl.Clear();
            tbl = db.ReadData(@"
                    SELECT 
                        US.Id,
                        US.Username,
                        US.Role,
                        TE.TeamName
                        FROM Users US JOIN Teams TE
                        ON US.TeamId = TE.Id", useGlobal: true,
                    "");
            dgUsers.ItemsSource = tbl.DefaultView;


            txtUsers.Clear();
            txtSearch.Clear();

            cbxTeams.SelectedIndex = 0;
            cbxRoles.SelectedIndex = 0;

            btnAdd.IsEnabled = true;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnReset.IsEnabled = false;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
            {
                LoadConnections();
                AutoNumber();

                _isInitialized = true; // flag يمنع التحميل التاني
            }
        }

        


        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgUsers.SelectedItem == null) return;

                // Get the data of selected row
                DataRowView row = dgUsers.SelectedItem as DataRowView;
                if (row == null) return;

                //First column in DG is Id
                int selectedId = Convert.ToInt32(row["Id"]);

                DataTable tblShow = new DataTable();
                tblShow.Clear();
                tblShow = db.ReadData("SELECT * FROM Users WHERE Id = " + selectedId, useGlobal: true, "");

                if (tblShow.Rows.Count > 0)
                {
                    queryId = Convert.ToInt32(tblShow.Rows[0]["Id"]);
                    txtUsers.Text = tblShow.Rows[0]["Username"].ToString();
                    cbxRoles.Text = tblShow.Rows[0]["Role"].ToString();

                    cbxTeams.SelectedValue = tblShow.Rows[0]["TeamId"];

                    btnAdd.IsEnabled = false;
                    btnUpdate.IsEnabled = true;
                    btnDelete.IsEnabled = true;
                    btnReset.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var rs = MyMessageBox.Show("Are you want to delete this User?!", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Warning);
            if (rs == MessageBoxResult.Yes)
            {
                if (queryId == int.Parse(_userId)) // _userId هو المستخدم الحالي
                {
                    MyMessageBox.Show("You cannot delete the currently logged-in user!", "WARNING",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    return;
                }
                db.ReadData("delete from Users where Id=" + queryId + "", useGlobal: true, "User has been deleted successfully!");
                AutoNumber();
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // التحقق من أن المستخدمين موجودين
            if (string.IsNullOrWhiteSpace(txtUsers.Text))
            {
                MyMessageBox.Show("Please enter Usernames!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            // التحقق من اختيار الفريق
            if (cbxTeams.SelectedIndex < 0)
            {
                MyMessageBox.Show("Please select Team first!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            // التحقق من اختيار الدور
            if (cbxRoles.SelectedIndex < 0)
            {
                MyMessageBox.Show("Please select Role first!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }
            else
            {
                var rs = MyMessageBox.Show("Are you want to update this User?", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Question);
                if (rs == MessageBoxResult.Yes)
                {


                    

                    try
                    {
                        db.ExecuteData("update Users set Username='" + txtUsers.Text.ToString() + "', Role= '" + cbxRoles.Text + "', TeamId=" + cbxTeams.SelectedValue + " where Id=" + queryId + " ", useGlobal: true, "User has been updated successfully");

                    }
                    catch (Exception ex)
                    {
                        // التحقق من أن الخطأ بسبب UNIQUE constraint
                        if (ex.Message.Contains("Error 19"))
                        {
                            MyMessageBox.Show($"The username '{txtUsers.Text.Trim()}' already exists!", "ERROR",
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
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // التحقق من أن المستخدمين موجودين
            if (string.IsNullOrWhiteSpace(txtUsers.Text))
            {
                MyMessageBox.Show("Please enter Usernames!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            // التحقق من اختيار الفريق
            if (cbxTeams.SelectedIndex < 0)
            {
                MyMessageBox.Show("Please select Team first!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            // التحقق من اختيار الدور
            if (cbxRoles.SelectedIndex < 0)
            {
                MyMessageBox.Show("Please select Role first!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            // تقسيم النص إلى أسطر والتعامل مع كل username على حدة
            string[] usernames = txtUsers.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string username in usernames)
            {
                string trimmedUsername = username.Trim();
                if (!string.IsNullOrEmpty(trimmedUsername))
                {
                    try
                    {
                        string salt = PasswordHelper.GenerateSalt();
                        string passwordHash = PasswordHelper.HashPassword("0000", salt);

                        db.ExecuteData(
                            "INSERT INTO Users (Username, Password, Role, TeamId, Salt) " +
                            "VALUES ('" + trimmedUsername + "', '" + passwordHash + "', '" + cbxRoles.Text + "', " + cbxTeams.SelectedValue + ", '" + salt + "')",
                            useGlobal: true,
                            "New User '" + trimmedUsername + "' added successfully"
                        );
                    }
                    catch (Exception ex)
                    {
                        // التحقق من أن الخطأ بسبب UNIQUE constraint
                        if (ex.Message.Contains("Error 19"))
                        {
                            MyMessageBox.Show($"The username '{trimmedUsername}' already exists!", "ERROR",
                                MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                        }
                        else
                        {
                            // أي خطأ آخر
                            MyMessageBox.Show("Error: " + ex.Message, "ERROR",
                                MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                        }
                    }
                }
            }

            // إعادة ضبط الأرقام أو أي عمليات أخرى بعد الإدخال
            AutoNumber();

        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            _helper.SearchKeyPress(e, txtSearch, btnSearch, "Please enter Username what you want to search for");
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable tblSearch = new DataTable();
            tblSearch.Clear();
            tbl.Clear();
            tblSearch = db.ReadData("select * from Users where Username like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
            tbl = db.ReadData(@"
                                SELECT 
                                    US.Id,
                                    US.Username,
                                    US.Role,
                                    TE.TeamName
                                    FROM Users US JOIN Teams TE
                                    ON US.TeamId = TE.Id
                                WHERE US.Username like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
            try
            {
                if (txtSearch.Text == "")
                {
                    MyMessageBox.Show("Please enter Username what you want to search for", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    txtSearch.Clear();
                    AutoNumber();
                    return;
                }
                if (tblSearch.Rows.Count >= 1 && tbl.Rows.Count >= 1)
                {
                    queryId = Convert.ToInt32(tblSearch.Rows[0]["Id"]);
                    txtUsers.Text = tblSearch.Rows[0]["Username"].ToString();
                    
                    cbxTeams.SelectedValue = tblSearch.Rows[0]["TeamId"];
                    cbxRoles.Text = tblSearch.Rows[0]["Role"].ToString();
                    dgUsers.ItemsSource = tbl.DefaultView;

                }
                else
                {
                    MyMessageBox.Show("Username not found.", "ERROR", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
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

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (queryId == 0)
            {
                MyMessageBox.Show("Please select a user to reset password.", "WARNING",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            var rs = MyMessageBox.Show("Are you sure you want to reset this user's password to default (0000)?",
                "CONFIRMATION",
                MyMessageBox.MyMessageBoxButtons.YesNo,
                MyMessageBox.MyMessageBoxIcon.Warning);

            if (rs == MessageBoxResult.Yes)
            {
                try
                {
                    if (queryId == int.Parse(_userId)) // _userId هو المستخدم الحالي
                    {
                        MyMessageBox.Show("You cannot reset the currently logged-in user!", "WARNING",
                            MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                        return;
                    }
                    // إنشاء salt جديد
                    string salt = PasswordHelper.GenerateSalt();

                    // عمل hash للـ default password
                    string passwordHash = PasswordHelper.HashPassword("0000", salt);

                    // تحديث البيانات في الجدول
                    string updateQuery = $"UPDATE Users SET Password = '{passwordHash}', Salt = '{salt}' WHERE Id = {queryId}";
                    db.ExecuteData(updateQuery, useGlobal: true, "User password has been reset successfully!");
                }
                catch (Exception ex)
                {
                    MyMessageBox.Show("Error: " + ex.Message, "ERROR",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                }
            }
        }

        private void btnTeams_Click_1(object sender, RoutedEventArgs e)
        {
            _helper.OpenWindow<TeamsManagementPage>();
        }
    }
    
}
