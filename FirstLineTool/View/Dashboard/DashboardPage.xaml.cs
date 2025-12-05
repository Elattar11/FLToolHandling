using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
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
using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.Core;
using System.Data;
using FirstLineTool.View.Alert;
using Oracle.ManagedDataAccess.Client;
using SQLitePCL;
using FirstLineTool.Helper;
using Renci.SshNet.Messages;
using FirstLineTool.View.Server_Pages;
using OxyPlot.Utilities;
using FirstLineTool.Core.ConnectionManager;

namespace FirstLineTool.View.Dashboard
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    /// 
    
    public partial class DashboardPage : Page
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Local);
        HelperMethods _helper = new HelperMethods();
        private string _id;
        private string _role;
        public DashboardPage(string id , string role)
        {
            InitializeComponent();
            _id = id;
            _role = role;

            if (role == "User")
            {
                btnSettings.Visibility = Visibility.Collapsed;

            }
            else if (role == "Admin")
            {
                btnSettings.Visibility = Visibility.Visible;
            }
        }


        

        private async Task<DataTable> LoadDashboardDataAsync()
        {
            try
            {

                //Getting logged user 
                var loggedUser = db.ReadData("SELECT Username FROM Users WHERE Id = "+_id+"", useGlobal: false, "");
                string username = loggedUser.Rows[0]["Username"].ToString();

                //Getting Dashboard Query
                var dashboardQuery = db.ReadData("SELECT Query FROM HandledToday WHERE Id = 1", useGlobal: false, "");
                if (dashboardQuery.Rows.Count == 0)
                {
                    MyMessageBox.Show("Dashboard query not found!", "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    return new DataTable();
                }

                string query = dashboardQuery.Rows[0]["Query"].ToString();

                string oracleUser = $"'{username.ToUpper()}'";
                query = query.Replace("@Logged_User", oracleUser);

  
                // Get Siebel oracle connection
                var connectionData = db.ReadData($"SELECT * FROM DatabaseConnections WHERE ConnectionName = 'Siebel-New'", useGlobal: false, "");
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
                        using (OracleCommand cmd = new OracleCommand(query, conn))
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
                MyMessageBox.Show($"Error loading dashboard data: {ex.Message}", "",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                return new DataTable();
            }
        }



        private async Task<DataTable> LoadUsersSRsAsync()
        {
            try
            {

                //Getting logged user 
                var loggedUser = db.ReadData("SELECT Username FROM Users WHERE Id = " + _id + "", useGlobal: false, "");
                string username = loggedUser.Rows[0]["Username"].ToString();

                //Getting Dashboard Query

                var assignedSRs = db.ReadData("SELECT SRNUM FROM SRAssignments WHERE AssignedTo = "+_id+"", useGlobal: true, "");
                if (assignedSRs.Rows.Count == 0)
                {
                    MyMessageBox.Show("There is no assigned SRs for this User!", "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    return new DataTable();
                }

                var assignedList = assignedSRs.AsEnumerable()
                              .Select(r => "'" + r["SRNUM"].ToString() + "'")
                              .ToList();

                string srList = string.Join(",", assignedList);


                var dashboardQuery = db.ReadData("SELECT QueryText FROM QueueQuires WHERE QueryName = 'SpecSRs'", useGlobal: false, "");
                if (dashboardQuery.Rows.Count == 0)
                {
                    MyMessageBox.Show("Export query not found!", "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    return new DataTable();
                }

                string query = dashboardQuery.Rows[0]["QueryText"].ToString();

                string oracleUser = $"'{username.ToUpper()}'";
                query = query.Replace("@SRNum", srList);


                // Get Siebel oracle connection
                var connectionData = db.ReadData($"SELECT * FROM DatabaseConnections WHERE ConnectionName = 'Siebel-New'", useGlobal: false, "");
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
                                      AND SystemName = 'Siebel-New'",useGlobal: false, "");

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
                        using (OracleCommand cmd = new OracleCommand(query, conn))
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
                MyMessageBox.Show($"Error loading dashboard data: {ex.Message}", "",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                return new DataTable();
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            _helper.OpenWindow<DashboadrQuery>();
        }


        private async void btnRun_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                // إظهار spinner
                btnRun.IsEnabled = false;

                // Load Dashboard Data
                DataTable dt = await LoadDashboardDataAsync(); 

                if (dt == null || dt.Rows.Count == 0)
                {
                    lblHandled.Text = "0";
                    lblRejected.Text = "0";
                    lblTotal.Text = "0";
                    return;
                }

                int handledCount = 0;
                int rejectedCount = 0;

                foreach (DataRow row in dt.Rows)
                {
                    string status = row["SR_STATUS"].ToString();
                    int count = Convert.ToInt32(row["TOTAL_UPDATED_SR"]);

                    if (status.Contains("Handled", StringComparison.OrdinalIgnoreCase))
                        handledCount += count;
                    else if (status.Contains("Rejected", StringComparison.OrdinalIgnoreCase))
                        rejectedCount += count;
                }

                int totalCount = handledCount + rejectedCount;

                // عرض الأعداد على الـ cards
                lblHandled.Text = handledCount.ToString();
                lblRejected.Text = rejectedCount.ToString();
                lblTotal.Text = totalCount.ToString();
            }
            catch (Exception ex)
            {
                MyMessageBox.Show($"Error loading dashboard: {ex.Message}", "",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
            }
            finally
            {
                // إخفاء spinner بعد التحميل
                btnRun.IsEnabled = true;
            }
        }

        private async void btnExport_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                

                // إظهار الـ Overlay Loader
                btnExport.IsEnabled = false;

                // Load data from Oracle
                DataTable dt = await LoadUsersSRsAsync();

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
                btnExport.IsEnabled = true;
            }
        }

        private void ResultsDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Width = 120;
        }

        

        private async void btnBackup_ClickAsync(object sender, RoutedEventArgs e)
        {
            btnBackup.IsEnabled = false;

            try
            {
                var db = new DatabaseSqliteConnection(DatabaseType.Global);

                bool success = await Task.Run(() =>
                {
                    // أغلق كل connections قبل النسخ
                    ConnectionManager.CloseAllConnections();

                    // نفذ النسخ
                    return db.GetBackupFromGlobalToLocal();
                });

                // بعد النسخ: امسح الكاش و افتح connections جديدة
                CacheManager.Clear();
                ConnectionManager.CloseAllConnections();
                ConnectionManager.ReinitializeConnection(
                    $"Data Source={DatabasePaths.GetLocalPath()};Cache=Shared;"
                );

                if (success)
                {
                    MyMessageBox.Show("Local database has been updated from Global successfully.",
                                      "Success",
                                      MyMessageBox.MyMessageBoxButtons.OK,
                                      MyMessageBox.MyMessageBoxIcon.Accept);
                }
                else
                {
                    MyMessageBox.Show("Failed to update Local database from Global.",
                                      "Error",
                                      MyMessageBox.MyMessageBoxButtons.OK,
                                      MyMessageBox.MyMessageBoxIcon.Error);
                }
            }
            finally
            {
                btnBackup.IsEnabled = true;
            }
        }
    }
}
