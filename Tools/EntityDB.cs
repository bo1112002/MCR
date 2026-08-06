using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Diagnostics;

namespace Tools
{
    /// <summary>IAccessDB的抽象实现</summary>
    public abstract class EntityDB :  IAccessDB
    {
        /// <summary>创建记录</summary>
        public abstract Result Insert();
        /// <summary>更新记录</summary>
        public abstract Result Update();
        /// <summary>删除记录</summary>
        public abstract Result Delete();
        /// <summary>把一个reader对象填充当前对象的属性值</summary>
        public abstract void ToEntity(EntityReader reader);
        /// <summary>获取当前操作的实例</summary>
        public abstract EntityBase Entity { get; }


        #region============= Access DB=========>>>

        /// <summary>自定义更新</summary>
        public virtual Result CustomUpdate(string tag, object obj)
        {
            return Result.NONE;
        }
        /// <summary>获取sql参数</summary>
        protected virtual ParameterTag[] GetSqlParameters()
        {
            List<ParameterTag> list = new List<ParameterTag>();

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            PropertyInfo[] pInfos = Entity.GetTypeBase().GetProperties(flags);
            foreach (PropertyInfo info in pInfos)
            {
                object[] objs = info.GetCustomAttributes(true);
                foreach (object o in objs)
                {
                    ParameterTag p = o as ParameterTag;
                    p.LoadValue(this.Entity, info);
                    list.Add(p);
                    continue;
                }
            }
            return list.ToArray();
        }


        /// <summary>创建记录(Insert)</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected Result Insert(string sql_Insert, ParameterTag[] ps)
        {
            int key = Thread.CurrentThread.ManagedThreadId;
            if (_MapSqlItemBeginReady.ContainsKey(key))
            {
                SqlItem item = new SqlItem(this, sql_Insert , ps);
                //设置事务更新后的回调处理方法
                item.Action_CallBack = (obj) => {
                    EntityDB.CallEvent_ObjectChanged(this.Entity, ObjectChangedTag.Insert); 
                };
                _MapSqlItemBeginReady[key].Add(item);
                return Result.NONE;
            }
            else
            {
                Result rs = Result.ERR;
                SqlConnectionItem.Do_SQLHELP(delegate(SQLHELP help)
                {
                    rs = help.ExecuteByParameterSQL(sql_Insert, ps);
                });

                if (rs.IsOK)
                {
                    EntityDB.CallEvent_ObjectChanged(this.Entity, ObjectChangedTag.Insert);
                }

                return rs;
            }
        }
        /// <summary>更新记录</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected Result Update(string sql_Update, ParameterTag[] ps)
        {
            int key = Thread.CurrentThread.ManagedThreadId;
            if (_MapSqlItemBeginReady.ContainsKey(key))
            {
                SqlItem item = new SqlItem(this, sql_Update, ps);
                //设置事务更新后的回调处理方法
                item.Action_CallBack = (obj) =>
                {
                    //更新时可能使用的是临时对象
                    EntityBase entity = this.Entity.EditTag == ObjectChangedTag.Temp ? this.Entity.EntitySourceMe : this.Entity;
                    EntityDB.CallEvent_ObjectChanged(entity, ObjectChangedTag.Update);
                };
                _MapSqlItemBeginReady[key].Add(item);
                return Result.NONE;
            }
            else
            {
                Result rs = Result.ERR;
                SqlConnectionItem.Do_SQLHELP(delegate(SQLHELP help)
                {
                    rs = help.ExecuteByParameterSQL(sql_Update, ps);
                });

                if (rs.IsOK)
                {
                    EntityDB.CallEvent_ObjectChanged(this.Entity, ObjectChangedTag.Update);
                }
                return rs;
            }
        }
        /// <summary>删除记录</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected Result Delete(string sql_Delete, ParameterTag[] ps)
        {
            int key = Thread.CurrentThread.ManagedThreadId;
            if (_MapSqlItemBeginReady.ContainsKey(key))
            {
                SqlItem item = new SqlItem(this, sql_Delete, ps);
                //设置事务更新后的回调处理方法
                item.Action_CallBack = (obj) =>
                {
                    EntityDB.CallEvent_ObjectChanged(this.Entity, ObjectChangedTag.Delete);
                };
                _MapSqlItemBeginReady[key].Add(item);
                return Result.NONE;
            }
            else
            {
                Result rs = Result.ERR;
                SqlConnectionItem.Do_SQLHELP(delegate(SQLHELP help)
                {
                    rs = help.ExecuteByParameterSQL(sql_Delete, ps);
                });

                if (rs.IsOK)
                {
                    EntityDB.CallEvent_ObjectChanged(this.Entity, ObjectChangedTag.Delete);
                }
                return rs;
            }
        }


