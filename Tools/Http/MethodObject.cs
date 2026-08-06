using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.Script.Serialization;
using System.ComponentModel;
using System.Reflection;

namespace Tools.Http
{


    /// <summary>方法体结构类</summary>
    [Serializable]
    public class MethodObject
    {
        #region *****json属性********
        string _ClassName = "";
        /// <summary>方法所在App_Code的类名</summary>
        public string ClassName
        {
            get { return _ClassName; }
            set { _ClassName = value.Trim(); }
        }

        string _MethodName = "";
        /// <summary>当前要调用的方法名</summary>
        public string MethodName
        {
            get { return _MethodName; }
            set { _MethodName = value.Trim(); }
        }

        ParamterMaps _Paramters = new ParamterMaps();
        ///// <summary>当前要调用方法的参数列表</summary>
        public ParamterMaps Paramters
        {
            get { return _Paramters; }
            set { _Paramters = value; }
        }

        //IList<ParamterObject> _Paramters = new List<ParamterObject>();
        ///// <summary>当前要调用方法的参数列表</summary>
        //public IList<ParamterObject> ParamterArray
        //{
        //    get { return _Paramters; }
        //    set { _Paramters = value; }
        //}

        string _CommandName = "";
        /// <summary>指令</summary>
        public string CommandName
        {
            get { return _CommandName; }
            set { _CommandName = value.Trim(); }
        }

        string _CommandValue = "";
        /// <summary>指令的参数值</summary>
        public string CommandValue
        {
            get { return _CommandValue; }
            set { _CommandValue = value.Trim(); }
        }

        string _Tag = "";
        /// <summary>备用标识符</summary>
        public string Tag
        {
            get { return _Tag; }
            set { _Tag = value.Trim(); }
        }
        #endregion ------------------------------------------------------------------------------


        //HttpContext _Context;
        //[System.Web.Script.Serialization.ScriptIgnore]
        //public HttpContext Context
        //{
        //    get { return _Context; }
        //    private set { _Context = value; }
        //}


        /// <summary>通过MethodObject对象调用source中指定的方法(方法需要实例对象的pubic)</summary>
        public static object Posting(object source, MethodObject m)
        {
            object targetObj = null;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            MethodInfo mInfo = source.GetType().GetMethod(m.MethodName, flags);

            ParameterInfo[] pams = mInfo.GetParameters();
            object[] objs = new object[pams.Length];
            if (pams.Length > 0)
            {
                for (int i = 0; i < pams.Length; i++)
                {
                    ParameterInfo pm = pams[i];
                    string strVal = m.Paramters[pm.Name].ToString() ;
                    if (pm.ParameterType  ==  typeof(string) )
                    {
                        objs[i] = strVal.Trim();
                    }
                    else if (pm.ParameterType == typeof(bool))
                    {
                        objs[i] = Convert.ToBoolean( strVal.Trim() ) ;
                    }
                    else if (pm.ParameterType == typeof(int))
                    {
                        objs[i] = Convert.ToInt32(strVal.Trim());
                    }
                    else if (pm.ParameterType == typeof(double))
                    {
                        objs[i] = Convert.ToDouble(strVal.Trim());
                    }
                    else if (pm.ParameterType == typeof(DateTime))
                    {
                        objs[i] = Convert.ToDateTime(strVal.Trim());
                    }
                    else
                    {
                        objs[i] = Convert.ChangeType(strVal, pm.ParameterType);
                    }
                }
            }
            targetObj = mInfo.Invoke(source, objs);
            return targetObj;
        }

    }

    /// <summary>参数结构类</summary>
    [Serializable]
    public class ParamterObject
    {
        #region *****MyRegion********
        string _ParamterName = "";
        /// <summary>参数的名称</summary>
        public string ParamterName
        {
            get { return _ParamterName; }
            set { _ParamterName = value.Trim(); }
        }

        string _ParamterType = "String";
        /// <summary>参数的类型全名</summary>
        public string ParamterType
        {
            get { return _ParamterType; }
            set { _ParamterType = value.Trim(); }
        }

        object _Data = "";
        /// <summary>参数的数据</summary>
        public object Data
        {
            get { return _Data; }
            set { _Data = value; }
        }
        #endregion ------------------------------------------------------------------------------

        public ParamterObject() { }

        public ParamterObject(string name, Type type, object data)
        {
            this._ParamterName = name;
            this._ParamterType = type.Name;
            this._Data = data;
        }
        
    }

    public class ParamterMaps : Dictionary<string, object>
    {
        /// <summary>
        /// 查找匹配key的Val,如果找不到则返回onFindVal的指定值
        /// </summary>
        public string FindVal(string key , string onFindVal = null )
        {
            if (this.ContainsKey(key))
            {
                return this[key].ToString();
            }
            else
            {
                return onFindVal ;
            }
        }

    }


    /// <summary>对http的请求参数进行二次封装的使用类</summary>
    public class HttpParamts 
    {

        readonly Dictionary<string, string> _Maps = new Dictionary<string, string>();

        readonly HttpContext _MyHttpContext = null;
        /// <summary>获取当前请求的上下文件对象</summary>
        public HttpContext MyHttpContext
        {
            get { return _MyHttpContext; }
        } 

        public HttpParamts(HttpContext context)
        {
            this._MyHttpContext = context ;
            this.LoadParamts(this._MyHttpContext);
        }

        /// <summary>获取Key对应的值，如果不存在则返回null</summary>
        public string this[string key]
        {
            get
            {
                return FindVal(key);
            }

        }

         /// <summary>加载http的请求参数到当前对象中)</summary>
        public void LoadParamts(HttpContext context)
        {
            foreach (string kName in context.Request.Params.Keys)
            {
                if (string.IsNullOrEmpty(kName) == false)
                {
                    this._Maps[kName] = context.Request.Params[kName];
                }
            }
        }


        /// <summary>
        /// 查找匹配key的Val,如果找不到则返回onFindVal的指定值
        /// </summary>
        public string FindVal(string key, string onFindVal = null)
        {
            if (this._Maps.ContainsKey(key))
            {
                return this._Maps[key].ToString();
            }
            else
            {
                return onFindVal;
            }
        }

    }



}
