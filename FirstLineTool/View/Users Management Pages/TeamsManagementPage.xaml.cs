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
using System.Windows.Shapes;
using FirstLineTool.View.Alert;
using System.Data.Common;

namespace FirstLineTool.View.Users_Management_Pages
{
    /// <summary>
    /// Interaction logic for TeamsManagementPage.xaml
    /// </summary>
    public partial class TeamsManagementPage : Window
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        private int queryId;
        HelperMethods _helper = new HelperMethods();
        public TeamsManagementPage()
        {
            InitializeComponent();
        }

        private void AutoNumber()
        {
            tbl.Clear();

            //Get all connections from database
            tbl = db.ReadData(@"
                    SELECT Id, TeamName as 'Team Name'
                        FROM Teams", useGlobal: true,
                    "");
            dgTeams.ItemsSource = tbl.DefaultView;


            txtTeamName.Clear();
            txtSearch.Clear();
            


            btnAdd.IsEnabled = true;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AutoNumber();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            AutoNumber();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtTeamName.Text == "")
            {
                MyMessageBox.Show("Please enter Team Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            else
            {
                
                


                try
                {
                    db.ExecuteData(
                                    "insert into Teams (TeamName) " +
                                    "values ('" + txtTeamName.Text + "')",
                                    useGlobal: true,
                                    "New Team Added successfully"
                                );
                }
                catch (Exception ex)
                {
                    // التحقق من أن الخطأ بسبب UNIQUE constraint
                    if (ex.Message.Contains("Error 19"))
                    {
                        MyMessageBox.Show($"The Team '{txtTeamName.Text.Trim()}' already exists!", "ERROR",
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

        private void dgTeams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgTeams.SelectedItem == null) return;

                // Get the data of selected row
                DataRowView row = dgTeams.SelectedItem as DataRowView;
                if (row == null) return;

                //First column in DG is Id
                int selectedId = Convert.ToInt32(row["Id"]);

                DataTable tblShow = new DataTable();
                tblShow.Clear();
                tblShow = db.ReadData("SELECT * FROM Teams WHERE Id = " + selectedId, useGlobal: true, "");

                if (tblShow.Rows.Count > 0)
                {
                    queryId = Convert.ToInt32(tblShow.Rows[0]["Id"]);
                    txtTeamName.Text = tblShow.Rows[0]["TeamName"].ToString();



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

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (txtTeamName.Text == "")
            {
                MyMessageBox.Show("Please enter Team Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }
            else
            {
                var rs = MyMessageBox.Show("Are you want to update this Team?", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Question);
                if (rs == MessageBoxResult.Yes)
                {


                    


                    try
                    {
                        db.ExecuteData("update Teams set TeamName='" + txtTeamName.Text.ToString() + "' where Id=" + queryId + " ", useGlobal: true, "Team name has been updated successfully");

                    }
                    catch (Exception ex)
                    {
                        // التحقق من أن الخطأ بسبب UNIQUE constraint
                        if (ex.Message.Contains("Error 19"))
                        {
                            MyMessageBox.Show($"The Team '{txtTeamName.Text.Trim()}' already exists!", "ERROR",
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

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var rs = MyMessageBox.Show("Are you want to delete this Team?!", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Warning);
            if (rs == MessageBoxResult.Yes)
            {
                db.ReadData("delete from Teams where Id=" + queryId + "", useGlobal: true, "Team has been deleted successfully!");
                AutoNumber();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable tblSearch = new DataTable();
            tblSearch.Clear();
            tbl.Clear();
            tblSearch = db.ReadData("select * from Teams where TeamName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
            tbl = db.ReadData("SELECT Id, TeamName as 'Team Name' FROM Teams where TeamName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
            try
            {
                if (txtSearch.Text == "")
                {
                    MyMessageBox.Show("Please enter Team name what you want to search for", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    txtSearch.Clear();
                    AutoNumber();
                    return;
                }
                if (tblSearch.Rows.Count >= 1 && tbl.Rows.Count >= 1)
                {
                    queryId = Convert.ToInt32(tblSearch.Rows[0]["Id"]);
                    txtTeamName.Text = tblSearch.Rows[0]["TeamName"].ToString();
                    

                    dgTeams.ItemsSource = tbl.DefaultView;

                }
                else
                {
                    MyMessageBox.Show("There is no Team with this name.", "ERROR", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
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

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            _helper.SearchKeyPress(e, txtSearch, btnSearch, "Please enter Team name what you want to search for");
        }
    }
}
