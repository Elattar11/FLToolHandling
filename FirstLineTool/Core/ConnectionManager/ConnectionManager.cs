using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstLineTool.Core.ConnectionManager
{
    public static class ConnectionManager
    {
        private static SqliteConnection _connection;
        private static readonly object _lock = new object();
        private static bool _initialized = false;

        //connection timeout busy
        private static int BusyTimeoutMs = 3000;


        public static SqliteConnection GetConnection(string connectionString)
        {
            lock (_lock)
            {
                if (_connection == null)
                {
                    _connection = new SqliteConnection(connectionString);
                    _connection.Open();
                    InitializeConnectionSettings(_connection);
                    _initialized = true;
                }
                else if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }

                return _connection;
            }
        }

        private static void InitializeConnectionSettings(SqliteConnection conn)
        {
            // Enable WAL [Write-Ahead Logging] and connection timeout
            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA journal_mode=WAL;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = $"PRAGMA busy_timeout = {BusyTimeoutMs};";
                    cmd.ExecuteNonQuery();
                }

                Logger.Log($"Connection initialized: WAL enabled, busy_timeout={BusyTimeoutMs}ms");
            }
            catch (Exception ex)
            {
                Logger.Log($"Connection initialization error: {ex.Message}");
            }
        }

        public static void CloseConnection()
        {
            lock (_lock)
            {
                try
                {
                    if (_connection != null && _connection.State != System.Data.ConnectionState.Closed)
                    {
                        _connection.Close();
                        _connection.Dispose();
                        _connection = null;
                        _initialized = false;
                        Logger.Log("SQLite connection closed.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Error closing connection: " + ex.Message);
                }
            }
        }


        public static void CloseAllConnections()
        {
            lock (_lock)
            {
                try
                {
                    if (_connection != null)
                    {
                        if (_connection.State != System.Data.ConnectionState.Closed)
                            _connection.Close();

                        _connection.Dispose();
                        _connection = null;
                        _initialized = false;

                        Logger.Log("All SQLite connections were closed.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Error closing all connections: " + ex.Message);
                }
            }
        }

        public static SqliteConnection ReinitializeConnection(string connectionString)
        {
            lock (_lock)
            {
                try
                {
                    // لو فيه connection قديم اقفله
                    if (_connection != null)
                    {
                        if (_connection.State != System.Data.ConnectionState.Closed)
                            _connection.Close();

                        _connection.Dispose();
                    }

                    // افتح connection جديدة
                    _connection = new SqliteConnection(connectionString);
                    _connection.Open();

                    InitializeConnectionSettings(_connection);

                    Logger.Log("SQLite connection reinitialized successfully.");

                    return _connection;
                }
                catch (Exception ex)
                {
                    Logger.Log("Error reinitializing connection: " + ex.Message);
                    return null;
                }
            }
        }

    }
}
