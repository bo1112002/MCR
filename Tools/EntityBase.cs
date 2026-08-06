using System;
using System.Collections.Generic;
using Tools.AccessDB;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Tools;
using Tools.Cache;
using System.Text;
using System.Collections;

namespace Tools
{

    /// <summary>所有业务对象的抽象类</summary>
    [Serializable]
    public abstract class EntityBase : IBase, IComparable, IDisposable, INotifyPropertyChanged
    {
        protected EntityBase()
        {
            this.CreateID(DateTime.Now);
            this.EditTag = ObjectChangedTag.Insert;
        }

        string _AutoID = string.Empty;
        /// <summary>自动编号</summary>
        public string AutoID
        {
            get { return _AutoID; }
            protected set { _AutoID = value.Trim(); }
        }

        private long _Timestamp;
        /// <summary>数据介质被更新时的标识</summary>
        public long Timestamp
        {
            get { return _Timestamp; }
            private set { _Timestamp = value; }
        }
        protected void SetTimespan(object obj)
        {
            byte[] bs = obj as byte[];
            if (bs != null)
            {
                Timestamp = BitConverter.ToInt64(bs, 0);
            }
        }


        object _Tag = null;
        /// <summary>获取或设置用于临时存储数据的对象</summary>
        public object Tag
        {
            get { return _Tag; }
            set
            {
                _Tag = value;
                this.NotifyPropertyChanged("Tag");
            }
        }

        readonly Dictionary<string, object> _ExtPropertys = new Dictionary<string, object>();
        /// <summary>扩展属性的键值对象组合</summary>
        public Dictionary<string, object> ExtPropertys
        {
            get { return _ExtPropertys; }
        }

        /// <summary>当前对象进行序列化后的处理</summary>
        public virtual void Serialize_After_Doing()
        {
            lock (this)
            {
                this.ExtPropertys.Clear();
            }
        }


        ObjectChangedTag _EditTag = ObjectChangedTag.None;
        /// <summary>设置或获取当前对象的编辑状态</summary>
        public virtual ObjectChangedTag EditTag
        {
            get { return _EditTag; }
            protected set
            {
                _EditTag = value;
                this.NotifyPropertyChanged("EditTag");
            }
        }

        /// <summary>获取当前对象的数据映射对象</summary>
        public EntityMaping GetEntityMaping()
        {
            EntityMaping em = EntityMapingMaps.EMapingMap[this.GetPrefixName()];
            return em;
        }
        /// <summary>
        /// 通过当前的数据映射对象来进行更新或查询的操作
        /// </summary>
        /// <param name="sqlKey">与映射文件中的匹配的sql语句结点的Key</param>
        /// <param name="ps">提供sql所需的参数集合</param>
        protected Result EntityMaping_Excute(string sqlKey, ParameterTag[] ps, Action<EntityReaderList> actionReader = null, SetReplaceSql replace_sql = null)
        {
            EntityMaping emp = this.GetEntityMaping();
            Result rs = emp.Excute(sqlKey, ps, this, actionReader, replace_sql);
            return rs;
        }
        /// <summary>批量更新</summary>
        protected Result EntityMaping_Excute(string sqlKey, IList<ParameterTag[]> ps)
        {
            EntityMaping emp = this.GetEntityMaping();
            Result rs = emp.Excute(sqlKey, ps, this);
            return rs;
        }


        /// <summary>如果当前是一个临时对象(ObjectChangedTag.Temp),则EntitySourceMe就是它的原始对象</summary>
        internal EntityBase EntitySourceMe;
        /// <summary>标记临时对象</summary>
        public Func<EntityBase> EidtTagSetTemp(EntityBase source, EntityMapingMaps mappings)
        {
            this.EditTag = ObjectChangedTag.Temp;
            EntityBase.SetValToTarget(source, this, mappings);
            EntitySourceMe = source;

            return delegate()
            {
                EntityBase.SetValToTarget(this, source, mappings);
                return source;
            };
        }

        /// <summary>把一条结果数据记录转换对象</summary>
        protected virtual void ToEntity(EntityReader reader)
        {
            foreach (string pName in reader.GetKeys())
            {
                PropertyInfo propInfo =
                    this.GetTypeBase().GetProperty(pName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetField);
                propInfo.SetValue(this, reader.GetValue(pName), null);
            }
        }

        /// <summary>获取数据更新接口对象</summary>
        protected virtual EntityDB GetAccessDB()
        {
            return null;
        }

        /// <summary>获取基础类型(用于反射原子类型,原子类型需要重写此方法)</summary>
        public virtual Type GetTypeBase()
        {
            return this.GetType();
        }


        #region IComparable 成员

