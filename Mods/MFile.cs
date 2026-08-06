using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>文件对象(MF)</summary>
    [Serializable]
    public class MFile : EntityBase
    {
        #region============= 持久属性=========>>>
        int _PType = 0;
        /// <summary>物理类别,如text,word,PDF,jpg,rar等，由配置文件来解释及运行,以实现文件的可视化工作</summary>
        public int PType
        {
            get { return _PType; }
            set { _PType = value; }
        }
        int _LType = 0;
        /// <summary>逻辑类别，如课件，作业等，由配置文件来解释及运行,以实现文件的可视化工作，并包含了文件的存储区信息</summary>
        public int LType
        {
            get { return _LType; }
            set { _LType = value; }
        }
        string _Name = string.Empty;
        /// <summary>文件名(用户)</summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value.Trim(); }
        }
        bool _IsTemp = false;
        /// <summary>是否为临时文件, true:保存1小时间后被清理</summary>
        public bool IsTemp
        {
            get { return _IsTemp; }
            set { _IsTemp = value; }
        }
        DateTime _CreateTime = DateTime.Now;
        /// <summary>创建的时间</summary>
        public DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
        string _MEM_ID = string.Empty;
        /// <summary>文件所有者(创建者)</summary>
        public string MEM_ID
        {
            get { return _MEM_ID; }
            set { _MEM_ID = value.Trim(); }
        }
        string _TagID = string.Empty;
        /// <summary>文件分组标识ID,解决一个业务对象多个文件的问题(业务对象ID)</summary>
        public string TagID
        {
            get { return _TagID; }
            set { _TagID = value.Trim(); }
        }

        string _ContentType = string.Empty;
        /// <summary>文件内容类型</summary>
        public string ContentType
        {
            get { return _ContentType; }
            set { _ContentType = value; }
        }

        #endregion=============END==========<<<


        #region============= 重写成员=========>>>
        protected override string GetPrefixName()
        {
            return "MF";
        }
        public override Type GetTypeBase()
        {
            return typeof(MFile);
        }

        public override Result Validate(ObjectChangedTag tag)
        {
            if (this.Name == string.Empty)
            {
                return new Result(false, "账号为必填项");
            }
            else if (this.MEM_ID == string.Empty)
            {
                return new Result(false, "证件号为必填项");
            }
            else
            {
                return base.Validate(tag);
            }
        }
        

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.PType = reader.GetValue<int>(this, "PType");
            this.LType = reader.GetValue<int>(this, "LType");
            this.Name = reader.GetValue<string>(this, "Name");
            this.IsTemp = reader.GetValue<bool>(this, "IsTemp");
            this.CreateTime = reader.GetValue<DateTime>(this, "CreateTime");
            this.MEM_ID = reader.GetValue<string>(this, "MEM_ID");
            this.TagID = reader.GetValue<string>(this, "TagID");
            this.ContentType = reader.GetValue<string>(this, "ContentType");
        }

        #endregion=============END==========<<<

        /// <summary>
        /// 获取当前的物理文件的解释对象
        /// </summary>
        KeyValueClass GetPTypeObject()
        {
            KeyValueClass kvTemp = null;
            foreach (KeyValueClass the in GetPTypeList())
            {
                if (the.Key == "0") //保存临时返回值
                    kvTemp = the["0"];

                KeyValueClass cKV = the[this.PType.ToString()];
                if (cKV != null)
                {
                    kvTemp = cKV;
                    break;
                }
            }
            return kvTemp;
        }
        /// <summary>
        /// 获取文件的访问的URL(文件流方式)
        /// </summary>
        public string URL
        {
            get
            {
                if (this == MFile.NONE)
                {
                    return string.Format(AppSettings.FS_URL, "xxx");
                }
                else
                {
                    string suffix = GetPTypeObject().Val;
                    if (suffix.Trim() == string.Empty)
                    {
                        suffix = Path.GetExtension(this.Name);
                    }
                    return string.Format(AppSettings.FS_URL, this.AutoID);
                }
            }
        }

        /// <summary>获取下载的URL</summary>
        public string GetDownloadURL()
        {
            if (File.Exists(this.GetFullPath()))
            {
                return AppSettings.FS_SerURL.TrimEnd('/') + "/" + this.MEM_ID + "/" + this.AutoID + Path.GetExtension(this.Name);
            }
            return string.Empty;
        }

        /*
        /// <summary>文件的所属成员名</summary>
        public string MemberName
        {
            get
            {
                if (string.IsNullOrEmpty(this.MEM_ID))
                    return string.Empty;
                Member theMember = Member.GetByID(this.MEM_ID);
                if (theMember == null)
                    return string.Empty;

                return theMember.Name;
            }
        }
        */

        #region============= 文件的物理操作=========>>>
        /// <summary>获取文件的完整的物理路径(目录+文件名)(如果目录不存在则创建目录)</summary>
        public string GetFullPath(bool isAddExtName = true)
        {
            string dir = AppSettings.FS_DIR.Trim().TrimEnd('\\') + "\\";
            if (string.IsNullOrEmpty(this.MEM_ID) == false)
            {
                dir = (dir + this.MEM_ID + "\\");
            }
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }
            string filePath = dir +  this.AutoID;
            if (isAddExtName)
            {
                filePath += Path.GetExtension(this.Name);
            }
            return filePath;
        }

        /// <summary>获取当前文件的资源文件(如：pptx文件)</summary>
        public string GetFullPath_SDocument( string extName = ".pptx")
        {
            string strPath =  GetFullPath(false) +  "\\" +  this.Name.Replace(".data", extName) ;
            return strPath;
        }


        /// <summary>新建物理文件</summary>
        void CreatePhysicalFile(byte[] bs)
        {
            string filePath = GetFullPath();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllBytes(filePath, bs);
        }
        /// <summary>删除物理文件</summary>
        void DeletePhysicalFile()
        {
            string filePath = GetFullPath();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            string pdfPath = GetFullPath(false) + ".pdf";
            if (File.Exists(pdfPath))
            {
                File.Delete(pdfPath);
            }

        }


        /// <summary>获取文件的二进制数据</summary>
        public byte[] GeBinaryData()
        {
            string filePath = GetFullPath();
            if (File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }
            return new byte[0];
        }

        #endregion=============END==========<<<

        static readonly Dictionary<string, KeyValueClass> _FTypeMaps = new Dictionary<string, KeyValueClass>();
        /// <summary>获取当前文件的主文件类别对象</summary>
        public KeyValueClass GetFTypeTag()
        {
            string extName = Path.GetExtension(this.Name).Trim('.').ToLower();
            if (_FTypeMaps.ContainsKey(extName))
            {
                return _FTypeMaps[extName];
            }
            else
            {
                foreach (KeyValueClass kv in GetPTypeList())
                {
                    foreach (KeyValueClass kv2 in kv.Childs)
                    {
                        if (kv2.Val.ToLower() == extName)
                        {
                            _FTypeMaps[extName] = kv;
                            return kv;
                        }
                    }
                }
                _FTypeMaps[extName] = GetPTypeList()[0];
                return GetPTypeList()[0];
            }
        }

        /// <summary>获取当前文件类型图标的URL</summary>
        public string MyFileIconURL
        {
            get
            {
                if (this.GetFTypeTag() == null)
                    return string.Empty;
                return AppSettings.FileTypeIconURL.TrimEnd('/') + "/" + this.GetFTypeTag().Val;
            }
        }

        /// <summary>把当前临时文件转换为正式文件</summary>
        public Result ConvertTempFile()
        {
            if (this == MFile.NONE)
            {
                return Result.Invalid;
            }
            else
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 30 )   
                                 };
                Result rs = this.EntityMaping_Excute("ConvertTempFile", ps);
                if (rs.IsOK)
                {
                    this.IsTemp = false; 
                }
                return rs;
            }
        }
        /// <summary>重命名文件</summary>
        public Result Rename(string name)
        {
            if (this == MFile.NONE)
                return Result.Invalid;
            else
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@Name" , name ,  E_DbType.VarChar , 100 )
                                 };
                Result rs = this.EntityMaping_Excute("Rename", ps);
                return rs;
            }
        }
        /// <summary>修改当前文件的所有者</summary>
        public Result Update_MemID(string memberID)
        {
            WX_Member the =  WX_Member.GetByID(memberID);
            if (the == null)
                return new Result(false , "无效的成员");

            ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MEM_ID" , the.AutoID  ,  E_DbType.VarChar , 50 )
                                 };
            Result rs = this.EntityMaping_Excute("Update_MemID", ps);
            return rs;
        }


        /// <summary>复制当前文件(为临时文件)</summary> 
        public MFile Clone()
        {
            string newID = this.CreateTagID(DateTime.Now) + "_T";
            ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@NewID" , newID ,  E_DbType.VarChar , 30 ) 
                                 };
            Result rs = this.EntityMaping_Excute("Insert_Clone", ps);
            if (rs.IsOK)
            {
                MFile newFile = MFile.GetByID(newID);
                if (newFile != null && newFile != MFile.NONE)
                {
                    File.Copy(this.GetFullPath(), newFile.GetFullPath());
                    return newFile;
                }
            }
            return MFile.NONE;
        }

        #region============= 静态成员=========>>>

        public static readonly MFile NONE = new MFile();

        /// <summary>创建文件(同时创建物理文件)</summary>
        public static Result Insert(MFile newFile, byte[] bs)
        {
            if (newFile == MFile.NONE || bs == null || bs.Length == 0)
            {
                return new Result(false, "不是有效的文件");
            }
            else if (Encoding.Default.GetByteCount(newFile.Name) > 100)
            {
                return new Result(false, "文件名过长，最大不可超过100个字符(包括文件扩展名在内)");
            }

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , newFile.AutoID ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@PType" , newFile.PType ,  E_DbType.Int , 30 ) ,
                    new ParameterTag("@LType" , newFile.LType ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@Name" , newFile.Name ,  E_DbType.VarChar , 100 ) ,
                    new ParameterTag("@IsTemp" , newFile.IsTemp ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@CreateTime" , newFile.CreateTime ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@MEM_ID" , newFile.MEM_ID ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@TagID" , newFile.TagID ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@ContentType" , newFile.ContentType ,  E_DbType.VarChar , 50 ) 
                };

            Result rs = newFile.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                newFile.CreatePhysicalFile(bs);
                lock (_MFile_Cache)
                {
                    _MFile_Cache[newFile.AutoID] = newFile;
                }

                if (newFile.IsTemp == false)
                {
                    newFile.ConvertTempFile(); //尝试进行文件转换
                }

            }
            return rs;
        }

        /// <summary>创建文件(物理文件需要在创建记录成功之后创建)</summary>
        public static Result Insert(MFile newFile)
        {
            if (newFile == MFile.NONE)
            {
                return new Result(false, "不是有效的文件对象");
            }
            else if (Encoding.Default.GetByteCount(newFile.Name) > 100)
            {
                return new Result(false, "文件名过长，最大不可超过100个字符(包括文件扩展名在内)");
            }

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , newFile.AutoID ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@PType" , newFile.PType ,  E_DbType.Int , 30 ) ,
                    new ParameterTag("@LType" , newFile.LType ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@Name" , newFile.Name ,  E_DbType.VarChar , 100 ) ,
                    new ParameterTag("@IsTemp" , newFile.IsTemp ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@CreateTime" , newFile.CreateTime ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@MEM_ID" , newFile.MEM_ID ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@TagID" , newFile.TagID ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@ContentType" , newFile.ContentType ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = newFile.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                lock (_MFile_Cache)
                {
                    _MFile_Cache[newFile.AutoID] = newFile;
                }

                if (newFile.IsTemp == false)
                {
                    newFile.ConvertTempFile(); //尝试进行文件转换
                }

            }
            return rs;
        }

        /// <summary>创建文件(物理文件需要在创建记录成功之后创建)</summary>
        public static Result Insert( byte[] bs ,  int pType , int lType , string name , bool isTemp , WX_Member theMember , string contentType , string tagID="")
        {
            MFile newFile = new MFile();
            newFile.PType = pType;
            newFile.LType = lType;
            newFile.Name = name;
            newFile.IsTemp = isTemp;
            newFile.MEM_ID = theMember.AutoID ;
            newFile.ContentType = contentType;
            newFile.TagID = tagID;
            Result rs =  Insert(newFile, bs);
            if (rs.IsOK)
            {
                rs.Data = newFile;
            }
            return rs;
        }


        /// <summary>删除文件</summary>
        public static Result Delete(MFile theMFile)
        {
            if (theMFile == MFile.NONE)
                return new Result(false, "不是有效的文件");

            ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , theMFile.AutoID ,  E_DbType.VarChar , 30 )   
                                 };
            Result rs = theMFile.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                theMFile.DeletePhysicalFile();
                lock (_MFile_Cache)
                {
                    if (_MFile_Cache.ContainsKey(theMFile.AutoID))
                    {
                        _MFile_Cache.Remove(theMFile.AutoID);
                    }
                }
            }
            else
            {
                Loger.Log("文件删除失败(" + theMFile.AutoID + "):" + rs.Description);
            }
            return rs;
        }

        static readonly Dictionary<string, MFile> _MFile_Cache = new Dictionary<string, MFile>();
        /// <summary>获取ID的文件对象</summary>
        public static MFile GetByID(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return MFile.NONE;
            }

            if (_MFile_Cache.ContainsKey(id))
            {
                return _MFile_Cache[id];
            }
            else
            {
                if (_MFile_Cache.Count > 50000)
                {
                    lock (_MFile_Cache)
                    {
                        IList<string> list = _MFile_Cache.Keys.ToList<string>();
                        int stop = (int)list.Count / 2;
                        for (int i = list.Count - 1; i >= stop; i--)
                        {
                            _MFile_Cache.Remove(list[i]);
                        }
                    }
                }

                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , id ,  E_DbType.VarChar , 30 ) 
                                 };
                MFile the = null;
                Result rs = MFile.NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new MFile();
                        the.ToEntity(readers[0]);
                    }
                });

                if (the == null)
                    return MFile.NONE;

                lock (_MFile_Cache)
                {
                    _MFile_Cache[the.AutoID] = the;
                }
                return the;
            }
        }

        /// <summary>
        /// 多条件查询，其中有一个为null则将被忽略，但不能两个为null,否则返回空集合
        /// </summary>
        public static IList<MFile> GetByTagID(string tagID, string memberID = null)
        {
            List<MFile> list = new List<MFile>();
            if (string.IsNullOrEmpty(memberID))
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@TagID" , tagID ,  E_DbType.VarChar , 30 ) 
                                 };
                MFile the = new MFile();
                Result rs = the.EntityMaping_Excute("GetByTagID2", ps, (readers) =>
                {
                    foreach (EntityReader reader in readers)
                    {
                        the = new MFile();
                        the.ToEntity(reader);
                        list.Add(the);
                    }
                });
            }
            else
            {

                ParameterTag[] ps =  { 
                    new ParameterTag("@MEM_ID" , memberID ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@TagID" , tagID ,  E_DbType.VarChar , 30 ) 
                                 };
                MFile the = new MFile();
                Result rs = the.EntityMaping_Excute("GetByTagID", ps, (readers) =>
                {
                    foreach (EntityReader reader in readers)
                    {
                        the = new MFile();
                        the.ToEntity(reader);
                        list.Add(the);
                    }
                });
            }
            return list;
        }

        /// <summary>
        /// 清理临时文件( >outTime && IsTemp==true)
        /// </summary>
        public static void Clear(DateTime outTime)
        {
            ParameterTag[] ps =  { 
                    new ParameterTag("@OutTime" , outTime ,  E_DbType.DateTime ,  -1 )
                                 };
            MFile the = new MFile();
            Result rs = the.EntityMaping_Excute("Clear", ps, (readers) =>
            {
                foreach (EntityReader reader in readers)
                {
                    the = new MFile();
                    the.ToEntity(reader);
                    MFile.Delete(the);
                }
            });
        }

        /// <summary>
        /// 获取文件类型集合
        /// </summary>
        public static IList<KeyValueClass> GetPTypeList()
        {
            return KeyValueClass.Map_KVs["PType"].Childs;
        }

        /// <summary>
        /// 获取课件自定义类型集合
        /// </summary>
        public static IList<KeyValueClass> GetUTypeList()
        {
            return KeyValueClass.Map_KVs["UType"].Childs;
        }

        /// <summary>依据文件名获取系统可以解释的文件类型值,如果不存则返回0</summary>
        public static int GetPType(string fileName)
        {
            string extName = Path.GetExtension(fileName).Trim('.').ToLower();
            foreach (KeyValueClass kv in GetPTypeList())
            {
                foreach (KeyValueClass kv2 in kv.Childs)
                {
                    if (kv2.Val.ToLower() == extName)
                        return Convert.ToInt32(kv2.Key);
                }
            }
            return 0;
        }

        #endregion=============END==========<<<


    }
}
