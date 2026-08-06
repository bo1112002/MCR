using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Tools;

namespace MCR.Mods.VSTO
{

    /// <summary>PPT文件结构类</summary>
    [Serializable]
    public class PPT_FileClass
    {
        string _Name = string.Empty;
        /// <summary>文档标题</summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }


        string _FileID = string.Empty;
        /// <summary>文档ID</summary>
        public string FileID
        {
            get { return _FileID; }
            set { _FileID = value; }
        }

        string _FileName = string.Empty;
        /// <summary>ppt的物理文件名</summary>
        public string FileName
        {
            get { return _FileName; }
            set { _FileName = value; }
        }

        PPT_FileType _FType = PPT_FileType.Courseware;
        /// <summary>当前文档的业务类型</summary>
        public PPT_FileType FType
        {
            get { return _FType; }
            set { _FType = value; }
        }

        Size _GlobalSize = new Size();
        /// <summary>演示页的通用Size</summary>
        public Size GlobalSize
        {
            get { return _GlobalSize; }
            set { _GlobalSize = value; }
        }


        bool _IsVertical = true;
        /// <summary>是否横向</summary>
        public bool IsVertical
        {
            get { return _IsVertical; }
            set { _IsVertical = value; }
        }
        bool _IsRun = false;
        /// <summary>是否正在演示运行中</summary>
        public bool IsRun
        {
            get { return _IsRun; }
            set { _IsRun = value; }
        }

        /// <summary>课堂明细信息</summary>
        public CourseDetail DetailInfo {get;set;}


        readonly IList<PPT_PageClass> _Pages = new List<PPT_PageClass>();
        /// <summary>文件的页集合</summary>
        public IList<PPT_PageClass> Pages
        {
            get { return _Pages; }
        }



        //============================================

        /// <summary>获取二进制序列后的数据</summary>
        public  byte[] Serialzable( )
        {
            byte[] bs =  Tools.SerializeObjectClass.SerializObjectForBinary(this);
            return bs;
        }

        /// <summary>反序列为当前的类型对象(如果序列失败，则返回null)</summary>
        public static PPT_FileClass Deserialzable(byte[] bs)
        {
            try
            {
                PPT_FileClass the = Tools.SerializeObjectClass.DeserializObjectForBinary<PPT_FileClass>(bs);
                return the;
            }
            catch(Exception err)
            {
                Loger.Log(err);
            }
            return null;
        }
        /// <summary>从文件中读取相应的序列数据，再进行反序列对象处理</summary>
        public static PPT_FileClass Deserialzable( FileStream fs )
        {
            fs.Position = 0;

            byte[] bs = new byte[4];
            fs.Read(bs, 0, bs.Length);
            //int fPosition = BitConverter.ToInt32(bs, 0);

            fs.Read(bs, 0, bs.Length);
            int fSize = BitConverter.ToInt32(bs, 0); //获取长度

            byte[] bs2 = new byte[fSize];
            fs.Read(bs2, 0, bs2.Length); //获取数据

            PPT_FileClass the = Deserialzable(bs2); //获取对象
            return the;
        }


    }

    /// <summary>PPT演示页结构类</summary>
    [Serializable]
    public class PPT_PageClass
    {

        string _Name = string.Empty;
        /// <summary>页名称(用于查找演示页中的Slide对象)</summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        int _Index = -1;
        /// <summary>页码</summary>
        public int Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        PPT_SlideType _PageType = PPT_SlideType.NONE;  
        /// <summary>当前页的业务类型</summary>
        public PPT_SlideType PageType
        {
            get { return _PageType; }
            set { _PageType = value; }
        }


        int _SlideID = 0;
        /// <summary>物理ppt文件页的标识</summary>
        public int SlideID
        {
            get { return _SlideID; }
            set { _SlideID = value; }
        }


        MC_QuestionClass _MyQuestion = null;
        /// <summary>题目信息对象</summary>
        public MC_QuestionClass MyQuestion
        {
            get { return _MyQuestion; }
            set { _MyQuestion = value; }
        }


        string _ImgName = string.Empty;
        /// <summary>图片名</summary>
        public string ImgName
        {
            get { return _ImgName; }
            set { _ImgName = value; }
        }


        string _Remark = string.Empty;
        /// <summary>页备注</summary>
        public string Remark
        {
            get { return _Remark; }
            set { _Remark = value; }
        }


        string _VedioURL = string.Empty;
        /// <summary>视频URL(如果为Empty,则表示不存在)</summary>
        public string VedioURL
        {
            get { return _VedioURL; }
            set { _VedioURL = value; }
        }

    }

    /// <summary>
    /// 文档的临时数据存储的处理类
    /// </summary>
    [Serializable]
    public class PPT_TagsClass : Dictionary<string, string>
    {
        MC_QuestionClass _QuestionObject = null;
        /// <summary>获取选择题对象</summary>
        public MC_QuestionClass GetQuestionObject()
        {
            return _QuestionObject;
        }
    }
}
