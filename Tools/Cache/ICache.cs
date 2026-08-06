using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools.Cache
{
    /// <summary>
    /// 过期时间(天)的级别
    /// </summary>
    public enum E_OverdueLevel
    {
        /// <summary>1天</summary>
        A = 1,
        /// <summary>3天</summary>
        A1= 3,
        /// <summary>6天</summary>
        B = 6,
        /// <summary>10天</summary>
        C = 10,
        /// <summary>20天</summary>
        D = 20,
        /// <summary>30天</summary>
        E = 30,
        /// <summary>60天</summary>
        F = 60
    }


    /// <summary>缓存操作接口</summary>
    public interface ICache
    {
        /// <summary>依据key值获取缓存对象，如果不存在则返回null</summary>
        object Get(string key);
        /// <summary>依据 key值设置一个缓存项对象</summary>
        void Set(string key,  object obj  );
        /// <summary>依据 key值设置一个缓存项对象并指定一个过期时间(这个方法会把obj对象包装成为一个ICacheDataItem对象)</summary>
        void Set(string key, object obj, DateTime outTime);

        /// <summary>清除所有缓存项</summary>
        void Clear();
        /// <summary>清除指定key的缓存项</summary>
        void Clear(string key);
        /// <summary>获取当前缓存项的数量</summary>
        int Count { get; }

        /// <summary>遍历所有的缓存项的Key</summary>
        void ForeachKey(string shortName, Action<string> go);
    }

    /// <summary>缓存项的包装类</summary>
    public interface ICacheDataItem
    {
        /// <summary>缓存时间</summary>
        DateTime OutTime {get;}
        /// <summary>实际的缓存数据</summary>
        object Data {get;}
        /// <summary>当前缓存项超出缓存时间的处理</summary>
        Action Action_OutTime {get;}
        /// <summary>当前缓存被Removing的时候会调用当前类型的委托实例对象</summary>
        RemovedCache RemovingMethod { get; }
    }

    public class CacheDataItemDefault : ICacheDataItem
    {
        /// <summary>过期时间</summary>
        readonly DateTime _OutTime;
        /// <summary>真实数据</summary>
        readonly object _Data;
        /// <summary>过期时的处理方法</summary>
        readonly Action _Action_OutTime;
        /// <summary>移除当前缓存项之后的方法</summary>
        readonly RemovedCache _RemovedCache;
        public CacheDataItemDefault(DateTime outTime, object data, Action actionOutTime, RemovedCache removedMethod)
        {
            this._OutTime = outTime;
            this._Data = data;
            this._Action_OutTime = actionOutTime;
            this._RemovedCache = removedMethod;
        }
    
        #region ICacheDataItem 成员

        DateTime  ICacheDataItem.OutTime
        {
	        get {  return _OutTime; }
        }

        object  ICacheDataItem.Data
        {
	        get { return _Data ; }
        }

        Action  ICacheDataItem.Action_OutTime
        {
	        get { return _Action_OutTime ; }
        }

        RemovedCache ICacheDataItem.RemovingMethod
        {
            get 
            {
                return _RemovedCache;
            }
        }

        #endregion
    }

    /// <summary>当前缓存被Removing的时候会调用当前类型的委托实例对象</summary>
    public delegate void RemovedCache(string key, object data);

    /*
    /// <summary>缓存项的key集合的管理接口(只有实现了该接口才能进行自动缓存管理)</summary>
    public interface ICacheKey
    {
        /// <summary> 设置当前缓存对象由2到3个字符组成的短名(用于缓存key值的分组)</summary>
        string GetShortName();
    }*/
}
