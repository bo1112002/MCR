using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Tools;
using Tools.Http;
using System.Net;
using System.Web;
using System.IO;
using System.Reflection;
using MCR.Mods;

namespace MCR
{

    public class AppSettings : AppSettingsBase, IMailServer
    {
        /// <summary>
        /// 是否测试状态
        /// </summary>
        public override bool IsDebug
        {
            get
            {
                return Convert.ToBoolean(KeyValueClass.Map_KVs["appSettings"]["IsDebug"].Val);
            }
        }
        /// <summary>获取由系统默认指定的密码值</summary>
        public override string DefaultPWD
        {
            get
            {
                return KeyValueClass.Map_KVs["appSettings"]["DefaultPWD"].Val;
            }
        }

        #region============= 静态成员=========>>>
        /// <summary>获取基本配置信息的值</summary>
        public static string GetAppSettingsVal(string key)
        {
            KeyValueClass kv = KeyValueClass.Map_KVs["appSettings"][key];
            if (kv == null)
                return null;
            return kv.Val;
        }



        /// <summary>日志文件的存储目录</summary>
        public static string Log
        {
            get
            {
                string strPath = KeyValueClass.Map_KVs["appSettings"]["Log"].Val;
                DirectoryInfo dir = new DirectoryInfo(strPath);
                if (dir.Exists == false)
                {
                    dir.Create();
                }
                return dir.FullName;
            }
        }

        /// <summary>临时文件存放目录</summary>
        public static string Temps_DIR
        {
            get
            {
                string strPath = KeyValueClass.Map_KVs["appSettings"]["Temps_DIR"].Val;
                DirectoryInfo dir = new DirectoryInfo(strPath);
                if (dir.Exists == false)
                {
                    dir.Create();
                }
                return dir.FullName;
            }
        }

        /// <summary>文件服务器的存储目录</summary>
        public static string FS_DIR
        {
            get
            {
                string strPath = KeyValueClass.Map_KVs["appSettings"]["FS_DIR"].Val;
                DirectoryInfo dir = new DirectoryInfo(strPath);
                if (dir.Exists == false)
                {
                    dir.Create();
                }
                return dir.FullName;
            }
        }
        /// <summary>文件服务器的访问URL模板</summary>
        public static string FS_URL
        {
            get
            {
                return KeyValueClass.Map_KVs["appSettings"]["FS_URL"].Val;
            }
        }

        /// <summary>文件服务器的虚拟目录的URL</summary>
        public static string FS_SerURL
        {
            get
            {
                return KeyValueClass.Map_KVs["appSettings"]["FS_SerURL"].Val;
            }
        }


        static string _Template_GetDocument_URL = null;
        /// <summary>获取浏览文档的URL(如果pageIndex不为空，则表示获取文档某一页的图片)</summary>
        public static string GetDocument_URL(string fileID, string docID, int pageIndex = 0)
        {
            string strPageIndex = string.Empty;
            if (pageIndex > 0)
            {
                strPageIndex = pageIndex.ToString();
            }

            if (_Template_GetDocument_URL == null)
                _Template_GetDocument_URL = KeyValueClass.Map_KVs["appSettings"]["GetDocument_URL"].Val;
            string str = _Template_GetDocument_URL.Replace('|', '&');
            string url = string.Format(str, fileID, docID, strPageIndex);
            return url;
        }


        /// <summary>当前站点的的URLURL</summary>
        public static string WebURL
        {
            get
            {
                return KeyValueClass.Map_KVs["appSettings"]["WebURL"].Val;
            }
        }

        /// <summary>当前站点的的后台登录页的URL</summary>
        public static string WebURL_Login
        {
            get
            {
                return KeyValueClass.Map_KVs["appSettings"]["WebURL_Login"].Val;
            }
        }
        /// <summary>临时文件存放目录的URL</summary>
        public static string Temps_URL
        {
            get
            {
                return KeyValueClass.Map_KVs["appSettings"]["Temps_URL"].Val;
            }
        }


        /// <summary>微信参数配置结点</summary>
        public static KeyValueClass WX
        {
            get
            {
                return KeyValueClass.Map_KVs["WX"];
            }
        }


        /// <summary>验证码的字符集</summary>
        public static char[] CheckCodes
        {
            get
            {
                string str = KeyValueClass.Map_KVs["appSettings"]["CheckCodes"].Val;
                return str.ToCharArray();
            }
        }



