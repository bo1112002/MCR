using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Collections;

namespace Tools.Config
{
    /// <summary>
    /// 接口实例化工厂
    /// </summary>
    public class Builder
    {
        private Builder()
        {
        }

        static Dictionary<string, Assembly> AssemblyDictionary = new Dictionary<string, Assembly>();


        static object CovnertObjFromString(ParameItem.ElementKeyToParamterType pt, string val)
        {
            Type t = Type.GetType(EnumDescription.GetFieldText(pt));
            if (t == typeof(object))
            {
                return GetInterface(val.Trim());
            }
            else
            {
                return Convert.ChangeType(val, t);
            }
        }

        public static object GetInterface(string appKey)
        {
            InterfaceElement element =
                        MyConfig.SystemConfigManager.GetOperationInterfaceOfConfig(appKey);
            if (element == null)
            {
                throw new Exception("接口的配置为NULL");
            }

            Monitor.Enter(AssemblyDictionary);

            if (AssemblyDictionary.ContainsKey(appKey) == false)
            {
                string strDll = "";
                if (element.DllPath.Contains("file:"))
                {
                    strDll = element.DllPath.TrimStart("file:".ToCharArray()); //取绝对路径
                }
                else
                {
                    strDll = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\" + element.DllPath.Trim();//取相对路径
                }

                Assembly asmMe = Assembly.LoadFrom(strDll);
                AssemblyDictionary.Add(appKey, asmMe);
            }
            Monitor.Exit(AssemblyDictionary); //解除锁定

            Type _type = AssemblyDictionary[appKey].GetType(element.ImpTypeFullName.Trim());
            //创建配置中的参数集
            ArrayList arrParames = new ArrayList();
            foreach (ParameItem pItem in element.Parames)
            {
                object objParam = CovnertObjFromString(pItem.ParamterType, pItem.Value);
                arrParames.Add(objParam);
            }



            //TODO:如果配置信息和代码都设置了存在构造参数 , 则会用配置信息中的构造函数来构造当前类型
            if (arrParames.Count > 0)
            {
                return Activator.CreateInstance(_type, arrParames.ToArray());
            }
            else
            {
                return Activator.CreateInstance(_type);
            }


        }

    }
}
