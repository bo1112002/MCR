using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Net;
using fastJSON;
using System.Web.Script.Serialization;

namespace Tools
{
    /// <summary>
    /// 系统的函数集
    /// </summary>
    public class PublicMethod
    {
        private PublicMethod() { }

        static readonly object OjectTemp = new object();

        /// <summary>对值类型T==DBNull值时,则替换指定的T值</summary>
        /// <param name="obj">要转换为T的object</param>
        /// <param name="whenDBNullReplaceT">当obj == DBNull或转换出错时，要替换为指定的DateTime值</param>
        public static T ConvertValue<T>(object obj, T defaultVal)
        {
            if (obj is DBNull)
            {
                return defaultVal;
            }
            else
            {
                try
                {
                    return (T)Convert.ChangeType(obj, typeof(T));
                }
                catch
                {
                    return defaultVal;
                }
            }

        }


        /// <summary>
        ///  判断当前值是否为null , 如果为null则使用取代值（displaceValue）进行替换,
        /// 这个方法只针对把值存到数据库时用
        /// </summary>
        /// <param name="currentData">当前值</param>
        /// <param name="displaceData">取代值</param>
        public static object DisplayIsNullValue(object currentData, object displaceData)
        {
            if (currentData == null)
            {
                return displaceData;
            }
            return currentData;
        }





