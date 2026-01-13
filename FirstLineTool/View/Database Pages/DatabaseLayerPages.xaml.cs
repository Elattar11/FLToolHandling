using ClosedXML.Excel;
using FirstLineTool.Core;
using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.Helper;
using FirstLineTool.View.Alert;
using FirstLineTool.View.Database_Pages;
using FirstLineTool.View.Users_Management_Pages;
using MaterialDesignThemes.Wpf;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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

namespace FirstLineTool.View.Layer_Pages
{
    /// <summary>
    /// Interaction logic for DatabaseLayerPages.xaml
    /// </summary>
    public partial class DatabaseLayerPages : Page
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Local);
        DataTable dt = new DataTable();

        private readonly DataGridFilterHelper _filterHelper;



        private string? authUsername;
        private string? authPassword;
        private string? authSystemName;
        HelperMethods _helper = new HelperMethods();
        private string _userId;
        private string _role;
        private bool _isInitialized = false;

        public DatabaseLayerPages(string userId, string role)
        {
            InitializeComponent();
            _filterHelper = new DataGridFilterHelper(ResultsDataGrid);


            _userId = userId;
            _role = role;

            if (role == "User")
            {
                btnConnections.Visibility = Visibility.Collapsed;
                btnQueries.Visibility = Visibility.Collapsed;

            }
            else if (role == "Admin")
            {
                btnConnections.Visibility = Visibility.Visible;
                btnQueries.Visibility = Visibility.Visible;
            }

        }

        private void LoadUserAuthForSelectedConnection()
        {
            if (string.IsNullOrEmpty(cbxConnections.Text))
                return;

            string query = $"SELECT Username, Password, SystemName " +
                   $"FROM UserAuth " +
                   $"WHERE UserId = {_userId} " +
                   $"AND SystemName = '{cbxConnections.Text.Replace("'", "''")}'";

            var dt = db.ReadData(query, useGlobal: false, "");

            if (dt.Rows.Count > 0)
            {
                authUsername = dt.Rows[0]["Username"].ToString();
                authPassword = dt.Rows[0]["Password"].ToString();
                authSystemName = dt.Rows[0]["SystemName"].ToString();
            }
            else
            {
                authUsername = authPassword = authSystemName = string.Empty;
            }
        }

        //private void LoadFakeData()
        //{
        //    DataTable dt = new DataTable();
        //
        //    // تعريف الأعمدة
        //    dt.Columns.Add("ID");
        //    dt.Columns.Add("Name");
        //    dt.Columns.Add("Description");
        //    dt.Columns.Add("Notes");
        //    dt.Columns.Add("Category");
        //    dt.Columns.Add("CreatedDate");
        //    dt.Columns.Add("Status");
        //    dt.Columns.Add("Owner");
        //
        //    // إضافة بيانات تجريبية
        //    for (int i = 1; i <= 20; i++) // 20 صف لاختبار scroll
        //    {
        //        dt.Rows.Add(
        //            i,
        //            "Item " + i,
        //            "This is a long description for item number " + i + ". It should test the horizontal scroll properly.",
        //            "Notes for item " + i + ":\n- Line 1\n- Line 2\n- Line 3",
        //            "Category " + ((i % 5) + 1),
        //            DateTime.Now.AddDays(-i).ToShortDateString(),
        //            (i % 2 == 0 ? "Active" : "Inactive"),
        //            "Owner " + i
        //        );
        //    }
        //
        //    ResultsDataGrid.ItemsSource = dt.DefaultView;
        //
        //    // إضافة tooltip لكل خلية بحيث يظهر النص بالكامل
        //    foreach (var column in ResultsDataGrid.Columns)
        //    {
        //        column.CellStyle = new Style(typeof(DataGridCell));
        //        column.CellStyle.Setters.Add(new Setter(ToolTipService.ShowDurationProperty, 60000)); // تظهر لفترة طويلة
        //        column.CellStyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty,
        //            new Binding(column.SortMemberPath))); // نص الخلية
        //    }
        //}
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //LoadFakeData();


            if (!_isInitialized)
            {
                LoadConnections();
                LoadQueries();

                _isInitialized = true;
            }
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
                UseGlobal = false
            };

            ComboBoxHelper.LoadComboBox(options);

            // Assign connections in list
            allConnections = cbxConnections.ItemsSource.Cast<dynamic>().ToList();

            // Filter Search
            cbxConnections.PreviewKeyUp += cbxConnections_PreviewKeyUp;

            //select first element in combo box by default 
            if (cbxConnections.Items.Count > 0)
                cbxConnections.SelectedIndex = 0;

        }

        //List returned queries for dynamic search
        private List<dynamic> allQueries = new List<dynamic>();

        private void LoadQueries()
        {
            if (cbxConnections.SelectedValue == null)
                return;

            var options = new ComboBoxLoadOptions
            {
                TargetComboBox = cbxQueries,
                ConditionComboBox = cbxConnections,
                TableName = "DatabaseQueries",
                WhereColumn = "DatabaseConnectionId",
                DisplayMember = "QueryName",
                ValueMember = "Id",
                Db = db,
                UseGlobal = false
            };

            ComboBoxHelper.LoadComboBox(options);

            if (cbxQueries.Items.Count > 0)
                cbxQueries.SelectedIndex = 0;

            // حفظ نسخة من العناصر الأصلية
            allQueries = cbxQueries.ItemsSource.Cast<dynamic>().ToList();

            // فلترة ديناميكية أثناء الكتابة
            cbxQueries.PreviewKeyUp += cbxQueries_PreviewKeyUp;

        }

        private void cbxConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxConnections.SelectedValue == null)
                return;

            LoadQueries();
        }

        private void cbxQueries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxQueries.SelectedItem == null)
                return;

            try
            {
                dt.Clear();
                dt = db.ReadData("SELECT * FROM DatabaseQueries WHERE Id = " + cbxQueries.SelectedValue, useGlobal:false, "");

                if (dt.Rows.Count == 0)
                    return;


                string sql = dt.Rows[0]["QueryText"].ToString();

                //Extract parameters from query
                var parameters = ExtractParameters(sql);

                //Clear pannel
                ParametersPanel.Children.Clear();


                //Generate text boxes
                foreach (var param in parameters)
                {
                    var textBox = new TextBox
                    {
                        Name = $"txt{param.TrimStart('@')}",
                        Width = 200,
                        Height = 60, // ارتفاع صغير علشان مايبقاش كبير
                        Margin = new Thickness(5, 0, 5, 0),
                        AcceptsReturn = true, // يسمح بأكتر من سطر
                        TextWrapping = TextWrapping.Wrap, // يكسر السطر تلقائيًا
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto // يظهر Scroll لو المحتوى زاد
                    };

                    HintAssist.SetHint(textBox, param.TrimStart('@').Replace("_", " "));

                    ParametersPanel.Children.Add(textBox);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Error loading query parameters: " + ex.Message);
            }

        }

        // Extract Parameters from SQL
        private List<string> ExtractParameters(string sql)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(sql, @"@\w+");
            return matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
        }

        private void cbxConnections_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            FilterComboBox(cbxConnections, allConnections);
        }

        private void cbxQueries_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            FilterComboBox(cbxQueries, allQueries);
        }


        private void FilterComboBox(ComboBox comboBox, List<dynamic> allItems)
        {
            string text = comboBox.Text.ToLower();

            var filtered = allItems
                .Where(x => x.Display.ToLower().Contains(text))
                .ToList();

            comboBox.ItemsSource = filtered;
            comboBox.IsDropDownOpen = true;

            if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
            {
                textBox.CaretIndex = text.Length;
            }
        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            btnRun.IsEnabled = false;
            pbLoading.Visibility = Visibility.Visible;

            try
            {
                // التحقق من اختيار الاتصال والاستعلام
                if (cbxConnections.SelectedItem == null)
                {
                    MyMessageBox.Show("Please select Connection first!", "WARNING",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    return;
                }

                if (cbxQueries.SelectedItem == null)
                {
                    MyMessageBox.Show("Please select Query first!", "WARNING",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    return;
                }

                // التحقق من قيم الـ parameters
                foreach (var child in ParametersPanel.Children)
                {
                    if (child is TextBox txt && string.IsNullOrWhiteSpace(txt.Text))
                    {
                        MyMessageBox.Show($"Please enter a value for '{HintAssist.GetHint(txt)}'.",
                            "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                        return;
                    }
                }

                // جلب معلومات الاتصال
                var connectionStringData = db.ReadData($"SELECT * FROM DatabaseConnections WHERE Id = {cbxConnections.SelectedValue}", useGlobal: false, "");
                string HostName = connectionStringData.Rows[0]["HostName"].ToString();
                string ConnectionPort = connectionStringData.Rows[0]["ConnectionPort"].ToString();
                string ServiceName = connectionStringData.Rows[0]["ServiceName"].ToString();

                var userAuthQuery = db.ReadData($@"SELECT Username, Password, SystemName
                                           FROM UserAuth
                                           WHERE UserId = {_userId} 
                                             AND SystemName = '{cbxConnections.Text.Replace("'", "''")}'", useGlobal: false, "");

                if (userAuthQuery.Rows.Count == 0)
                {
                    MyMessageBox.Show("No user credentials found for this connection!", "",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    return;
                }

                string userName = userAuthQuery.Rows[0]["Username"].ToString();
                string encryptedPassword = userAuthQuery.Rows[0]["Password"].ToString();
                string password = EncryptionHelper.Decrypt(encryptedPassword);

                string connectionString = $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={HostName})(PORT={ConnectionPort}))(CONNECT_DATA=(SERVICE_NAME={ServiceName})));User Id={userName};Password={password};";

                // جلب الاستعلام
                string mainQuery = $"SELECT QueryText FROM DatabaseQueries WHERE Id = {cbxQueries.SelectedValue}";
                string sql = db.ReadData(mainQuery, useGlobal: false, "").Rows[0]["QueryText"].ToString();

                // تجهيز القيم لكل parameter
                var parametersMap = new Dictionary<string, List<string>>();
                foreach (var child in ParametersPanel.Children)
                {
                    if (child is TextBox txt)
                    {
                        string paramName = "@" + txt.Name.Replace("txt", "");
                        string[] lines = txt.Text
                            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToArray();

                        if (lines.Length > 0)
                            parametersMap[paramName] = new List<string>(lines);
                    }
                }

                DataTable finalResult = await Task.Run(() =>
                {
                    DataTable result = new DataTable();

                    using (OracleConnection conn = new OracleConnection(connectionString))
                    {
                        conn.Open();
                        var combinations = GetParameterCombinations(parametersMap);

                        using (OracleTransaction tran = conn.BeginTransaction())
                        {
                            bool rollbackNeeded = false;

                            // 1️⃣ رسالة تأكيد واحدة قبل التنفيذ لو الأمر مش SELECT
                            if (!sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    var rs = MyMessageBox.Show(
                                        $"Are you sure you want to execute this operation? It may affect {combinations.Count} row(s).",
                                        "ATTENTION",
                                        MyMessageBox.MyMessageBoxButtons.YesNo,
                                        MyMessageBox.MyMessageBoxIcon.Question
                                    );

                                    if (rs != MessageBoxResult.Yes)
                                        rollbackNeeded = true;
                                });
                            }

                            // 2️⃣ تنفيذ كل العمليات
                            if (!rollbackNeeded)
                            {
                                foreach (var combo in combinations)
                                {
                                    string queryToRun = sql;
                                    foreach (var kv in combo)
                                        queryToRun = queryToRun.Replace(kv.Key, $"'{kv.Value.Replace("'", "''")}'");

                                    using (OracleCommand cmd = new OracleCommand(queryToRun, conn))
                                    {
                                        cmd.Transaction = tran;

                                        if (sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                                        {
                                            DataTable temp = new DataTable();
                                            using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                                            {
                                                adapter.Fill(temp);
                                                result.Merge(temp);
                                            }
                                        }
                                        else
                                        {
                                            // بدون MessageBox داخل الـ loop
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }

                                // 3️⃣ commit بعد انتهاء كل العمليات
                                if (!sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                                    tran.Commit();
                            }
                            else
                            {
                                tran.Rollback();
                            }
                        }
                    }

                    return result;
                });

                if (finalResult.Rows.Count > 0)
                {
                    _filterHelper.LoadData(finalResult);
                }
                else
                {
                    _filterHelper.LoadData(null); // هيفضي الجريد
                }

                // 4️⃣ رسالة نجاح بعد الانتهاء
                if (!sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    MyMessageBox.Show("Committed successfully!", "SUCCESS",
                        MyMessageBox.MyMessageBoxButtons.OK,
                        MyMessageBox.MyMessageBoxIcon.Accept);
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.Show($"Error: {ex.Message}", "",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
            }
            finally
            {
                btnRun.IsEnabled = true;
                pbLoading.Visibility = Visibility.Collapsed;
            }

        }


        private List<Dictionary<string, string>> GetParameterCombinations(Dictionary<string, List<string>> parameters)
        {
            var result = new List<Dictionary<string, string>> { new Dictionary<string, string>() };

            foreach (var kvp in parameters)
            {
                var newResult = new List<Dictionary<string, string>>();
                foreach (var existingCombo in result)
                {
                    foreach (var value in kvp.Value)
                    {
                        var newCombo = new Dictionary<string, string>(existingCombo)
                        {
                            [kvp.Key] = value
                        };
                        newResult.Add(newCombo);
                    }
                }
                result = newResult;
            }

            return result;
        }


        private string BuildQueryWithParameters(string sql)
        {
            string finalQuery = sql;

            foreach (var child in ParametersPanel.Children)
            {
                if (child is TextBox txt)
                {
                    string paramName = "@" + txt.Name.Substring(3);

                    // نفصل القيم على comma، و نضيف لكل قيمة اقتباسات
                    var values = txt.Text
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => $"'{v.Trim().Replace("'", "''")}'");

                    string joinedValues = string.Join(", ", values);
                    finalQuery = finalQuery.Replace(paramName, joinedValues);
                }
            }

            return finalQuery;
        }

        private void btnConnections_Click(object sender, RoutedEventArgs e)
        {
            _helper.OpenWindow<DatabaseConnections>();
        }

        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
            _helper.OpenWindow<DatabaseQueries>();
            //_helper.OpenWindow<ResetPasswordPage>();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadConnections();
            LoadQueries();
            ResultsDataGrid.ItemsSource = null;
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

        private void HeaderFilterComboBox_Loaded(object sender, RoutedEventArgs e)
            => _filterHelper.HeaderFilterComboBox_Loaded(sender, e);

        private void HeaderFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => _filterHelper.HeaderFilterComboBox_SelectionChanged(sender, e);

        private void ClearFilterButton_Loaded(object sender, RoutedEventArgs e)
            => _filterHelper.ClearFilterButton_Loaded(sender, e);

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
            => _filterHelper.ClearFilterButton_Click(sender, e);

        private void ResultsDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Width = 120;
        }
    }
}
