using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace Tools
{
    public class Loger
    {

        private Loger() { }

        //readonly static UdpClient _My_Udp = new UdpClient(8800);
        //readonly static IPEndPoint _RemotePoint = new IPEndPoint( IPAddress.Parse( "182.254.220.190" ) , 80);
        ///<summary>系统日志记录(按天数生成日志文件)</summary>
        [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining)]
        public static void Log(string str)
        {
            if (AppSettingsBase.Base.IsDebug)
            {
                StringBuilder sb = new StringBuilder("\r\n======");
                sb.Append(SessionUserBase.GetNewTime().ToString("yyyy-MM-dd HH:mm:ss"));
                sb.Append("======>>>\r\n");
                sb.Append(str);
                sb.Append("\r\n===============================<<<\r\n");

                if (KeyValueClass.Map_KVs == null ||
                    KeyValueClass.Map_KVs["appSettings"] == null ||
                    KeyValueClass.Map_KVs["appSettings"]["Log"] == null)
                    return;

                string dirLog = KeyValueClass.Map_KVs["appSettings"]["Log"].Val.Trim() ;
                lock (Result.NONE)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dirLog.TrimEnd('\\') + "\\");
                    if (dirInfo.Exists == false)
                    {
                        dirInfo.Create();
                    }

                    string filePath = dirLog + "\\Log(" + SessionUserBase.GetNewTime().ToString("yy-MM-dd") + ").txt";
                    File.AppendAllText(filePath, sb.ToString());

                    if (AppSettingsBase.Base.IsDebug)
                    {
                        //byte[] bs = Encoding.Default.GetBytes(sb.ToString());
                        //_Udp.Send(bs, bs.Length, _SendPoint);
                    }
                }

            }
        }
        static IPEndPoint _SendPoint = new IPEndPoint( IPAddress.Parse("127.0.0.1") , 6111 );
        static UdpClient _Udp = new UdpClient();


        public static void Log(Exception err)
        {
            Log(err.Message + err.StackTrace == null ? string.Empty : "(" + err.StackTrace + ")");
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
