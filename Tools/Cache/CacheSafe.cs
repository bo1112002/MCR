using System;
using System.Threading;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;


namespace Tools.Cache
{

	/// <summary>
    /// 一个的线程安全的缓存类
	/// </summary>
    public class CacheSafe : ICache
	{
        /// <summary>当缓存项达到最大数量时，需要移除某一个缓存项时的事件</summary>
		public event CacheItemExpiredEventHandler CacheItemExpired;
		
        /// <summary>取缓存项时，其项不存在的事件</summary>
		public event FetchItemEventHandler	FetchItem;

		DictionaryCache	 _Dictionary;   // Holds the instance of the dictionary container.
        int _MaxItemCount;      // The maximum size of the cache.
		ReaderWriterLock _DictionaryLock; //线程控制对象(一写多读的线程锁)
	
        /// <summary>构造函数Cache</summary>
        /// <param name="p_InitialSize">缓存的初始数量(默认值：1)</param>
        /// <param name="p_MaxCount">缓存项的最大数量(默认值：50)</param>
        public CacheSafe(int p_InitialSize  , int p_MaxCount )
            : base()
		{
            if (p_InitialSize < 1)
            {
                p_InitialSize = 1;
            }

            if (p_MaxCount < 0)
            {
                p_MaxCount = 50;
            }

			_Dictionary = new DictionaryCache( p_InitialSize );
			_MaxItemCount = p_MaxCount;
			_DictionaryLock = new ReaderWriterLock();  

		}

        public CacheSafe()
            : this(1000, 10000 * 1000)
        { }

        #region *****操作********
        /*
         * Allows the user to retrieve a cached item from the cache. If it doesn't exist in the cache, it is
         * retrieved with the FetchItem event and entered into the cache.
         */
        /// <summary>获取一个缓存项(当前缓存不存在时，调用事件,如果事件存在，则缓存事件返回的对象，并返回这个对象)</summary>
        /// <param name="p_key">项的标识符</param>
        public object Find(string p_key)
        {
            object tempItem;
            _DictionaryLock.AcquireReaderLock(-1);
            try
            {
                tempItem = _Dictionary[p_key];
            }
            finally
            {
                _DictionaryLock.ReleaseReaderLock();
            }

            if (tempItem != null)
            {
                return tempItem;
            }

            //以下是找到不缓存项的处理
            if (FetchItem != null)
            {
                //调用事件(把由事件返回的对象，添加到缓存中，并返回这个对象)
                tempItem = FetchItem(this, new FetchItemEventArgs(p_key));
                if (tempItem == null)
                    return null;

                RemoveItems();
                Add(p_key, tempItem);
                return tempItem;
            }
            return null;
        }

        /// <summary>添加一个缓存项(如果存在缓存项，则先移除，目的是为了把缓存项视为最新的)，
        /// 如果tempItem==null，则视为移除key的缓存</summary>
        public void Add(string p_key, object tempItem)
        {
            RemoveItems();

            _DictionaryLock.AcquireWriterLock(-1);
            try
            {
                if (_Dictionary[p_key] != null)
                {
                    _Dictionary.Remove(p_key);
                }

                if (tempItem != null)
                {
                    _Dictionary.Add(p_key, tempItem);
                }
            }
            finally
            {
                _DictionaryLock.ReleaseWriterLock();
            }
        }

        /// <summary>获取当前的缓存数量</summary>
        public int Count
        {
            get
            {
                _DictionaryLock.AcquireReaderLock(-1);
                try
                {
                    return _Dictionary.Count;
                }
                finally
                {
                    _DictionaryLock.ReleaseReaderLock();
                }
            }
        }

        /// <summary>清空缓存</summary>
        public void Clear()
        {
            _DictionaryLock.AcquireWriterLock(-1);
            _Dictionary.Clear();
            _DictionaryLock.ReleaseWriterLock();
        }

