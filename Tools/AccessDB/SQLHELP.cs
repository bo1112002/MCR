//#define Debug

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Reflection;
using System.Diagnostics;
using System.Transactions;
using Tools.Config;
using System.Runtime.CompilerServices;

namespace Tools.AccessDB
{
    

    /// <summary>数据库操作类</summary>
    public class SQLHELP : IDisposable
    {
        SqlConnection _Conn;
        IEntityReader _MyReaderHander;
        public SQLHELP(SqlConnection conn ,IEntityReader readerHandler )
        {
            _Conn = conn;
            _MyReaderHander = readerHandler;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Result ExecuteByParameterSQL(string sql, params ParameterTag[] pts)
        {
            SqlParameter[] ps = ParameterTag.GetSqlParameters(pts);
            SqlCommand comm = new SqlCommand();
            try
            {
                comm.CommandText = sql;
                comm.Connection = _Conn;
                if (ps != null)
                {
                    comm.Parameters.AddRange(ps);
                }

                if (_Conn.State != ConnectionState.Open)
                    _Conn.Open();
                if (comm.ExecuteNonQuery() > 0)
                {
                    return Result.OK;
                }
                return new Result(false, "更新记录数为0");
            }
            catch (SqlException err)
            {
#if Debug
                throw err;
                
#else
                MyConfig.Log(sql + "\r\n" + err.Message);
                return new Result(false, err.Message);
#endif
            }
            finally
            {
                _Conn.Close();
            }
        }

        /// <summary>批量更新</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public Result ExecuteBatch(IList<SqlItem> list)
        {
            try
            {
                using (TransactionScope ts = new TransactionScope())
                {
                    _Conn.Open();
                    foreach (SqlItem the in list)
                    {
                        SqlCommand comm = new SqlCommand(the.Sql, _Conn);
                        SqlParameter[] ps = ParameterTag.GetSqlParameters(the.Parames);
                        comm.Parameters.AddRange(ps);
                        int rs = comm.ExecuteNonQuery();
                        if (rs <= 0)
                        {
                            return new Result(false, "无更新记录(操作返回数为0)");
                        }
                    }
                    ts.Complete();
                }
                return Result.OK;
            }
            catch (Exception err)
            {
#if Debug
                throw err;
                
#else
                byte[] bs = SerializeObjectClass.SerializObjectForXml(list);
                string ss = Encoding.Default.GetString(bs);
                MyConfig.Log(ss + "\r\n" + err.Message);
                return new Result(false, err.Message);
#endif
            }
            finally
            {
                _Conn.Close();
            }
        }

        #region============= ReadData=========>>>

        public EntityReaderList ExecuteGetDataReaderSQL(string sql)
        {
            EntityReaderList list = null;
            SqlDataReader reader2;
            SqlCommand comm = new SqlCommand();
            try
            {
                comm.Connection = _Conn;
                comm.CommandText = sql;

                if (_Conn.State != ConnectionState.Open)
                    _Conn.Open();
                reader2 = comm.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.CloseConnection);
                list = new EntityReaderList(reader2, this._MyReaderHander );
                reader2.Close();
            }
            catch (SqlException err)
            {
#if Debug
                throw err;
                
#else
                MyConfig.Log(sql + "\r\n" + err.Message);
                return null;
#endif
            }
            finally
            {
                _Conn.Close();
            }
            return list ;
        }

        public void ExecuteGetDataReaderSQL(string sqlstr, Action<EntityReaderList> action)
        {
            SqlDataReader reader2;
            SqlCommand comm = new SqlCommand();
            try
            {
                comm.Connection = _Conn;
                comm.CommandText = sqlstr;

                if (_Conn.State != ConnectionState.Open)
                    _Conn.Open();
                reader2 = comm.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.CloseConnection);
                EntityReaderList list = new EntityReaderList(reader2, this._MyReaderHander);
                action(list);
                reader2.Close();
            }
            catch (SqlException err)
            {
#if Debug
                throw err;
                
#else
                MyConfig.Log(sqlstr + "\r\n" + err.Message);
#endif
            }
            finally
            {
                _Conn.Close();
            }
        }

