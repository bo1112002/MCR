using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Web.Configuration;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Tools.Config;

namespace Tools.Tcp
{

    /// <summary>
    /// 与Service服务进行通信的控制类
    /// </summary>
    public abstract class UDP_Handle : IDisposable
    {
        /// <summary>数据结构([tag:4b->len:4b->data:*b])</summary>
        protected class MsgClass
        {
            /// <summary>消息标识</summary>
            public readonly int Tag = 0;
            /// <summary>消息内容长度</summary>
            public readonly int Length = 0;
            /// <summary>消息内容长度</summary>
            public readonly byte[] Content = new byte[0];
            /// <summary>是否采用异步发送</summary>
            public readonly bool IsAsync = true;
            /// <summary>获取当前返回消息的异步回调方法对象（如果选择的是异步发送）</summary>
            readonly Action<MsgClass, IPEndPoint> DisposeBackData;


            public MsgClass(int tag, byte[] content, bool async, Action<MsgClass, IPEndPoint> action)
            {
                this.Tag = tag;
                this.Content = content;
                this.Length = this.Content.Length;

                this.IsAsync = async;
                DisposeBackData = action;
            }

            public MsgClass(byte[] bs)
            {
                this.Tag = BitConverter.ToInt32(bs, 0);
                this.Length = BitConverter.ToInt32(bs, 4);
                if (this.Length > 0)
                {
                    byte[] target = new byte[this.Length];
                    Buffer.BlockCopy(bs, 8, target, 0, target.Length);
                    this.Content = target;
                }
            }

            /// <summary>获取当前</summary>
            public byte[] ToBytes()
            {
                byte[] bs = new byte[this.Length + 8];
                byte[] tmps = BitConverter.GetBytes(Tag);
                bs[0] = tmps[0];
                bs[1] = tmps[1];
                bs[2] = tmps[2];
                bs[3] = tmps[3];
                tmps = BitConverter.GetBytes(Length);
                bs[4] = tmps[0];
                bs[5] = tmps[1];
                bs[6] = tmps[2];
                bs[7] = tmps[3];

                Buffer.BlockCopy(Content, 0, bs, 8, Content.Length);

                return bs;
            }


            public void GetCallback(byte[] rtnByts, IPEndPoint point)
            {
                if (DisposeBackData == null)
                    return;
                MsgClass msg = new MsgClass(rtnByts);
                DisposeBackData(msg, point);

            }


        }
        /// <summary>UDP_Handle构造器</summary>
        protected UDP_Handle()
        {
        }

        /// <summary>发送消息</summary>
        protected void SendMsg(MsgClass sendMsg, IPEndPoint sendPoint)
        {
            byte[] sends = sendMsg.ToBytes();
            if (sendMsg.IsAsync)
            {
                MyUdpClient.BeginSend(sends, sends.Length, sendPoint, Callback, sendMsg);
            }
            else
            {
                MyUdpClient.Send(sends, sends.Length, sendPoint);
                try
                {
                    IPEndPoint point = new IPEndPoint(IPAddress.Any, 0);
                    byte[] rtnByts = MyUdpClient.Receive(ref point);
                    sendMsg.GetCallback(rtnByts, point);
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return;
                }
            }
        }
        /// <summary></summary>
        protected void Callback(IAsyncResult ar)
        {
            MsgClass old = ar.AsyncState as MsgClass;
            if (old == null)
                return;
            IPEndPoint point = new IPEndPoint(IPAddress.None, 0);
            try
            {
                byte[] bs = MyUdpClient.EndReceive(ar, ref  point);
                if (bs.Length > 8)
                {
                    old.GetCallback(bs, point);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return;
            }
            
        }


        /// <summary>当前的UdpClient对象</summary>
        public abstract UdpClient MyUdpClient { get; }
        /// <summary>获取一个唯一标识ID</summary>
        public abstract string GetID(string prefixName);
        /// <summary>注册IP</summary>
        public abstract void RegistIP();
        /// <summary>获取当前时间</summary>
        public abstract DateTime GetNowTime();


        #region============= 静态成员=========>>>

        /// <summary>获取一个客户端访问对象</summary>
        public static UDP_Handle CreateUDP_Client()
        {
            return new UDP_Client();
        }
        /// <summary>获取一个外部服务的访问对象</summary>
        public static UDP_Handle CreateUDP_Service(ServiceMe me)
        {
            return new UDP_Service(me);
        }

        #endregion=============END==========<<<



        /// <summary>关闭连接</summary>
        public void Close()
        {
            if( MyUdpClient != null )
            {
                MyUdpClient.Close();
            }
        }

        #region IDisposable 成员

        void IDisposable.Dispose()
        {
            this.Close() ;
        }

        #endregion
    }


    /// <summary>
    /// 客户端
    /// </summary>
    class UDP_Client : UDP_Handle
    {
        readonly UdpClient _Udp;
        public UDP_Client()
        {
            int port = GetServerPoint().Port; //取服务的端口作为当前监视的端口
            _Udp = new UdpClient(port + 1);
            _Udp.Client.ReceiveTimeout = 10000; //等待10秒
        }

