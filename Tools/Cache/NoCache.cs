using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools.Cache
{
    /// <summary>
    /// 无实现的缓存接口的实现类
    /// </summary>
    public class NoCache : ICache
    {

        static readonly ICache _TempCache = new CacheSafe( 10 , 100 );

        #region ICache 成员

        object ICache.Get(string key)
        {
            if ( key != null && key.Contains("Cart"))
            {
                return  _TempCache.Get(key);
            }

            return null;
        }

        void ICache.Set(string key, object obj)
        {
            if (key != null && key.Contains("Cart"))
            {
                _TempCache.Set(key,  obj);
            }
        }

        void ICache.Clear()
        {
        }

        void ICache.Clear(string key)
        {
        }

        int ICache.Count
        {
            get { return 0; }
        }

        void ICache.ForeachKey(string shortName, Action<string> go)
        {
            return;
        }


        void ICache.Set(string key, object obj, DateTime outTime)
        {
            return;
        }

        #endregion
    }
}