        /// <summary>检查文件夹是否存在如果不存在则创建</summary>
        public static void CheckFolderPath(string path)
        {
            string folderPath = "";
            if (path.Contains("\\"))
            {
                folderPath = path.Substring(0, path.LastIndexOf('\\'));
                if (!Directory.Exists(folderPath))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(folderPath);
                    }
                    catch
                    {
                        CheckFolderPath(folderPath);
                        System.IO.Directory.CreateDirectory(folderPath);
                    }
                }
            }
        }
        /// <summary>删除文件夹</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DeleteFolder(string path)
        {
            lock (OjectTemp)
            {
                string[] strTemp;
                //先删除该目录下的文件
                strTemp = System.IO.Directory.GetFiles(path);
                foreach (string str in strTemp)
                {
                    System.IO.File.Delete(str);
                }
                //删除子目录，递归
                strTemp = System.IO.Directory.GetDirectories(path);
                foreach (string str in strTemp)
                {
                    DeleteFolder(str);
                }
                //删除该目录
                System.IO.Directory.Delete(path);
            }
        }



        static int AddNum = 1;
        /// <summary>
        /// 通过当前时间生成一个随机数
        /// </summary>
        public static string CreateAutoCode()
        {
            int num = Math.Abs(((int)DateTime.Now.Ticks) / 2) + (AddNum++);
            return DateTime.Now.ToString("yyMM") + ((uint)num).ToString();
        }


        static int _CountOnlyNum = 0;
        /// <summary>创建并返回一个全局唯一标识值</summary>
        public static string CreateAutoCode(string prefixName)
        {
            lock (OjectTemp)
            {
                if (++_CountOnlyNum > 9999)
                {
                    _CountOnlyNum = 0;
                }
                long lg = long.Parse(DateTime.Now.ToString("yyMMddHHmmss"));
                long lg2 = Convert.ToInt64(PublicMethod.CreateAutoCode());
                string strTime = PublicMethod.To36String(System.Math.Abs(lg) + _CountOnlyNum) + PublicMethod.To36String(lg2);
                string tagID = string.Format("{0}-{1}", prefixName, strTime);
                return tagID;
            }
        }



        //================================================
        static DateTime _NONE_DateTime = DateTime.Parse("1900-1-1");
        /// <summary>获取一个无效时间</summary>
        public static DateTime NONE_DateTime
        {
            get
            {
                return _NONE_DateTime;
            }
        }

        /// <summary>Unix时间戳转换DateTime</summary>
        public static DateTime GetUnixTimeStampToDateTime(long timeStamp)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
            DateTime dt = startTime.AddSeconds(timeStamp);
            return dt;
        }
        /// <summary>Unix时间戳转换DateTime(dt为null则转换当前时间)</summary>
        public static long GetDateTimeToUnixTimeStamp(DateTime? dt)
        {
            DateTime ddd = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            long timeStamp = (long)( (dt??DateTime.Now) - ddd).TotalSeconds; // 相差秒数
            return timeStamp;
        }

        //================================================


        static decimal _DifferenaceValue = -1m;
        /// <summary>设置当前对比数值可接受范围的精度值
        ///  (正负范围内可视为相等,如精度为0.0005 目标值5.0000 ,结果值5.0004或4.996 , 这样系统认为结果值与目标值是相等的 )
        ///  </summary>
        /// <param name="tagetValue">目标值</param>
        /// <param name="resultValue">结果值</param>
        public static bool DifferenaceValueOfCompare(decimal tagetValue, decimal resultValue)
        {
            if (_DifferenaceValue < 0)
            {
                string tmpValue = System.Configuration.ConfigurationManager.AppSettings["DifferenaceValue"];
                if (string.IsNullOrEmpty(tmpValue) || tmpValue.Trim() == string.Empty)
                {
                    _DifferenaceValue = 0;
                }
                else
                {
                    if (decimal.TryParse(tmpValue, out _DifferenaceValue) == false)
                    {
                        _DifferenaceValue = 0;
                    }
                }
            }

            //对比

            if (tagetValue >= resultValue - _DifferenaceValue &&
                tagetValue <= resultValue + _DifferenaceValue)
            {
                return true;
            }
            else return false;
        }

        /// <summary>把16进制字符串数转换为十进制数</summary>
        /// <param name="Num">字符串数</param>
        public static string ToD(string Num)
        {
            Num = Num.ToUpper();
            char[] nums = Num.ToCharArray();
            Int64 ddd = 0;
            for (int i = 0; i < nums.Length; i++)
            {
                char number = nums[i];
                switch (number)
                {
                    case 'A':
                        ddd = ddd * 16 + 10;
                        break;
                    case 'B':
                        ddd = ddd * 16 + 11;
                        break;
                    case 'C':
                        ddd = ddd * 16 + 12;
                        break;
                    case 'D':
                        ddd = ddd * 16 + 13;
                        break;
                    case 'E':
                        ddd = ddd * 16 + 14;
                        break;
                    case 'F':
                        ddd = ddd * 16 + 15;
                        break;
                    default:
                        ddd = ddd * 16 + int.Parse(number.ToString());
                        break;
                }
            }
            return ddd.ToString();
        }

        static readonly char[] C36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        /// <summary>把指定数转换为36进制的字符串</summary>
        public static string To36String(long num)
        {
            int toBase = C36.Length;
            List<char> numList = new List<char>();
            do
            {
                long remainder = num % toBase;
                if (remainder < 0) remainder = remainder * -1;
                numList.Add(C36[remainder]);
                num = num / toBase;
                if (num != 0)
                    continue;

                numList.Reverse();
                return new string(numList.ToArray());

            } while (true);
        }

        /// <summary>
        /// 获取指定标识(tag)的字符串数组
        /// （如 参数str: "ABCE%FF%GLNM1354%55%45" , 参数tag: % 返回为[]{ "FF" ,"55" } ）
        /// </summary>
        /// <param name="str">解释的字符串</param>
        /// <param name="tag">标识符</param>
        public static string[] GetSubStringList(string str, char tag)
        {
            List<string> listString = new List<string>();
            int start = -1, len = 0;

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == tag)
                {
                    if (start <= 0)
                    {
                        start = i + 1;
                    }
                    else
                    {
                        len = i - start;
                        string val = str.Substring(start, len);
                        listString.Add(val);
                        start = -1;
                    }
                }
            }
            return listString.ToArray();
        }


        static MD5 md5Hasher = MD5.Create();
        /// <summary>MD5加密的方法</summary>
        public static string GetMd5Hash(string input)
        {
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("X2"));
            }
            return sBuilder.ToString();
        }

        /// <summary>检查url链接是否有效</summary>
        public static bool CheckUri(string strUri)
        {
            try
            {
                System.Net.HttpWebRequest.DefaultCachePolicy = new
                    System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.Revalidate);

                Uri uri = new Uri(strUri);
                System.Net.HttpWebRequest.CreateDefault(uri).GetResponse();
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>GZip压缩</summary>
        /// <param name="bytsInput">要进行压缩的数据</param>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public static byte[] Zip(byte[] bytsInput)
        {
            /*
            __MemoryZip.SetLength(0);
            __MemoryZip.Position = 0;

            __Zip.Write(bytsInput, 0, bytsInput.Length);
            byte[] bsZip = __MemoryZip.ToArray();
            return bsZip;*/
            MemoryStream ms = new MemoryStream();
            GZipStream mZip = new GZipStream(ms, CompressionMode.Compress);
            mZip.Write(bytsInput, 0, bytsInput.Length);
            mZip.Close();
            byte[] bs = ms.ToArray();
            ms.Close();
            return bs;
        }
        /// <summary>GZip解压缩</summary>
        /// <param name="bytsInput">要进行解压缩的数据</param>
        public static byte[] UnZip(byte[] bytsInput)
        {
            /**/
            byte[] bsDezip = new byte[0];
            using (MemoryStream msDe = new MemoryStream(bytsInput))
            using (GZipStream mZip2 = new GZipStream(msDe, CompressionMode.Decompress))
            using (MemoryStream ms2 = new MemoryStream())
            {
                byte[] bsTemp = new byte[1024];
                while (true)
                {
                    int n = mZip2.Read(bsTemp, 0, bsTemp.Length);
                    ms2.Write(bsTemp, 0, n);
                    if (n != bsTemp.Length) break;
                }
                bsDezip = ms2.ToArray();
            }
            return bsDezip;

        }










        /// <summary>加密</summary>
        public static string Encryption(string str, string DEFAULT_KEY = "c2770c8a4b82ea66", string DEFAULT_IV = "4572b33f0d790ad2" )
        {
            RC2CryptoServiceProvider rc2CSP = new RC2CryptoServiceProvider();
            byte[] key = Convert.FromBase64String(DEFAULT_KEY);
            byte[] IV = Convert.FromBase64String(DEFAULT_IV);
            ICryptoTransform encryptor = rc2CSP.CreateEncryptor(key, IV);
            MemoryStream msEncrypt = new MemoryStream();
            CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            byte[] toEncrypt = Encoding.ASCII.GetBytes(str);
            csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
            csEncrypt.FlushFinalBlock();
            byte[] rs = msEncrypt.ToArray();
            return Convert.ToBase64String(rs);
        }

        /// <summary>解密</summary>
        public static string Decryption(string str, string DEFAULT_KEY = "c2770c8a4b82ea66", string DEFAULT_IV = "4572b33f0d790ad2")
        {
            byte[] bs = Convert.FromBase64String(str);
            RC2CryptoServiceProvider rc2CSP = new RC2CryptoServiceProvider();
            byte[] key = Convert.FromBase64String(DEFAULT_KEY);
            byte[] IV = Convert.FromBase64String(DEFAULT_IV);
            ICryptoTransform decryptor = rc2CSP.CreateDecryptor(key, IV);
            MemoryStream msDecrypt = new MemoryStream(bs);
            CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            StringBuilder roundtrip = new StringBuilder();
            int b = 0;
            do
            {
                b = csDecrypt.ReadByte();

                if (b != -1)
                {
                    roundtrip.Append((char)b);
                }

            } while (b != -1);

            return roundtrip.ToString();
        }


        //=================================
        /// <summary>依据索引数返回ABC...的字符序列(索引数最小值为0，最大值为：25)</summary>
        public static string IndexToABC(int index)
        {
            if (index < 0)
            {
                index = 0;
            }
            else if (index > 25)
            {
                index = 25;
            }

            return ((char)(65 + index)).ToString();
        }



        /// <summary>执行Cmd命令</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Result RunCmd(string c)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo("cmd.exe");
                info.RedirectStandardOutput = false;
                info.UseShellExecute = false;
                Process p = Process.Start(info);
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.Start();
                p.StandardInput.WriteLine(c);
                p.StandardInput.AutoFlush = true;
                Thread.Sleep(1000);
                p.StandardInput.WriteLine("exit");
                p.WaitForExit();
                string outStr = p.StandardOutput.ReadToEnd();
                p.Close();

                return new Result(true, outStr);
            }
            catch (Exception ex)
            {
                return new Result( false , "error" + ex.Message ) ;
            }
        }


        public static readonly JavaScriptSerializer _Jserialize = new JavaScriptSerializer();
        /// <summary>http请求返回Josn字符格式的Dictionary对象, 如果解释失败则返回带错误信息的Dictionary对象</summary>
        public static Dictionary<string, object> GetWebJsonString(string url , Action<WebRequest> action_Request = null,  int sleep = 100 )
        {
            try
            {
                WebRequest req = WebRequest.Create(url);
                if(action_Request != null)
                {
                    action_Request( req )  ;
                }

                WebResponse rep = req.GetResponse();
                Stream stm = rep.GetResponseStream();
                Thread.Sleep(sleep);

                byte[] bsData = null;
                if (rep.ContentLength > 0)
                {

                    int sumLen = 0;
                    bsData = new byte[rep.ContentLength];

                    sumLen = stm.Read(bsData, 0, bsData.Length);
                    while (rep.ContentLength > sumLen)
                    {
                        sumLen += stm.Read(bsData, sumLen, bsData.Length - sumLen);
                    }
                }
                else
                {
                    byte[]  bsData2 = new byte[1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        while (true)
                        {
                            int len = stm.Read(bsData2, 0, bsData2.Length);
                            ms.Write(bsData2, 0, len);
                            if (len <= 0 )
                                break;
                        }
                        bsData = ms.ToArray();
                    } 
                }
                string strJson = Encoding.UTF8.GetString(bsData);
                Dictionary<string, object> dicRS = fastJSON.JSON.ToObject(strJson, typeof(object)) as Dictionary<string, object>;
                //Dictionary<string, object> dicRS = _Jserialize.Deserialize<Dictionary<string, object>>(strJson);
                return dicRS;
                /*
                using (MemoryStream ms = new MemoryStream())
                {
                    while (true)
                    {
                        int len = stm.Read(bsData, 0, bsData.Length);
                        ms.Write(bsData, 0, len);
                        if (len < bsData.Length)
                        {
                            break;
                        }
                    }
                    string strJson = Encoding.UTF8.GetString(ms.ToArray());
                    //Console.WriteLine(strJson);
                    Dictionary<string, object> dicRS = fastJSON.JSON.ToObject(strJson, typeof(object)) as Dictionary<string, object>;
                    return dicRS;
                
                } 
                 */
            }
            catch(Exception err)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic["ERR"] = err;
                return dic;
            }
        }
        /// <summary>http请求返回Josn字符格式的Dictionary对象, 如果解释失败则返回带错误信息的Dictionary对象</summary>
        public static Dictionary<string, object> GetWebJsonString(string url, Dictionary<string,object> postData , string method = "POST")
        {
            string strPostData = JSON.ToJSON(postData);
            byte[] bsPostData = Encoding.UTF8.GetBytes(strPostData);

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Method = method ;
                request.ContentType = "application/json";
                request.ContentLength = bsPostData.Length;
                request.GetRequestStream().Write(bsPostData, 0, bsPostData.Length);
                request.Timeout = 1000 * 120;
               
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Thread.Sleep(100);
                    Stream stm = response.GetResponseStream();
                    byte[] bsData = new byte[1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        while (true)
                        {
                            int len = stm.Read(bsData, 0, bsData.Length);
                            ms.Write(bsData, 0, len);
                            if (len <= 0 )
                            {
                                break;
                            }
                        }
                        string strJson = Encoding.UTF8.GetString(ms.ToArray());
                        //Console.WriteLine(strJson);
                        Dictionary<string, object> dicRS = fastJSON.JSON.ToObject(strJson, typeof(object)) as Dictionary<string, object>;
                        return dicRS;
                    }
                }
            }
            catch (Exception err)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic["ERR"] = err;
                return dic;
            }
        }

        //===========================


        /// <summary>过滤非常用的字符</summary>
        public static string ConvertString(string strTarget)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char ss in strTarget)
            {
                if ( ss <= 0x9fbb)
                    sb.Append(ss);
            }
            return sb.ToString();
        }

    }

    /// <summary>系统时间结构</summary>
    public struct SystemTime
    {
        /// <summary>设置本地的系统时间</summary>
        [DllImport("kernel32.dll")]
        public static extern int SetLocalTime(ref SystemTime lpSystemTime);
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }


}
