using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Tools.Config;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Configuration;
using System.Diagnostics;

namespace Tools
{
    /// <summary>
    /// 用于描述一对键值结构的数据类
    /// </summary>
    [Serializable]
    public class KeyValueStruct
    {
        public KeyValueStruct(string key, object val)
        {
            this._Key = key;
            this._Val = val;
        }

        string _Key;
        /// <summary>获取键</summary>
        public string Key
        {
            get { return _Key; }
        }
        object _Val;
        /// <summary>获取值</summary>
        public object Val
        {
            get { return _Val; }
        }

        readonly DateTime _Createtime = DateTime.Now;
        /// <summary>获取创建的时间</summary>
        public DateTime Createtime
        {
            get { return _Createtime; }
        } 


        /// <summary>重写:字符串样式输出</summary>
        public override string ToString()
        {
            return this._Key + ":" + _Val;
        }
    }


    /// <summary>表示一个文件结构的类</summary>
    [Serializable]
    public class KeyValueFile : IComparable<KeyValueFile>
    {
        string _FileName = "";
        /// <summary>
        ///  获取或设置文件的名称
        /// </summary>
        public string FileName
        {
            get { return _FileName; }
            set { _FileName = value; }
        }

        byte[] _Content = null;
        /// <summary>
        /// 获取或设置文件的内容
        /// </summary>
        public byte[] Content
        {
            get { return _Content; }
            set { _Content = value; }
        }

        DateTime _CreateTime = SessionUserBase.GetNewTime();
        /// <summary>获取当前文件的创建时间</summary>
        public DateTime CreateTime
        {
            get { return _CreateTime; }
        }


        public KeyValueFile() { }
        public KeyValueFile(string name, byte[] byts)
        {
            this._FileName = name;
            this._Content = byts;
        }


        #region IComparable<KeyValueFile> 成员

        int IComparable<KeyValueFile>.CompareTo(KeyValueFile other)
        {
            return this.FileName.CompareTo(other.FileName);
        }

        #endregion
    }




    /// <summary>表示一个文件结构的集合类</summary>
    [Serializable]
    public class KeyValueFileCollection : List<KeyValueFile>, IComparer<KeyValueFile>
    {
        object _InfoObject = null;
        /// <summary>
        /// 获取或设置当前文件集所附加的数据对象
        /// </summary>
        public object InfoObject
        {
            get { return _InfoObject; }
            set { _InfoObject = value; }
        }

        public KeyValueFileCollection()
        { }
        public KeyValueFileCollection(IEnumerable<KeyValueFile> collection)
            : base(collection)
        { }



        #region IComparer<KeyValueFile> 成员

        int IComparer<KeyValueFile>.Compare(KeyValueFile x, KeyValueFile y)
        {
            return x.FileName.CompareTo(y.FileName);
        }

        #endregion

    }




    /// <summary>Key-Value结构</summary>
    [Serializable]
    public class KeyValueClass
    {
        #region============= 原子属性=========>>>
        string _Key = string.Empty;
        /// <summary>Key</summary>
        [System.Xml.Serialization.XmlAttribute]
        public string Key
        {
            get { return _Key; }
            set { _Key = value; }
        }

        string _Val = string.Empty;
        /// <summary>Value</summary>
        [System.Xml.Serialization.XmlAttribute]
        public string Val
        {
            get { return _Val; }
            set { _Val = value; }
        }

        string _Tag = "UI";
        /// <summary>tag标识</summary>
        [System.Xml.Serialization.XmlAttribute]
        public string Tag
        {
            get { return _Tag; }
            set { _Tag = value; }
        }

        string _M = string.Empty;
        /// <summary>Model</summary>
        [System.Xml.Serialization.XmlAttribute]
        public string M
        {
            get { return _M; }
            set { _M = value; }
        }

        string _V = string.Empty;
        /// <summary>Value</summary>
        [System.Xml.Serialization.XmlAttribute]
        public string V
        {
            get { return _V; }
            set { _V = value; }
        }

