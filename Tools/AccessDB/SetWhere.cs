using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools.AccessDB
{
    /// <summary>sql条件的构建类</summary>
    public class SetWhere
    {
        readonly string _SqlWhere = string.Empty;
        /// <summary>条件部分的sql</summary>
        public string SqlWhere
        {
            get { return _SqlWhere; }
        }

        readonly ParameterTag[] _Parames;
        /// <summary>条件部分的参数</summary>
        public ParameterTag[] Parames
        {
            get { return _Parames; }
        }

        /// <summary>构造方法SetWhere</summary>
        public SetWhere(string where, ParameterTag[] ps)
        {
            this._SqlWhere = where;
            this._Parames = ps;
        }
    }

    /// <summary>设置替换的sql字符串的类</summary>
    public class SetReplaceSql
    {
        /// <summary>用于查找并替换的标识字符串</summary>
        public string TagKey { get; set; }
        /// <summary>要替换的sql字符串</summary>
        public string ReplaceSql { get; set; }

        public SetReplaceSql( string key , string replaceSql )
        {
            this.TagKey = key;
            this.ReplaceSql = replaceSql;
        }
    }


}
