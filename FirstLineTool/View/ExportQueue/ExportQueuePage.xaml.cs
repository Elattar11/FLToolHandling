using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.Core;
using FirstLineTool.Helper;
using FirstLineTool.View.Alert;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
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
using FirstLineTool.View.Database_Pages;
using ClosedXML.Excel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FirstLineTool.View.ExportQueue
{
    /// <summary>
    /// Interaction logic for ExportQueuePage.xaml
    /// </summary>
    public partial class ExportQueuePage : Page
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Local);
        HelperMethods _helper = new HelperMethods();

        private string _id;
        private string _role;

        private bool _isInitialized = false;
        public ExportQueuePage(string id , string role)
        {

            InitializeComponent();
            _id = id;
            _role = role;

            if (role == "User")
            {
                btnSettings.Visibility = Visibility.Collapsed;
                btnQueries.Visibility = Visibility.Collapsed;
                cbxUsers.Visibility = Visibility.Collapsed;
                btnAssignTo.Visibility = Visibility.Collapsed;

            }
            else if (role == "Admin")
            {
                btnSettings.Visibility = Visibility.Visible;
                btnQueries.Visibility = Visibility.Visible;
                cbxUsers.Visibility = Visibility.Visible;
                btnAssignTo.Visibility = Visibility.Visible;
            }
        }


        //private void LoadFakeData()
        //{
        //    DataTable dt = new DataTable();

        //    // تعريف الأعمدة
        //    dt.Columns.Add("Id");
        //    dt.Columns.Add("desc");
        //    dt.Columns.Add("SRNUM");
        //    dt.Columns.Add("Notes");
        //    dt.Columns.Add("Category");
        //    dt.Columns.Add("CreatedDate");
        //    dt.Columns.Add("Status");
        //    dt.Columns.Add("Owner");

        //    // إضافة بيانات تجريبية
        //    for (int i = 1; i <= 20; i++) // 20 صف لاختبار scroll
        //    {
        //        dt.Rows.Add(
        //            i,
        //            "Item " + i,
        //            "2-10101010 ",
        //            "Notes for item " + i + ":\n- Line 1\n- Line 2\n- Line 3",
        //            "Category " + ((i % 5) + 1),
        //            DateTime.Now.AddDays(-i).ToShortDateString(),
        //            (i % 2 == 0 ? "Active" : "Inactive"),
        //            "Owner " + i
        //        );
        //    }

        //    ResultsDataGrid.ItemsSource = dt.DefaultView;

        //    // إضافة tooltip لكل خلية بحيث يظهر النص بالكامل
        //    foreach (var column in ResultsDataGrid.Columns)
        //    {
        //        column.CellStyle = new Style(typeof(DataGridCell));
        //        column.CellStyle.Setters.Add(new Setter(ToolTipService.ShowDurationProperty, 60000)); // تظهر لفترة طويلة
        //        column.CellStyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty,
        //            new Binding(column.SortMemberPath))); // نص الخلية
        //    }
        //}


        private void LoadQueues()
        {
            var options = new ComboBoxLoadOptions
            {
                TargetComboBox = cbxConnections,
                TableName = "ExportQueue",
                DisplayMember = "QueueName",
                ValueMember = "Id",
                Db = db,
                UseGlobal = false
            };

            ComboBoxHelper.LoadComboBox(options);


            //select first element in combo box by default 
            if (cbxConnections.Items.Count > 0)
                cbxConnections.SelectedIndex = 0;

        }

        private void LoadUsers()
        {
            var options = new ComboBoxLoadOptions
            {
                TargetComboBox = cbxUsers,
                TableName = "Users",
                DisplayMember = "Username",
                ValueMember = "Id",
                Db = db,
                UseGlobal = false
                
            };

            ComboBoxHelper.LoadComboBox(options);


            //select first element in combo box by default 
            if (cbxUsers.Items.Count > 0)
                cbxUsers.SelectedIndex = 0;

        }

        private void LoadQueries()
        {
            if (cbxConnections.SelectedValue == null)
                return;

            var options = new ComboBoxLoadOptions
            {
                TargetComboBox = cbxQueueQueries,
                ConditionComboBox = cbxConnections,
                TableName = "QueueQuires",
                WhereColumn = "ExportId",
                DisplayMember = "QueryName",
                ValueMember = "Id",
                Db = db,
                UseGlobal = false
            };

            ComboBoxHelper.LoadComboBox(options);

            if (cbxQueueQueries.Items.Count > 0)
                cbxQueueQueries.SelectedIndex = 0;

            

        }



        private async Task<DataTable> LoadDashboardDataAsync()
        {
            try
            {


                //Getting Export Queue query 
                var dashboardQuery = db.ReadData("SELECT QueryText FROM QueueQuires WHERE Id = " + cbxQueueQueries.SelectedValue+"", useGlobal:false, "");
                if (dashboardQuery.Rows.Count == 0)
                {
                    MyMessageBox.Show("Dashboard query not found!", "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    return new DataTable();
                }

                string queueQuery = dashboardQuery.Rows[0]["QueryText"].ToString();

                
                // Get Siebel oracle connection
                var connectionData = db.ReadData($"SELECT * FROM DatabaseConnections WHERE ConnectionName = 'Siebel-New'", useGlobal:false, "");
                if (connectionData.Rows.Count == 0)
                {
                    MyMessageBox.Show("Connection data not found!", "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    return new DataTable();
                }

                string host = connectionData.Rows[0]["HostName"].ToString();
                string port = connectionData.Rows[0]["ConnectionPort"].ToString();
                string service = connectionData.Rows[0]["ServiceName"].ToString();

                var authData = db.ReadData($@"SELECT Username, Password FROM UserAuth 
                                      WHERE UserId = {_id} 
                                      AND SystemName = 'Siebel-New'", useGlobal: false, "");

                if (authData.Rows.Count == 0)
                {
                    MyMessageBox.Show("No user credentials found for this connection!", "",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    return new DataTable();
                }



                string userName = authData.Rows[0]["Username"].ToString();
                string encryptedPassword = authData.Rows[0]["Password"].ToString();
                string password = EncryptionHelper.Decrypt(encryptedPassword);

                string connectionString = $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))(CONNECT_DATA=(SERVICE_NAME={service})));User Id={userName};Password={password};";
                // 3️⃣ تنفيذ الكويري على Oracle
                return await Task.Run(() =>
                {
                    DataTable dt = new DataTable();
                    using (OracleConnection conn = new OracleConnection(connectionString))
                    {
                        conn.Open();
                        using (OracleCommand cmd = new OracleCommand(queueQuery, conn))
                        using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                    return dt;
                });
            }
            catch (Exception ex)
            {
                MyMessageBox.Show($"Error loading export queue data: {ex.Message}", "",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                return new DataTable();
            }
        }
        private void ResultsDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Width = 120;
            
        }

        

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {



            if (!_isInitialized)
            {
                LoadQueues();
                LoadQueries();
                LoadUsers();

                _isInitialized = true;
            }
        }

        private async void btnExport_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                // تأكد إن المستخدم اختار connection
                if (cbxConnections.SelectedValue == null)
                {
                    MyMessageBox.Show("Please select a Queue!", "",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    return;
                }

                if (cbxQueueQueries.SelectedValue == null)
                {
                    MyMessageBox.Show("Please select a Query of Queue!", "",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    return;
                }

                // إظهار الـ Overlay Loader
                OverlayLoader.Visibility = Visibility.Visible;

                // Load data from Oracle
                DataTable dt = await LoadDashboardDataAsync();

                // عرض البيانات في الـ DataGrid
                ResultsDataGrid.ItemsSource = dt.DefaultView;

                // تحديث عدد الصفوف في TextBlock
                txtTotalQueue.Text = $"Total: {dt.Rows.Count}";
            }
            catch (Exception ex)
            {
                MyMessageBox.Show($"Error loading export queue: {ex.Message}", "",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
            }
            finally
            {
                // إخفاء الـ Overlay Loader بعد التحميل
                OverlayLoader.Visibility = Visibility.Collapsed;
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            _helper.OpenWindow<ExportQueueSettings>();
        }

        private void btnExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsDataGrid.Items.Count == 0)
            {
                
                MyMessageBox.Show($"No data to export.", "Warning",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            DataView dv = ResultsDataGrid.ItemsSource as DataView;
            if (dv == null)
            {
                

                MyMessageBox.Show($"Invalid data source.", "Error",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                return;
            }

            DataTable dt = dv.ToTable();

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = "ExportQueue.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Export");
                        ws.Cell(1, 1).InsertTable(dt);

                        // =============================
                        //        Excel Styling
                        // =============================

                        var table = ws.Table(0);

                        // Header Style
                        table.Theme = XLTableTheme.None;
                        var header = table.HeadersRow();

                        header.Style.Font.Bold = true;
                        header.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#b9052f");
                        header.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                        header.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                        // Rows Style
                        foreach (var row in table.DataRange.Rows())
                        {
                            row.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;
                        }

                        // Auto-size columns
                        ws.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }


                    MyMessageBox.Show($"Export completed successfully!", "Success",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Informative);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting to Excel:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
        }

        private void btnQueries_Click(object sender, RoutedEventArgs e)
        {
            _helper.OpenWindow<QueueQuerySettings>();
        }

        private void cbxConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxConnections.SelectedValue == null)
                return;

            LoadQueries();
        }

        private void btnAssignTo_Click(object sender, RoutedEventArgs e)
        {
            string assigneTo = cbxUsers.SelectedValue.ToString();

            if (assigneTo == _id)
            {
                MyMessageBox.Show($"Can't assign SRs to the same logged in user!", "Error",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                return;
            }

            if (cbxUsers.SelectedItem == null)
            {
                MyMessageBox.Show($"Please Select a user first", "Error",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                return;
            }

            // SelectedCells NOT SelectedItems
            var selectedCells = ResultsDataGrid.SelectedCells;

            if (selectedCells.Count == 0)
            {
                MyMessageBox.Show("Please select at least one SR number.", "Error",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                return;
            }

            foreach (var cell in selectedCells)
            {
                // لازم نتأكد إن العمود هو SRNUM بس
                if (cell.Column.Header.ToString() != "SR_NUM")
                    continue;

                // الحصول على الصف
                var rowView = cell.Item as DataRowView;
                if (rowView == null) continue;

                string srNum = rowView["SR_NUM"]?.ToString();

                var parameters = new Dictionary<string, object>()
                {
                    { "@SRNum", srNum },
                    { "@AssignedTo", assigneTo },
                    { "@AssignedBy", _id },
                    { "@AssignedDate", DateTime.Now }
                };

                db.ExecuteDataParameterized(
                    "INSERT INTO SRAssignments (SRNUM, AssignedTo, AssignedBy, AssignedDate) " +
                    "VALUES (@SRNum, @AssignedTo, @AssignedBy, @AssignedDate)",
                    parameters,
                    useGlobal: true,
                    ""
                );
            }

            MyMessageBox.Show("SRs assigned successfully", "Success",
                MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Informative);



        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadQueues();
            LoadQueries();
            LoadUsers();
        }
    }
}
