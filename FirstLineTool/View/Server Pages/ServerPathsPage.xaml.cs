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
using System.Data.Common;
using FirstLineTool.View.Alert;

namespace FirstLineTool.View.Server_Pages
{
    /// <summary>
    /// Interaction logic for ServerPathsPage.xaml
    /// </summary>
    public partial class ServerPathsPage : Window
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        private int queryId;
        HelperMethods _helper = new HelperMethods();

        public ServerPathsPage()
        {
            InitializeComponent();
        }

        private void AutoNumber()
        {
            tbl.Clear();

            //Get all connections from database
            tbl = db.ReadData(@"
                    SELECT Id, 
                        PathName AS 'Path Name', 
                        LinuxPath AS 'Linux Path', 
                        CommandTemplate AS 'Command Template'
                        FROM ServerPaths", useGlobal: true,
                    "");
            dgPaths.ItemsSource = tbl.DefaultView;


            txtPathName.Clear();
            txtLinuxPath.Clear();
            txtCommandTemplate.Clear();
            txtSearch.Clear();

            btnAdd.IsEnabled = true;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
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

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtPathName.Text == "")
            {
                MyMessageBox.Show("Please enter Path Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }


            if (txtLinuxPath.Text == "")
            {
                MyMessageBox.Show("Please enter Linux Path!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (txtCommandTemplate.Text == "")
            {
                MyMessageBox.Show("Please enter Command Template!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            else
            {
                
                

                try
                {
                    db.ExecuteData(
                                    "insert into ServerPaths (PathName, LinuxPath, CommandTemplate) " +
                                    "values ('" + txtPathName.Text + "', '" + txtLinuxPath.Text + "', '" + txtCommandTemplate.Text + "')",
                                    useGlobal: true,
                                    "New Path Added successfully"
                                );
                }
                catch (Exception ex)
                {
                    // التحقق من أن الخطأ بسبب UNIQUE constraint
                    if (ex.Message.Contains("Error 19"))
                    {
                        MyMessageBox.Show($"There is a Unique data! Path Name, Linux Path or Command Template, check the inserted data and try again.", "ERROR",
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
            if (txtPathName.Text == "")
            {
                MyMessageBox.Show("Please enter Path Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }


            if (txtLinuxPath.Text == "")
            {
                MyMessageBox.Show("Please enter Linux Path!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (txtCommandTemplate.Text == "")
            {
                MyMessageBox.Show("Please enter Command Template!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }
            else
            {
                var rs = MyMessageBox.Show("Are you want to update this Path?", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Question);
                if (rs == MessageBoxResult.Yes)
                {


                    


                    try
                    {
                        db.ExecuteData("update ServerPaths set PathName='" + txtPathName.Text.ToString() + "', LinuxPath='" + txtLinuxPath.Text.ToString() + "', CommandTemplate='" + txtCommandTemplate.Text.ToString() + "' where Id=" + queryId + " ", useGlobal: true, "Path has been updated successfully");

                    }
                    catch (Exception ex)
                    {
                        // التحقق من أن الخطأ بسبب UNIQUE constraint
                        if (ex.Message.Contains("Error 19"))
                        {
                            MyMessageBox.Show($"There is a Unique data! Path Name, Linux Path or Command Template, check the inserted data and try again.", "ERROR",
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
            var rs = MyMessageBox.Show("Are you want to delete this Path?!", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Warning);
            if (rs == MessageBoxResult.Yes)
            {
                db.ReadData("delete from ServerPaths where Id=" + queryId + "", useGlobal: true, "Path has been deleted successfully!");
                AutoNumber();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable tblSearch = new DataTable();
            tblSearch.Clear();
            tbl.Clear();
            tblSearch = db.ReadData("select * from ServerPaths where PathName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
            tbl = db.ReadData("select Id , PathName AS 'Path Name' , LinuxPath AS 'Linux Path' , CommandTemplate AS 'Command Template' from ServerPaths where PathName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
            try
            {
                if (txtSearch.Text == "")
                {
                    MyMessageBox.Show("Please enter Path name what you want to search for", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    txtSearch.Clear();
                    AutoNumber();
                    return;
                }
                if (tblSearch.Rows.Count >= 1 && tbl.Rows.Count >= 1)
                {
                    queryId = Convert.ToInt32(tblSearch.Rows[0]["Id"]);
                    txtPathName.Text = tblSearch.Rows[0]["PathName"].ToString();
                    txtLinuxPath.Text = tblSearch.Rows[0]["LinuxPath"].ToString();
                    txtCommandTemplate.Text = tblSearch.Rows[0]["CommandTemplate"].ToString();

                    dgPaths.ItemsSource = tbl.DefaultView;

                }
                else
                {
                    MyMessageBox.Show("There is no Path with this name.", "ERROR", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
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
            _helper.SearchKeyPress(e, txtSearch, btnSearch, "Please enter Path name what you want to search for");

        }



        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            AutoNumber();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AutoNumber();
        }

        private void dgPaths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgPaths.SelectedItem == null) return;

                // Get the data of selected row
                DataRowView row = dgPaths.SelectedItem as DataRowView;
                if (row == null) return;

                //First column in DG is Id
                int selectedId = Convert.ToInt32(row["Id"]);

                DataTable tblShow = new DataTable();
                tblShow.Clear();
                tblShow = db.ReadData("SELECT * FROM ServerPaths WHERE Id = " + selectedId, useGlobal: true, "");

                if (tblShow.Rows.Count > 0)
                {
                    queryId = Convert.ToInt32(tblShow.Rows[0]["Id"]);
                    txtPathName.Text = tblShow.Rows[0]["PathName"].ToString();
                    txtLinuxPath.Text = tblShow.Rows[0]["LinuxPath"].ToString();
                    txtCommandTemplate.Text = tblShow.Rows[0]["CommandTemplate"].ToString();
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
    }
}
