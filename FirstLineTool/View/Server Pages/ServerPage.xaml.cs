using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.Core;
using Renci.SshNet;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FirstLineTool.View.Alert;
using System.Net;
using FirstLineTool.Helper;
using MaterialDesignThemes.Wpf;
using FirstLineTool.View.Database_Pages;

namespace FirstLineTool.View.Server_Pages
{
    /// <summary>
    /// Interaction logic for ServerPage.xaml
    /// </summary>
    public partial class ServerPage : Page
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Local);
        DataTable tbl = new DataTable();
        HelperMethods _helper = new HelperMethods();
        //List returned commands for dynamic search
        private List<dynamic> allCommands = new List<dynamic>();

        private string _userId;
        private string _role;

        private bool _isInitialized = false;
        public ServerPage(string userId, string role)
        {
            InitializeComponent();

            _userId = userId;
            _role = role;

            if (role == "User")
            {
                btnConnections.Visibility = Visibility.Collapsed;
                btnParameters.Visibility = Visibility.Collapsed;
                btnSettings.Visibility = Visibility.Collapsed;

            }
            else if (role == "Admin")
            {
                btnConnections.Visibility = Visibility.Visible;
                btnParameters.Visibility = Visibility.Visible;
                btnSettings.Visibility = Visibility.Visible;
            }
        }

        private void LoadCommands()
        {
            var options = new ComboBoxLoadOptions
            {
                TargetComboBox = cbxCommands,
                TableName = "ServerPaths",
                DisplayMember = "PathName",
                ValueMember = "Id",
                Db = db,
                UseGlobal = false
            };

            ComboBoxHelper.LoadComboBox(options);

            // Assign connections in list
            allCommands = cbxCommands.ItemsSource.Cast<dynamic>().ToList();

            // Filter Search
            cbxCommands.PreviewKeyUp += cbxCommands_PreviewKeyUp;

            //select first element in combo box by default 
            if (cbxCommands.Items.Count > 0)
                cbxCommands.SelectedIndex = 0;

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

            // Disable & show loading
            btnRun.IsEnabled = false;
            btnRefresh.IsEnabled = false;
            pbLoading.Visibility = Visibility.Visible;

            try
            {
                if (cbxCommands.SelectedIndex <= -1)
                {
                    MyMessageBox.Show("Please select command first!", "WARNING",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    return;
                }

                // 1️⃣ جمع كل السطور لكل TextBox
                List<List<string>> allLines = new List<List<string>>();

                foreach (var child in ParametersPanel.Children)
                {
                    if (child is TextBox txt)
                    {
                        var lines = txt.Text
                            .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => !string.IsNullOrEmpty(l))
                            .Select(l => l.Replace("\"", "\\\"")) // escape quotes
                            .ToList();

                        if (lines.Count == 0)
                        {
                            MyMessageBox.Show($"Please fill all required fields!", "",
                                MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                            return;
                        }

                        allLines.Add(lines);
                    }
                }

                // 2️⃣ lazy generator
                IEnumerable<string> GenerateCombinations(List<List<string>> lists, int depth = 0, string current = "")
                {
                    if (depth == lists.Count)
                    {
                        yield return current.TrimEnd(',');
                        yield break;
                    }

                    foreach (var line in lists[depth])
                    {
                        string next = string.IsNullOrEmpty(current) ? line : current + "," + line;
                        foreach (var combo in GenerateCombinations(lists, depth + 1, next))
                            yield return combo;
                    }
                }

                var combinations = GenerateCombinations(allLines);

                // 3️⃣ بيانات المستخدم والسيرفر
                var userAuthQuery = db.ReadData($@"SELECT Username, Password, SystemName 
                                           FROM UserAuth 
                                           WHERE UserId = {_userId} 
                                           AND SystemName = 'INServer'", useGlobal: false, "");

                if (userAuthQuery.Rows.Count == 0)
                {
                    MyMessageBox.Show("No user credentials found for this connection!", "",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    AddStatusMessage("No server auth for current logged user!", Brushes.Red);
                    return;
                }

                string userName = userAuthQuery.Rows[0]["Username"].ToString();
                string encryptedPassword = userAuthQuery.Rows[0]["Password"].ToString();
                string password = EncryptionHelper.Decrypt(encryptedPassword);

                var serverData = db.ReadData("SELECT IPAddress, Port FROM INServerSettings WHERE Id = 1", useGlobal: false, "");
                string IPAddress = serverData.Rows[0]["IPAddress"].ToString();
                int Port = Convert.ToInt32(serverData.Rows[0]["Port"]);

                var serverPaths = db.ReadData($"SELECT * FROM ServerPaths WHERE Id = {cbxCommands.SelectedValue}", useGlobal: false, "");
                string linuxPath = serverPaths.Rows[0]["LinuxPath"].ToString();
                string commandTemplate = serverPaths.Rows[0]["CommandTemplate"].ToString();

                if (string.IsNullOrEmpty(linuxPath))
                {
                    MyMessageBox.Show("Please select a path first!", "",
                        MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    return;
                }

                AddStatusMessage("Connecting to server...", Brushes.Black);

                // 4️⃣ تشغيل العمليات
                await Task.Run(() =>
                {
                    using var client = new Renci.SshNet.SshClient(IPAddress, Port, userName, password);
                    client.Connect();

                    if (!client.IsConnected)
                    {
                        AddStatusMessage("Connection failed", Brushes.Red);
                        return;
                    }

                    AddStatusMessage("Server connected successfully", Brushes.Black);

                    client.RunCommand($"{linuxPath} && echo -n > attar.txt");

                    foreach (var combo in combinations)
                        client.RunCommand($"{linuxPath} && echo \"{combo}\" >> attar.txt");

                    AddStatusMessage("All combinations written to attar.txt", Brushes.Black);

                    var result = client.RunCommand($"{linuxPath} && {commandTemplate}");

                    if (!string.IsNullOrEmpty(result.Error))
                        AddStatusMessage($"Error: {result.Error}", Brushes.Red);
                    else
                        AddStatusMessage("Command executed successfully", Brushes.Black);

                    client.Disconnect();
                });
            }
            catch (Exception ex)
            {
                AddStatusMessage($"Exception: {ex.Message}", Brushes.Red);
            }
            finally
            {
                // ALWAYS restore UI
                btnRun.IsEnabled = true;
                btnRefresh.IsEnabled = true;
                pbLoading.Visibility = Visibility.Collapsed;
            }

        }

        private void cbxCommands_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            FilterComboBox(cbxCommands, allCommands);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
            {
                LoadCommands();
                _isInitialized = true;
            }
            
        }

        private void cbxCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ParametersPanel.Children.Clear();

                if (cbxCommands.SelectedItem == null)
                    return;

                
                int pathId = Convert.ToInt32(cbxCommands.SelectedValue);

                // 🟩 جلب الـ parameters الخاصة بالـ path
                var paramData = db.ReadData(
                    $"SELECT ParamName, DisplayName FROM PathParameters WHERE ServerPathId = {pathId}", useGlobal: false,
                    "");

                if (paramData.Rows.Count == 0)
                {
                    var lbl = new TextBlock
                    {
                        Text = "No parameters required for this script.",
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(5)
                    };
                    ParametersPanel.Children.Add(lbl);
                    return;
                }

                foreach (DataRow row in paramData.Rows)
                {
                    string paramName = row["ParamName"].ToString();
                    string displayName = row["DisplayName"].ToString();

                    // TextBox متعدد الأسطر
                    var textBox = new TextBox
                    {
                        Name = $"txt{paramName}",
                        Width = 200,
                        Height = 60, // ارتفاع معقول ل multiline
                        Margin = new Thickness(5, 0, 5, 0),
                        AcceptsReturn = true, // مهم لتعدد الأسطر
                        TextWrapping = TextWrapping.Wrap, // لف النصوص الطويلة
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto, // Scroll داخلي لو النص طويل
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                    };

                    // الـ Hint بدل الـ Label
                    HintAssist.SetHint(textBox, displayName);

                    // إضافة العنصر إلى الـ Panel
                    ParametersPanel.Children.Add(textBox);
                }

            }
            catch (Exception ex)
            {
                MyMessageBox.Show($"Error while loading parameters: {ex.Message}", "",
                    MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
            }
        }


        //Generating labels for logs
        private void AddStatusMessage(string message, Brush color)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var lbl = new Label
                {
                    Content = message,
                    Foreground = color,
                    FontSize = 12,
                    Margin = new Thickness(0, 3, 0, 0),
                    Padding = new Thickness(4),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,  
                    VerticalContentAlignment = VerticalAlignment.Top,
                };
                lbl.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
                StatusPanel.Children.Add(lbl);
            });
        }

        private void btnConnections_Click(object sender, RoutedEventArgs e)
        {
            _helper.OpenWindow<ServerPathsPage>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _helper.OpenWindow<PathParametersPage>();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            _helper.OpenWindow<ServerSettingsPage>();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadCommands();
            StatusPanel.Children.Clear();
        }
    }
}