        public EntityReaderList ExecuteGetDataReaderSQL(string sqlstr, params ParameterTag[] pts)
        {
            SqlParameter[] ps = ParameterTag.GetSqlParameters(pts);
            EntityReaderList list = null ;
            SqlCommand comm = new SqlCommand();
            try
            {
                comm.Connection = _Conn;
                comm.CommandType = CommandType.Text;
                comm.CommandText = sqlstr;

                if (ps != null)
                {
                    comm.Parameters.AddRange(ps);
                }

                if (_Conn.State != ConnectionState.Open)
                    _Conn.Open();
                SqlDataReader reader2 = comm.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.CloseConnection);
                list = new EntityReaderList(reader2, this._MyReaderHander);
                reader2.Close();
            }
            catch (SqlException err)
            {
#if Debug
                throw err;
                
#else
                MyConfig.Log(sqlstr + "\r\n" + err.Message);
                return null;
#endif
            }
            finally
            {
                _Conn.Close();
            }
            return list;
        }

        public void ExecuteGetDataReaderSQL(Action<EntityReaderList> action, string sqlstr, params ParameterTag[] pts)
        {
            SqlParameter[] ps = ParameterTag.GetSqlParameters(pts);
            SqlDataReader reader2;
            SqlCommand comm = new SqlCommand();
            try
            {
                comm.Connection = _Conn;
                comm.CommandType = CommandType.Text;
                comm.CommandText = sqlstr;
                if (ps != null)
                {
                    comm.Parameters.AddRange(ps);
                }

                if (_Conn.State != ConnectionState.Open)
                    _Conn.Open();
                reader2 = comm.ExecuteReader();

                int indexRead = -1;
                do
                {
                    indexRead++ ;
                    EntityReaderList list = new EntityReaderList(reader2, this._MyReaderHander, indexRead );
                    action(list);

                } while (reader2.NextResult());

                reader2.Close();
            }
            catch (SqlException err)
            {
#if Debug
                throw err;
#else
                MyConfig.Log(sqlstr + "\r\n" + err.Message);
#endif
            }
            finally
            {
                _Conn.Close();
            }
        }

        public DataTable ExecuteGetDataTableSQL(string sqlstr)
        {
            SqlCommand comm = new SqlCommand();
            try
            {
                DataTable dataTable = new DataTable();
                comm.CommandText = sqlstr;
                comm.Connection = _Conn;

                if (_Conn.State != ConnectionState.Open)
                    _Conn.Open();

                SqlDataAdapter adapter = new SqlDataAdapter(comm);
                adapter.Fill(dataTable);
                comm.ExecuteNonQuery();
                return dataTable;
            }
            catch (SqlException err)
            {
#if Debug
                throw err;
                
#else
                MyConfig.Log(sqlstr + "\r\n" + err.Message);
                return null;
#endif
            }
            finally
            {
                _Conn.Close();
            }
        }

        public DataTable ExecuteGetDataTableSQL(string sqlstr, params ParameterTag[] pts)
        {
            SqlParameter[] ps = ParameterTag.GetSqlParameters(pts);
            SqlCommand comm = new SqlCommand();
            DataTable dataTable = new DataTable("T1");
            try
            {
                comm.CommandText = sqlstr;
                comm.Connection = _Conn;
                if (ps != null)
                {
                    comm.Parameters.AddRange(ps);
                }
                SqlDataAdapter adapter = new SqlDataAdapter(comm);

                if (_Conn.State != ConnectionState.Open)
                    _Conn.Open();
                adapter.Fill(dataTable);
                return dataTable;
            }
            catch (SqlException err)
            {
#if Debug
                throw err;
                
#else
                MyConfig.Log(sqlstr + "\r\n" + err.Message);
                return null;
#endif
            }
            finally
            {
                _Conn.Close();
            }
        }

        #endregion=============END==========<<<

        //===============================================================================================

        /// <summary>
        /// 查询sql的方法 , 返回一个值(第一行第一列的值)
        /// </summary>
        public object QueryValue(string sqlstr, params ParameterTag[] pts)
        {
            SqlParameter[] ps = ParameterTag.GetSqlParameters(pts);
            SqlCommand comm = new SqlCommand(sqlstr);
            try
            {
                comm.Connection = _Conn;
                comm.CommandType = System.Data.CommandType.Text;
                if (ps != null)
                {
                    comm.Parameters.AddRange(ps);
                }

                if (_Conn.State != ConnectionState.Open)
                    _Conn.Open();
                return comm.ExecuteScalar();
            }
            catch (Exception err)
            {
#if Debug
                throw err;
                
#else
                MyConfig.Log(sqlstr + "\r\n" + err.Message);
                return null;
#endif
            }
            finally
            {
                _Conn.Close();
            }
        }

        /*
        /// <summary>创建一个SqlParameter对象，typeLen如果小于等于0，则不对size进行赋值处理s</summary>
        public static ParameterTagAttribute NewParameter(string name, object value, E_DbType dbType, int typeLen)
        {
            ParameterTagAttribute p = new ParameterTagAttribute(name, dbType, typeLen);
            SqlParameter p = new SqlParameter(name, value);
            p.E_DbType = dbType;
            if (typeLen > 0)
            {
                p.Size = typeLen;
            }
            return p;
        }

        /// <summary>创建一个SqlParameter对象，typeLen如果小于等于0，则不对size进行赋值处理s</summary>
        public static SqlParameter NewParameter_Out(string name, object value, E_DbType dbType, int typeLen)
        {
            SqlParameter p = new SqlParameter(name, value);
            p.SqlDbType = dbType;
            p.Size = typeLen;
            p.Direction = ParameterDirection.Output;
            return p;
        }
*/

        #region IDisposable 成员

        void IDisposable.Dispose()
        {
            if (_Conn != null)
                _Conn.Close();
        }

        #endregion
    }

    /// <summary>进行指量更新的项结构类</summary>
    [Serializable]
    public class SqlItem
    {
        public SqlItem() { }
        public SqlItem(IBase theIBase ,string sql, params ParameterTag[] ps )
        {
            this._Sql = sql;
            this._Parames = ps;
            this._TheIBase = theIBase;
        }

        readonly string _Sql = string.Empty;
        /// <summary>要执行的更新的sql</summary>
        [System.Xml.Serialization.XmlAttribute]
        public string Sql
        {
            get { return _Sql; }
        }
        readonly ParameterTag[] _Parames;
        /// <summary>sql需要的参数集合</summary>
        [System.Xml.Serialization.XmlElement]
        public ParameterTag[] Parames
        {
            get { return _Parames; }
        }
        
        
        [System.Xml.Serialization.SoapIgnore]
        Action<object> _Action_CallBack;
        /// <summary>获取或设置更新后的处理方法对象</summary>
        [System.Xml.Serialization.XmlIgnore]
        public Action<object> Action_CallBack
        {
            get { return _Action_CallBack; }
            set { _Action_CallBack = value; }
        }

        [System.Xml.Serialization.SoapIgnore]
        readonly IBase _TheIBase;
        /// <summary>需要进行更新的实体对象</summary>
        [System.Xml.Serialization.XmlIgnore]
        public IBase TheIBase
        {
            get { return _TheIBase; }
        }

    }

    /// <summary>数据连接池->对象</summary>
    public class SqlConnectionItem : PoolableToKey
    {
        SQLHELP _Help;
        public SQLHELP Help
        {
            get { return _Help; }
            private set { _Help = value; }
        }


        static IEntityReader _ReaderHandler = null ;
        string _Kye;
        public override string Key
        {
            get
            {
                return _Kye;
            }
            set
            {
                _Kye = value;

                KeyValueClass kv =  KeyValueClass.Map_KVs["connectionStrings"][value.Trim()];
                if (kv == null) {
                    throw new Exception("数据访问配置结点(" + value + ")不存在");
                }
                string connString = KeyValueClass.Map_KVs["connectionStrings"][value.Trim()].Val;
                SqlConnection conn = new SqlConnection(connString);
                if (_ReaderHandler == null)
                {
                    _ReaderHandler = KeyValueClass.Map_KVs["connectionStrings"].CreateM_Object() as IEntityReader;
                }

                Help = new SQLHELP(conn, _ReaderHandler );
            }
        }

        /// <summary>对数据库访问标识为ConnA的</summary>
        public static void Do_SQLHELP(Action<SQLHELP> action)
        {
            string dbKey = KeyValueClass.Map_KVs["connectionStrings"].Val; //获取默认的连接标识
            SqlConnectionItem item = ObjectPoolToKey.New<SqlConnectionItem>(dbKey);
            action(item.Help);
            ObjectPoolToKey.Delete(item);
        }

        /// <summary>通过指定的标识来访问数据库</summary>
        public static void Do_SQLHELP( string dbKey , Action<SQLHELP> action)
        {
            SqlConnectionItem item = ObjectPoolToKey.New<SqlConnectionItem>(dbKey);
            action(item.Help);
            ObjectPoolToKey.Delete(item);
        }

        /// <summary>依据数据库访问标识,返回SQLHELP对象，如果访问标识为null则返回默认的SQLHELP对象</summary>
        public static SQLHELP CreateSQLHELP(string helpTag)
        {
            if (string.IsNullOrEmpty(helpTag))
            {
                helpTag = KeyValueClass.Map_KVs["connectionStrings"].Val;
            }
            SqlConnectionItem item = new SqlConnectionItem();
            item.Key = helpTag;
            return item.Help;
        }

        /*
        static readonly List<SqlItem> _SqlItemList = new List<SqlItem>() ;
        public static int AddSqlItemToBath(IList<SqlItem> list)
        {
            _SqlItemList.AddRange( list) ;
            return _SqlItemList.Count ;
        }
        public static Result ExcuteSqlItemToBath()
        {
            SQLHELP help = CreateSQLHELP( null ) ;
            Result rs = help.ExecuteBatch(_SqlItemList);

            if (rs.IsOK == false)
            {
                List<SqlItem> list = new List<SqlItem>(  _SqlItemList ) ;
                rs.Data = list;
            }
            _SqlItemList.Clear();
            return rs;
        }
*/


        static void Test(Action<SQLHELP> action)
        {
            SqlConnectionItem.Do_SQLHELP(delegate(SQLHELP help)
            {
                object rsObj = help.QueryValue("Select * From Table",
                    new ParameterTag("@name", "Tom", E_DbType.VarChar, 50),
                    new ParameterTag("@Index", 100, E_DbType.Int, -1),
                    new ParameterTag("@Return", string.Empty, E_DbType.VarChar, 50, E_DbDirection.Output));
            });
        }

    }


    /*================================================*/

    

}
