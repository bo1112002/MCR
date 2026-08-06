using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;
using System.Runtime.CompilerServices;

namespace Tools
{
    /// <summary>
    /// 用于序列和反序列化的功能类
    /// </summary>
    public class SerializeObjectClass
    {
        private SerializeObjectClass()
        {
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static byte[] SerializObjectForXml(object obj)
        {
            MemoryPoolItem ms = ObjectPool.New<MemoryPoolItem>();

            XmlSerializer xmlSerialize = GetXmlSerializer(obj.GetType());
            xmlSerialize.Serialize(ms.GetBase(), obj);
            byte[] rtnBs = ms.ToArray();
            ObjectPool.Delete<MemoryPoolItem>(ms);

            return rtnBs;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static T DeserializObjectForXml<T>(byte[] byts)
        {
            MemoryPoolItem ms = ObjectPool.New<MemoryPoolItem>();
            ms.Write(byts, byts.Length);
            ms.Position = 0;
            XmlSerializer xmlSerialize = GetXmlSerializer( typeof(T) );
            object obj = xmlSerialize.Deserialize(ms.GetBase());
            ObjectPool.Delete<MemoryPoolItem>(ms);
            return (T)obj;
        }


        /// <summary>
        /// 获取一个XmlSerializer对象,
        /// 由于使用new XmlSerializer( t )方式会产生异常，
        /// 则使用XmlSerializer.FromTypes,但这种方式不会缓存Type所属性的Assembly相关信息,
        /// 所以需要做个缓存,以使下次XmlSerializer.FromTypes会自动查找到相关的信息
        /// </summary>
        static XmlSerializer GetXmlSerializer( Type t )
        {
            //XmlSerializer xmlSerialize = new XmlSerializer( t );
            XmlSerializer xmlSerialize = XmlSerializer.FromTypes( new Type[]{t})[0];
            SessionUserBase.MyCache.Set(t.FullName, xmlSerialize);
            return xmlSerialize;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static object DeserializObjectForXml(byte[] byts, Type t)
        {
            MemoryPoolItem ms = ObjectPool.New<MemoryPoolItem>();
            ms.Write(byts,   byts.Length);
            ms.Position = 0;

            XmlSerializer xmlSerialize = GetXmlSerializer(t);
            SessionUserBase.MyCache.Set(t.FullName, xmlSerialize);

            object obj = xmlSerialize.Deserialize(ms.GetBase());
            ObjectPool.Delete<MemoryPoolItem>(ms);
            return obj;
        }


        static BinaryFormatter binary = new BinaryFormatter();
        /// <summary>二进制序列</summary>
        /// <param name="obj">二进制序列的对象</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static byte[] SerializObjectForBinary(object obj)
        {
            MemoryPoolItem ms = ObjectPool.New<MemoryPoolItem>();
            binary.Serialize(ms.GetBase(), obj);
            byte[] rtnBs = ms.ToArray();
            ObjectPool.Delete<MemoryPoolItem>(ms);
            return rtnBs;
        }

        /// <summary>二进制序列</summary>
        /// <param name="obj">二进制序列的对象</param>
        /// <param name="head">二进制前加上一个自定义头数据</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static byte[] SerializObjectForBinary(object obj, byte[] head)
        {
            MemoryPoolItem ms = ObjectPool.New<MemoryPoolItem>();
            ms.Write(head, head.Length);
            binary.Serialize(ms.GetBase(), obj);
            byte[] rtnBs = ms.ToArray();
            ObjectPool.Delete<MemoryPoolItem>(ms);
            return rtnBs;
        }


        /// <summary>二进制的反序列</summary>
        /// <param name="byts">二进制数据</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static T DeserializObjectForBinary<T>(byte[] byts)
        {
            MemoryPoolItem ms = ObjectPool.New<MemoryPoolItem>();
            ms.Write(byts, byts.Length);
            ms.Position = 0;
            Stream msTemp = ms.GetBase();
            msTemp.Position = 0;
            object obj = binary.Deserialize(msTemp);
            ObjectPool.Delete<MemoryPoolItem>(ms);
            return (T)obj;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static object CloneObject(object obj)
        {
            byte[] bs = SerializObjectForBinary(obj);
            return DeserializObjectForBinary<object>(bs);
        }


        /* 示例：
        using (MemoryStream ms2 = new MemoryStream(rtnBs))
        {
           BinaryFormatter binary = new BinaryFormatter();
           Type[] types = new Type[] { typeof(MyType1), typeof(List'MyType2') ,typeof(MyType2)} ;
           int i = 0;
           BinderTagetType myBinder = new BinderTagetType(delegate(string strNamespace, string typeName) {
               return types[i++];
           });
           binary.Binder = myBinder;
           object obj = binary.Deserialize(ms2);
           Info_Head2 info2 = obj as Info_Head2;
        }
        */
        /// <summary>
        /// 用于对数据结构相同但类型名不一样或都需要指定一个特定类，
        /// 进行反序列化的操作处理类
        /// </summary>
        public class BinderTagetType : SerializationBinder
        {
            Func<string, string, Type> _FuncRetType;
            public BinderTagetType(Func<string, string, Type> funcRetType)
            {
                _FuncRetType = funcRetType;
            }


            public override Type BindToType(string assemblyName, string typeName)
            {
                return _FuncRetType(assemblyName, typeName);
            }
        }
    }
}
