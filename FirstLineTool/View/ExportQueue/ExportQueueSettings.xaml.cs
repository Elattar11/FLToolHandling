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

namespace FirstLineTool.View.ExportQueue
{
    /// <summary>
    /// Interaction logic for ExportQueueSettings.xaml
    /// </summary>
    public partial class ExportQueueSettings : Window
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        private int queryId;
        HelperMethods _helper = new HelperMethods();

        public ExportQueueSettings()
        {
            InitializeComponent();
        }

        private void AutoNumber()
        {
            tbl.Clear();

            //Get all connections from database
            tbl = db.ReadData(@"
                    SELECT Id,
                        QueueName AS 'Queue Name'
                        FROM ExportQueue",
                        useGlobal: true,
                    "");
            dgQueues.ItemsSource = tbl.DefaultView;


            txtQueryName.Clear();
            

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

            

            var parameters = new Dictionary<string, object>()
            {
                { "@QueryName", txtQueryName.Text },
                
            };

            db.ExecuteDataParameterized(
                "INSERT INTO ExportQueue (QueueName) " +
                "VALUES (@QueryName)",
                parameters,
                useGlobal: true,
                "New Queue Added successfully"
            );

            AutoNumber();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtQueryName.Text))
            {
                MyMessageBox.Show("Please enter Query Name!", "WARNING",
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
                    
                    { "@Id", queryId }
                };

                db.ExecuteDataParameterized(
                    "UPDATE ExportQueue SET QueueName = @QueryName WHERE Id = @Id",
                    parameters,
                    useGlobal: true,
                    "Queue has been updated successfully"
                );

                AutoNumber();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var rs = MyMessageBox.Show("Are you want to delete this Query?!", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Warning);
            if (rs == MessageBoxResult.Yes)
            {
                db.ReadData("delete from ExportQueue where Id=" + queryId + "", useGlobal: true, "Queue has been deleted successfully!");
                AutoNumber();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable tblSearch = new DataTable();
            tblSearch.Clear();
            tbl.Clear();
            tblSearch = db.ReadData("select * from ExportQueue where QueueName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
            tbl = db.ReadData(@"
                                SELECT Id,
                                    QueueName AS 'Query Name'
                                    FROM ExportQueue
                                WHERE QueueName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
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
                    txtQueryName.Text = tblSearch.Rows[0]["QueueName"].ToString();
                    
                    dgQueues.ItemsSource = tbl.DefaultView;

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

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            AutoNumber();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dgQueues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgQueues.SelectedItem == null) return;

                // Get the data of selected row
                DataRowView row = dgQueues.SelectedItem as DataRowView;
                if (row == null) return;

                //First column in DG is Id
                int selectedId = Convert.ToInt32(row["Id"]);

                DataTable tblShow = new DataTable();
                tblShow.Clear();
                tblShow = db.ReadData("SELECT * FROM ExportQueue WHERE Id = " + selectedId, useGlobal: true, "");

                if (tblShow.Rows.Count > 0)
                {
                    queryId = Convert.ToInt32(tblShow.Rows[0]["Id"]);
                    txtQueryName.Text = tblShow.Rows[0]["QueueName"].ToString();
                    

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
            _helper.SearchKeyPress(e, txtSearch, btnSearch, "Please enter query name what you want to search for");
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
            AutoNumber();
        }
    }
}
