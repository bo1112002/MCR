using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;

namespace Tools.AccessDB
{


    /// <summary>用于映射存储过程(Sql语句)中的参数的类</summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ParameterTag : Attribute
    {
        #region============= 属性=========>>>

        string _PropertyName = "";
        /// <summary>要映射类型中属性的名称</summary>
        [System.Xml.Serialization.XmlAttribute]
        public string PropertyName
        {
            get { return _PropertyName; }
            set { _PropertyName = value; }
        }

        string _SourceColumn = "";
        /// <summary>源列的名称(如果不填则取当前属性名)</summary>
        [System.Xml.Serialization.XmlAttribute]
        public string SourceColumn
        {
            get { return _SourceColumn; }
            set { _SourceColumn = value; }
        }

        E_DbType dbType = E_DbType.NVarChar;
        /// <summary>E_DbType 值之一</summary>
        [System.Xml.Serialization.XmlAttribute]
        public E_DbType DbType
        {
            get { return dbType; }
            set { dbType = value; }
        }

        int size = -1;
        /// <summary>参数的长度</summary>
        [System.Xml.Serialization.XmlAttribute]
        public int Size
        {
            get { return size; }
            set { size = value; }
        }
        E_DbDirection direction = E_DbDirection.Input;
        /// <summary>ParameterDirection 值之一</summary>
        [System.Xml.Serialization.XmlAttribute]
        public E_DbDirection Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        bool isNullable = true;
        /// <summary>如果字段的值可为空，则为 true；否则为 false</summary>
        [System.Xml.Serialization.XmlAttribute]
        public bool IsNullable
        {
            get { return isNullable; }
            set { isNullable = value; }
        }
        byte precision = 15;
        /// <summary>要将 Value 解析为的小数点左右两侧的总位数</summary>
        [System.Xml.Serialization.XmlAttribute]
        public byte Precision
        {
            get { return precision; }
            set { precision = value; }
        }
        byte scale = 4;
        /// <summary>要将 Value 解析为的总小数位数</summary>
        [System.Xml.Serialization.XmlAttribute]
        public byte Scale
        {
            get { return scale; }
            set { scale = value; }
        }


        object _Value = string.Empty;
        /// <summary>参数值</summary>
        public object Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        #endregion=============END==========<<<


        /// <summary>DataParameterAttribute构造函数 </summary>
        public ParameterTag(string pName, E_DbType dbType, int size)
        {
            this._PropertyName = pName;
            this.dbType = dbType;
            this.Size = size;
        }

        public ParameterTag(string pName, object value, E_DbType dbType, int size)
        {
            this._PropertyName = pName;
            this.dbType = dbType;
            this.Size = size;
            this.Value = value;
        }
        public ParameterTag(string pName, object value, E_DbType dbType, int size, E_DbDirection direct)
        {
            this._PropertyName = pName;
            this.dbType = dbType;
            this.Size = size;
            this.Value = value;
            this.Direction = direct;
        }

        /// <summary>DataParameterAttribute构造函数 </summary>
        public ParameterTag()
        {
        }

        /// <summary>通过当前实例返回一个SqlParameter对象</summary>
        public void LoadValue(IBase the, PropertyInfo pInfo)
        {
            object val = pInfo.GetValue(the, null);
            this.Value = val;
        }

        [System.Xml.Serialization.XmlIgnore]
        SqlParameter _SqlParameter;
        /// <summary>获取当前的SqlParameter实例对象</summary>
        public SqlParameter GetParameter()
        {
            _SqlParameter = new SqlParameter(this.PropertyName, (SqlDbType)this.dbType, this.Size);
            _SqlParameter.Value = this.Value;
            _SqlParameter.Direction = (ParameterDirection)this.Direction;
            return _SqlParameter;
        }

        /// <summary>如果当前是返回参数，则该方法会返回参数的值(如果在执行前持久数据前调用则返回null)</summary>
        public object GetReturn()
        {
            if (_SqlParameter == null)
                return null;
            return _SqlParameter.Value;
        }


        public override string ToString()
        {
            return string.Format("{0}-{1}", this.PropertyName, this.DbType.ToString());
        }

        public static SqlParameter[] GetSqlParameters(ParameterTag[] pts)
        {
            if (pts != null)
            {
                SqlParameter[] ps = new SqlParameter[pts.Length];
                for (int i = 0; i < ps.Length; i++)
                {
                    ps[i] = pts[i].GetParameter();
                }
                return ps;
            }
            else
            {
                return new SqlParameter[0];
            }
        }

        readonly static IToDBType _ToDBType = new Convert_DBType_Mssql();
        /// <summary>获取数据库的中字符段类型的转换接口对象</summary>
        public static IToDBType GetIToDBType()
        {
            return _ToDBType;
        }

        /// <summary>
        /// 获取一个空数组，用以表示无效的数组对象 
        /// </summary>
        public static readonly ParameterTag[] NONE = new ParameterTag[0]; 

    }



    /// <summary>数据表的字段类型</summary>
    public enum E_DbType
    {
        BigInt = 0,
        Binary = 1,
        Bit = 2,
        Char = 3,
        DateTime = 4,
        Decimal = 5,
        Float = 6,
        Image = 7,
        Int = 8,
        Money = 9,
        NChar = 10,
        NText = 11,
        NVarChar = 12,
        Text = 18,
        Timestamp = 19,
        VarBinary = 21,
        VarChar = 22,
    }

    /// <summary>指定sql参数的输出或输入的类</summary>
    public enum E_DbDirection
    {
        Input = 1,
        Output = 2,
        InputOutput = 3,
        ReturnValue = 6,
    }

    /// <summary>
    /// 数据库的中字符段类型的转换接口
    /// </summary>
    public interface IToDBType
    {
        E_DbType Convert(int n);
    }
    class Convert_DBType_Mssql : IToDBType
    {
        #region IToDBType 成员

        E_DbType IToDBType.Convert(int n)
        {
            switch (n)
            {
                case 34:
                    return E_DbType.Image;
                case 35:
                    return E_DbType.Text;
                case 48:
                case 52:
                case 56:
                case 108:
                case 127:
                    return E_DbType.Int;
                case 60:
                    return E_DbType.Money;
                case 61:
                    return E_DbType.DateTime;
                case 62:
                    return E_DbType.Float;
                case 99:
                    return E_DbType.NText;
                case 104:
                    return E_DbType.Bit;
                case 106:
                    return E_DbType.Decimal;
                case 165:
                    return E_DbType.VarBinary;
                case 167:
                    return E_DbType.VarChar;
                case 173:
                    return E_DbType.Binary;
                case 175:
                    return E_DbType.Char;
                case 189:
                    return E_DbType.Timestamp;
                case 231:
                    return E_DbType.NVarChar;
                case 239:
                    return E_DbType.NChar;
            }
            return E_DbType.Int;
        }

        #endregion
    }

}
