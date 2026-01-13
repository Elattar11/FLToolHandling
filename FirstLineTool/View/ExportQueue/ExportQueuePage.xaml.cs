using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.Core;
using FirstLineTool.Helper;
using FirstLineTool.View.Alert;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClosedXML.Excel;
using MaterialDesignThemes.Wpf;
using System.Windows.Threading;
using FirstLineTool.Services.GenerateConnectionString;
using FirstLineTool.Extensions.OracleDatabaseNamesExtensions;
using FirstLineTool.Extensions.OracleQueryTextExtensions;
using FirstLineTool.Services.UpdateSRStatusService;
using FirstLineTool.Services.ExportQueueServices;
namespace FirstLineTool.View.ExportQueue
{
    /// <summary>
    /// Interaction logic for ExportQueuePage.xaml
    /// </summary>
    public partial class ExportQueuePage : Page
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Local);
        HelperMethods _helper = new HelperMethods();


        private readonly DataGridFilterHelper _filterHelper;
        private readonly CountersUi _ui;


        private string _id;
        private string _role;

        private readonly SnackbarMessageQueue _slaQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(6));
        private DispatcherTimer _slaTimer;
        private DateTime _lastSlaNotify = DateTime.MinValue;

        private bool _isGridLoading;
        private bool _isInitialized = false;
        public ExportQueuePage(string id , string role)
        {

            InitializeComponent();
            _ui = new CountersUi
            {
                TotalQueue = txtTotalQueue,
                TotalRepeated7Days = txtTotalRepeated7Days,
                TotalDuplicated = txtTotalDuplicated,
                UnAssigned = txtUnAssigned,
                SlaWarning = txtSlaWarning,
                SlaCritical = txtSlaCritical,
                SlaOverdue = txtSlaOverdue
            };

            txtTotalQueue.Text = "Total Queue: 0";
            txtTotalRepeated7Days.Text = "Repeated last 7 days: 0";
            txtTotalDuplicated.Text = "Duplicated: 0";
            txtSlaWarning.Text = "SLA Warning: 0";
            txtSlaCritical.Text = "SLA Critical: 0";
            txtSlaOverdue.Text = "SLA Overdue: 0";
            txtUnAssigned.Text = "Un-Assigned: 0";
            SlaSnackbar.MessageQueue = _slaQueue;
            _filterHelper = new DataGridFilterHelper(ResultsDataGrid);
            _id = id;
            _role = role;

            ApplyRoleUi(role);
        }

        private void ApplyRoleUi(string role)
        {
            bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            btnSettings.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            btnQueries.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            cbxUsers.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            btnAssignTo.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            btnUnAssign.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            assignedCard.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }


        private void LoadQueues()
        {
            var options = new ComboBoxLoadOptions
            {
                TargetComboBox = cbxConnections,
                TableName = "ExportQueue",
                DisplayMember = "QueueName",
                ValueMember = "Id",
                Db = db,
                UseGlobal = false,
                ExtraWhere = "IFNULL(NotRead,0)=0"
            };

            ComboBoxHelper.LoadComboBox(options);

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


        private void ResultsDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Width = 120;
            e.Column.SortMemberPath = e.PropertyName;
        }

        

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            //LoadFakeData();
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

                OverlayLoader.Visibility = Visibility.Visible;
                btnExport.IsEnabled = false;

                ResultsDataGrid.ItemsSource = null;
                UpdateCountersService.ResetCounters(_ui);

                DataTable dt = await DashboardDataService.LoadDashboardDataAsync(db, _id, cbxQueueQueries);


                // 2) عرض البيانات في DataGrid مع الفلترة
                _isGridLoading = true;

                _filterHelper.LoadData(dt);

                // ✅ استنى لحد ما UI يخلص تحميل الهيدرز/الكمبوبوكس
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _isGridLoading = false;

                    UpdateCountersService.UpdateCounters(ResultsDataGrid, _ui);
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);

                // 3) نحدّث العداد الكلي فورًا
                txtTotalQueue.Text = $"Total Queue: {dt.Rows.Count}";

                // 4) نكتب للمستخدم إن الـ repeated لسه بيتحسب في الخلفية
                txtTotalRepeated7Days.Text = "Repeated last 7 days: calculating...";

                StartSlaMonitoring();

                // 5) شغّل شغل الـ 7 أيام في الخلفية (من غير await)
                
                _ = RepeatedSRsService.LoadMsisdnSRsFromGridAsync(ResultsDataGrid,db,_id, _ui);
            }
            catch (Exception ex)
            {
                MyMessageBox.Show($"Error loading export queue: {ex.Message}", "",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
            }
            finally
            {
                OverlayLoader.Visibility = Visibility.Collapsed;
                btnExport.IsEnabled = true;
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            _helper.OpenWindow<ExportQueueSettings>();
        }

        private void btnExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportExcelService.ExportDataGridToExcel(ResultsDataGrid);
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
        private async void btnUnAssign_Click(object sender, RoutedEventArgs e)
        {
            await SrAssignmentService.UnassignSelectedSrsAsync(ResultsDataGrid, db, _id, _role);
        }

        private async void btnAssignTo_ClickAsync(object sender, RoutedEventArgs e)
        {

            string assigneTo = cbxUsers.SelectedValue?.ToString();

            await SrAssignmentService.AssignSelectedSrAsync(
                ResultsDataGrid,
                db,
                _id,
                assigneTo
            );

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadQueues();
            LoadQueries();
            LoadUsers();
        }




        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isGridLoading) return;
            _filterHelper.HeaderFilterComboBox_SelectionChanged(sender, e);
            UpdateCountersService.UpdateCounters(ResultsDataGrid, _ui);
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            
            if (sender is not ComboBox combo)
                return;

            // اسم العمود جاي من الـ Tag في XAML
            string columnName = combo.Tag as string;
            if (string.IsNullOrWhiteSpace(columnName))
            {
                // لو مش عارف العمود لأي سبب، استخدم اللوجيك العادي
                _filterHelper.HeaderFilterComboBox_Loaded(sender, e);
                return;
            }

            // 👉 special case لعمود Repeated Last 7 days بس
            if (string.Equals(columnName, UpdateCountersService.RepeatedColumnName, StringComparison.OrdinalIgnoreCase))
            {
                // قيم ثابتة للفلتر
                var items = new List<string>
                {
                    "(All)",
                    "Yes",
                    "No"
                };

                combo.ItemsSource = items;
                combo.SelectedIndex = 0;
                return;
            }

            if (string.Equals(columnName, UpdateCountersService.DuplicatedColumnName, StringComparison.OrdinalIgnoreCase))
            {
                combo.ItemsSource = new List<string> { "(All)", "Yes", "(Blank)" };
                combo.SelectedIndex = 0;
                return;
            }


            // باقي الأعمدة → زي ما هو
            _filterHelper.HeaderFilterComboBox_Loaded(sender, e);
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            _filterHelper.ClearFilterButton_Click(sender, e);
            UpdateCountersService.UpdateCounters(ResultsDataGrid, _ui);
        }

        
        private void btnClearFilter_Loaded(object sender, RoutedEventArgs e)
            => _filterHelper.ClearFilterButton_Loaded(sender, e);

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is not DataGridRow row)
                    return;

                if (row.Item is not DataRowView rowView)
                    return;

                var table = rowView.Row.Table;

                const string repeatedColName = UpdateCountersService.RepeatedColumnName;

                if (!table.Columns.Contains(repeatedColName))
                    return;

                
                string msisdnColName = RepeatedSRsService.GetMsisdnColumnName(table);
                if (string.IsNullOrWhiteSpace(msisdnColName) || !table.Columns.Contains(msisdnColName))
                    return;

                string repeatedFlag = rowView[repeatedColName]?.ToString();
                if (!string.Equals(repeatedFlag, "Yes", StringComparison.OrdinalIgnoreCase))
                    return;

                string msisdn = rowView[msisdnColName]?.ToString();
                if (string.IsNullOrWhiteSpace(msisdn))
                    return;

                var win = new RepeatedComplaintsWindow(msisdn, _id)
                {
                    Owner = Application.Current.MainWindow
                };

                win.Show();
                win.Focus();
            }
            catch (Exception ex)
            {
                MyMessageBox.Show(
                    $"Error opening repeated SRs window: {ex.Message}",
                    "",
                    MyMessageBox.MyMessageBoxButtons.OK,
                    MyMessageBox.MyMessageBoxIcon.Error);
            }
        }

        private void StartSlaMonitoring()
        {
            StopSlaMonitoring();

            _lastSlaNotify = DateTime.MinValue; // reset مع كل Run

            _slaTimer = new DispatcherTimer(DispatcherPriority.Background);
            _slaTimer.Interval = TimeSpan.FromMinutes(1);   // Tick خفيف
            _slaTimer.Tick += (_, __) => CheckSlaAndNotify();
            _slaTimer.Start();

            CheckSlaAndNotify();

        }

        private void CheckSlaAndNotify()
        {
            if (ResultsDataGrid.ItemsSource is not DataView view || view.Table == null)
                return;

            var table = view.Table;

            // لازم الأعمدة الأساسية
            if (!table.Columns.Contains("PLANNED_COMMITED_TIME"))
                return;

            

            DashboardDataService.EnsureSlaStatusColumn(table);

            var now = DateTime.Now;

            int warning = 0, critical = 0, overdue = 0;

            foreach (DataRowView rv in view) // view = بعد الفلاتر
            {
               
                DateTime planned = DashboardDataService.GetDateSafe(rv["PLANNED_COMMITED_TIME"]);

                if (planned == DateTime.MinValue)
                {
                    rv[UpdateCountersService.SlaStatusColumnName] = "";
                    continue;
                }

                var remaining = planned - now;

                // ✅ Overdue: لو وصل وقت الـ planned أو عداه
                if (remaining <= TimeSpan.Zero) // now >= planned
                {
                    rv[UpdateCountersService.SlaStatusColumnName] = "OVERDUE";
                    overdue++;
                }
                // ✅ Critical: فاضل <= 30 دقيقة
                else if (remaining <= TimeSpan.FromMinutes(30))
                {
                    rv[UpdateCountersService.SlaStatusColumnName] = "CRITICAL";
                    critical++;
                }
                // ✅ Warning: فاضل <= ساعتين
                else if (remaining <= TimeSpan.FromHours(2))
                {
                    rv[UpdateCountersService.SlaStatusColumnName] = "WARNING";
                    warning++;
                }
                else
                {
                    rv[UpdateCountersService.SlaStatusColumnName] = "";
                }
            }

            UpdateCountersService.UpdateSlaCounters(warning, critical, overdue, _ui);

            // مفيش حاجة محتاجة تنبيه
            if (warning == 0 && critical == 0 && overdue == 0)
                return;

            // Interval حسب أخطر حالة
            TimeSpan interval =
                critical > 0 ? TimeSpan.FromMinutes(10) :
                TimeSpan.FromMinutes(30);

            // Rate limit (عشان ما نزعجش)
            if (now - _lastSlaNotify < interval)
                return;

            _lastSlaNotify = now;

            // رسالة ملخصة واحدة
            var parts = new List<string>();
            if (critical > 0) parts.Add($"CRITICAL: {critical} SRs (<= 30 min)");
            if (warning > 0) parts.Add($"WARNING: {warning} SRs (<= 2 hours)");
            if (overdue > 0) parts.Add($"OVERDUE: {overdue} SRs");

            _slaQueue.Enqueue("SLA Alert: " + string.Join(" | ", parts));
        }
        private void StopSlaMonitoring()
        {
            if (_slaTimer != null)
            {
                _slaTimer.Stop();
                _slaTimer = null;
            }
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            StopSlaMonitoring();
            
        }
    }
}
