using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Tools.Config;

namespace Tools.AccessDB
{

    /// <summary>
    /// 表示一个IDataReader中的一条记录的实体对象
    /// <remarks>
    /// (参考：ImpEntityReader)
    /// </remarks>
    /// </summary>
    public abstract class EntityReader 
    {
        readonly Dictionary<string, object> _Map = new Dictionary<string, object>();
        protected EntityReader(IDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                this._Map.Add(reader.GetName(i), reader[i]);
            }
        }

        /// <summary>指定索引值返回对应的值</summary>
        public virtual object GetValue(int index)
        {
            if (_Map.Values.Count > index)
            {
                return _Map.Values.ElementAt(index) ;
            }
            return null;
        }

        /// <summary>指定一个列名返回对应的值</summary>
        public virtual object GetValue(string sourceName)
        {
            return _Map[sourceName];
        }

        /// <summary>获取</summary>
        public virtual object GetValueByProperty( EntityBase entity ,  string propertyName)
        {
            try
            {
                EntityMaping em = entity.GetEntityMaping();
                string columnName = em.Parames[propertyName].SourceColumn;
                object objVal = this.GetValue(columnName);
                return objVal;
            }
            catch (Exception e)
            {
                MyConfig.Log(e.Message);
                return null;
            }
        }

        /// <summary>获取列名集合</summary>
        public string[] GetKeys()
        {
            return _Map.Keys.ToArray();
        }


        /// <summary>获取指定context上下文对象中name的对应值</summary>
        public abstract T GetValue<T>(object context, string name) ;
        /// <summary>依据context上下文对象进行批量赋值</summary>
        public abstract bool SetValues(object context);
    }




    /// <summary>用于对EntityReader进行操作接口</summary>
    public interface IEntityReader
    {
        EntityReader New(IDataReader reader);
    }


    /// <summary>对实体对象集合进行转换的中间容器</summary>
    public class EntityReaderList : List<EntityReader>
    {

        public EntityReaderList(IDataReader reader,IEntityReader readerHandler , int readerIndex )
        {
            this._ReaderIndex = readerIndex;

            while (reader.Read())
            {
                EntityReader theNew = readerHandler.New(reader);
                this.Add(theNew);
            }
        }

        public EntityReaderList(IDataReader reader, IEntityReader readerHandler)
            : this(reader, readerHandler, 0)
        { }


        int _ReaderIndex = 0;
        /// <summary>结果集的索引值</summary>
        public int ReaderIndex
        {
            get { return _ReaderIndex; }
            private set { _ReaderIndex = value; }
        }
    }

}