        //
        string _RefChildsKey = string.Empty;
        /// <summary>引用另外的Childs(RootKeyName1->RootKeyName2...->RootKeyNameN[0,5,...])</summary>
        [System.Xml.Serialization.XmlAttribute]
        public string RefChildsKey
        {
            get { return _RefChildsKey; }
            set { _RefChildsKey = value; }
        }


        /// <summary>属性RefChildsKey的解释类</summary>
        class RefChildsKeyClass
        {
            readonly string[] Keys;
            readonly string[] ItemKeys;
            public RefChildsKeyClass(string strKey)
            {
                string[] tmps = strKey.Trim().Split(new char[] { '[' }, StringSplitOptions.RemoveEmptyEntries);
                if (tmps.Length >= 2)
                {
                    string tmpA = tmps[1].Trim().TrimEnd(']');
                    ItemKeys = tmpA.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                string tmpB = tmps[0].Trim();
                Keys = tmpB.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
            }

            List<KeyValueClass> _DefChilds;
            public List<KeyValueClass> GetTagetChilds()
            {
                if (_DefChilds == null)
                {
                    _DefChilds = new List<KeyValueClass>();
                    KeyValueClass kv = KeyValueClass.Map_KVs[Keys[0].Trim()];
                    if (kv != null)
                    {
                        for (int i = 1; i < Keys.Length; i++)
                        {
                            kv = kv[Keys[i].Trim()];
                            if (kv == null)
                                break;
                        }
                    }

                    if (kv != null)
                    {
                        if (ItemKeys == null || ItemKeys.Length == 0 || ItemKeys[0] == "*")
                        {
                            _DefChilds.AddRange(kv.Childs);
                        }
                        else
                        {
                            foreach (string s in ItemKeys)
                            {
                                KeyValueClass tmp = kv[s.Trim()];
                                if (tmp != null)
                                {
                                    _DefChilds.Add(tmp);
                                }
                            }
                        }

                    }
                }
                return _DefChilds;
            }
        }

        #endregion=============END==========<<<

        [NonSerialized]
        RefChildsKeyClass _RefKey = null;
        List<KeyValueClass> _Childs = new List<KeyValueClass>();

        /// <summary>
        /// 返回当前结点的子结点
        /// </summary>
        [System.Xml.Serialization.XmlArray]
        public List<KeyValueClass> Childs
        {
            get
            {
                if (RefChildsKey.Trim().Length > 0)
                {
                    if (_RefKey == null)
                    {
                        _RefKey = new RefChildsKeyClass(RefChildsKey);
                    }
                    return _RefKey.GetTagetChilds();
                }
                else
                {
                    return _Childs;
                }
            }
            set
            {
                _Childs = value;
            }
        }


        #region============= 通过配置的值，构造指定的对象=========>>>
        /// <summary>构造属性M中指定的对象，如果不存在则返回null</summary>
        public object CreateM_Object(params object[] objs)
        {
            if (this.M.Trim() == string.Empty)
                return null;
            return Create_Object(this.M, objs);
        }
        /// <summary>构造属性M中指定的对象，如果不存在则返回null</summary>
        public object CreateV_Object(params object[] objs)
        {
            if (this.V.Trim() == string.Empty)
                return null;
            return Create_Object(this.V, objs);
        }
        #endregion=============END==========<<<

        public KeyValueClass() { }
        public KeyValueClass(string k, string v, List<KeyValueClass> childs)
        {
            this.Key = k;
            this.Val = v;
            if (childs == null)
                return;

            this._Childs = childs;
        }

        /// <summary>
        /// 查找当前子结点的key匹配的结点对象，如果不存在则返回null
        /// </summary>
        public KeyValueClass this[string key]
        {
            get
            {
                foreach (KeyValueClass kv in Childs)
                {
                    if (kv.Key == key)
                    {
                        return kv;
                    }
                }
                return null;
            }
        }



        public override string ToString()
        {
            return string.Format("[{0}] [{1}] [{2}]", this.Key, this.Val, this.Childs.Count);
        }



        #region============= 静态成员=========>>>