        // Removes oldest items until the number of items in the cache is below capacity.
        //处理当缓存项到达最大数量时，移除先前的(最旧的)缓存项,并调用移除事件
        void RemoveItems()
        {
            string tempKey;
            object tempItem;

            _DictionaryLock.AcquireWriterLock(-1);
            try
            {
                if (_MaxItemCount == 0)
                    return;
                else
                {
                    while (_MaxItemCount - 1 < _Dictionary.Count)
                    {
                        tempItem = _Dictionary.RemoveFirst(out tempKey);
                        if (CacheItemExpired != null)
                            CacheItemExpired(this, new CacheItemExpiredEventArgs(tempKey, ref tempItem));
                    }
                }
            }
            finally
            {
                _DictionaryLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 获取或设置缓存项的最大数量
        /// </summary>
        public int MaxItemCount
        {
            get
            {
                return _MaxItemCount;
            }
            set
            {
                _MaxItemCount = value;

                if (_MaxItemCount < 0)
                    _MaxItemCount = 0;
            }

        } 
        #endregion ------------------------------------------------------------------------------



        readonly List<string> _Keys = new List<string>();
        #region ICache 成员

        object ICache.Get(string key)
        {
            Debug.WriteLine("Get==>key==>" + key);
            object data = this.Find(key);
            ICacheDataItem cacheItem = data as ICacheDataItem;
            if (cacheItem != null)
            {
                if (SessionUserBase.GetNewTime() >= cacheItem.OutTime) //过期的处理
                {
                    (this as ICache).Clear(key);
                    if (cacheItem.Action_OutTime != null)
                    {
                        cacheItem.Action_OutTime();
                    }
                    return null;
                }
                else
                {
                    return cacheItem.Data;
                }
            }
            else
            {
                return data;
            }
        }

        void ICache.Set(string key, object obj)
        {

            Debug.WriteLine("Set==>key==>" + key);
            if (this._Keys.Contains(key) == false)
            {
                this._Keys.Add(key);
            }
            this.Add(key, obj);
        }

        void ICache.Set(string key, object obj, DateTime outTime)
        {
            CacheDataItemDefault defaultItem = new CacheDataItemDefault(outTime, obj, null, null);
            this.Add( key, defaultItem );
        }


        void ICache.Clear()
        {
            lock (this._Keys)
            {
                this._Keys.Clear();
                this.Clear();
            }
        }

        void ICache.Clear(string key)
        {
            if (this._Keys.Contains(key))
            {
                lock (this._Keys)
                {
                    this._Keys.Remove(key);
                }
            }
            this.Add(key, null );
        }

        int ICache.Count
        {
            get { return _Dictionary.Count; }
        }

        void ICache.ForeachKey(string targetKey  , Action<string> go)
        {
            foreach (string k in this._Keys)
            {
                go(k);
            }
        }

        #endregion


    }


    #region *****其它辅助类型********
    /// <summary>This delegate will be used as a definition for the event  to notify the caller that an item has expired.</summary>
    public delegate void CacheItemExpiredEventHandler(object p_source, CacheItemExpiredEventArgs p_e);
    /// <summary>This delegate will be used as a definition to get an item if it does not exist in the cache.</summary>
    public delegate object FetchItemEventHandler(object p_source, FetchItemEventArgs p_e);



    /* Inherits from the NameObjectCollectionBase abstract class.
     * This class will act as the content container and is also
     * a wrapper for the functionality of the
     * NameObjectCollectionBase class.
     */
    /// <summary>用于存储当前缓存项的字典对象</summary>
    internal class DictionaryCache : NameObjectCollectionBase
    {

        // Nothing special is done in the constructor and
        // the base class' constructor is called.
        public DictionaryCache(int p_initialCapacity) : base(p_initialCapacity)
        {
        }

        // Removes the oldest item from the cache.
        /// <summary>移除第一个缓存项，如果没有，则返回null</summary>
        /// <param name="p_key"></param>
        public object RemoveFirst(out string p_key)
        {
            object toReturn;
            if (this.Count < 1)
            {
                // Nothing to remove.
                p_key = null;
                return null;

            }
            toReturn = this.BaseGet(0);// Get the oldest cache item.
            p_key = this.BaseGetKey(0);//Get the oldest item key.
            this.BaseRemoveAt(0); // Remove the oldest item.
            return toReturn;

        }

        /// <summary>移除指定的缓存项</summary>
        public void Remove(string p_Key)
        {
            base.BaseRemove(p_Key);
        }

        // Indexer to get a cached item.
        public object this[string p_key]
        {
            get
            {
                return ResetItem(p_key);
            }

        }

        // Add a cache item.
        public void Add(string p_key, object p_item)
        {
            // The cache item will automatically be
            // added to the end of the underlying ArrayList.
            this.BaseAdd(p_key, p_item);
        }

        // Retrieves a cached item from the NameObjectCollectionBase
        // and returns it. Also, the retrieved item is removed and
        // then added again to ensure its age is reset.
        object ResetItem(string p_key)
        {
            object tempItem;
            tempItem = this.BaseGet(p_key);
            // If the retrived item is null,it isn't reset.
            if (tempItem != null) 
            {
                this.BaseRemove(p_key);
                this.BaseAdd(p_key, tempItem);
            }
            return tempItem;
        }

        // Clears the entire contents of the cache.
        public void Clear()
        {
            this.BaseClear();
        }

    }


    /// <summary>事件参数类(Holds the CacheItemExpired event arguments.)</summary>
    public class CacheItemExpiredEventArgs
    {
        string key;
        object item;
        public CacheItemExpiredEventArgs(string p_key, ref object p_item)
        {
            key = p_key;
            item = p_item;
        }

        public object Item
        {
            get { return item; }
        }

        public string Key
        {
            get { return key; }
        }

    }
    /// <summary>事件参数类(Holds the FetchItem event arguments.)</summary>
    public class FetchItemEventArgs
    {

        string key;

        public FetchItemEventArgs(string p_key)
        {

            key = p_key;

        }

        public string Key
        {

            get
            {

                return key;

            }

        }

    } 


    

    #endregion ------------------------------------------------------------------------------
}
