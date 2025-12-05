using FirstLineTool.Core;
using FirstLineTool.View;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FirstLineTool.Helper
{
    public class ComboBoxLoadOptions
    {
        public ComboBox TargetComboBox { get; set; }
        public ComboBox ConditionComboBox { get; set; }

        public string TableName { get; set; }
        public string DisplayMember { get; set; }
        public string ValueMember { get; set; }
        public string WhereColumn { get; set; }
        public DatabaseSqliteConnection Db { get; set; }

        public bool UseGlobal { get; set; } = false;


    }
}
