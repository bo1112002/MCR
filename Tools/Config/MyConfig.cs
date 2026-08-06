using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Net;
using System.Collections;
using System.Web.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.IO;
using System.Diagnostics;
using Tools.Tcp;
using System.Net.Sockets;

namespace Tools.Config
{
    /// <summary>
    /// 系统内部的配置的管理类
    /// </summary>
    public sealed class MyConfig
    {
        /// <summary>
        /// TODO:获取当前系统的配置文件的管理对象
        /// </summary>
        public readonly static MyConfig SystemConfigManager = new MyConfig();

        Configuration _MyConfig = null;
        InterfaceSection _InterfaceSection = null;

        readonly Dictionary<string, QueryElementTag> _TypeVsQItem = new Dictionary<string, QueryElementTag>();
        private MyConfig()
        {
            Reload(); 
        }


        //重新加载配置文件
        void Reload()
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename =
                Assembly.GetExecutingAssembly().CodeBase.TrimStart("file:///".ToCharArray()) + ".config";
            _MyConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            _InterfaceSection = _MyConfig.GetSection("InterfaceSection") as InterfaceSection;

            //foreach (QueryElementTag tag in InterfaceSectionMe.QueryElementList)
            //{
            //    if (tag.MapingType != null && _TypeVsQItem.ContainsKey(tag.MapingType) == false)
            //    {
            //        _TypeVsQItem.Add(tag.MapingType, tag);
            //    }
            //}
        }

        /// <summary> 获取当前配置中的Settings结点集合</summary>
        public static KeyValueConfigurationCollection Settings
        {
            get
            {
                return SystemConfigManager.CurrentConfigInfo.AppSettings.Settings;
            }
        }

        /// <summary> 获取当前配置中的Settings结点的Value值,如果不存在则返回string.Empty</summary>
        public static string GetSettingVal(string name)
        {
            if (Settings[name] != null)
            {
                return Settings[name].Value;
            }
            return string.Empty;
        }

        

        /// <summary>获取当前系统的配置信息</summary>
        public Configuration CurrentConfigInfo
        {
            get  { return _MyConfig;  }
        }

        /// <summary>获取配置信息的主结点</summary>
        public InterfaceSection InterfaceSectionMe
        {
            get 
            {
                return _InterfaceSection; 
            }
        }

        /// <summary> 获取当前所有实现的接口配置结点对象</summary>
        /// <param name="appKey"></param>
        public InterfaceElement GetOperationInterfaceOfConfig(string appKey)
        {
            return InterfaceSectionMe.InterfaceElements[appKey.Trim()];
        }


        /// <summary>获取查询项配置信息</summary>
        /// <param name="appKey">A.Q1或A</param>
        public void GetQueryElementToConfig(string key, out QueryElementTag tag, out QItem item )
        {
            key = key.Trim();
            if (key.Contains("."))
            {
                string[] strS = key.Split('.');
                tag = InterfaceSectionMe.QueryElementList[strS[0].Trim()];
                item = tag.QItems[strS[1].Trim()];
                return;
            }
            else
            {
                foreach (QueryElementTag tmp in InterfaceSectionMe.QueryElementList)
                {
                    QItem qi = tmp.QItems[key];
                    if ( qi!= null &&  qi.Key.ToLower() == key.Trim().ToLower() )
                    {
                        tag = tmp;
                        item = qi;
                        return;
                    }
                }
            }

            tag = null;
            item = null;
        }


        /// <summary>通过类型返回一个QueryElementTag对象</summary>
        public QueryElementTag GetQueryElementToConfig(Type type)
        {
            if (_TypeVsQItem.ContainsKey(type.FullName) == false)
                return null;
            return _TypeVsQItem[type.FullName];
        }

        /// <summary>获取当前程序集合所在的目录</summary>
        public static string GetDir()
        {
            string sm = Assembly.GetExecutingAssembly().Location;
            sm = sm.Substring(0, sm.LastIndexOf('\\'));
            return sm ;
        }



        //readonly static UdpClient _My_Udp = new UdpClient(8800);
        //readonly static IPEndPoint _RemotePoint = new IPEndPoint( IPAddress.Parse( "182.254.220.190" ) , 80);
        ///<summary>系统日志记录(按天数生成日志文件)</summary>
        [MethodImpl(MethodImplOptions.Synchronized| MethodImplOptions.NoInlining )]
        public static void Log(string str)
        {
            /*
            byte[] bs = Encoding.Default.GetBytes( str) ;
            if (bs.Length > 1024)
            {
                int sendLen = 0 ;
                byte[] sendDatas = new byte[1024] ;
                while (sendLen < bs.Length)
                {
                    int  rema = bs.Length - sendLen ;
                    int cCount = ( rema>1024 ? 1024: rema ) ;
                    Buffer.BlockCopy(bs, sendLen, sendDatas, 0, cCount);
                    sendLen += _My_Udp.Send(sendDatas, sendDatas.Length, _RemotePoint);
                }
            }
            else
            {
                _My_Udp.Send(bs, bs.Length, _RemotePoint);
            }*/

            if (AppSettingsBase.Base.IsDebug )
            {
                StringBuilder sb = new StringBuilder("\r\n======");
                sb.Append( SessionUserBase.GetNewTime().ToString("yyyy-MM-dd HH:mm:ss") );
                sb.Append( "======>>>\r\n" ) ;
                sb.Append( str  ) ;
                sb.Append( "\r\n===============================<<<\r\n") ;


                Debug.WriteLine( sb.ToString() );

                if (KeyValueClass.Map_KVs == null ||
                    KeyValueClass.Map_KVs["appSettings"] == null ||
                    KeyValueClass.Map_KVs["appSettings"]["Log"] == null)
                    return;


                string dirPath = KeyValueClass.Map_KVs["appSettings"]["Log"].Val.Trim('\\');
                if (Directory.Exists(dirPath) == false)
                    return;
                string filePath = dirPath + "\\Log(" + SessionUserBase.GetNewTime().ToString("yy-MM-dd") + ").txt";

                lock (SystemConfigManager)
                {
                    File.AppendAllText(filePath, sb.ToString() );
                }
            }
        }

        /*
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Log2( string str)
        {
            if (AppSettingsBase.Base.IsDebug == false)
            {

                Debug.WriteLine(str);
                if (KeyValueClass.Map_KVs == null ||
                    KeyValueClass.Map_KVs["appSettings"] == null ||
                    KeyValueClass.Map_KVs["appSettings"]["Log"] == null)
                    return;
                string filePath = KeyValueClass.Map_KVs["appSettings"]["Log"].Val.Trim('\\')
                    + "\\Log(" + SessionUserBase.GetNewTime().ToString("yy-MM-dd") + ").txt";

                lock (SystemConfigManager)
                {
                    File.AppendAllText(filePath, "======" + SessionUserBase.GetNewTime().ToString("yyyy-MM-dd HH:mm:ss") + "======>>>\r\n");
                    File.AppendAllText(filePath, str + "\r\n");
                    File.AppendAllText(filePath, "===============================<<<\r\n");
                }
            }
        }*/


    }
    



    



}
