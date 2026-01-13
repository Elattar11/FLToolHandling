using FirstLineTool.Core.ConnectionManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstLineTool.Core.TypesAndPaths
{
    public static class DatabasePaths
    {
        private static readonly string SafeFolder =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FirstLineTool",
                "Database"
            );

        


        // GLOBAL (على الشبكة)
        public static string GetGlobalPath()
        {
            return Path.Combine(
                @"\\caisvfps02\sd$\IT\First Line Tool",
                "Database",
                "FirstLineHandlingTool.db"
            );


            //return Path.Combine(
            //    @"D:\Database",
            //
            //    "FirstLineHandlingTool.db"
            //);
        }

        // LOCAL (على جهاز المستخدم)
        public static string GetLocalPath()
        {
            // أنشئ الفولدر لو مش موجود
            if (!Directory.Exists(SafeFolder))
                Directory.CreateDirectory(SafeFolder);

            return Path.Combine(SafeFolder, "FirstLineHandlingTool_local.db");
        }

        // Copy from Global → Local
        public static void EnsureLocalCopy()
        {
            string global = GetGlobalPath();
            string local = GetLocalPath();

            if (!File.Exists(local))
            {
                try
                {
                    File.Copy(global, local, overwrite: true);
                    Logger.Log("Local DB copied from Global DB successfully.");
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Failed to copy Global DB to Local.");
                }
            }
        }

        // مساعدة للحصول على path حسب النوع
        public static string GetPath(DatabaseType dbType)
        {
            return dbType switch
            {
                DatabaseType.Global => GetGlobalPath(),
                DatabaseType.Local => GetLocalPath(),
                _ => throw new ArgumentException("Unknown DatabaseType")
            };
        }
    }
}