        [MethodImpl(MethodImplOptions.Synchronized)]
        static KeyValueMaps GetALL()
        {
            string fPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.TrimStart("file:///".ToCharArray())) + "\\KV.xml";
            KeyValueMaps mapKV = new KeyValueMaps();
            if (File.Exists(fPath) == false) //如果在运行目录下找不到，则在app.config文件下获取KV_File结点下的目标文件路径
            {
                fPath = ConfigurationManager.AppSettings["KV_File"] ?? string.Empty;
                if (File.Exists(fPath) == false)
                {
                    return mapKV;
                }
            }


            try
            {
                byte[] bs = File.ReadAllBytes(fPath);
                List<KeyValueClass> listKV = SerializeObjectClass.DeserializObjectForXml<List<KeyValueClass>>(bs);
                foreach (KeyValueClass kv in listKV)
                {
                    if (mapKV.ContainsKey(kv.Key) == false)
                    {
                        mapKV.Add(kv.Key, kv);
                    }
                }
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("EventSystem", "读取语言文件(KV.xml)错误：" + e.Message, EventLogEntryType.Error);
            }
            return mapKV;
        }

        public static void Writer(string fPath)
        {
            List<KeyValueClass> listKV = new List<KeyValueClass>();

            string[] ss = Directory.GetFiles(@"E:\NewBo\BaseFrom\CStarry\Imgs");
            foreach (string s in ss)
            {
                string fn = Path.GetFileName(s);
                fn = fn.Substring(0, fn.LastIndexOf('.'));
                string path = "/" + s.Replace('\\', '/');
                KeyValueClass kv = new KeyValueClass(fn, s, null);
                listKV.Add(kv);
            }
            ss = Directory.GetFiles(@"E:\NewBo\BaseFrom\CStarry\Imgs\P64");
            foreach (string s in ss)
            {
                string fn = Path.GetFileName(s);
                fn = fn.Substring(0, fn.LastIndexOf('.'));
                string path = "/" + s.Replace('\\', '/');
                KeyValueClass kv = new KeyValueClass(fn, s, null);
                listKV.Add(kv);
            }
            byte[] bs = SerializeObjectClass.SerializObjectForXml(listKV);
            File.WriteAllBytes(fPath, bs);
        }


        private static  KeyValueMaps _Map_KVs = null;
        /// <summary>获取所有语言标签</summary>
        public static KeyValueMaps Map_KVs
        {
            get 
            {
                if (_Map_KVs == null || _Map_KVs.Count == 0 )
                {
                    _Map_KVs = KeyValueClass.GetALL(); 
                }
                return KeyValueClass._Map_KVs; 
            }
        } 



        /// <summary>提供主结点名(key)递归查找Childs，如果找到返回目标对象(targetKey)，否则返回null</summary>
        public static KeyValueClass Find(string key, string targetKey)
        {
            if (Map_KVs.ContainsKey(key))
            {
                KeyValueClass p = Map_KVs[key];
                return FindChild(p, targetKey);
            }
            return null;
        }
        /// <summary>递归查找Childs，如果找到返回目标对象，否则返回null</summary>
        public static KeyValueClass FindChild(KeyValueClass kv, string targetKey)
        {
            if (kv.Key == targetKey)
                return kv;

            foreach (KeyValueClass the in kv.Childs)
            {
                KeyValueClass rs = FindChild(the, targetKey);
                if (rs != null)
                {
                    return rs;
                }
            }
            return null;
        }



        /// <summary>构造指定的对象，如果不存在则返回null</summary>
        public static object Create_Object(string typeName, params object[] objs)
        {
            Type t = Type.GetType(typeName.Trim(), false);
            if (t == null)
                return null;

            object rtnObject = Activator.CreateInstance(t, objs);
            return rtnObject;
        }

        #endregion=============END==========<<<

        /*
        /// <summary>搜索Control类型的容器中的子控件，并把对应的标签值赋给控件的Text属性</summary>
        public static void SetValToControl(Control contina)
        {
            KeyValueClass kv = MapLanguage[contina.GetType().Name];
            if (kv.Tag != "UI")
                return;
            if (string.IsNullOrEmpty(kv.Val) == false)
            {
                try
                {
                    contina.Text = kv.Val;
                }
                catch
                { }
            }
            foreach (KeyValueClass c in kv.Childs)
            {
                if (contina.Controls.ContainsKey(c.Key))
                {
                    contina.Controls[c.Key].Text = c.Val;
                }
                else
                {
                    Control[] ctrls = contina.Controls.Find(c.Key, true);
                    if (ctrls != null && ctrls.Length > 0)
                    {
                        ctrls[0].Text = c.Val;
                    }
                }

            }
        }
*/
    }

