using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstLineTool.Core.ConnectionManager
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FirstLineTool.log");

        public static void Log(string message)
        {
            lock (_lock)
            {
                try
                {
                    var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}{Environment.NewLine}";
                    File.AppendAllText(_logFilePath, line);
                }
                catch
                {
                    
                }
            }
        }

        public static void LogException(Exception ex, string context = "")
        {
            Log($"{context} Exception: {ex.GetType().Name} - {ex.Message} | Stack: {ex.StackTrace}");
        }


    }
}
