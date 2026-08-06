using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools
{
    /// <summary>
    /// 分页处理的数据结构(‘更多’的处理方式)
    /// </summary>
    [Serializable]
    public class PaginationMore
    {
        int _TotalRows = 0;
        /// <summary>总记录数</summary>
        public int TotalRows
        {
            get { return _TotalRows; }
            set { _TotalRows = value; }
        }

        object _LastVal = null;
        /// <summary>最后的记录的标识值</summary>
        public object LastVal
        {
            get { return _LastVal; }
            set { _LastVal = value; }
        }

        int _UnitRows = 10 ;
        /// <summary>每次加载的记录数</summary>
        public int UnitRows
        {
            get  
            {
                return _UnitRows; 
            }
            set  {  _UnitRows = value;  }
        }

        public PaginationMore(int total, object initVal )
        {
            this._TotalRows = total;
            this._LastVal = initVal;
        }

        /// <summary>
        /// 分布的sql模板(按升序的查询)--> 
        /// {0}:表名 , {1}:该表中的排序字段名,{2}:目标读取数， {3}:读取的终结数（起始数+目标读取数）, 
        /// </summary>
        public static readonly string SQL_TPL_DESC = @"
SELECT * FROM {0} Where {1} in
(
    SELECT TOP {2} {1} FROM 
    (
        SELECT TOP {3} {1} FROM {0} ORDER BY {1} desc
    ) T1  ORDER BY {1} ASC
    
) ORDER BY {1} desc";
        /// <summary>分页的sql模板2</summary>
        public static readonly string SQL_TPL_DESC2 = @"
SELECT TOP(@UnitRows) * FROM {0} Where @LastChangeTime > {1} ORDER BY {1} desc";


    }
}
