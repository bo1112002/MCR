using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Tools.Tcp
{

    /// <summary>用于标识传输包的类型</summary>
    public enum TransmitPackType : uint
    {
        /// <summary>向顶端服务转包</summary>
        Top,
        /// <summary>成功返回包</summary>
        Return_Success,
        /// <summary>错误返回包</summary>
        Return_Err,
        /// <summary>无效包</summary>
        None = UInt16.MaxValue,
        /// <summary>不做处理</summary>
        Stop,
    }
    
    /// <summary>用于服务器间的传输包的结构类</summary>
    [Serializable]
    public class TransmitPack : IComparable
    {
        string _Key = Guid.NewGuid().ToString();
        /// <summary>当前包的唯一标识符,并且对象之间的比较也以此为依据</summary>
        public string Key
        {
            get { return _Key; }
            set { _Key = value; }
        }

        byte[] _Data;
        /// <summary>传输包的数据</summary>
        public byte[] Data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        bool _IsZipCompress = false;
        /// <summary>Data数据是否进行了ZIP压缩处理</summary>
        public bool IsZipCompress
        {
            get { return _IsZipCompress; }
            set { _IsZipCompress = value; }
        }

        TransmitPackType _PackType = TransmitPackType.None;
        /// <summary>传输包的类型</summary>
        public TransmitPackType PackType
        {
            get { return _PackType; }
            set { _PackType = value; }
        }

        string _DisposerKey = "";
        /// <summary>处理者的标识符</summary>
        public string DisposeKey
        {
            get { return _DisposerKey; }
            set { _DisposerKey = value; }
        }


        bool _IsWriteBack = true;
        /// <summary>是否需要等待回复</summary>
        public bool IsWriteBack
        {
            get { return _IsWriteBack; }
            set { _IsWriteBack = value; }
        }

        DateTime _SendTime = DateTime.Now;
        /// <summary>发送时间</summary>
        public DateTime SendTime
        {
            get { return _SendTime; }
            internal set { _SendTime = value; }
        }

        /// <summary>
        /// 需要返回对象的IPEndPoint
        /// </summary>
        public System.Net.IPEndPoint IpEndPointInfo { get; set; }



        /// <summary>设置一个数据返回时的处理通知的结构对象</summary>
        [NonSerialized]
        public TransmitPack ReturnPack;


        public override string ToString()
        {
            return string.Format("Key={0},Data Lenght={1},PackType={2},Packkey={3},SendTime={4}",
                this.Key, this.Data.Length, this.PackType,this.Key, this.SendTime.ToString("yyyy-MM-dd HH:mm ss"));
        }

        
        #region IComparable 成员

        int IComparable.CompareTo(object obj)
        {
            TransmitPack tmp = obj as TransmitPack;
            if (tmp != null)
            {
                return this.Key.CompareTo(tmp.Key);
            }
            else
            {
                return -1;
            }
        }

        public override bool Equals(object obj)
        {
            TransmitPack tmp = obj as TransmitPack;
            if (tmp != null)
            {
                return this.Key.Equals(tmp.Key);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }
        #endregion

        /// <summary>包加包头序列化</summary>
        public byte[] TransformtionToByte()
        {
            return TransformtionToByte(this);
        }


        #region 包加包头序列化
        /// <summary>包加包头序列化</summary>
        public static byte[] TransformtionToByte(TransmitPack pack)
        {
            byte[] m_Head = new byte[4]; //用于计算包的长度

            byte[] bs = SerializeObjectClass.SerializObjectForBinary(pack,  new byte[4] ); //预留4个字符(int32)
            byte[] bLen = BitConverter.GetBytes(bs.Length - 4); //计算包的长度,长度为 4 的字节数组
            bLen.CopyTo(bs, 0); //填充到预留区(4个字符)
            return bs;
        }
        #endregion
    }

    /// <summary>传输包的集合类</summary>
    [Serializable]
    public class TransmitPackList : List<TransmitPack>
    {
    }

    /// <summary>返回的包的结构类</summary>
    [Serializable]
    public class ReturnPackClass
    {
        public readonly TransmitPack ReturnPack;
        public readonly Action<TransmitPack> ReturningNotify;

        public ReturnPackClass(TransmitPack pack, Action<TransmitPack> notify)
        {
            this.ReturningNotify = notify;
            this.ReturnPack = pack;
        }
    }




    /// <summary>传输包的处理接口</summary>
    public interface IDataOfDispose
    {
        void Dispose(TransmitPack pack);
    }


    /// <summary>处理接收到的数据并以TransmitPack对象输出</summary>
    public sealed class StateData
    {
        readonly byte[] Buff = new byte[1024 * 10];

        readonly Socket SocketMe;
        MemoryPoolItem Memory = ObjectPool.New<MemoryPoolItem>();
        int _TargetLen = 0;
        readonly Action<Socket, TransmitPack> _ActionPack;


        struct ThreadData
        {
            public readonly StateData ThisData;
            public readonly byte[] TargetData;

            public ThreadData(StateData sd, byte[] datas)
            {
                this.ThisData = sd;
                this.TargetData = datas;
            }
        }


        public StateData(Socket skt, Action<Socket, TransmitPack> actionPack)
        {
            this.SocketMe = skt;
            this._ActionPack = actionPack;
            skt.BeginReceive(Buff, 0, Buff.Length, SocketFlags.None, RecevieFromSocketMe, this);
        }

        //static int CountNum = 0; //Debug

        /// <summary>写入本地缓冲区</summary>
        void WriteMemory(int len)
        {
            if (len > 0)
            {
                this.Memory.Write(this.Buff,  len);
                _TargetLen = 0;
            }

            //Print(string.Format("===============>ThreadID:{0}  ,  Socket:{1}",
            //   Thread.CurrentThread.GetHashCode(), this.SocketMe.RemoteEndPoint.ToString()));

            if (_TargetLen == 0)
            {
                long writePoint = this.Memory.Position;
                this.Memory.Position = 0;
                byte[] tmpHead = new byte[4];
                this.Memory.Read(tmpHead,  tmpHead.Length);
                _TargetLen = BitConverter.ToInt32(tmpHead, 0) + 4;
                
                this.Memory.Position = writePoint;
            }

            if (this.Memory.Length >= _TargetLen)
            {
                long writePoint = this.Memory.Position;

                this.Memory.Position = 4;
                byte[] bsTarget = new byte[_TargetLen - 4];
                this.Memory.Read(bsTarget, bsTarget.Length);

                /**/
                ThreadData thrData = new ThreadData(this, bsTarget);
                ThreadPool.QueueUserWorkItem( DisposePack , thrData);

                byte[] bs = new byte[0];
                if (this.Memory.Length > _TargetLen)
                {
                    bs = new byte[this.Memory.Length - _TargetLen];
                    this.Memory.Read(bs, bs.Length);
                }

                ObjectPool.Delete<MemoryPoolItem>(this.Memory);

                this.Memory = ObjectPool.New<MemoryPoolItem>();
                this.Memory.Write(bs, bs.Length);

                _TargetLen = 0;
                if (bs.Length >= 4)
                {
                    this.WriteMemory(0);
                    
                }
            }
        }

        static void  DisposePack( object obj )
        {
            ThreadData threadData = (ThreadData)obj;
            TransmitPack pack =
                SerializeObjectClass.DeserializObjectForBinary<TransmitPack>(threadData.TargetData);

            //Console.WriteLine(CountNum++); //Debug

            if (threadData.ThisData._ActionPack != null)
            {
                Socket skt = threadData.ThisData.SocketMe;
                threadData.ThisData._ActionPack(skt, pack);
            }
        }

        static void RecevieFromSocketMe(IAsyncResult ar)
        {
            StateData sd = ar.AsyncState as StateData;

            try
            {
                int len = sd.SocketMe.EndReceive(ar);

                //Thread.Sleep(200);
                if (len!=0)
                {
                    sd.WriteMemory(len);
                }
                
            }
            catch (Exception e)
            {
                OutWriteInfo_Waite(e.Message);
            }

            try
            {
                sd.SocketMe.BeginReceive(sd.Buff, 0, sd.Buff.Length, SocketFlags.None, RecevieFromSocketMe, sd);
            }
            catch (Exception e2)
            {
                OutWriteInfo_Waite(e2.Message);
            }
        }


        public static void Print(string ss)
        {
            Console.WriteLine(ss);
        }

        #region MyRegion

        public static void OutWriteInfo_Waite(string str)
        {
            OutWriteInfo_Waite("{0}\r\n", str);
        }


        //static StreamWriter sw = new StreamWriter(@"E:\temp123.txt");
        public static void OutWriteInfo_Waite(string str, params object[] obj)
        {
            Console.WriteLine(str, obj);



            // SysLog.WriteLog(str,obj);
            //sw.WriteLine(str, obj);

            //sw.Flush();
            //sw.Close();
        }
        #endregion
    }


    
}
