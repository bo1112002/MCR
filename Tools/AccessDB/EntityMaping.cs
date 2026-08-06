using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Data;
using System.Xml;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Tools.AccessDB
{

    /// <summary>
    /// 数据访问与实体对象间的映射类
    /// </summary>
    [Serializable]
    public class EntityMaping : ISerializable, IXmlSerializable
    {
        #region *****MyRegion********
        string _Key = string.Empty;
        /// <summary>简称</summary>
        public string Key
        {
            get { return _Key; }
        }

        string _MapingType = string.Empty;
        /// <summary>当前映射的实体类型</summary>
        public string MapingType
        {
            get { return _MapingType; }
        }

        string _ParentKey = string.Empty;
        /// <summary>所继承的类型（这里的继承是指物理继承(表继承)并非逻辑继承(类型继承)）</summary>
        public string ParentKey
        {
            get { return _ParentKey; }
        }


        string _TableName = string.Empty;
        /// <summary>数据表的名称</summary>
        public string TableName
        {
            get { return _TableName; }
        }

        string _DataBaseKey = "";
        /// <summary>数据源访问的标识符</summary>
        public string DataBaseKey
        {
            get { return _DataBaseKey; }
        }


        /*
        string _IncludFile = string.Empty;
        /// <summary>包括的子配置文件</summary>
        public string IncludFile
        {
            get { return _IncludFile; }
        }
        */



        readonly Dictionary<string, ParameterTag> _Parames = new Dictionary<string, ParameterTag>();
        /// <summary>获取映射实体的参数集合</summary>
        public Dictionary<string, ParameterTag> Parames
        {
            get { return _Parames; }
        }


        readonly Dictionary<string, SqlClass> _UItems = new Dictionary<string, SqlClass>();
        /// <summary>获取实体的sql集合</summary>
        public Dictionary<string, SqlClass> UItems
        {
            get { return _UItems; }
        }





        #endregion ------------------------------------------------------------------------------


        public EntityMaping()
        { }
        public EntityMaping(string fullTypeName, string tableName, string dbKey)
        {
            this._MapingType = fullTypeName;
            this._TableName = tableName;
            this._DataBaseKey = dbKey;
        }



        /// <summary>执行数据库的操作</summary>
        /// <param name="sqlKey">sql标识</param>
        /// <param name="ps">参数</param>
        /// <param name="theEntity">操作对象</param>
        /// <param name="actionReader">如果是查询，并自定义处理查询结果则需这个参数的实例，
        /// 再如果查询返回DataTable则为null,DataTable对象会放在Result.Data</param>
        public Result Excute(string sqlKey, ParameterTag[] ps, EntityBase theEntity, Action<EntityReaderList> actionReader = null, SetReplaceSql replace_sql = null)
        {
            Result rs = Result.NONE;
            SqlClass sql =this.UItems[sqlKey];

            if (replace_sql != null)
            {
                sql.SetChangeSql(replace_sql);
            }

            SqlConnectionItem.Do_SQLHELP(this.DataBaseKey, delegate(SQLHELP help) {

                if (sql.SqlType == E_SqlType.Edit)
                {
                    rs = help.ExecuteByParameterSQL(sql.SqlString, ps);
                    if (rs.IsOK) 
                    {
                        rs = new Result(true, "数据更新成功"); 
                        //触发对象改变的事件
                        theEntity.Call_Evt_EntityChange();
                    }

                }
                else if (sql.SqlType == E_SqlType.View)
                {
                    rs = new Result(true);
                    if (actionReader == null)
                    {
                        DataTable table = help.ExecuteGetDataTableSQL(sql.SqlString, ps);
                        rs.Data = table;
                    }
                    else
                    {
                        help.ExecuteGetDataReaderSQL(actionReader, sql.SqlString, ps);
                    }
                }

            });
            return rs;
        }
        /// <summary>批量更新</summary>
        public Result Excute(string sqlKey, IList<ParameterTag[]> ps, EntityBase entityBase )
        {
            Result rs = Result.NONE;
            SqlClass sql = this.UItems[sqlKey];
            SqlConnectionItem.Do_SQLHELP(this.DataBaseKey, delegate(SQLHELP help)
            {
                if (sql.SqlType != E_SqlType.Edit)
                {
                    rs = new Result(false, "sqlKey需指定为更新标识(E_SqlType.Edit)");
                }
                else
                {
                    List<SqlItem> itms = new List<SqlItem>() ;
                    foreach(ParameterTag[] p in ps  )
                    {
                        SqlItem itm = new SqlItem( entityBase , sql.SqlString , p ) ;
                        itms.Add( itm) ;
                    }
                    rs = help.ExecuteBatch(itms);
                }
            });

            return rs;
        }



        #region ISerializable 成员

        /// <summary></summary>
        protected EntityMaping(SerializationInfo info, StreamingContext context)
        {
            this._Key = info.GetString("Key");
            this._TableName = info.GetString("TableName");
            this._ParentKey = info.GetString("ParentKey");
            this._MapingType = info.GetString("MapingType");
            this._DataBaseKey = info.GetString("DataBaseKey");

            this._Parames = info.GetValue("Parames", typeof(IDictionary)) as Dictionary<string, ParameterTag>;
            this._UItems = info.GetValue("UItems", typeof(IDictionary)) as Dictionary<string, SqlClass>;

        }


        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Key", this._Key);
            info.AddValue("TableName", this._TableName);
            info.AddValue("ParentKey", this._ParentKey);

            info.AddValue("MapingType", this._MapingType);
            info.AddValue("DataBaseKey", this._DataBaseKey);
            info.AddValue("Parames", Parames);
            info.AddValue("UItems", UItems);
        }

        #endregion

        #region IXmlSerializable 成员

        XmlSchema _xmls;
        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            _xmls = new XmlSchema();
            return null;
        }

        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToFirstAttribute();
            do
            {
                //加载子配置文件
                if (reader.LocalName == "IncludFile")
                {
                    string fileName = reader.ReadContentAs(typeof(string), null).ToString();
                    if (File.Exists(fileName) == false)
                    {
                        //fileName =  Directory.GetCurrentDirectory().TrimEnd('\\') + "\\" + fileName;
                        fileName = AppSettingsBase.GetExecuting_DIR()  + fileName;
                        if (File.Exists(fileName) == false)
                            continue;
                    }
                    //记录当前子配置的文件路径
                    EntityMapingMaps.AddChildsFileName(fileName);
                    continue;
                }



                if (reader.LocalName == "Key")
                {
                    this._Key = reader.ReadContentAs(typeof(string), null).ToString();
                    //if (this._Key == "OD")
                    //Debug.WriteLine("this.Key---->" + this._Key);
                }
                else if (reader.LocalName == "MapingType")
                    this._MapingType = reader.ReadContentAs(typeof(string), null).ToString();
                else if (reader.LocalName == "TableName")
                    this._TableName = reader.ReadContentAs(typeof(string), null).ToString();
                else if (reader.LocalName == "DataBaseKey")
                    this._DataBaseKey = reader.ReadContentAs(typeof(string), null).ToString();
                else if (reader.LocalName == "ParentKey")
                    this._ParentKey = reader.ReadContentAs(typeof(string), null).ToString();
            } while (reader.MoveToNextAttribute());

            reader.Read();

            while (reader.Read())
            {
                if (reader.LocalName != "P")
                    break;
                ParameterTag theP = new ParameterTag();
                reader.MoveToFirstAttribute();
                do
                {
                    if (reader.LocalName == "PropertyName")
                    {
                        theP.PropertyName = reader.ReadContentAs(typeof(string), null).ToString();
                        //Debug.WriteLine("theP.PropertyName---->" + theP.PropertyName);
                    }
                    else if (reader.LocalName == "SourceColumn")
                        theP.SourceColumn = reader.ReadContentAs(typeof(string), null).ToString();
                    else if (reader.LocalName == "DbType")
                        theP.DbType = (E_DbType)Enum.Parse(typeof(E_DbType), reader.ReadContentAs(typeof(string), null).ToString());
                    else if (reader.LocalName == "Size")
                        theP.Size = (int)reader.ReadContentAs(typeof(int), null);
                    else if (reader.LocalName == "Direction")
                        theP.Direction = (E_DbDirection)Enum.Parse(typeof(E_DbDirection), reader.ReadContentAs(typeof(string), null).ToString());
                } while (reader.MoveToNextAttribute());
                this.Parames.Add(theP.PropertyName, theP);
            }
            reader.Read();

            if (reader.LocalName == "UItems")
            {
                while (reader.Read())
                {
                    if (reader.LocalName != "Sql")
                        break;
                    SqlClass theSql = new SqlClass();
                    reader.MoveToFirstAttribute();
                    do
                    {
                        if (reader.LocalName == "Key")
                        {
                            theSql.Key = reader.ReadContentAs(typeof(string), null).ToString();
                        }
                        else if (reader.LocalName == "SqlType")
                        {
                            theSql.SqlType = (E_SqlType)Enum.Parse(typeof(E_SqlType), reader.ReadContentAs(typeof(string), null).ToString());
                        }
                    } while (reader.MoveToNextAttribute());

                    reader.Read();
                    theSql.SqlString = reader.ReadContentAsString();
                    this.UItems.Add(theSql.Key, theSql);
                }
            }
        }

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("EntityMaping");

            writer.WriteAttributeString("Key", this._Key);
            writer.WriteAttributeString("TableName", this._TableName);
            writer.WriteAttributeString("ParentKey", this._ParentKey);
            writer.WriteAttributeString("MapingType", this._MapingType);
            writer.WriteAttributeString("TableName", this._TableName);
            writer.WriteAttributeString("DataBaseKey", this._DataBaseKey);

            writer.WriteStartElement("Parames");
            foreach (string k in Parames.Keys)
            {
                writer.WriteStartElement("P");
                ParameterTag p = Parames[k];
                writer.WriteAttributeString("PropertyName", p.PropertyName);
                writer.WriteAttributeString("SourceColumn", p.SourceColumn);
                writer.WriteAttributeString("DbType", p.DbType.ToString());
                writer.WriteAttributeString("Size", p.Size.ToString());
                writer.WriteAttributeString("Direction", p.Direction.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("UItems");
            foreach (string k in UItems.Keys)
            {
                SqlClass sqlObj = UItems[k];
                writer.WriteStartElement("Sql");
                writer.WriteAttributeString("Key", k);
                writer.WriteAttributeString("SqlType", sqlObj.SqlType.ToString());
                writer.WriteString(sqlObj.SqlString);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();


            writer.WriteEndElement();

        }

        #endregion


        #region *****静态成员********

        /// <summary>EntityMaping集合对象写入指定的文件中</summary>
        public static void WriteXMLTemplateFile(string filePath, params EntityMaping[] ems)
        {
            FileStream fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            foreach (EntityMaping em in ems)
            {
                byte[] bs = SerializeObjectClass.SerializObjectForXml(em);
                fs.Write(bs, 0, bs.Length);
            }
            fs.Flush();
            fs.Close();
        }
        /// <summary></summary>
        public static EntityMaping ReadXMLTemplateFile(string filePath)
        {
            byte[] bs = File.ReadAllBytes(filePath);
            EntityMaping em = SerializeObjectClass.DeserializObjectForXml<EntityMaping>(bs);
            return em;
        }


        #endregion ------------------------------------------------------------------------------






        
    }
    /// <summary>EntityMaping类型的键值对集合</summary>
    [Serializable]
    public class EntityMapingMaps : Dictionary<string, EntityMaping>, IXmlSerializable
    {
        public EntityMapingMaps()
        { 
        }

        public EntityMaping this[Type t]
        {
            get
            {
                if (this.ContainsKey(t.FullName))
                {
                    return this[t.FullName];
                }
                return null;
            }
        }

        #region IXmlSerializable 成员

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        string AddParentKey(EntityMaping em)
        {
            if (em.ParentKey.Trim() == string.Empty || em.Key == em.ParentKey || this.ContainsKey(em.ParentKey)== false)
            {
                return string.Empty; 
            }
            string key = AddParentKey(this[em.ParentKey]); //向下找到叶结点
            

            //从叶结点向根结点一路返回的处理
            foreach (ParameterTag p in this[em.ParentKey].Parames.Values)
            {
                if (em.Parames.ContainsKey(p.PropertyName) == false)
                {
                    em.Parames.Add(p.PropertyName, p);
                }
            }
            return key;
        }
        /// <summary>添加子集到当前集合中</summary>
        public void Add(EntityMapingMaps childs)
        {
            foreach (EntityMaping entityM in childs.Values)
            {
                if (this.ContainsKey(entityM.Key) == false)
                {
                    this.Add(entityM.Key, entityM);
                }
            }
        }


        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName != "EntityMaping")
                    continue;

                EntityMaping the = new EntityMaping();
                (the as IXmlSerializable).ReadXml(reader);
                if (string.IsNullOrEmpty(the.Key) == false)
                {
                    this.Add(the.Key, the);
                }
                reader.Read();
            }

            reader.Close();

            foreach (EntityMaping em in this.Values)
            {
                AddParentKey(em);
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            foreach (IXmlSerializable the in this.Values)
            {
                the.WriteXml(writer);
            }
        }

        #endregion
        /// <summary>指定一个文件反序化成一个EntityMapingMaps类型对象(如果处理过程失败，则返回null)</summary>
        public static EntityMapingMaps GetEntityMapingMaps(string filePath)
        {
            EntityMapingMaps  theMaps = null ;
            if (File.Exists(filePath))
            {
                byte[] bs = File.ReadAllBytes(filePath);
                object obj = SerializeObjectClass.DeserializObjectForXml(bs, typeof(EntityMapingMaps));
                theMaps = obj as EntityMapingMaps;
            }
            return theMaps;
        }


        readonly static List<string> _ChildsFileNames = new List<string>();
        /// <summary>记录当前子配置文件的路径(可以是相对主配置文件的相对路径) ，文件路径如果已存在，则将被忽略</summary>
        public static void AddChildsFileName(string fileName)
        {
            string path = fileName.Trim();
            if (File.Exists(path) == false)
            {
                string pathRoot = Path.GetFullPath(KeyValueClass.Map_KVs["EntityMapping_XML"].Val);
                path =  Path.GetDirectoryName( pathRoot ).TrimEnd('\\') + "\\" +  fileName.Trim();
            }
            if (_ChildsFileNames.Contains(path) == false && File.Exists(path) == true)
            {
                _ChildsFileNames.Add(path);
            }
        }


        readonly static EntityMapingMaps _EMapingMap_Root = new EntityMapingMaps() ;
        /// <summary>获取当前实体类型与数据表的映射关系的描述对象</summary>
        public static EntityMapingMaps EMapingMap
        {
            get
            {
                if (_EMapingMap_Root.Count == 0  )
                {
                    lock (_EMapingMap_Root)
                    {
                        string path = Path.GetFullPath(KeyValueClass.Map_KVs["EntityMapping_XML"].Val);
                        if (File.Exists(path) == false)
                            throw new Exception("配置文件下的EntityMapping_XML结点项没有指定或不存在的文件路径");
                        EntityMapingMaps maps = EntityMapingMaps.GetEntityMapingMaps(path);

                        if (maps != null)
                        {
                            foreach (EntityMaping entityM in maps.Values)
                            {
                                if (_EMapingMap_Root.ContainsKey(entityM.Key) == false)
                                {
                                    _EMapingMap_Root.Add(entityM.Key, entityM);
                                }
                            }

                            //加载子配置
                            foreach (string thePath in _ChildsFileNames)
                            {
                                EntityMapingMaps childMaps = EntityMapingMaps.GetEntityMapingMaps(thePath);
                                if (childMaps != null)
                                {
                                    _EMapingMap_Root.Add(childMaps);
                                }
                            }

                        }
                    }

                }
                return _EMapingMap_Root;
            }
        }

       


    }

    /// <summary>实体映射对象的接口</summary>
    public interface IEntityMapingFormFils
    {
        /// <summary>通过一个实体类型获取实体的映射对象</summary>
        EntityMaping GetEntityMapings(Type entityType);
    }

    /// <summary>
    /// 指定某个sql的类型
    /// </summary>
    public enum E_SqlType
    {
        /// <summary>更新</summary>
        Edit,
        /// <summary>查询</summary>
        View
    }

    [Serializable]
    public class SqlClass
    {

        string _Key = string.Empty;
        public string Key
        {
            get { return _Key; }
            set { _Key = value; }
        }

        E_SqlType _SqlType = E_SqlType.Edit;
        /// <summary>sql语句的类型：更新还是查询</summary>
        public E_SqlType SqlType
        {
            get { return _SqlType; }
            set { _SqlType = value; }
        }

        string _SqlString = "";
        /// <summary>sql语句</summary>
        public string SqlString
        {
            get 
            {
                if (_ReplaceSQL != null)
                {
                    return _SqlString.Replace(_ReplaceSQL.TagKey, _ReplaceSQL.ReplaceSql);
                }
                else
                {
                    return _SqlString;
                }
            }
            set { _SqlString = value; }
        }


        SetReplaceSql _ReplaceSQL = null;
        /// <summary>设置需要的改变sql语句对象</summary>
        public void SetChangeSql(SetReplaceSql replace_sql)
        {
            _ReplaceSQL = replace_sql;
        }

    }



    ///// <summary>用于描述Sql参数对象的集合类</summary>
    //public class ParameterList : Dictionary<string, ParameterTag>
    //{ }


}
