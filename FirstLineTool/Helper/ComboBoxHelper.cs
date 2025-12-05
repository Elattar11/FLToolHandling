using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstLineTool.Helper
{
    public static class ComboBoxHelper
    {
        public static void LoadComboBox(ComboBoxLoadOptions options)
        {
            if (options.Db == null || options.TargetComboBox == null)
                return;

            string query;

            if (options.ConditionComboBox != null && !string.IsNullOrEmpty(options.WhereColumn))
            {
                if (options.ConditionComboBox.SelectedValue != null &&
                    int.TryParse(options.ConditionComboBox.SelectedValue.ToString(), out int conditionValue))
                {
                    query = $"SELECT {options.ValueMember}, {options.DisplayMember} " +
                            $"FROM {options.TableName} " +
                            $"WHERE {options.WhereColumn} = {conditionValue}";
                }
                else
                {
                    // لو الشرط مش متحقق نفرغ الكومبو الهدف
                    options.TargetComboBox.ItemsSource = null;
                    return;
                }
            }
            else
            {
                // من غير شرط
                query = $"SELECT {options.ValueMember}, {options.DisplayMember} FROM {options.TableName}";
            }

            DataTable dt;
            if (options.UseGlobal)
            {
                dt = options.Db.ReadData(query, useGlobal: true, "");
            }
            else
            {
                dt = options.Db.ReadData(query, useGlobal: false, "");
            }
            

            var items = dt.AsEnumerable()
                .Select(row => new
                {
                    Value = row[options.ValueMember],
                    Display = row[options.DisplayMember]?.ToString()
                })
                .ToList();

            options.TargetComboBox.ItemsSource = items;
            options.TargetComboBox.DisplayMemberPath = "Display";
            options.TargetComboBox.SelectedValuePath = "Value";
        }

    }
}