        /// <summary>获取外部服务的访问点</summary>
        public IPEndPoint GetServerPoint()
        {
            IPAddress ip = IPAddress.Parse( KeyValueClass.Map_KVs["appSettings"]["ServiceIP"].Val.Trim() ) ;
            int port = int.Parse( KeyValueClass.Map_KVs["appSettings"]["ServicePort"].Val.Trim());
            return new IPEndPoint(ip, port);
        }


        /// <summary></summary>
        public override UdpClient MyUdpClient
        {
            get { return this._Udp; }
        }

        static int _CountNum = 0;
        /// <summary>获取一个唯一标识ID</summary>
        public override string GetID(string prefixName)
        {
            if (string.IsNullOrEmpty(prefixName))
            {
                prefixName = "XXX";
            }

            string rtnString = string.Empty;
            byte[] rtnByts = Encoding.Default.GetBytes(prefixName.Trim());
            MsgClass the = new MsgClass(101, rtnByts, false, delegate(MsgClass rtnMsg, IPEndPoint point)
            {
                rtnString = Encoding.Default.GetString(rtnMsg.Content);
            });
            this.SendMsg(the, this.GetServerPoint());

            if (string.IsNullOrEmpty(rtnString))
            { 
                if( (_CountNum++) >9999 )
                {
                    _CountNum = 0 ;
                }
                rtnString = prefixName + DateTime.Now.ToBinary() + _CountNum.ToString("0000");
            }

            return rtnString;
        }

        public override void RegistIP()
        {
            MsgClass the = new MsgClass(10, new byte[0], false, null);
            this.SendMsg(the, this.GetServerPoint());
        }

        public override DateTime GetNowTime()
        {
            DateTime curTime = DateTime.Now;
            MsgClass the = new MsgClass(100, new byte[0], false, delegate(MsgClass rtnMsg, IPEndPoint point)
            {
                long time = BitConverter.ToInt64(rtnMsg.Content, 0);
                curTime = DateTime.FromBinary(time);
            });
            this.SendMsg(the, this.GetServerPoint());
            return curTime;
        }

        
    }

    /// <summary>
    /// 服务端
    /// </summary>
    class UDP_Service : UDP_Handle
    {
        ServiceMe _ServiceMe;
        readonly UdpClient _Udp;
        public UDP_Service(ServiceMe service)
        {
            _ServiceMe = service;
            _Udp = new UdpClient(_ServiceMe.Port);
            _Udp.BeginReceive(Udp_BeginReceive, null); //从指定端口接收指定，并进行相应的处理
        }

        void Udp_BeginReceive(IAsyncResult ar)
        {
            IPEndPoint refPoint = new IPEndPoint(0, 0);
            byte[] bs = _Udp.EndReceive(ar, ref refPoint);
            if (bs.Length >= 8)
            {
                MsgClass the = new MsgClass(bs);
                ReceiveDispose(the, refPoint);
            }

            if (_ServiceMe.IsRuning)
            {
                _Udp.BeginReceive(Udp_BeginReceive, null);
            }
        }

        static readonly Dictionary<string, IPEndPoint> _MapsEndPoint = new Dictionary<string, IPEndPoint>();
        void ReceiveDispose(MsgClass the, IPEndPoint refPoint)
        {
            if (the.Tag == 10) //注册IP,用于记录客户IP，进行事件消息的广播
            {
                if (_MapsEndPoint.ContainsKey(refPoint.ToString()) == false)
                {
                    _MapsEndPoint.Add(refPoint.ToString(), new IPEndPoint(refPoint.Address, refPoint.Port));
                }
            }
            else if (the.Tag == 100) //获取当前时间
            {
                byte[] time = BitConverter.GetBytes(GetNowTime().ToBinary());
                MsgClass rtnThe = new MsgClass(the.Tag, time, true, null);
                this.SendMsg(rtnThe, refPoint);
            }
            else if (the.Tag == 101) //获取唯一标识ID
            {
                string strPrefix = "XXX";
                if (the.Length > 0)
                {
                    strPrefix = Encoding.Default.GetString(the.Content);
                }
                string s = this.GetID(strPrefix);
                MsgClass rtnThe = new MsgClass(the.Tag, Encoding.Default.GetBytes(s), true, null);
                this.SendMsg(rtnThe, refPoint);

            }
            else if (the.Tag == 200)
            {
            }
        }

        public override UdpClient MyUdpClient
        {
            get { return _Udp; }
        }


        static int _AginVal = -1;
        static readonly int AginMax = 9999;
        /// <summary>获取一个理论上的唯一标识值</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override string GetID(string prefixName)
        {
            if (_AginVal >= AginMax)
            {
                _AginVal = -1;
            }
            _AginVal++;
            string s = string.Format( "{0}{1}{2}" , 
                prefixName.Trim() , 
                _ServiceMe.TimerRunerOne.CurrentTime.ToString("yyMMddHHmm") + DateTime.Now.Second.ToString("00") , 
                _AginVal.ToString("0000") ) ;
            return s;
        }

        public override void RegistIP()
        {
        }

        public override DateTime GetNowTime()
        {
            return _ServiceMe.TimerRunerOne.CurrentTime;
        }
    }



}