        /// <summary>获取文档空图片的URL</summary>
        public static string NONE_DOC_ImgURL
        {
            get
            {
                return KeyValueClass.Map_KVs["appSettings"]["NONE_DOC_ImgURL"].Val;
            }
        }

        /// <summary>获取无效页的URL</summary>
        public static string NONE404_URL
        {
            get
            {
                return KeyValueClass.Map_KVs["appSettings"]["NONE404_URL"].Val;
            }
        }

        /// <summary>获取用户端功能导航结点对象</summary>
        public static KeyValueClass User_Navigation
        {
            get
            {
                return KeyValueClass.Map_KVs["User_Navigation"];
            }
        }
        /// <summary>依据用户类别获取相应的导航信息集合</summary>
        public static IList<KeyValueClass> GetUser_NavigationByMember(WX_Member theMember)
        {
            MemberType mt = theMember.MType;
            if (mt == MemberType.E_SysAdmin)
            {
                mt = MemberType.E_SchoolAdmin;
            }
            List<KeyValueClass> list = User_Navigation[mt.ToString()].Childs;
            return list;
        }



        /// <summary>获取用户管理后台的功能导航结点对象</summary>
        public static KeyValueClass Admin_Navigation
        {
            get
            {
                return KeyValueClass.Map_KVs["Admin_Navigation"];
            }
        }

        /// <summary>依据管理后台的用户类别获取相应的导航信息集合</summary>
        public static IList<KeyValueClass> GetAdmin_NavigationByMember(WX_Member theMember)
        {
            if (theMember.MType == MemberType.E_Student)
                return new List<KeyValueClass>();
            else
            {
                List<KeyValueClass> list = Admin_Navigation[theMember.MType.ToString()].Childs;
                return list;
            }


        }
        /*====================================================================*/




        /// <summary>
        /// 获取文件类型的图标URL目录
        /// </summary>
        public static string FileTypeIconURL
        {
            get
            {
                return KeyValueClass.Map_KVs["appSettings"]["FileTypeIcon_URL"].Val;
            }
        }


        /// <summary>WinRAR.exe的路径</summary>
        public static string RAR_EXE
        {
            get
            {
                return KeyValueClass.Map_KVs["appSettings"]["RAR_EXE"].Val;
            }
        }

        /// <summary>app的版本更新信息</summary>
        public static KeyValueClass App_Version
        {
            get
            {
                KeyValueClass kv = KeyValueClass.Map_KVs["appSettings"]["App_Version"];
                return kv;
                //AppVersion theVersion = new AppVersion(string.Empty, kv.Val, kv.M);
                //return theVersion;
            }
        }



        /// <summary>获取访问远程服务的IPEndPoint对象</summary>
        public static IPEndPoint GetServicePoint()
        {
            string serIP = KeyValueClass.Map_KVs["appSettings"]["ServiceIP"].Val;
            int serPort = int.Parse(KeyValueClass.Map_KVs["appSettings"]["ServicePort"].Val);

            IPEndPoint end = new IPEndPoint(IPAddress.Parse(serIP), serPort);
            return end;
        }


        /// <summary>获取文档转换的工作进程入口配置信息</summary>
        public static IPEndPoint GetTool_ConvertHTMLPoint()
        {
            string serIP = KeyValueClass.Map_KVs["Tool_ConvertHTML"]["IPAddress"].Val;
            int serPort = int.Parse(KeyValueClass.Map_KVs["Tool_ConvertHTML"]["Port"].Val);

            IPEndPoint end = new IPEndPoint(IPAddress.Parse(serIP), serPort);
            return end;
        }



        /*====================================================================*/

        /// <summary>QST平台配置</summary>
        public static KeyValueClass QST( string key )
        {
            if (string.IsNullOrEmpty(key))
            {
                return KeyValueClass.Map_KVs["appSettings"]["QST"];
            }
            else
            {
                KeyValueClass kv = KeyValueClass.Map_KVs["appSettings"]["QST"] ;
                return kv[key] ;
            }
        }

        /*====================================================================*/

        #endregion=============END==========<<<

        #region IMailServer 成员

        string IMailServer.UserName
        {
            get
            {
                return KeyValueClass.Map_KVs["E_Mail_Setting"]["UserName"].Val;
            }
        }

        string IMailServer.Password
        {
            get
            {
                return KeyValueClass.Map_KVs["E_Mail_Setting"]["Password"].Val;
            }
        }

        string IMailServer.Server
        {
            get { return KeyValueClass.Map_KVs["E_Mail_Setting"]["Server"].Val; }
        }

        #endregion
    }
}
