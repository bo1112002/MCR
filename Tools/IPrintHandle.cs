using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;

namespace Tools
{
    /// <summary>打印接口</summary>
    public interface IPrintHandle
    {
        /// <summary>
        ///  设置或获取打印模板的标识值
        /// </summary>
        object TemplateTag { get; set; }

        /// <summary> 添加参数 </summary>
        void Add(DataRow row);
        /// <summary> 添加参数 </summary>
        void Add(Hashtable hst);
        /// <summary> 添加参数 </summary>
        void Add(string name, object value);
        /// <summary>添加参数</summary>
        /// <param name="expandValue">是否为对象,如果true:则对其属性创建参数对象</param>
        void Add(string name, object value, bool expandValue);
        /// <summary>
        /// 清空当前的参数集
        /// </summary>
        void ClearParames();
        /// <summary>
        /// 打印
        /// </summary>
        void Excuter();
    }
}
