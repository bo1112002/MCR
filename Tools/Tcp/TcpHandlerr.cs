using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Tools.Tcp
{

    /// <summary>Tcp通信接口</summary>
    public interface ITcpHandler
    {
        /// <summary>指定一个数据包发送到远程服务器终端，并返回结果</summary>
        /// <param name="pack">要发送的数据包</param>
        /// <param name="callback">返回结果的委托对象</param>
        Result SendAsync(TransmitPack pack, Action<TransmitPack> callback);
        /// <summary></summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        Result Send(TransmitPack pack);

    }


    /// <summary>Tcp通信的控制类</summary>
    public class TcpHandler : IDisposable, ITcpHandler
    {
        readonly TcpClient _TcpAsync;
        readonly TcpClient _Tcp;

        /// <summary>构造函数TcpHandler</summary>
        /// <param name="ip">要访问的服务端的IP</param>
        /// /// <param name="port">要访问的服务端的端口</param>
        public TcpHandler(string ip, int port)
        {
            IPEndPoint serPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            try
            {
                _TcpAsync = new TcpClient();
                _TcpAsync.ReceiveTimeout = 15000;
                _TcpAsync.Connect(serPoint);
                StateData stat = new StateData(_TcpAsync.Client, WhenReceivePacks);

                _Tcp = new TcpClient();
                _Tcp.ReceiveTimeout = 15000;
                _Tcp.Connect(serPoint);
            }
            catch (Exception e)
            {
                //SysInfo.ISystemLog().LogMsSqlSend(LoggerMethed.Fatal, LoggerForm.FormSystem, e.Message);
                //SysInfo.ISystemLog().SendUDP(e.Source, e.Message);
            }
        }

        static readonly byte[] m_Head = new byte[4]; //用于计算包的长度
        static readonly Hashtable m_TableCallback = new Hashtable();
        static void WhenReceivePacks(Socket skt , TransmitPack pack)
        {
            Action<TransmitPack> callback = m_TableCallback[pack.Key] as Action<TransmitPack>;
            if (callback != null)
            {
                callback(pack);
                lock (m_TableCallback)
                {
                    m_TableCallback.Remove(pack.Key);
                }
            }
        }

        #region IDisposable 成员

        void IDisposable.Dispose()
        {
            if (_TcpAsync != null)
            {
                _TcpAsync.Close();
            }
            if (_Tcp != null)
            {
                _Tcp.Close();
            }
        }

        #endregion

        #region ITcpHandler 成员
        int ij = 0;
        [MethodImpl(MethodImplOptions.Synchronized)]
        Result ITcpHandler.Send(TransmitPack pack)
        {
            Console.WriteLine(System.Threading.Thread.CurrentThread.GetHashCode().ToString());
            try
            {
                Console.WriteLine(ij.ToString()); 
                ij++;
                byte[] bs = pack.TransformtionToByte();

                int n = _Tcp.Client.Send(bs);
                if (n > 0)
                {
                    byte[] rtnBytes = new byte[4]; // m_Head;

                    int len = _Tcp.Client.Receive(rtnBytes, rtnBytes.Length, SocketFlags.None);
                    if (len == 4)
                    {
                        int targetLen = BitConverter.ToInt32(rtnBytes, 0);

                        byte[] bsBody = new byte[10240];
                        MemoryPoolItem memoryItem = ObjectPool.New<MemoryPoolItem>();

                        while (true)
                        {
                            int len2 = _Tcp.Client.Receive(bsBody, bsBody.Length, SocketFlags.None);
                            memoryItem.Write(bsBody, len2);
                            if (memoryItem.Length >= targetLen)
                                break;
                        }


                        /*return OResult.Success;*/

                        TransmitPack rtnPack =
                            SerializeObjectClass.DeserializObjectForBinary<TransmitPack>(memoryItem.ToArray());
                        pack.PackType = rtnPack.PackType;
                        pack.Data = rtnPack.Data;
                        ObjectPool.Delete<MemoryPoolItem>(memoryItem);

                        return Result.OK;

                    }
                }
                Console.WriteLine(System.Threading.Thread.CurrentThread.GetHashCode().ToString());
                return Result.ERR;
            }
            catch (Exception err)
            {
                return new Result( false, err.Message  );
            }
        }

        /// <summary>发送数据包到指定的服务端，并有回复的时候参数callback将被调用,callback参数的值可以为null
        /// (如果TransmitPack对象的IsWriteBack为false,callback不会被调用)</summary>
        Result ITcpHandler.SendAsync(TransmitPack pack, Action<TransmitPack> callback)
        {
            if (pack.IsWriteBack && callback != null)
            {
                lock (m_TableCallback)
                {
                    m_TableCallback[pack.Key] = callback;
                }
            }

            try
            {
                byte[] bs = SerializeObjectClass.SerializObjectForBinary(pack, m_Head); //预留4个字符(int32)
                byte[] bLen = BitConverter.GetBytes(bs.Length - 4); //计算包的长度
                bLen.CopyTo(bs, 0); //填充到预留区(4个字符)
                _TcpAsync.Client.Send(bs); //发送包
                return Result.OK;
            }
            catch (Exception err)
            {
                return new Result(false, err.Message);
            }
        }

        #endregion
    }


}
