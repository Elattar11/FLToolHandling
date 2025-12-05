using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.Core;
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
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using FirstLineTool.Helper;

namespace FirstLineTool.View.Database_Pages
{
    /// <summary>
    /// Interaction logic for DatabaseConnections.xaml
    /// </summary>
    public partial class DatabaseConnections : Window
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        private int queryId;
        HelperMethods _helper = new HelperMethods();


        public DatabaseConnections()
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void AutoNumber()
        {
            tbl.Clear();

            //Get all connections from database
            tbl = db.ReadData(@"
                    SELECT Id, 
                        ConnectionName AS 'Connection Name', 
                        HostName AS 'Host Name', 
                        ConnectionPort AS 'Connection Port', 
                        ServiceName AS 'Service Name'
                        FROM DatabaseConnections",
                        useGlobal: true,
                    "");
            dgConnections.ItemsSource = tbl.DefaultView;


            txtConnectionName.Clear();
            txtConnectionPort.Clear();

            txtHostName.Clear();
            txtServiceName.Clear();
            txtSearch.Clear();

            btnAdd.IsEnabled = true;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AutoNumber();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtConnectionName.Text == "")
            {
                MyMessageBox.Show("Please enter Connection Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }


            if (txtHostName.Text == "")
            {
                MyMessageBox.Show("Please enter Host Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (txtConnectionPort.Text == "")
            {
                MyMessageBox.Show("Please enter Connection Port!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (txtServiceName.Text == "")
            {
                MyMessageBox.Show("Please enter Service Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            else
            {
                
                

                try
                {
                    db.ExecuteData(
                                    "insert into DatabaseConnections (ConnectionName, HostName, ConnectionPort, ServiceName) " +
                                    "values ('" + txtConnectionName.Text + "', '" + txtHostName.Text + "', '" + txtConnectionPort.Text + "', '" + txtServiceName.Text + "')",
                                    useGlobal: true,
                                    "New connection Added successfully"
                                );
                }
                catch (Exception ex)
                {
                    // التحقق من أن الخطأ بسبب UNIQUE constraint
                    if (ex.Message.Contains("Error 19"))
                    {
                        MyMessageBox.Show($"The Connection '{txtConnectionName.Text.Trim()}' already exists!", "ERROR",
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

        private void dgConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgConnections.SelectedItem == null) return;

                // Get the data of selected row
                DataRowView row = dgConnections.SelectedItem as DataRowView;
                if (row == null) return;

                //First column in DG is Id
                int selectedId = Convert.ToInt32(row["Id"]);

                DataTable tblShow = new DataTable();
                tblShow.Clear();
                tblShow = db.ReadData("SELECT * FROM DatabaseConnections WHERE Id = " + selectedId, useGlobal:true, "");

                if (tblShow.Rows.Count > 0)
                {
                    queryId = Convert.ToInt32(tblShow.Rows[0]["Id"]);
                    txtConnectionName.Text = tblShow.Rows[0]["ConnectionName"].ToString();
                    txtHostName.Text = tblShow.Rows[0]["HostName"].ToString();
                    txtConnectionPort.Text = tblShow.Rows[0]["ConnectionPort"].ToString();
                    txtServiceName.Text = tblShow.Rows[0]["ServiceName"].ToString();

                    

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
            if (txtConnectionName.Text == "")
            {
                MyMessageBox.Show("Please enter Connection Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }


            if (txtHostName.Text == "")
            {
                MyMessageBox.Show("Please enter Host Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (txtConnectionPort.Text == "")
            {
                MyMessageBox.Show("Please enter Connection Port!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (txtServiceName.Text == "")
            {
                MyMessageBox.Show("Please enter Service Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }
            else
            {
                var rs = MyMessageBox.Show("Are you want to update this Connection?", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Question);
                if (rs == MessageBoxResult.Yes)
                {


                    

                    try
                    {
                        db.ExecuteData("update DatabaseConnections set ConnectionName='" + txtConnectionName.Text.ToString() + "', HostName='" + txtHostName.Text.ToString() + "', ConnectionPort='" + txtConnectionPort.Text.ToString() + "', ServiceName='" + txtServiceName.Text.ToString() + "' where Id=" + queryId + " ", useGlobal:true, "Connection has been updated successfully");

                    }
                    catch (Exception ex)
                    {
                        // التحقق من أن الخطأ بسبب UNIQUE constraint
                        if (ex.Message.Contains("Error 19"))
                        {
                            MyMessageBox.Show($"The Connection '{txtConnectionName.Text.Trim()}' already exists!", "ERROR",
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

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            AutoNumber();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var rs = MyMessageBox.Show("Are you want to delete this Connection?!", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Warning);
            if (rs == MessageBoxResult.Yes)
            {
                db.ExecuteData("delete from DatabaseConnections where Id=" + queryId + "", useGlobal: true, "Connection has been deleted successfully!");
                AutoNumber();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable tblSearch = new DataTable();
            tblSearch.Clear();
            tbl.Clear();
            tblSearch = db.ReadData("select * from DatabaseConnections where ConnectionName like '%" + txtSearch.Text.ToString() + "%'", useGlobal:true, "");
            tbl = db.ReadData("select Id , ConnectionName AS 'Connection Name' , HostName AS 'Host Name' , ConnectionPort AS 'Connection Port' , ServiceName AS 'Service Name' from DatabaseConnections where ConnectionName like '%" + txtSearch.Text.ToString() + "%'", useGlobal:true, "");
            try
            {
                if (txtSearch.Text == "")
                {
                    MyMessageBox.Show("Please enter connection name what you want to search for", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    txtSearch.Clear();
                    AutoNumber();
                    return;
                }
                if (tblSearch.Rows.Count >= 1 && tbl.Rows.Count >= 1)
                {
                    queryId = Convert.ToInt32(tblSearch.Rows[0]["Id"]);
                    txtConnectionName.Text = tblSearch.Rows[0]["ConnectionName"].ToString();
                    txtHostName.Text = tblSearch.Rows[0]["HostName"].ToString();
                    txtConnectionPort.Text = tblSearch.Rows[0]["ConnectionPort"].ToString();
                    txtServiceName.Text = tblSearch.Rows[0]["ServiceName"].ToString();

                    dgConnections.ItemsSource = tbl.DefaultView;
                    
                }
                else
                {
                    MyMessageBox.Show("There is no connection with this name.", "ERROR", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
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
            _helper.SearchKeyPress(e, txtSearch, btnSearch, "Please enter connection name what you want to search for");
        }
    }
}
