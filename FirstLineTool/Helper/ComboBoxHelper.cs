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

            // ✅ تجهيز فلتر إضافي ثابت
            string extraWhere = string.IsNullOrWhiteSpace(options.ExtraWhere)
                ? ""
                : options.ExtraWhere.Trim();

            if (options.ConditionComboBox != null && !string.IsNullOrEmpty(options.WhereColumn))
            {
                if (options.ConditionComboBox.SelectedValue != null &&
                    int.TryParse(options.ConditionComboBox.SelectedValue.ToString(), out int conditionValue))
                {
                    query = $"SELECT {options.ValueMember}, {options.DisplayMember} " +
                            $"FROM {options.TableName} " +
                            $"WHERE {options.WhereColumn} = {conditionValue}";

                    // ✅ لو فيه ExtraWhere ضيفه AND
                    if (!string.IsNullOrWhiteSpace(extraWhere))
                        query += $" AND ({extraWhere})";
                }
                else
                {
                    options.TargetComboBox.ItemsSource = null;
                    return;
                }
            }
            else
            {
                query = $"SELECT {options.ValueMember}, {options.DisplayMember} FROM {options.TableName}";

                // ✅ لو فيه ExtraWhere ضيف WHERE
                if (!string.IsNullOrWhiteSpace(extraWhere))
                    query += $" WHERE ({extraWhere})";
            }

            DataTable dt = options.Db.ReadData(query, useGlobal: options.UseGlobal, "");

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
