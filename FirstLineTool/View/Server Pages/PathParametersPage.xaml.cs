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
    /// Interaction logic for PathParametersPage.xaml
    /// </summary>
    public partial class PathParametersPage : Window
    {
        DatabaseSqliteConnection db = new DatabaseSqliteConnection(DatabaseType.Global);
        DataTable tbl = new DataTable();
        HelperMethods _helper = new HelperMethods();
        private int queryId;



        public PathParametersPage()
        {
            InitializeComponent();
        }

        private List<dynamic> allPaths = new List<dynamic>();

        private void LoadConnections()
        {
            var options = new ComboBoxLoadOptions
            {
                TargetComboBox = cbxPaths,
                TableName = "ServerPaths",
                DisplayMember = "PathName",
                ValueMember = "Id",
                Db = db,
                UseGlobal = true,
            };

            ComboBoxHelper.LoadComboBox(options);


            allPaths = cbxPaths.ItemsSource.Cast<dynamic>().ToList();

            if (cbxPaths.Items.Count > 0)
                cbxPaths.SelectedIndex = 0;

        }

        private void AutoNumber()
        {
            tbl.Clear();
            tbl = db.ReadData(@"
                    SELECT
                        PP.Id,
                        SP.PathName as 'Path Name',
                        PP.ParamName as 'Parameter Name',
                        PP.DisplayName as 'Display Name'
                        FROM PathParameters PP JOIN ServerPaths SP
                        ON PP.ServerPathId = SP.Id",
                        useGlobal: true,
                    "");
            dgParameters.ItemsSource = tbl.DefaultView;


            txtParamName.Clear();
            txtDisplatName.Clear();
            cbxPaths.SelectedIndex = -1;
            txtSearch.Clear();

            btnAdd.IsEnabled = true;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
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
            LoadConnections();
            AutoNumber();
            
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            AutoNumber();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable tblSearch = new DataTable();
            tblSearch.Clear();
            tbl.Clear();
            tblSearch = db.ReadData("select * from PathParameters where ParamName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
            tbl = db.ReadData(@"
                                 SELECT
                                    PP.Id,
                                    SP.PathName as 'Path Name',
                                    PP.ParamName as 'Parameter Name',
                                    PP.DisplayName as 'Display Name'
                                    FROM PathParameters PP JOIN ServerPaths SP
                                    ON PP.ServerPathId = SP.Id
                                WHERE DQ.QueryName like '%" + txtSearch.Text.ToString() + "%'", useGlobal: true, "");
            try
            {
                if (txtSearch.Text == "")
                {
                    MyMessageBox.Show("Please enter Parameter name what you want to search for", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                    txtSearch.Clear();
                    AutoNumber();
                    return;
                }
                if (tblSearch.Rows.Count >= 1 && tbl.Rows.Count >= 1)
                {
                    queryId = Convert.ToInt32(tblSearch.Rows[0]["Id"]);
                    cbxPaths.SelectedValue = tblSearch.Rows[0]["ServerPathId"];
                    txtParamName.Text = tblSearch.Rows[0]["ParamName"].ToString();
                    txtDisplatName.Text = tblSearch.Rows[0]["DisplayName"].ToString();
                    

                    dgParameters.ItemsSource = tbl.DefaultView;

                }
                else
                {
                    MyMessageBox.Show("There is no Parameter with this name.", "ERROR", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
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
            _helper.SearchKeyPress(e, txtSearch, btnSearch, "Please enter Parameter name what you want to search for");
        }

        private void dgParameters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgParameters.SelectedItem == null) return;

                // Get the data of selected row
                DataRowView row = dgParameters.SelectedItem as DataRowView;
                if (row == null) return;

                //First column in DG is Id
                int selectedId = Convert.ToInt32(row["Id"]);

                DataTable tblShow = new DataTable();
                tblShow.Clear();
                tblShow = db.ReadData("SELECT * FROM PathParameters WHERE Id = " + selectedId, useGlobal: true, "");

                if (tblShow.Rows.Count > 0)
                {
                    queryId = Convert.ToInt32(tblShow.Rows[0]["Id"]);
                    cbxPaths.SelectedValue = tblShow.Rows[0]["ServerPathId"];
                    txtParamName.Text = tblShow.Rows[0]["ParamName"].ToString();
                    txtDisplatName.Text = tblShow.Rows[0]["DisplayName"].ToString();

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

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtParamName.Text == "")
            {
                MyMessageBox.Show("Please enter Parameter Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }


            if (cbxPaths.SelectedIndex < 0)
            {
                MyMessageBox.Show("Please select Server Path!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }
            if (txtDisplatName.Text == "")
            {
                MyMessageBox.Show("Please Enter Path Display Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }
            else
            {
                db.ExecuteData(
                                    "insert into PathParameters (ServerPathId, ParamName, DisplayName) " +
                                    "values (" + cbxPaths.SelectedValue + ", '" + txtParamName.Text + "', '" + txtDisplatName.Text + "')",
                                    useGlobal: true,
                                    "New Parameter Added successfully"
                                );
                AutoNumber();
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (txtParamName.Text == "")
            {
                MyMessageBox.Show("Please enter Parameter Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }


            if (cbxPaths.SelectedIndex < 0)
            {
                MyMessageBox.Show("Please select Server Path!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }
            if (txtDisplatName.Text == "")
            {
                MyMessageBox.Show("Please Enter Path Display Name!", "WARNING", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Warning);
                return;
            }
            else
            {
                var rs = MyMessageBox.Show("Are you want to update this Parameter?", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Question);
                if (rs == MessageBoxResult.Yes)
                {


                    db.ReadData("update PathParameters set ServerPathId= " + cbxPaths.SelectedValue + ", ParamName='" + txtParamName.Text.ToString() + "', DisplayName='" + txtDisplatName.Text.ToString() + "' where Id=" + queryId + " ", useGlobal: true, "Parameter has been updated successfully");
                    AutoNumber();
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var rs = MyMessageBox.Show("Are you want to delete this Parameter?!", "ATTENTION", MyMessageBox.MyMessageBoxButtons.YesNoCancel, MyMessageBox.MyMessageBoxIcon.Warning);
            if (rs == MessageBoxResult.Yes)
            {
                db.ReadData("delete from PathParameters where Id=" + queryId + "", useGlobal: true, "Parameter has been deleted successfully!");
                AutoNumber();
            }
        }

        
    }
}
