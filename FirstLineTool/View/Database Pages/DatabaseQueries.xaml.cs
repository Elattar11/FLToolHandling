using FirstLineTool.Core;
using FirstLineTool.Core.TypesAndPaths;
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
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using FirstLineTool.View.Alert;
using System.Data.Common;

namespace FirstLineTool.View.Database_Pages
{
    /// <summary>
    /// Interaction logic for DatabaseQueries.xaml
    /// </summary>
    public partial class DatabaseQueries : Window
    {

        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        HelperMethods _helper = new HelperMethods();
        private int queryId;
        public DatabaseQueries()
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

        //List returned connections for dynamic search
        private List<dynamic> allConnections = new List<dynamic>();

        private void LoadConnections()
        {
            var options = new ComboBoxLoadOptions
            {
                TargetComboBox = cbxConnections,   
                TableName = "DatabaseConnections",
                DisplayMember = "ConnectionName",
                ValueMember = "Id",
                Db = db,
                UseGlobal = true,
            };

            ComboBoxHelper.LoadComboBox(options);

           
            allConnections = cbxConnections.ItemsSource.Cast<dynamic>().ToList();

            if (cbxConnections.Items.Count > 0)
                cbxConnections.SelectedIndex = 0;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConnections();
            AutoNumber();
        }


        private void AutoNumber()
        {
            tbl.Clear();
            tbl = db.ReadData(@"
                    SELECT 
                        DQ.Id,
                        DQ.QueryName as 'Query Name',
                        DQ.QueryText as 'Query Text',
                        DC.ConnectionName as 'Connection Name'
                        FROM DatabaseQueries DQ JOIN DatabaseConnections DC
                        ON DQ.DatabaseConnectionId = DC.Id",
                        useGlobal: true,
                    "");
            dgQueries.ItemsSource = tbl.DefaultView;


            txtQueryName.Clear();
            txtQueryText.Clear();
            cbxConnections.SelectedIndex = -1;
            txtSearch.Clear();

            btnAdd.IsEnabled = true;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtQueryName.Text))
            {
                MyMessageBox.Show("Please enter Query Name!", "WARNING",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (cbxConnections.SelectedIndex < 0)
            {
                MyMessageBox.Show("Please select Database Connection!", "WARNING",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtQueryText.Text))
            {
                MyMessageBox.Show("Please Enter Query Text!", "WARNING",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            var parameters = new Dictionary<string, object>()
            {
                { "@QueryName", txtQueryName.Text },
                { "@QueryText", txtQueryText.Text },
                { "@ConnectionId", cbxConnections.SelectedValue }
            };

            db.ExecuteDataParameterized(
                "INSERT INTO DatabaseQueries (QueryName, QueryText, DatabaseConnectionId) " +
                "VALUES (@QueryName, @QueryText, @ConnectionId)",
                parameters,
                useGlobal: true,
                "New Query Added successfully"
            );

            AutoNumber();
        }

        private void dgQueries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgQueries.SelectedItem == null) return;

                // Get the data of selected row
                DataRowView row = dgQueries.SelectedItem as DataRowView;
                if (row == null) return;

                //First column in DG is Id
                int selectedId = Convert.ToInt32(row["Id"]);

                DataTable tblShow = new DataTable();
                tblShow.Clear();
                tblShow = db.ReadData("SELECT * FROM DatabaseQueries WHERE Id = " + selectedId, useGlobal: true,"");

                if (tblShow.Rows.Count > 0)
                {
                    queryId = Convert.ToInt32(tblShow.Rows[0]["Id"]);
                    txtQueryName.Text = tblShow.Rows[0]["QueryName"].ToString();
                    txtQueryText.Text = tblShow.Rows[0]["QueryText"].ToString();

                    cbxConnections.SelectedValue = tblShow.Rows[0]["DatabaseConnectionId"];

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
            if (string.IsNullOrWhiteSpace(txtQueryName.Text))
            {
                MyMessageBox.Show("Please enter Query Name!", "WARNING",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (cbxConnections.SelectedIndex < 0)
            {
                MyMessageBox.Show("Please select Database Connection!", "WARNING",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtQueryText.Text))
            {
                MyMessageBox.Show("Please Enter Query Text!", "WARNING",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            var rs = MyMessageBox.Show("Are you want to update this Query?", "ATTENTION",
                MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Question);

            if (rs == MessageBoxResult.Yes)
            {
                var parameters = new Dictionary<string, object>()
                {
                    { "@QueryName", txtQueryName.Text },
                    { "@QueryText", txtQueryText.Text },
                    { "@ConnectionId", cbxConnections.SelectedValue },
                    { "@Id", queryId }
                };

                db.ExecuteDataParameterized(
                    "UPDATE DatabaseQueries SET QueryName = @QueryName, QueryText = @QueryText, DatabaseConnectionId = @ConnectionId WHERE Id = @Id",
                    parameters,
                    useGlobal: true,
                    "Query has been updated successfully"
                );

                AutoNumber();
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            AutoNumber();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var rs = MyMessageBox.Show("Are you want to delete this Query?!", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Warning);
            if (rs == MessageBoxResult.Yes)
            {
                db.ExecuteData("delete from DatabaseQueries where Id=" + queryId + "", useGlobal: true, "Query has been deleted successfully!");
                AutoNumber();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable tblSearch = new DataTable();
            tblSearch.Clear();
            tbl.Clear();
            tblSearch = db.ReadData("select * from DatabaseQueries where QueryName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
            tbl = db.ReadData(@"
                                SELECT 
                                    DQ.Id,
                                    DQ.QueryName as 'Query Name',
                                    DQ.QueryText as 'Query Text',
                                    DC.ConnectionName as 'Connection Name'
                                    FROM DatabaseQueries DQ JOIN DatabaseConnections DC
                                    ON DQ.DatabaseConnectionId = DC.Id
                                WHERE DQ.QueryName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true,"");
            try
            {
                if (txtSearch.Text == "")
                {
                    MyMessageBox.Show("Please enter query name what you want to search for", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    txtSearch.Clear();
                    AutoNumber();
                    return;
                }
                if (tblSearch.Rows.Count >= 1 && tbl.Rows.Count >= 1)
                {
                    queryId = Convert.ToInt32(tblSearch.Rows[0]["Id"]);
                    txtQueryName.Text = tblSearch.Rows[0]["QueryName"].ToString();
                    txtQueryText.Text = tblSearch.Rows[0]["QueryText"].ToString();
                    cbxConnections.SelectedValue = tblSearch.Rows[0]["DatabaseConnectionId"];

                    dgQueries.ItemsSource = tbl.DefaultView;

                }
                else
                {
                    MyMessageBox.Show("There is no query with this name.", "ERROR", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
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


        private void txtSearch_KeyDown_1(object sender, KeyEventArgs e)
        {
            _helper.SearchKeyPress(e, txtSearch, btnSearch, "Please enter query name what you want to search for");
        }
    }
}
