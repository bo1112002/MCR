using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tools
{
    #region Example Usage
    namespace ObjectPoolTester
    {
        #region ObjectPoolTester
        public class ObjectPoolTester
        {
            public void Test()
            {
                // Obtain objects from pool
                SampleForm x = ObjectPool.New<SampleForm>();
                SampleForm x1 = ObjectPool.New<SampleForm>();
                SampleForm x2 = ObjectPool.New<SampleForm>();
                SampleClass x3 = ObjectPool.New<SampleClass>();

                // return objects to object pool
                ObjectPool.Delete<SampleForm>(x);
                ObjectPool.Delete<SampleForm>(x1);
                ObjectPool.Delete<SampleForm>(x2);
                ObjectPool.Delete<SampleClass>(x3);

                // again obtain objects from object pool, note that
                // objects will be reused
                SampleForm x4 = ObjectPool.New<SampleForm>();
                SampleClass x5 = ObjectPool.New<SampleClass>();
            }
        }

        public class SampleClass : IPoolable
        {
            public void Newing()
            {

            }

            public void Deleting()
            {

            }
        }
        public class SampleForm : IPoolable
        {
            public void Newing()
            {
            }

            public void Deleting()
            {
            }

        }
        
        
        #endregion
    }
    #endregion


    /// <summary>对象池中的对象接口</summary>
    public interface IPoolable
    {
        /// <summary>创建对象(从对象池返回,如果没有就创建)时被调用</summary>
        void Newing();
        /// <summary>删除对象(返回对象池)时被调用</summary>
        void Deleting();
    }

    /// <summary>继承自IPoolable的类（没有实质的意义，只有单纯的继承了IPoolable接口,避免派生实现接口）
    /// 如果需要对对象池操作过程进行控制，请使用IPoolable接口</summary>
    public abstract class Poolable : IPoolable
    {

        #region IPoolable 成员

        void IPoolable.Newing()
        {
        }

        void IPoolable.Deleting()
        {
        }

        #endregion
    }


    /// <summary>对象池中标识符对象的抽象类</summary>
    public abstract class PoolableToKey : IPoolable
    {

        /// <summary>获取当前的所属对象池的标识</summary>
        public abstract string Key { get; set; }

        /// <summary>重写Newing</summary>
        public virtual void Newing()
        {
        }

        /// <summary>重写Deleting</summary>
        public virtual void Deleting()
        {
        }


        #region IPoolable 成员

        void IPoolable.Newing()
        {
            this.Newing();
        }

        void IPoolable.Deleting()
        {
            this.Deleting();
        }

        #endregion
    }


    /// <summary>对象池类</summary>
    /// <remarks>依据对象的类型创建类型池,通过对象的类型进行相应的类型池的操作(获取对象，返还对象)</remarks>
    public sealed class ObjectPool
    {
        private static Dictionary<System.Type, PoolableObject> pools = new Dictionary<Type, PoolableObject>();

        private ObjectPool()
        { }

        #region *****静态方法********
        /// <summary>获取对象</summary>
        /// <typeparam name="T">指定对象的类型</typeparam>
        public static T New<T>() where T : IPoolable, new()
        {
            T x = default(T);

            if (pools.ContainsKey(typeof(T)))
            {
                x = (T)pools[typeof(T)].Pop();
            }
            else
            {
                lock (pools)
                {
                    pools[typeof(T)] = new PoolableObject(10);
                }
            }

            if (x == null)
            {
                x = new T();
            }

            x.Newing();

            return x;
        }

        /// <summary>返还对象</summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="obj"></param>
        public static void Delete<T>(T obj) where T : IPoolable
        {
            if (pools.ContainsKey(typeof(T)))
            {
                obj.Deleting();
                pools[typeof(T)].Push(obj);
            }
            else
            {
                throw new Exception("没有创建相应的类型对象池，不能进行Delete操作");
            }
        }
        /// <summary>清空所有类型对象池</summary>
        public static void Clear()
        {
            lock (pools)
            {
                foreach (PoolableObject po in pools.Values)
                {
                    po.Clear();
                }

                pools.Clear();
            }
        }

        /// <summary>清空指定的类型对象池</summary>
        /// <typeparam name="T">对象池的类型</typeparam>
        public static void Clear<T>()
        {
            if (pools.ContainsKey(typeof(T)))
            {
                pools[typeof(T)].Clear();
            }
        }

        /// <summary>获取指定类型对象池中的数量</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int PoolObjectCount<T>()
        {
            if (pools.ContainsKey(typeof(T)))
            {
                return pools[typeof(T)].Count;
            }
            return 0 ;
        } 
        #endregion ------------------------------------------------------------------------------
    }

    /// <summary>对象池类</summary>
    /// <remarks>依据标识符，创建对象池,通过标识符进行相应的类型池的操作(获取对象，返还对象)</remarks>
    public sealed class ObjectPoolToKey
    {
        private static Dictionary<string, PoolableObject> pools = new Dictionary<string , PoolableObject>();

        private ObjectPoolToKey()
        { }

        #region *****静态方法********
        /// <summary>获取对象</summary>
        /// <typeparam name="T">指定对象的类型</typeparam>
        public static T New<T>(string key) where T : PoolableToKey, new()
        {
            T x = null;
            if (pools.ContainsKey( key ) )
            {
                x = (T)pools[key].Pop();
            }
            else
            {
                lock (pools)
                {
                    pools[key] = new PoolableObject(99);
                }
            }

            if (x == null)
            {
                x = new T();
                x.Key = key;
            }
            x.Newing() ;
            return x;
        }


        /// <summary>返还对象</summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="obj"></param>
        public static void Delete( PoolableToKey obj )  
        {
            if (pools.ContainsKey(obj.Key))
            {
                obj.Deleting();
                pools[obj.Key].Push( obj );
            }
            else
            {
                throw new Exception("没有创建相应的类型对象池，不能进行Delete操作");
            }
        }
        /// <summary>清空所有类型对象池</summary>
        public static void Clear()
        {
            lock (pools)
            {
                foreach (PoolableObject po in pools.Values)
                {
                    po.Clear();
                }
                pools.Clear();
            }
        }

        /// <summary>清空指定的类型对象池</summary>
        /// <typeparam name="T">对象池的类型</typeparam>
        public static void Clear(string key)
        {
            if (pools.ContainsKey(key))
            {
                pools[key].Clear();
            }
        }

        /// <summary>获取指定类型对象池中的数量</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int PoolObjectCount( string key )
        {
            if (pools.ContainsKey(key))
            {
                return pools[key].Count;
            }
            return 0 ;
        } 
        #endregion ------------------------------------------------------------------------------
    }


    sealed class PoolableObject
    {
        private Stack<IPoolable> pool;

        public PoolableObject(int capacity)
        {
            pool = new Stack<IPoolable>(capacity);
        }

        public Int32 Count
        {
            get { return pool.Count; }
        }

        public IPoolable Pop()
        {
            lock (pool)
            {
                if (pool.Count > 0)
                {
                    return pool.Pop();
                }

                return null;
            }
        }

        public void Push(IPoolable obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("IPoolable对象在返还对象时不能为NULL");
            }

            lock (pool)
            {
                pool.Push(obj);
            }
        }

        public void Clear()
        {
            lock (pool)
            {
                pool.Clear();
            }
        }
    }

    /// <summary>MemoryStream对象缓存项</summary>
    public class MemoryPoolItem : IPoolable, IDisposable
    {
        MemoryStream _Memory ;
        public MemoryPoolItem()
        {
            _Memory = new MemoryStream();
        }

        public void Write(byte[] bs, int count)
        {
            this._Memory.Write(bs, 0, count);
        }


        public void Read(byte[] bs, int count)
        {
            this._Memory.Read(bs, 0, count);
        }

        public long Position
        {
            get { return this._Memory.Position; }
            set { this._Memory.Position = value; }
        }

        public long Length
        {
            get { return this._Memory.Length; }
            set { this._Memory.SetLength(value); }
        }

        public Stream GetBase()
        {
            return this._Memory;
        }

        public byte[] ToArray()
        {
            return this._Memory.ToArray();
        }

        #region IPoolable 成员

        void IPoolable.Newing()
        {
            if (this._Memory.CanWrite == false)
            {
                _Memory.Dispose();
                _Memory = new MemoryStream();
            }
            else
            {
                this._Memory.Seek(0, SeekOrigin.Begin);
                this._Memory.SetLength(0);
            }
        }

        void IPoolable.Deleting()
        {
        }

        #endregion

        #region IDisposable 成员

        void IDisposable.Dispose()
        {
            _Memory.Close();
        }

        #endregion
    }
}