using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Tools.Config;

namespace Tools
{
    /// <summary>
    /// 基础设置信息
    /// </summary>
    public abstract class AppSettingsBase
    {
        static AppSettingsBase _Me;
        public static AppSettingsBase Base
        {
            get
            {
                if (_Me == null)
                {
                    KeyValueClass kv =  KeyValueClass.Map_KVs["appSettings"] ;
                    _Me = kv.CreateM_Object() as AppSettingsBase;
                }

                if (_Me == null)
                {
                    MyConfig.Log("KeyValueClass.Map_KVs[\"appSettings\"].CreateM_Object()返回null,配置类型存在问题");
                }

                return _Me;
            }
        }

        /// <summary>
        /// 是否调试状态
        /// </summary>
        public abstract bool IsDebug { get; }
        /// <summary>获取由系统默认指定的密码值</summary>
        public abstract string DefaultPWD{ get;}



        /// <summary>获取当前DLL的所在目录(尾部已包含\)</summary>
        public static string GetExecuting_DIR()
        {
            string asmFileName = Assembly.GetExecutingAssembly().CodeBase.TrimStart();
            string dirPath = Path.GetDirectoryName(asmFileName).TrimEnd('\\') + "\\";
            return dirPath.TrimStart("file:\\".ToCharArray());
        }
    }
}
