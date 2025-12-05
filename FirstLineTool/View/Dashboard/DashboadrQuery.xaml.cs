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

namespace FirstLineTool.View.Dashboard
{
    /// <summary>
    /// Interaction logic for DashboadrQuery.xaml
    /// </summary>
    public partial class DashboadrQuery : Window
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        int row;

        public DashboadrQuery()
        {
            InitializeComponent();
        }

        private void AutoNumber()
        {
            tbl.Clear();

            string sql = @"SELECT * FROM HandledToday WHERE Id = @id";

            var prms = new Dictionary<string, object>
            {
                { "@id", 1 }
            };

            tbl = db.ReadDataParameterized(sql, prms, useGlobal: true, "");

            txtDashboardQuery.Text = tbl.Rows[0]["Query"].ToString();

        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDashboardQuery.Text))
            {
                MyMessageBox.Show("Please enter Query of Dashboard!", "WARNING",
                    MyMessageBox.MyMessageBoxButtons.OK,
                    MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }

            var result = MyMessageBox.Show(
                "Are you want to update the Query of Dashboard?",
                "ATTENTION",
                MyMessageBox.MyMessageBoxButtons.YesNoCancel,
                MyMessageBox.MyMessageBoxIcon.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                string sql = "UPDATE HandledToday SET Query = @query WHERE Id = @id";

                var prms = new Dictionary<string, object>
                {
                    { "@query", txtDashboardQuery.Text },
                    { "@id", 1 }
                };

                db.ExecuteDataParameterized(sql, prms, useGlobal:true, "Query Dashboard has been updated successfully");
                AutoNumber();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AutoNumber();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
