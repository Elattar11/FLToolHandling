using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstLineTool.Core.ConnectionManager
{
    public static class CacheManager
    {
        private static Dictionary<string, object> _cache = new Dictionary<string, object>();
        private static readonly object _lock = new object();

        public static void Set(string key, object value)
        {
            lock (_lock)
            {
                _cache[key] = value;
            }
        }

        public static T Get<T>(string key)
        {
            lock (_lock)
            {
                if (_cache.ContainsKey(key))
                    return (T)_cache[key];
                return default;
            }
        }

        public static void Remove(string key)
        {
            lock (_lock)
            {
                if (_cache.ContainsKey(key))
                    _cache.Remove(key);
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _cache.Clear();
            }
        }
    }
}