        static readonly Dictionary<int, ThreadBaths> _MapSqlItemBeginReady = new Dictionary<int, ThreadBaths>();
        //static readonly Hashtable _MapSqlItemBeginReady = new Hashtable();
        /// <summary>开始事务(收集过程)->指定一个指处理名,可以为null</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void BeginReady()
        {
            int key = Thread.CurrentThread.ManagedThreadId;
            if (_MapSqlItemBeginReady.ContainsKey(key) == false)
            {
                _MapSqlItemBeginReady.Add(key, new ThreadBaths(key));
            }
            else
            {
                _MapSqlItemBeginReady[key].Count += 1;
            }
        }

        /// <summary>中止当前收集过程->指定一个指处理名,可以为null</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Stop()
        {
            lock (_MapSqlItemBeginReady)
            {
                int key = Thread.CurrentThread.ManagedThreadId;
                if (_MapSqlItemBeginReady.ContainsKey(key))
                {
                    _MapSqlItemBeginReady.Remove(key);
                }
            }
        }

        /// <summary>对在收集过程中操作进行批量处理->指定一个指处理名,可以为null</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Result Go()
        {
            int key = Thread.CurrentThread.ManagedThreadId;
            if (_MapSqlItemBeginReady.ContainsKey(key) == false)
            {
                Stop();
                return new Result(false, "事务未初始化,事务标识(" + key + ")不存在");
            }
            else
            {
                if (_MapSqlItemBeginReady[key].Count != 0)
                {
                    _MapSqlItemBeginReady[key].Count -= 1;
                    return Result.NONE;
                }
                else
                {
                    Result rs = new Result(false, "事务更新失败");
                    SqlConnectionItem.Do_SQLHELP(delegate(SQLHELP help)
                    {
                        rs = help.ExecuteBatch(_MapSqlItemBeginReady[key].SqlItems);
                    });

                    if (rs.IsOK)
                    {
                        foreach (SqlItem item in _MapSqlItemBeginReady[key].SqlItems)
                        {
                            EntityDB theEntityDB = item.TheIBase as EntityDB;
                            if (theEntityDB != null && item.Action_CallBack != null)
                            {
                                item.Action_CallBack(theEntityDB);//对每个更新实体时的通知
                            }
                        }
                    }

                    Stop();
                    return rs;
                }
            }

        }

        class ThreadBaths
        {
            public readonly int ThreadId = 0;
            public readonly List<SqlItem> SqlItems = new List<SqlItem>();
            public int Count = 0;
            public ThreadBaths(int thd_ID)
            {
                this.ThreadId = thd_ID;
            }

            public void Add(SqlItem item)
            {
                this.SqlItems.Add(item);
            }

            public void Add(IBase entity, string sql, ParameterTag[] ps)
            {
                SqlItem item = new SqlItem(entity, sql, ps);
                this.Add(item);
            }

            public override int GetHashCode()
            {
                return ThreadId.GetHashCode();
            }
        }



        #endregion=============END==========<<<


        /// <summary>对象改变时的通知事件</summary>
        public static event Action<EntityBase, ObjectChangedTag> Event_ObjectChanged;
        /// <summary>通知某个实体已发生了改变</summary>
        internal static void CallEvent_ObjectChanged(EntityBase entity, ObjectChangedTag changedTag)
        {
            Event_ObjectChanged(entity, changedTag);
            Debug.WriteLine( string.Format( "{0}-->{1}", entity.ToString(), changedTag.ToString() ) );
        }
        #region IBase 成员
        /// <summary>用于附加的验证(默认实现为NONE)</summary>
        public virtual Result Validate(ObjectChangedTag tag)
        {
            return Result.NONE;
        }

        #endregion
    }
}