        int IComparable.CompareTo(object obj)
        {
            EntityBase en = obj as EntityBase;
            if (en != null && string.IsNullOrEmpty(en.AutoID) == false && string.IsNullOrEmpty(this.AutoID) == false)
            {
                return this.AutoID.CompareTo(en.AutoID);
            }
            return this.GetHashCode().CompareTo(obj.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            EntityBase en = obj as EntityBase;
            if (en != null && string.IsNullOrEmpty(en.AutoID) == false && string.IsNullOrEmpty(this.AutoID) == false)
            {
                return this.AutoID.Equals(en.AutoID);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            //return base.GetHashCode();
            if (string.IsNullOrEmpty(this.AutoID) == false)
            {
                return this.AutoID.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        #endregion


        /// <summary>获取当前类型名的前缀名(当前类型的简称)</summary>
        protected virtual string GetPrefixName()
        {
            if (this.GetType().Name.Length > 4)
            {
                return this.GetType().Name.Substring(0, 4).ToUpper();
            }
            else
            {
                return this.GetType().Name;
            }

        }
        /// <summary>获取当前类型的简称</summary>
        public string GetTypeKey()
        {
            return this.GetPrefixName();
        }


        /// <summary>创建新的AutoID的值</summary>
        internal virtual void CreateID(DateTime newTime)
        {
            this.AutoID = this.CreateTagID(newTime);
        }

        static int _CountOnlyNum = 0;
        /// <summary>创建并返回一个全局唯一标识值</summary>
        public string CreateTagID(DateTime newTime)
        {
            lock (Result.OK)
            {
                string prefixName = this.GetPrefixName();
                //this.AutoID = SessionUser.UdpHandler.GetID(prefixName); //从服务器获取全局唯一ID值
                if (++_CountOnlyNum > 9999)
                {
                    _CountOnlyNum = 0;
                }

                long lg = long.Parse(newTime.ToString("yyMMddHHmmss"));
                long lg2 = Convert.ToInt64(PublicMethod.CreateAutoCode());
                string strTime = PublicMethod.To36String(System.Math.Abs(lg) + _CountOnlyNum) + PublicMethod.To36String(lg2);
                string tagID = string.Format("{0}-{1}", prefixName, strTime);
                return tagID;
            }
        }


        protected ParameterTag[] CreateParametes()
        {
            List<ParameterTag> list = new List<ParameterTag>();
            return list.ToArray();
        }


        /// <summary>字符串格式化</summary>
        public override string ToString()
        {
            return this.AutoID;
        }


        #region IDisposable 成员
        /// <summary>当前对象被释放时的处理</summary>
        public virtual void Dispose()
        {

        }
        #endregion

        #region INotifyPropertyChanged 成员
        /// <summary>通知某个属性发生改变</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>通知某个属性发生改变</summary>
        protected virtual void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        /// <summary>对象改变的事件</summary>
        public static event Action<EntityBase> Evt_EntityChange;
        /// <summary>调用Evt_EntityChange事件(由底层对象EntityMaping对象调用)</summary>
        internal void Call_Evt_EntityChange()
        {
            if (Evt_EntityChange != null)
                Evt_EntityChange(this);

        }


        #region============= 静态成员=========>>>

        /// <summary>获取数据访问的操作对象</summary>
        public static EntityDB GetAccessDB(EntityBase the)
        {
            return the.GetAccessDB();
        }

        /// <summary>把getObject对属性复制到setObject( getObject可以为setObject父类  )</summary>
        public static void SetValToTarget(EntityBase getObject, EntityBase setObject, EntityMapingMaps mapings)
        {
            //if (setObject.GetType() != getObject.GetType()) return;

            foreach (ParameterTag p in mapings[getObject.GetTypeKey()].Parames.Values)
            {
                PropertyInfo pInfo = getObject.GetTypeBase().GetProperty(p.PropertyName);
                if (pInfo != null && pInfo.CanWrite)
                {
                    object v = pInfo.GetValue(getObject, null);
                    pInfo.SetValue(setObject, v, null);
                }
            }
        }

        static ICache _MyICache = null;
        /// <summary>获取当前的缓存处理对象</summary>
        public static ICache GetMyICache()
        {
            lock (Result.NONE)
            {
                if (_MyICache == null)
                {
                    string cKey = KeyValueClass.Map_KVs["Cache"].Val;
                    KeyValueClass kvCache = KeyValueClass.Map_KVs["Cache"][cKey];
                    if (kvCache != null && string.IsNullOrEmpty(kvCache.M) == false)
                    {
                        _MyICache = kvCache.CreateM_Object() as ICache;
                    }
                    else
                    {
                        _MyICache = new NoCache();
                    }
                    //_MyICache = new CacheSafe(1000, 10000 * 1000);
                    //_MyICache = new NoCache();
                }
            }
            return _MyICache;
        }

        /// <summary>把记录中的数据转换为实体对象并添加到指定的集合中，[可选参数]actionAdding：在添加时对当前实体对象的处理,并返回新的实体对象,如果不为null,则会替换当前实体对象</summary>
        public static void AddToList<T>(IList<T> list, EntityReaderList readers, Func<EntityReader, T> createNew, Func<T, EntityReader, T> actionAdding = null) where T : EntityBase
        {
            foreach (EntityReader reader in readers)
            {
                string autoID = reader.GetValue("AutoID").ToString();
                T the = EntityBase.GetMyICache().Get(autoID) as T;
                if (the == null)
                {
                    the = createNew(reader);
                    the.ToEntity(reader);
                    EntityBase.GetMyICache().Set(the.AutoID, the);
                }

                if (actionAdding != null)
                {
                    T newT = actionAdding(the, reader);
                    if (newT != null)
                    {
                        the = newT;
                    }
                }
                list.Add(the);
            }
        }

        #endregion=============END==========<<<

        #region IBase 成员

        /// <summary>验证当前实体的业务有效性(默认为OK)</summary>
        public virtual Result Validate(ObjectChangedTag tag)
        {
            //if (this.EditTag == ObjectChangedTag.Insert && tag != ObjectChangedTag.Insert)
            //{
            //    return new Result(false, "当前操作状态与编辑状态不一致,操作被终止");
            //}

            StackTrace trace = new StackTrace(true);
            for (int i = 1; i < trace.GetFrames().Length; i++)
            {
                //StackFrame sf = trace.GetFrame(i);
                //MethodBase mm = sf.GetMethod();
                //object[] objs = mm.GetCustomAttributes(typeof(CompetenceKey), true);
                //Debug.WriteLine("===>{0},{1},{2}", mm.Name, mm.GetType().FullName,objs.Length );

                //if (objs.Length > 0)
                //{
                //    CompetenceKey cKey = objs[0] as CompetenceKey;
                //    Debug.WriteLine("===>{0}", cKey.Key);
                //    break;
                //}
            }

            return Result.OK;
        }

        #endregion

    }


    /*================================================*/




    /// <summary>视图类别</summary>
    public enum ViewType
    {
        Web,
        WinForms,
        WPF,
    }


    /// <summary>查找当前标记为MainMenusAttribute的键值集合</summary>
    public interface IFind_MainMenusAttribute
    {
        Dictionary<string, MainMenusAttribute> Maps { get; }
    }

    /// <summary>用于标识某个Page对象映射的页视图的URL</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MainMenusAttribute : Attribute
    {
        /// <summary>名称</summary>
        public readonly string ID;
        /// <summary>图标名</summary>
        public readonly string Icon;
        /// <summary>页视图的URL</summary>
        public readonly string Url;


        public MainMenusAttribute(string id, string url)
        {
            this.ID = id.Trim();
            this.Url = url.Trim();
        }
    }


    /// <summary>获取子集的接口</summary>
    public interface IChilds<T> where T : IBase
    {
        /// <summary>获取子集对象</summary>
        IList<T> GetChilds();
        /// <summary>创建子项对象</summary>
        T New(object obj);
        /// <summary>重新加载当前的子集对象</summary>
        void ReloadChilds();
    }




    /*
        /// <summary>应用于链表结构( 父子对象结构)的模板类->具体实现可能参考：MainMenus_View</summary>
        /// <typeparam name="T">源类型(需要包含的类)</typeparam>
        /// <typeparam name="V">包装类型(需要继承自View_Parent的子类)</typeparam>
        [Serializable]
        public abstract class View_Parent<T, V> where T : class, IChilds<T> ,new()
        {
            T _The;
            /// <summary>父对象</summary>
            public T The
            {
                get { return _The; }
                set
                {
                    _The = value;
                    this.SetThe();
                }
            }
        
            /// <summary>子集合对象</summary>
            public List<V> Childs
            {
                get
                {
                    List<V> childs = new List<V>();
                    if (_The != null)
                    {
                        IList<T> mms = _The.GetChilds();
                        if (mms != null && mms.Count > 0 )
                        {
                            foreach (T mm in mms)
                            {
                                V mv = NewView(mm);
                                childs.Add(mv);
                            }
                        }
                    }
                    return childs;
                }
                set { }
            }

            /// <summary>包含下一个子对象(next)的父对象(V)->形成链表结构的关联</summary>
            protected abstract V NewView(T next);
            /// <summary>当前设置父对象(The)时的处理，如果没有处理则可以空实现</summary>
            protected abstract void SetThe();
        }


    */









}
