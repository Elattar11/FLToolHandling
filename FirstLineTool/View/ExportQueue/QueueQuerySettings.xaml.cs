using FirstLineTool.Core;
using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.Helper;
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

namespace FirstLineTool.View.ExportQueue
{
    /// <summary>
    /// Interaction logic for QueueQuerySettings.xaml
    /// </summary>
    public partial class QueueQuerySettings : Window
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        HelperMethods _helper = new HelperMethods();
        private int queryId;

        public QueueQuerySettings()
        {
            InitializeComponent();
        }

        private void LoadConnections()
        {
            var options = new ComboBoxLoadOptions
            {
                TargetComboBox = cbxQueues,
                TableName = "ExportQueue",
                DisplayMember = "QueueName",
                ValueMember = "Id",
                Db = db,
                UseGlobal = true,
            };

            ComboBoxHelper.LoadComboBox(options);


            

            if (cbxQueues.Items.Count > 0)
                cbxQueues.SelectedIndex = 0;

        }

        private void AutoNumber()
        {
            tbl.Clear();
            tbl = db.ReadData(@"
                    SELECT 
                        QQ.Id, QQ.QueryName as 'Query Name', QQ.QueryText as 'Query Text', EQ.QueueName as 'Queue Name'
                        FROM QueueQuires QQ JOIN ExportQueue EQ
                        ON QQ.ExportId = EQ.Id",
                        useGlobal: true,
                    "");
            dgQueries.ItemsSource = tbl.DefaultView;


            txtQueryName.Clear();
            txtQueryText.Clear();
            cbxQueues.SelectedIndex = -1;
            txtSearch.Clear();

            btnAdd.IsEnabled = true;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConnections();
            AutoNumber();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtQueryName.Text))
            {
                MyMessageBox.Show("Please enter Query Name!", "WARNING",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (cbxQueues.SelectedIndex < 0)
            {
                MyMessageBox.Show("Please select Queue!", "WARNING",
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
                { "@ExportQueueId", cbxQueues.SelectedValue }
            };

            db.ExecuteDataParameterized(
                "INSERT INTO QueueQuires (QueryText, QueryName, ExportId) " +
                "VALUES (@QueryText, @QueryName, @ExportQueueId)",
                parameters,
                useGlobal: true,
                "New Query Added successfully"
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

            if (cbxQueues.SelectedIndex < 0)
            {
                MyMessageBox.Show("Please select Queue!", "WARNING",
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
                    { "@ConnectionId", cbxQueues.SelectedValue },
                    { "@Id", queryId }
                };

                db.ExecuteDataParameterized(
                    "UPDATE QueueQuires SET QueryText = @QueryText, QueryName = @QueryName, ExportId = @ConnectionId WHERE Id = @Id",
                    parameters,
                    useGlobal: true,
                    "Query has been updated successfully"
                );

                AutoNumber();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var rs = MyMessageBox.Show("Are you want to delete this Query?!", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Warning);
            if (rs == MessageBoxResult.Yes)
            {
                db.ReadData("delete from QueueQuires where Id=" + queryId + "", useGlobal: true, "Query has been deleted successfully!");
                AutoNumber();
            }
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
                tblShow = db.ReadData("SELECT * FROM QueueQuires WHERE Id = " + selectedId, useGlobal: true, "");

                if (tblShow.Rows.Count > 0)
                {
                    queryId = Convert.ToInt32(tblShow.Rows[0]["Id"]);
                    txtQueryName.Text = tblShow.Rows[0]["QueryName"].ToString();
                    txtQueryText.Text = tblShow.Rows[0]["QueryText"].ToString();

                    cbxQueues.SelectedValue = tblShow.Rows[0]["ExportId"];

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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable tblSearch = new DataTable();
            tblSearch.Clear();
            tbl.Clear();
            tblSearch = db.ReadData("select * from QueueQuires where QueryName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
            tbl = db.ReadData(@"
                                SELECT 
                                    QQ.Id, QQ.QueryName as 'Query Name', QQ.QueryText as 'Query Text', EQ.QueueName as 'Queue Name'
                                    FROM QueueQuires QQ JOIN ExportQueue EQ
                                    ON QQ.ExportId = EQ.Id
                                WHERE DQ.QueryName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
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
                    cbxQueues.SelectedValue = tblSearch.Rows[0]["ExportId"];

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

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            AutoNumber();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
