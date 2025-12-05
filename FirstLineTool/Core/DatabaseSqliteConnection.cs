using FirstLineTool.Core.ConnectionManager;
using FirstLineTool.Core.TypesAndPaths;
using FirstLineTool.View.Alert;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;

namespace FirstLineTool.Core
{
    public class DatabaseSqliteConnection
    {
        private readonly string _localConnectionString;
        private readonly string _globalConnectionString;

        private readonly int _maxRetries = 5;
        private readonly int _retryDelayMs = 300;

        public DatabaseSqliteConnection(DatabaseType dbType)
        {
            string localPath = DatabasePaths.GetLocalPath();
            string globalPath = DatabasePaths.GetGlobalPath();

            _localConnectionString = $"Data Source={localPath};Cache=Shared;";
            _globalConnectionString = $"Data Source={globalPath};Cache=Shared;";

            if (dbType == DatabaseType.Local)
                DatabasePaths.EnsureLocalCopy();

            try
            {
                var test = FirstLineTool.Core.ConnectionManager.ConnectionManager.GetConnection(_localConnectionString);
                var test2 = FirstLineTool.Core.ConnectionManager.ConnectionManager.GetConnection(_globalConnectionString);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error initializing DatabaseSqliteConnection");
            }
        }

        private SqliteConnection GetLocalConn()
        {
            var conn = new SqliteConnection(_localConnectionString);
            conn.Open();
            return conn;
        }

        private SqliteConnection GetGlobalConn()
        {
            var conn = new SqliteConnection(_globalConnectionString);
            conn.Open();
            return conn;
        }

