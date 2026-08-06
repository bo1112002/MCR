using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools
{
    /// <summary>自定义字典功能类</summary>
    /// <typeparam name="T"></typeparam>
    public class MyDictionary<T> : Dictionary<string, T>
    {
    }


    /// <summary>自定义字典功能类</summary>
    /// <typeparam name="T"></typeparam>
    public class MyDictionary2
    {
        readonly Dictionary<string, object> _Map = null;

        public MyDictionary2()
        {
            _Map = new Dictionary<string, object>();
        }

        public MyDictionary2(Dictionary<string, object> map)
        {
            _Map = map;
        }

        public object Get(string key)
        {
            if (_Map.ContainsKey(key) == false)
                return null;

            return _Map[key];
        }

        public void Set(string key, object val)
        {
            lock (_Map)
            {
                _Map[key] = val;
            }
        }

        public object this[string key]
        {
            get
            {
                return Get(key);
            }
        }

        public bool ContainsKey(string key)
        {
            return _Map.ContainsKey(key);
        }

        public int Count
        {
            get
            {
                return _Map.Count;
            }
        }

    }
}