    /// <summary>KeyValueClass对象的配置集合(字典)</summary>
    [Serializable]
    public class KeyValueMaps 
    {
        readonly Dictionary<string, KeyValueClass> _Map = new Dictionary<string, KeyValueClass>();

        public KeyValueClass this[string key]
        {
            get
            {
                if (_Map.ContainsKey(key) == true)
                {
                    return _Map[key];
                }
                return null ;
            }
            set
            {
                _Map[value.Key] = value;
            }
        }


        public int Count
        {
            get
            {
                return _Map.Count;
            }
        }

        public bool ContainsKey(string key)
        {
            return _Map.ContainsKey(key);
        }

        public void Add(KeyValueClass kv)
        {
            lock (_Map) {
                _Map[kv.Key] = kv;
            }
            
        }

        public void Add( string key ,  KeyValueClass kv)
        {
            lock (_Map) {
                _Map[key] = kv;
            }
            
        }

        public void Remove(string key)
        {
            lock (_Map)
            {
                if (_Map.ContainsKey(key))
                {
                    _Map.Remove(key);

                }
            }
        }
    }


    
    /// <summary>压缩文件的结构处理类</summary>
    [Serializable]
    public class ZipFileClass
    {
        /// <summary>获取或设置文件的名称</summary>
        public string Name { get; set; }
        /// <summary>是否为文件(true:文件, false:文件夹)</summary>
        public bool IsTheFile { get; set; }
        /// <summary>文件起始位置</summary>
        public long BeginPoint { get; set; }
        /// <summary>文件长度</summary>
        public long Size { get; set; }

        readonly IList<ZipFileClass> _Childs = new List<ZipFileClass>();
        /// <summary>子文件及文件夹的集合对象</summary>
        public IList<ZipFileClass> Childs
        {
            get { return _Childs; }
        }


        public ZipFileClass()
        { }

        public ZipFileClass(string name, bool isTheFile = false, long beginPoint = 0, long size = 0)
        {
            this.Name = name;
            this.IsTheFile = isTheFile;
            this.BeginPoint = beginPoint;
            this.Size = size;
        }

        public ZipFileClass( FileInfo info )
        {
            this.Name = info.Name;
            this.IsTheFile = true;
            this.BeginPoint = 0;
            this.Size = info.Length ;
        }

        public ZipFileClass(DirectoryInfo theDirInfo)
        {
            this.Name = theDirInfo.Name;
            this.IsTheFile = false;
            this.BeginPoint = 0;
            this.Size = 0;
        }


        /// <summary>压缩文件夹集合</summary>
        void ZipChilds(DirectoryInfo theDirInfo, FileStream zipFile )
        { 
             ZipFileClass theFile = null;
            foreach (FileInfo fInfo in theDirInfo.GetFiles() )
            {
                if (fInfo.Attributes == (FileAttributes.Hidden | FileAttributes.Archive | FileAttributes.NotContentIndexed) 
                    || fInfo.Attributes == FileAttributes.System)
                {
                    continue;
                }
                theFile = new ZipFileClass(fInfo);
                theFile.BeginPoint = zipFile.Position;

                byte[] bs = File.ReadAllBytes(fInfo.FullName) ;
                zipFile.Write(bs, 0, bs.Length);

                this.Childs.Add(theFile);
            }

            foreach (DirectoryInfo theDirInfo2 in theDirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                theFile = new ZipFileClass(theDirInfo2);
                this.Childs.Add(theFile);

                theFile.ZipChilds(theDirInfo2, zipFile);
            }


        }