        public bool GetBackupFromGlobalToLocal()
        {
            try
            {
                string localPath = DatabasePaths.GetLocalPath();
                string globalPath = DatabasePaths.GetGlobalPath();

                // تأكد إن مجلد الـ Local موجود
                Directory.CreateDirectory(Path.GetDirectoryName(localPath));

                // اقفل أي connection للـ Local DB قبل البدء
                FirstLineTool.Core.ConnectionManager.ConnectionManager.CloseConnection();

                // نفتح اتصالين: واحد للـ Global وواحد للـ Local
                using (var source = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={globalPath};Mode=ReadOnly"))
                using (var destination = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={localPath}"))
                {
                    source.Open();
                    destination.Open();

                    // نسخ فعلي باستخدام الـ Backup API
                    source.BackupDatabase(destination);
                }

                Logger.Log("Local DB successfully backed up from Global using SQLite Backup API.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error during SQLite backup process.");
                return false;
            }
        }

        // ---------------------
        //   Cache Key Builder
        // ---------------------
        private string BuildCacheKey(string query, Dictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0) return query;

            string key = query;
            foreach (var p in parameters)
                key += $"|{p.Key}:{p.Value}";

            return key;
        }

        // -------------------------
        //        READ
        // -------------------------
        public DataTable ReadData(string statement, bool useGlobal = false, string msg = "")
        {
            var cached = CacheManager.Get<DataTable>(statement);
            if (cached != null) return cached;

            var dt = new DataTable();
            int attempt = 0;

            while (true)
            {
                try
                {
                    using (var conn = useGlobal ? GetGlobalConn() : GetLocalConn())
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = statement;
                        using (var reader = command.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }

                    CacheManager.Set(statement, dt);

                    if (!string.IsNullOrEmpty(msg))
                        MyMessageBox.Show(msg, "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Accept);

                    return dt;
                }
                catch (SqliteException ex)
                {
                    if ((ex.SqliteErrorCode == 5 || ex.SqliteErrorCode == 6) && attempt < _maxRetries)
                    {
                        attempt++;
                        Logger.Log($"ReadData busy/locked (attempt {attempt}): {statement}");
                        Thread.Sleep(_retryDelayMs);
                        continue;
                    }

                    Logger.LogException(ex, $"ReadData Failed: {statement}");
                    MyMessageBox.Show(ex.Message, "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    return dt;
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, $"ReadData Unexpected: {statement}");
                    MyMessageBox.Show(ex.Message, "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    return dt;
                }
            }
        }

        // -------------------------
        //   PARAMETERIZED READ
        // -------------------------
        public DataTable ReadDataParameterized(string query, Dictionary<string, object> parameters = null, bool useGlobal = false, string msg = "")
        {
            string cacheKey = BuildCacheKey(query, parameters);
            var cached = CacheManager.Get<DataTable>(cacheKey);
            if (cached != null) return cached;

            var dt = new DataTable();
            int attempt = 0;

            while (true)
            {
                try
                {
                    using (var conn = useGlobal ? GetGlobalConn() : GetLocalConn())
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = query;

                        if (parameters != null)
                        {
                            foreach (var p in parameters)
                                command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }

                    CacheManager.Set(cacheKey, dt);

                    if (!string.IsNullOrEmpty(msg))
                        MyMessageBox.Show(msg, "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Accept);

                    return dt;
                }
                catch (SqliteException ex)
                {
                    if ((ex.SqliteErrorCode == 5 || ex.SqliteErrorCode == 6) && attempt < _maxRetries)
                    {
                        attempt++;
                        Logger.Log($"ReadDataParameterized busy/locked (attempt {attempt}) for {query}");
                        Thread.Sleep(_retryDelayMs);
                        continue;
                    }

                    Logger.LogException(ex, $"ReadDataParameterized Failed: {query}");
                    MyMessageBox.Show(ex.Message, "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);

                    return dt;
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, $"ReadDataParameterized Unexpected: {query}");
                    MyMessageBox.Show(ex.Message, "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);

                    return dt;
                }
            }
        }

        // -------------------------
        //   WRITE (GLOBAL OR LOCAL)
        // -------------------------
        public bool ExecuteData(string statement, bool useGlobal = true, string msg = "")
        {
            int attempt = 0;

            while (true)
            {
                try
                {
                    using (var conn = useGlobal ? GetGlobalConn() : GetLocalConn())
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = statement;
                        command.ExecuteNonQuery();
                    }

                    CacheManager.Clear();

                    if (!string.IsNullOrEmpty(msg))
                        MyMessageBox.Show(msg, "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Accept);

                    return true;
                }
                catch (SqliteException ex)
                {
                    if ((ex.SqliteErrorCode == 5 || ex.SqliteErrorCode == 6) && attempt < _maxRetries)
                    {
                        attempt++;
                        Logger.Log($"ExecuteData busy/locked (attempt {attempt}) for {statement}");
                        Thread.Sleep(_retryDelayMs);
                        continue;
                    }

                    Logger.LogException(ex, $"ExecuteData Failed: {statement}");
                    MyMessageBox.Show(ex.Message, "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, $"ExecuteData Unexpected: {statement}");
                    MyMessageBox.Show(ex.Message, "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);
                    return false;
                }
            }
        }

        // -------------------------
        //   PARAMETERIZED WRITE
        // -------------------------
        public bool ExecuteDataParameterized(string query, Dictionary<string, object> parameters = null, bool useGlobal = true, string msg = "")
        {
            int attempt = 0;

            while (true)
            {
                try
                {
                    using (var conn = useGlobal ? GetGlobalConn() : GetLocalConn())
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = query;

                        if (parameters != null)
                        {
                            foreach (var p in parameters)
                                command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                        }

                        command.ExecuteNonQuery();
                    }

                    CacheManager.Clear();

                    if (!string.IsNullOrEmpty(msg))
                        MyMessageBox.Show(msg, "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Accept);

                    return true;
                }
                catch (SqliteException ex)
                {
                    if ((ex.SqliteErrorCode == 5 || ex.SqliteErrorCode == 6) && attempt < _maxRetries)
                    {
                        attempt++;
                        Logger.Log($"ExecuteDataParameterized busy/locked (attempt {attempt}) for query: {query}");
                        Thread.Sleep(_retryDelayMs);
                        continue;
                    }

                    Logger.LogException(ex, $"ExecuteDataParameterized Failed: {query}");
                    MyMessageBox.Show(ex.Message, "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);

                    return false;
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, $"ExecuteDataParameterized Unexpected: {query}");
                    MyMessageBox.Show(ex.Message, "", MyMessageBox.MyMessageBoxButtons.OK, MyMessageBox.MyMessageBoxIcon.Error);

                    return false;
                }
            }
        }


    }
}
