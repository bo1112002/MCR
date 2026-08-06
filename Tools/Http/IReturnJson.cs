using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Tools.Http
{
    /// <summary>用于处理Json对象的接口</summary>
    public interface IReturnJson
    {
        /// <summary>获取一个json格式的字符串</summary>
        /// <param name="jSerialize">序列json字符串的功能对象(特殊或自定义情况下,可不用该对象)</param>
        string GetJsonString(JavaScriptSerializer jSerialize);
    }
}
