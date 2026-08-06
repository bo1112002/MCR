using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>弹幕消息类(DOCS)</summary>
    [Serializable]
    public class Document_Speak : EntityBase
    {
        #region 持久属性
        string _MemberID = string.Empty;
        /// <summary>成员ID(发言人)</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }
        string _CourseInfoID = string.Empty;
        /// <summary>所属的课程ID</summary>
        public string CourseInfoID
        {
            get { return _CourseInfoID; }
            set { _CourseInfoID = value; }
        }
        string _DocumentID = string.Empty;
        /// <summary>资源文件的阅读ID</summary>
        public string DocumentID
        {
            get { return _DocumentID; }
            set { _DocumentID = value; }
        }
        long _Time_SeqVale = 0;
        /// <summary>时序值(控制当前弹幕消息出现时间点)</summary>
        public long Time_SeqVale
        {
            get { return _Time_SeqVale; }
            set { _Time_SeqVale = value; }
        }
        string _MsgContent = string.Empty;
        /// <summary>消息内容</summary>
        public string MsgContent
        {
            get { return _MsgContent; }
            set { _MsgContent = value; }
        }
        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }
        #endregion

        protected Document_Speak()
        {
        }

        #region============= 重写成员=========>>>
        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.DocumentID = reader.GetValue<string>(this, "DocumentID");
            this.CourseInfoID = reader.GetValue<string>(this, "CourseInfoID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.Time_SeqVale = reader.GetValue<long>(this, "Time_SeqVale");
            this.MsgContent = reader.GetValue<string>(this, "MsgContent");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
        }


        public override Type GetTypeBase()
        {
            return typeof(Document_Speak);
        }

        protected override string GetPrefixName()
        {
            return "DOCSPK";
        }
        #endregion=============END==========<<<



        /// <summary>获取当前成员对象</summary>
        public WX_Member GetMember()
        {
            return WX_Member.GetByID(this.MemberID);
        }
        /// <summary>获取当前文档</summary>
        public SourceDocument GetDocument()
        {
            return SourceDocument.GetByID(this.DocumentID);
        }
        /// <summary>获取当前课程</summary>
        public CourseInfo GetCourseInfo()
        {
            return CourseInfo.GetByID(this.CourseInfoID);
        }

        /// <summary>获取当前发言人的头像URL</summary>
        public string MemberHeadImgURL
        {
            get
            {
                WX_Member theMember = this.GetMember() ;
                if (theMember == null)
                    return string.Empty;
                return theMember.HeadImgURL;
            }
        }


        #region 静态成员
        public static readonly Document_Speak NONE = new Document_Speak();

        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static Document_Speak GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            Document_Speak the = EntityBase.GetMyICache().Get(autoID) as Document_Speak;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 30 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new Document_Speak();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        /// <summary>获取某个课程的弹幕记录</summary>
        public static IList<Document_Speak> GetItems(CourseInfo theCourseInfo)
        {
            List<Document_Speak> list = new List<Document_Speak>();
            if (theCourseInfo == null)
                return null;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@CourseInfoID" , theCourseInfo.AutoID,  E_DbType.VarChar , 50 ) 
            };
            Result rs = Document_Speak.NONE.EntityMaping_Excute("GetItems", ps, (readers) =>
            {
                AddToList<Document_Speak>(list, readers, (r) => { return new Document_Speak(); });
            });
            return list;
        }

        //==========更新操作===========

        /// <summary>添加弹幕记录</summary>
        public static Result Insert(WX_Member theMember, CourseInfo theCourseInfo, string msgContent, long timeSeqVale)
        {

            SourceDocument theDocument = theCourseInfo.GetDocument();
            if (theDocument == null)
                return new Result(false, "无效的课程对象,课程中没有包含文档对象");

            Result rs = Result.NONE;
            Document_Speak the = new Document_Speak(); //AutoID属性会在new 的时候就会被赋值
            the.MsgContent = msgContent;
            the.MemberID = theMember.AutoID;
            the.CourseInfoID = theCourseInfo.AutoID;
            the.DocumentID = theDocument.AutoID;
            the.Time_SeqVale = timeSeqVale;

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , the.MemberID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseInfoID" , the.CourseInfoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@DocumentID" , the.DocumentID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Time_SeqVale" , the.Time_SeqVale  ,  E_DbType.BigInt , 8 ) ,
                    new ParameterTag("@MsgContent" , the.MsgContent ,  E_DbType.VarChar , 200 ) ,
                    new ParameterTag("@CTime" , the.CTime ,  E_DbType.DateTime , 8 ) ,
            };
            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;
            }
            return rs;
        }


        /// <summary>删除(管理员权限)</summary>
        public static Result Delete(Document_Speak theSpeak)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theSpeak.AutoID ,  E_DbType.VarChar , 50 ) ,
                };
            rs = theSpeak.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear(theSpeak.AutoID);
            }
            return rs;
        }



        #endregion


    }
}
