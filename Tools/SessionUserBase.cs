using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.Cache;
using Tools.Tcp;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace Tools
{
    /// <summary>用户的状态类(令牌)</summary>
    public abstract class SessionUserBase
    {
        /*
        /// <summary>通过用户名及密码获取SessionUser(令牌)</summary>
        public abstract SessionUserBase Login(string loginName, string loginPwd);*/


        static DateTime _MyTime = DateTime.Now;
        /// <summary>获取系统现在时间</summary>
        public static DateTime GetNewTime()
        {

            return DateTime.Now;
        }

        static readonly DateTime _NONE_DATE = DateTime.Parse("2100-1-1 00:00");
        /// <summary>表示一个无效时间(2100-1-1 00:00)</summary>
        public static DateTime NONE_DATE
        {
            get
            {
                return SessionUserBase._NONE_DATE;
            }
        }
        /// <summary>获取当前缓存操作对象</summary>
        public static ICache MyCache = new CacheSafe(10, 1000);



 /*
       
        /// <summary>与外部服务进行通信的提供者</summary>
        public static readonly UDP_Handle UdpHandler = UDP_Handle.CreateUDP_Client();



        /// <summary>后台线程处理的工作事件( 后台线程：时间触发器)</summary>
        public static event Action<object> BackgroundWorking;
       
        static SessionUserBase()
        {
            
            ThreadPool.QueueUserWorkItem(delegate(object obj)
            {
                int nSleep = 1000 * 60;
                while (true)
                {
                    Thread.Sleep(nSleep);
                    lock (BackgroundWorking)
                    {
                        BackgroundWorking(obj);
                    }
                }
            });
        }*/


        



    }



    public class CurrentTimeHandler
    {
        private CurrentTimeHandler() { }

        const string LASTTIMEFILE = "LastTime";

        static readonly Func<DateTime?>[] _ActionList = new Func<DateTime?>[] { GetBeijingTime, DataStandardTime, GetChineseDateTime };

        public static DateTime GetTime()
        {
            DateTime resultTime = DateTime.Now;
            try
            {
                foreach (Func<DateTime?> the in _ActionList)
                {
                    DateTime? tmp = the();
                    if (tmp != null)
                    {
                        resultTime = tmp.Value;
                        break;
                    }
                }
            }
            catch
            { }

            DateTime lastTime = GetLastTime(resultTime);
            if (resultTime < lastTime)
            {
                throw new Exception("获取系统时间为无效值(最后记录时间大于当前获取时间)");
            }

            SetLastTime(resultTime);
            return resultTime;

        }

        // <summary>获取最后的操作时间</summary>
        public static DateTime GetLastTime( DateTime initTime )
        {
            DateTime lastTime = initTime;
            if (File.Exists(LASTTIMEFILE))
            {
                string str = File.ReadAllText(LASTTIMEFILE);
                if (DateTime.TryParse(str, out lastTime) == false)
                {
                    lastTime = DateTime.Now;
                }
            }
            return lastTime;
        }

        static readonly object LOCK_TAG = new object();
        /// <summary>记录最后的操作时间</summary>
        public static void  SetLastTime(DateTime initTime)
        {
            lock (LOCK_TAG)
            {
                File.WriteAllText("LastTime", initTime.ToString());
            }
        }


        #region 获取网络时间
        ///<summary>获取中国国家授时中心网络服务器时间发布的当前时间</summary>
        static DateTime? GetChineseDateTime()
        {
            DateTime res = DateTime.MinValue;
            try
            {
                HttpItem item = new HttpItem("http://www.time.ac.cn/stime.asp");
                HttpHelper helper = new HttpHelper();
                HttpResult httpRS = helper.GetHtml(item);
                if (httpRS == null)
                    throw new Exception("请求" + item.URL + "失败");
                string html = httpRS.Html;
                string patDt = @"\d{4}年\d{1,2}月\d{1,2}日";
                string patHr = @"hrs\s+=\s+\d{1,2}";
                string patMn = @"min\s+=\s+\d{1,2}";
                string patSc = @"sec\s+=\s+\d{1,2}";
                Regex regDt = new Regex(patDt);
                Regex regHr = new Regex(patHr);
                Regex regMn = new Regex(patMn);
                Regex regSc = new Regex(patSc);

                res = DateTime.Parse(regDt.Match(html).Value);
                int hr = GetInt(regHr.Match(html).Value, false);
                int mn = GetInt(regMn.Match(html).Value, false);
                int sc = GetInt(regSc.Match(html).Value, false);
                res = res.AddHours(hr).AddMinutes(mn).AddSeconds(sc);
            }
            catch
            {
                return null;
            }
            return res;
        }

        ///<summary>
        /// 从指定的字符串中获取整数
        ///</summary>
        ///<param name="origin">原始的字符串</param>
        ///<param name="fullMatch">是否完全匹配，若为false，则返回字符串中的第一个整数数字</param>
        ///<returns>整数数字</returns>
        static int GetInt(string origin, bool fullMatch)
        {
            if (string.IsNullOrEmpty(origin))
            {
                return 0;
            }
            origin = origin.Trim();
            if (!fullMatch)
            {
                string pat = @"-?\d+";
                Regex reg = new Regex(pat);
                origin = reg.Match(origin.Trim()).Value;
            }
            int res = 0;
            int.TryParse(origin, out res);
            return res;
        }





        ///<summary>
        /// 获取标准北京时间2
        ///</summary>
        ///<returns></returns>
        static DateTime? GetBeijingTime()
        {
            DateTime dt;
            WebRequest wrt = null;
            WebResponse wrp = null;
            try
            {
                wrt = WebRequest.Create("http://www.beijing-time.org/time.asp");
                wrp = wrt.GetResponse();

                string html = string.Empty;
                using (Stream stream = wrp.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                    {
                        html = sr.ReadToEnd();
                    }
                }

                string[] tempArray = html.Split(';');
                for (int i = 0; i < tempArray.Length; i++)
                {
                    tempArray[i] = tempArray[i].Replace("\r\n", "");
                }

                string year = tempArray[1].Substring(tempArray[1].IndexOf("nyear=") + 6);
                string month = tempArray[2].Substring(tempArray[2].IndexOf("nmonth=") + 7);
                string day = tempArray[3].Substring(tempArray[3].IndexOf("nday=") + 5);
                string hour = tempArray[5].Substring(tempArray[5].IndexOf("nhrs=") + 5);
                string minite = tempArray[6].Substring(tempArray[6].IndexOf("nmin=") + 5);
                string second = tempArray[7].Substring(tempArray[7].IndexOf("nsec=") + 5);
                string strTime = string.Format("{0}-{1}-{2} {3}:{4}:{5}", year, month, day, hour, minite, second);
                dt = DateTime.Parse(strTime);
            }
            catch (Exception e)
            {
                return null;
            }
            finally
            {
                if (wrp != null)
                    wrp.Close();
                if (wrt != null)
                    wrt.Abort();
            }
            return dt;
        }

        /// <summary>
        /// 获取标准北京时间
        /// </summary>
        /// <returns></returns>
        static DateTime? DataStandardTime()
        {
            DateTime dt;

            //返回国际标准时间
            //只使用的时间服务器的IP地址，未使用域名
            try
            {
                string[,] 时间服务器 = new string[14, 2];
                int[] 搜索顺序 = new int[] { 3, 2, 4, 8, 9, 6, 11, 5, 10, 0, 1, 7, 12 };
                时间服务器[0, 0] = "time-a.nist.gov";
                时间服务器[0, 1] = "129.6.15.28";
                时间服务器[1, 0] = "time-b.nist.gov";
                时间服务器[1, 1] = "129.6.15.29";
                时间服务器[2, 0] = "time-a.timefreq.bldrdoc.gov";
                时间服务器[2, 1] = "132.163.4.101";
                时间服务器[3, 0] = "time-b.timefreq.bldrdoc.gov";
                时间服务器[3, 1] = "132.163.4.102";
                时间服务器[4, 0] = "time-c.timefreq.bldrdoc.gov";
                时间服务器[4, 1] = "132.163.4.103";
                时间服务器[5, 0] = "utcnist.colorado.edu";
                时间服务器[5, 1] = "128.138.140.44";
                时间服务器[6, 0] = "time.nist.gov";
                时间服务器[6, 1] = "192.43.244.18";
                时间服务器[7, 0] = "time-nw.nist.gov";
                时间服务器[7, 1] = "131.107.1.10";
                时间服务器[8, 0] = "nist1.symmetricom.com";
                时间服务器[8, 1] = "69.25.96.13";
                时间服务器[9, 0] = "nist1-dc.glassey.com";
                时间服务器[9, 1] = "216.200.93.8";
                时间服务器[10, 0] = "nist1-ny.glassey.com";
                时间服务器[10, 1] = "208.184.49.9";
                时间服务器[11, 0] = "nist1-sj.glassey.com";
                时间服务器[11, 1] = "207.126.98.204";
                时间服务器[12, 0] = "nist1.aol-ca.truetime.com";
                时间服务器[12, 1] = "207.200.81.113";
                时间服务器[13, 0] = "nist1.aol-va.truetime.com";
                时间服务器[13, 1] = "64.236.96.53";
                int portNum = 13;
                string hostName;
                byte[] bytes = new byte[1024];
                int bytesRead = 0;
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
                for (int i = 0; i < 13; i++)
                {
                    hostName = 时间服务器[搜索顺序[i], 1];
                    try
                    {
                        client.Connect(hostName, portNum);
                        System.Net.Sockets.NetworkStream ns = client.GetStream();
                        bytesRead = ns.Read(bytes, 0, bytes.Length);
                        client.Close();
                        break;
                    }
                    catch (System.Exception)
                    {
                    }
                }
                char[] sp = new char[1];
                sp[0] = ' ';
                dt = new DateTime();
                string str1;
                str1 = System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRead);

                string[] s;
                s = str1.Split(sp);
                if (s.Length >= 2)
                {
                    dt = System.DateTime.Parse(s[1] + " " + s[2]);//得到标准时间
                    dt = dt.AddHours(8);//得到北京时间*/
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
            return dt;
        }

        #endregion










    }
}
