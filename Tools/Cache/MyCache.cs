using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;
using System.Web;
using System.Collections;
using System.Diagnostics;

namespace Tools.Cache
{
    
    /// <summary>
    /// 当前系统的缓存功能类
    /// </summary>
    public sealed class MyCache : ICache
    {
        readonly Dictionary<string, object> _CacheMe = new Dictionary<string, object>();
        public MyCache()  
        { 
        }

        RemovedCache _CallBack = null; //定义当前缓存被移除时的调用对象
        void RemoveCallback(string key, object data , CacheItemRemovedReason reason )
        {
            if (_CallBack != null)
            {
                _CallBack(key, data);
            }
        }


        #region ICache 成员

        object ICache.Get(string key)
        {
            Debug.WriteLine("ICache==>GetKey==>" + key);
            if (_CacheMe.ContainsKey(key))
            {
                object data =  _CacheMe[key];
                if (data is ICacheDataItem)
                {
                    ICacheDataItem item = data as ICacheDataItem;
                    if (DateTime.Now > item.OutTime)
                    {
                        (this as ICache).Clear( key );
                    }
                }
                else
                {
                    return data;
                }
            }
            return null;
        }

        void ICache.Set(string key, object obj)
        {
            Debug.WriteLine("ICache==>SetKey==>" + key);
            lock (_CacheMe)
            {
                _CacheMe[key] = obj;
            }
        }

        void ICache.Set(string key, object obj, DateTime outTime)
        {
            Debug.WriteLine("ICache==>SetKey==>" + key + " , outTime==>" + outTime.ToString());
            CacheDataItemDefault defaultItem = new CacheDataItemDefault(outTime, obj, null, null);
            lock (_CacheMe)
            {
                _CacheMe[key] = obj;
            }
        }



        void ICache.Clear()
        {
            lock (_CacheMe)
            {
                _CacheMe.Clear();
            }
        }

        void ICache.Clear(string key)
        {
            lock (_CacheMe)
            {
                if (_CacheMe.ContainsKey(key))
                {
                    _CacheMe.Remove(key);
                }
            }
        }

        int ICache.Count
        {
            get { return _CacheMe.Count; }
        }

        void ICache.ForeachKey( string targetKey  , Action<string> go)
        {
            foreach (string k in _CacheMe.Keys)
            {
                go(k);
            }
        }

        
        #endregion
    }
}
