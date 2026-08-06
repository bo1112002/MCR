using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Collections.Specialized;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;

namespace Tools.Tcp.MyHttpListener
{
    public class MyHttpListener
    {

        string _HttpUrlPrefixe;
        HttpListener _Listener;
        TcpListener _TcpServer;
        byte[] bsBuff = new byte[1024 * 100];

        Socket _MySocket = null;

        public MyHttpListener(string httpUrlPrefixe, IPEndPoint serverTcpPoint)
        {
            _HttpUrlPrefixe = httpUrlPrefixe.Trim().TrimEnd('/') + "/";

            _Listener = new HttpListener();


            _TcpServer = new TcpListener(serverTcpPoint);
            _TcpServer.Start();

            ThreadPool.QueueUserWorkItem((obj) =>
            {
                while (true)
                {
                    try
                    {
                        Socket socket = _TcpServer.AcceptSocket();
                        Console.WriteLine("-->" + socket.RemoteEndPoint.ToString());
                        if (_MySocket != null)
                        {
                            _MySocket.Close();
                        }
                        _MySocket = socket;
                    }
                    catch
                    {
                        _MySocket = null;
                    }
                }

            }, null);


        }

        public void Start()
        {
            _Listener.Prefixes.Add(this._HttpUrlPrefixe); //添加需要监听的url范围
            _Listener.Start(); //开始监听端口，接收客户端请求

            Console.WriteLine("Listening...");

            ThreadPool.QueueUserWorkItem((sender) =>
            {

                while (true)
                {
                    //阻塞主函数至接收到一个客户端请求为止
                    HttpListenerContext context = _Listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;


                    Console.WriteLine("Request--->" + request.Url);
                    RequestInfo reqInfo = new RequestInfo(request);


                    if (_MySocket == null)
                    {
                        response.Close();
                        continue;
                    }
                    else
                    {
                        ResponseInfo repInfo = TcpSend(_MySocket, reqInfo);
                        if (repInfo == null)
                        {
                            Console.WriteLine("repInfo == null");
                            response.Close();
                            continue;
                        }


                        try
                        {
                            Console.WriteLine("Response===>" + response.StatusCode + " , " + repInfo.ContentType);
                            response.ContentType = repInfo.ContentType;
                            response.ContentEncoding = Encoding.UTF8;
                            if (string.IsNullOrEmpty(repInfo.ContentEncoding) == false)
                            {
                                response.ContentEncoding = Encoding.GetEncoding(repInfo.ContentEncoding);
                            }

                            if (repInfo.ProtocolVersion != null)
                            {
                                response.ProtocolVersion = repInfo.ProtocolVersion;
                            }
                            response.StatusCode = (int)repInfo.StatusCode;

                            if (repInfo.ResponseStream != null)
                            {
                                byte[] buffer = repInfo.ResponseStream;
                                response.ContentLength64 = buffer.Length;

                                if (buffer.Length > bsBuff.Length)
                                {
                                    int offset = 0;
                                    while(true)
                                    {
                                        if (buffer.Length - offset > bsBuff.Length)
                                        {
                                            response.OutputStream.Write(buffer, offset, bsBuff.Length);
                                            offset += bsBuff.Length;
                                        }
                                        else
                                        {
                                            response.OutputStream.Write(buffer, offset, buffer.Length - offset);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    response.OutputStream.Write(buffer, 0, buffer.Length);
                                }
                            }

                            if (repInfo.Headers != null)
                            {
                                foreach (string k in repInfo.Headers.AllKeys)
                                {
                                    if (response.Headers.AllKeys.Contains(k) == false)
                                    {
                                        response.Headers.Add(k, repInfo.Headers[k]);
                                    }
                                }
                            }
                            response.OutputStream.Flush();
                            response.Close();
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine("err-->" + err.Message + "-->" + err.StackTrace);
                        }
                    }
                }

            }, null);



        }

        public void Stop()
        {
            try
            {
                _Listener.Stop();
                _Listener.Abort();
            }
            catch { }

            try
            {
                _TcpServer.Stop();
            }
            catch { }
        }

        ResponseInfo TcpSend(Socket skt, RequestInfo reqInfo)
        {
            //skt.ReceiveBufferSize = bsBuff.Length;
            //skt.ReceiveTimeout = 1000 * 3;

            try
            {
                byte[] bs = SerializeObjectClass.SerializObjectForBinary(reqInfo);
                int rs = skt.Send(bs);

                while (true)
                {
                    int rLen = skt.Receive(bsBuff);
                    if (rLen == 4)
                    {
                        int size = BitConverter.ToInt32(bsBuff, 0);
                        int size2 = 0;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            while (size2 < size)
                            {
                                int len = skt.Receive(bsBuff);
                                if (len > 0)
                                {
                                    ms.Write(bsBuff, 0, len);
                                }
                                size2 += len;
                            }

                            byte[] bs2 = ms.ToArray();
                            Console.WriteLine("bs2_Length-->" + bs2.Length);
                            ResponseInfo repInfo = SerializeObjectClass.DeserializObjectForBinary<ResponseInfo>(bs2);
                            return repInfo;
                        }
                    }
                    else
                    {
                        return null;
                    }

                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return null;
            }
        }

    }


    [Serializable]
    public class RequestInfo
    {
        public RequestInfo()
        { }
        public RequestInfo(HttpRequest request)
        {
            this.AcceptTypes = request.AcceptTypes;
            this.AnonymousID = request.AnonymousID;
            this.ApplicationPath = request.ApplicationPath;
            this.AppRelativeCurrentExecutionFilePath = request.AppRelativeCurrentExecutionFilePath;
            this.Browser = request.Browser.Browser;
            this.ContentEncoding = request.ContentEncoding.EncodingName;
            this.ContentLength = (request.ContentLength > 0 ? request.ContentLength : 0);
            this.ContentType = request.ContentType;
            this.CurrentExecutionFilePath = request.CurrentExecutionFilePath;
            this.CurrentExecutionFilePathExtension = request.CurrentExecutionFilePath;
            this.FilePath = request.FilePath;
            this.Form = request.Form;
            this.Headers = request.Headers;
            this.HttpMethod = request.HttpMethod;

            if (this.ContentLength > 0)
            {
                this.InputStream = new byte[this.ContentLength];
                request.InputStream.Read(this.InputStream, 0, request.ContentLength);
            }


            this.IsLocal = request.IsLocal;
            this.IsSecureConnection = request.IsSecureConnection;
            this.Params = request.Params;
            this.Path = request.Path;
            this.PathInfo = request.PathInfo;
            this.PhysicalApplicationPath = request.PhysicalApplicationPath;
            this.PhysicalPath = request.PhysicalPath;
            this.QueryString = request.QueryString;
            this.RawUrl = request.RawUrl;
            this.RequestType = request.RequestType;
            this.ServerVariables = request.ServerVariables;
            this.TotalBytes = request.TotalBytes;
            this.Url = request.Url;
            this.UserAgent = request.UserAgent;
            this.UserHostAddress = request.UserHostAddress;
            this.UserHostName = request.UserHostName;
            this.UserLanguages = request.UserLanguages;
        }

        public RequestInfo(HttpListenerRequest request)
        {
            this.AcceptTypes = request.AcceptTypes;
            this.ContentEncoding = request.ContentEncoding.EncodingName;
            this.ContentLength = (request.ContentLength64 > 0 ? request.ContentLength64 : 0);
            this.ContentType = request.ContentType;
            this.Headers = request.Headers;
            this.HttpMethod = request.HttpMethod;

            if (this.ContentLength > 0)
            {
                this.InputStream = new byte[this.ContentLength];
                request.InputStream.Read(this.InputStream, 0, this.InputStream.Length);
            }

            this.IsLocal = request.IsLocal;
            this.IsSecureConnection = request.IsSecureConnection;
            this.QueryString = request.QueryString;
            this.RawUrl = request.RawUrl;
            this.Url = request.Url;
            this.UserAgent = request.UserAgent;
            this.UserHostAddress = request.UserHostAddress;
            this.UserHostName = request.UserHostName;
            this.UserLanguages = request.UserLanguages;
        }

        public RequestInfo(WebRequest request)
        {
            this.ContentLength = (request.ContentLength > 0 ? request.ContentLength : 0);
            this.ContentType = request.ContentType;
            this.Headers = request.Headers;

            if (this.ContentLength > 0)
            {
                this.InputStream = new byte[this.ContentLength];
                request.GetRequestStream().Read(this.InputStream, 0, this.InputStream.Length);
            }

            this.HttpMethod = request.Method;
            this.Url = request.RequestUri;
        }


        public string[] AcceptTypes { get; set; }
        public string AnonymousID { get; set; }
        public string ApplicationPath { get; set; }
        public string AppRelativeCurrentExecutionFilePath { get; set; }
        public string Browser { get; set; }
        public string ContentEncoding { get; set; }
        public long ContentLength { get; set; }
        public string ContentType { get; set; }
        public string CurrentExecutionFilePath { get; set; }
        public string CurrentExecutionFilePathExtension { get; set; }
        public string FilePath { get; set; }
        public NameValueCollection Form { get; set; }
        public NameValueCollection Headers { get; set; }
        public string HttpMethod { get; set; }
        public byte[] InputStream { get; set; }
        public bool IsLocal { get; set; }
        public bool IsSecureConnection { get; set; }
        public NameValueCollection Params { get; set; }
        public string Path { get; set; }
        public string PathInfo { get; set; }
        public string PhysicalApplicationPath { get; set; }
        public string PhysicalPath { get; set; }
        public NameValueCollection QueryString { get; set; }
        public string RawUrl { get; set; }
        public string RequestType { get; set; }
        public NameValueCollection ServerVariables { get; set; }
        public int TotalBytes { get; set; }
        public Uri Url { get; set; }
        public string UserAgent { get; set; }
        public string UserHostAddress { get; set; }
        public string UserHostName { get; set; }
        public string[] UserLanguages { get; set; }

        public ResponseInfo ToWebRequest(string httpUrL)
        {
            string url = httpUrL + this.Url.PathAndQuery;
            WebRequest req = WebRequest.Create(url);
            req.ContentLength = this.ContentLength;
            req.ContentType = this.ContentType;
            if (this.ContentLength > 0)
            {
                req.GetRequestStream().Write(this.InputStream, 0, this.InputStream.Length);
            }
            if (this.Params != null)
            {
                req.Headers.Add(this.Params);
            }
            req.Method = this.HttpMethod;

            ResponseInfo repInfo = null;
            try
            {
                WebResponse rep = req.GetResponse();
                repInfo = new ResponseInfo(rep);
            }
            catch (Exception err)
            {
                repInfo = new ResponseInfo();
                repInfo.Method = req.Method;
                repInfo.StatusCode = HttpStatusCode.BadRequest;
                repInfo.StatusDescription = err.Message;
            }


            return repInfo;
        }




    }


    [Serializable]
    public class ResponseInfo
    {
        public ResponseInfo()
        { }


        public ResponseInfo(WebResponse response)
        {
            this.ContentLength = (response.ContentLength > 0 ? response.ContentLength : 0);
            this.ContentType = response.ContentType;
            this.Headers = response.Headers as NameValueCollection;
            this.IsFromCache = response.IsFromCache;
            this.IsMutuallyAuthenticated = response.IsMutuallyAuthenticated;
            this.ResponseUri = response.ResponseUri;

            if (this.ContentLength > 0)
            {
                this.ResponseStream = new byte[this.ContentLength];
                int wLen = 0;
                while (wLen < this.ContentLength)
                {
                    int rLen = response.GetResponseStream().Read(this.ResponseStream, wLen, (int)this.ContentLength - wLen);
                    wLen += rLen;
                }
            }

            //=================================

            HttpWebResponse httpRep = response as HttpWebResponse;
            if (httpRep == null)
                return;

            this.CharacterSet = httpRep.CharacterSet;
            this.ContentEncoding = httpRep.ContentEncoding;
            this.LastModified = httpRep.LastModified;
            this.Method = httpRep.Method;

            this.ProtocolVersion = httpRep.ProtocolVersion;
            this.Server = httpRep.Server;
            this.StatusCode = httpRep.StatusCode;
            this.StatusDescription = httpRep.StatusDescription;


        }


        public long ContentLength { get; set; }
        public string ContentType { get; set; }
        public NameValueCollection Headers { get; set; }
        public bool IsFromCache { get; set; }
        public bool IsMutuallyAuthenticated { get; set; }
        public Uri ResponseUri { get; set; }
        //protected void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext);
        public byte[] ResponseStream { get; set; }

        //==================================================
        public string CharacterSet { get; set; }
        public string ContentEncoding { get; set; }
        public DateTime LastModified { get; set; }
        public string Method { get; set; }
        public Version ProtocolVersion { get; set; }
        public string Server { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }

    }
}
