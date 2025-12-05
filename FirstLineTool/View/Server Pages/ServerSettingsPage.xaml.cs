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

namespace FirstLineTool.View.Server_Pages
{
    /// <summary>
    /// Interaction logic for ServerSettingsPage.xaml
    /// </summary>
    public partial class ServerSettingsPage : Window
    {

        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        int row;
        public ServerSettingsPage()
        {
            InitializeComponent();
        }

        private void AutoNumber()
        {
            tbl.Clear();
            tbl = db.ReadData(@"SELECT *
                                FROM INServerSettings
                                WHERE Id = 1", useGlobal: true, "");

            txtIPAddress.Text = tbl.Rows[row]["IPAddress"].ToString();
            txtPort.Text = tbl.Rows[row]["Port"].ToString();

        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AutoNumber();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (txtIPAddress.Text == "")
            {
                MyMessageBox.Show("Please enter IP Address of the server!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            if (txtPort.Text == "")
            {
                MyMessageBox.Show("Please enter Port of the server!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }
            else
            {
                var rs = MyMessageBox.Show("Are you want to update IN Server Settings?", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Question);
                if (rs == MessageBoxResult.Yes)
                {


                    db.ExecuteData("update INServerSettings set IPAddress='" + txtIPAddress.Text.ToString() + "', Port='" + txtPort.Text.ToString() + "' where Id = 1 ", useGlobal: true, "Server settings has been updated successfully");
                    AutoNumber();
                }
            }

        }
    }
}