        /// <summary>解压缩文件夹集合</summary>
        void UnZipChilds(DirectoryInfo theDirInfo, FileStream zipFile)
        {
            foreach (ZipFileClass the in this.Childs) {
                if (the.IsTheFile)
                {
                    byte[] bs = new byte[the.Size];
                    zipFile.Position = the.BeginPoint ;
                    zipFile.Read(bs, 0 , bs.Length);
                    string fileName = theDirInfo.FullName   + the.Name;
                    if (File.Exists(fileName) == true)
                    {
                        File.Delete(fileName);
                    }
                    File.WriteAllBytes(fileName, bs);
                }
                else
                {
                    string dirName = theDirInfo.FullName  + the.Name + "\\" ;
                    DirectoryInfo dirInfo2 = new DirectoryInfo(dirName);
                    if (dirInfo2.Exists == false)
                    {
                        dirInfo2.Create();
                    }
                    the.UnZipChilds(dirInfo2, zipFile);
                }
            }

        }

        /*=============================================*/

        private readonly static ZipFileClass _TOP = new ZipFileClass("");
        /// <summary>获取或设置当前文件处理的根目录</summary>
        public static ZipFileClass TOP
        {
            get { return ZipFileClass._TOP; }
        }



        /// <summary>
        /// 压缩文件夹集合
        /// <remarks>
        /// </remarks>
        /// 文件存储结构如下：
        /// |===其它数据===|===文件数据===|===对象(ZipFileClass)序列数据===|===对象(ZipSizeClass)序列数据 ===|===对象(ZipSizeClass)序列数据长度 ===|
        /// </summary>
        public static void  Zip_Files(DirectoryInfo dirRoot , FileStream zipFile)
        {
            TOP.Childs.Clear();

            long theStart = zipFile.Position;

            TOP.Name = dirRoot.FullName ;
            TOP.ZipChilds( dirRoot , zipFile ) ;

            long theEndBody = zipFile.Position;

            byte[] bs =  SerializeObjectClass.SerializObjectForBinary(TOP) ;
            zipFile.Write(bs, 0, bs.Length);

            long theHeadSize = bs.Length;


            ZipSizeClass zipSize = new ZipSizeClass( theStart , theEndBody , theHeadSize );
            bs = SerializeObjectClass.SerializObjectForBinary(zipSize);
            zipFile.Write(bs, 0, bs.Length);

            bs = BitConverter.GetBytes(bs.Length);
            zipFile.Write(bs, 0, bs.Length); //4个byte


            

        }


        public static void UnZip_Files(FileStream zipFile, DirectoryInfo dirRoot )
        {
            TOP.Name = dirRoot.FullName;

            byte[] bs = new byte[4] ;
            int offset = (int)(zipFile.Length - bs.Length ) ;
            zipFile.Position = offset;
            zipFile.Read(bs, 0, bs.Length);

            int len = BitConverter.ToInt32( bs , 0  ) ;

            byte[] bs2 = new byte[len] ;  
            int offset2 = (int)(zipFile.Length - bs.Length - len ) ;
            zipFile.Position = offset2;
            zipFile.Read( bs2 , 0  , bs2.Length ) ;
            ZipSizeClass theZipSize =  SerializeObjectClass.DeserializObjectForBinary<ZipSizeClass>(bs2);

            /* * */
            byte[] bs3 = new byte[ theZipSize.HeadSize ] ;
            zipFile.Position = theZipSize.BodyEndPosition ;
            zipFile.Read( bs3 , 0 , bs3.Length ) ;
            ZipFileClass theFile = SerializeObjectClass.DeserializObjectForBinary<ZipFileClass>(bs3) ;
            theFile.UnZipChilds(dirRoot, zipFile);
            

        }

    }

    /// <summary>与ZipFileClass生成的压缩文件结构Size描述类</summary>
    [Serializable]
    public class ZipSizeClass
    {
        /// <summary>
        /// 文件数据的起始位置
        /// </summary>
        public long StartPosition { get; set; }
        /// <summary>
        /// 文件数据的结束位置
        /// </summary>
        public long BodyEndPosition { get; set; }
        /// <summary>
        /// 对象(ZipFileClass)序列数据的长度
        /// </summary>
        public long HeadSize { get; set; }

        public ZipSizeClass() { }
        public ZipSizeClass( long start , long end , long size ) {

            this.StartPosition = start;
            this.BodyEndPosition = end;
            this.HeadSize = size;
        }
    }


}
