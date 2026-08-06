using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>页语音信息类(SOUP)</summary>
    [Serializable]
    public class SoundPage : EntityBase
    {
        #region MyRegion 持久属性
        string _MemberID = string.Empty;
        /// <summary>创建者成员</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }


        string _DocumentID = string.Empty;
        /// <summary>所属的资源文件ID</summary>
        public string DocumentID
        {
            get { return _DocumentID; }
            set { _DocumentID = value; }
        }

        string _CourseInfoID = string.Empty;
        /// <summary>所属的课程ID</summary>
        public string CourseInfoID
        {
            get { return _CourseInfoID; }
            set { _CourseInfoID = value; }
        }

        int _PageIndex = -1 ;
        /// <summary>所属的页索引</summary>
        public int PageIndex
        {
            get { return _PageIndex; }
            set { _PageIndex = value; }
        }
        int _SoundIndex = 0;
        /// <summary>语音索引</summary>
        public int SoundIndex
        {
            get { return _SoundIndex; }
            set { _SoundIndex = value; }
        }
        int _SType = 0;
        /// <summary>语音类型(0:微信录音,1：网络音频)</summary>
        public int SType
        {
            get { return _SType; }
            set { _SType = value; }
        }
        string _URL = string.Empty;
        /// <summary>录音的链接(可选)</summary>
        public string URL
        {
            get { return _URL; }
            set { _URL = value; }
        }
        bool _IsShare = true;
        /// <summary>是否公开(对非创建人查看是不显示)</summary>
        public bool IsShare
        {
            get { return _IsShare; }
            set { _IsShare = value; }
        }

        string _FileID = string.Empty;
        /// <summary>语音文件ID</summary>
        public string FileID
        {
            get { return _FileID; }
            set { _FileID = value; }
        }

        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }
        #endregion


        protected SoundPage() { }


        #region============= 重写成员=========>>>
        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.DocumentID = reader.GetValue<string>(this, "DocumentID");
            this.CourseInfoID = reader.GetValue<string>(this, "CourseInfoID");
            this.PageIndex = reader.GetValue<int>(this, "PageIndex");
            this.SoundIndex = reader.GetValue<int>(this, "SoundIndex");
            this.SType = reader.GetValue<int>(this, "SType");
            this.URL = reader.GetValue<string>(this, "URL");
            this.IsShare = reader.GetValue<bool>(this, "IsShare");
            this.FileID = reader.GetValue<string>(this, "FileID");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
        }

        public override Type GetTypeBase()
        {
            return typeof(SoundPage);
        }

        protected override string GetPrefixName()
        {
            return "SOUP";
        }
        #endregion=============END==========<<<


        /// <summary>获取当前成员对象</summary>
        public WX_Member GetMember()
        {
            return WX_Member.GetByID( this.MemberID );
        }

        /// <summary>获取当前成员对象</summary>
        public SourceDocument GetDocument()
        {
            return SourceDocument.GetByID(this.DocumentID);
        }

        /// <summary>获取当前的语音文件对象</summary>
        public MFile GetFile()
        {
            return MFile.GetByID(this.FileID);
        }

        /// <summary>创建时间的字符串</summary>
        public string CTimeString
        {
            get
            {
                return this.CTime.ToString("yyyy年MM月dd日 HH:mm");
            }
        }


        //============更新操作====================

        /// <summary>修改是否公开</summary>
        public Result Update_IsShare(bool isShare)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@IsShare" , isShare ,  E_DbType.Bit , 50 ) ,
            };
            rs = this.EntityMaping_Excute("UpdateIsShare", ps);
            if (rs.IsOK)
            {
                this.IsShare = isShare;
            }
            return rs;
        }



        #region 静态成员
        public static readonly SoundPage NONE = new SoundPage();

        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static SoundPage GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            SoundPage the = EntityBase.GetMyICache().Get(autoID) as SoundPage;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 30 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new SoundPage();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        /// <summary>获取某个文档的语音对象集合(pageIndex==0表示显示所有的记录，否则只返回某个页的记录)</summary>
        public static IList<SoundPage> GetByDocument(SourceDocument theDocument, int pageIndex  )
        {
            List<SoundPage> list = new List<SoundPage>();
            ParameterTag[] ps =  { 
                new ParameterTag("@DocumentID" , theDocument.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@PageIndex" , pageIndex ,  E_DbType.Int , 4 ) 
                                };
            Result rs = SoundPage.NONE.EntityMaping_Excute("GetItemByDocumentID", ps, (readers) =>  
            {
                AddToList<SoundPage>(list, readers, (r) => { return new SoundPage(); });
            });
            return list;
        }


        /// <summary>获取某个课程文档某页的语音记录</summary>
        public static IList<SoundPage> GetItems(CourseInfo theCourseInfo, int pageIndex)
        {
            List<SoundPage> list = new List<SoundPage>();
            ParameterTag[] ps =  
            { 
                new ParameterTag("@CourseInfoID" , theCourseInfo.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@PageIndex" , pageIndex ,  E_DbType.Int , 4 ) 
            };
            Result rs = SoundPage.NONE.EntityMaping_Excute("GetItems", ps, (readers) =>
            {
                AddToList<SoundPage>(list, readers, (r) => { return new SoundPage(); });
            });
            return list;
        }

        /// <summary>获取某个课程文档某页的语音记录数</summary>
        public static int GetCount_Items(CourseInfo theCourseInfo, int pageIndex )
        {
            int count = 0;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@CourseInfoID" , theCourseInfo.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@PageIndex" , pageIndex ,  E_DbType.Int , 4 ) 
            };
            Result rs = SoundPage.NONE.EntityMaping_Excute("GetCount_Items", ps, (readers) =>
            {
                if (readers.Count > 0)
                    count = Convert.ToInt32(readers[0].GetValue(0));
            });
            return count;
        }

        //==========更新操作===========

        /// <summary>添加</summary>
        public static Result Insert(WX_Member theMember, CourseInfo theCourseInfo, int pageIndex, int soundIndex, int sType,
            MFile theFile, string url = "", bool isShare = false)
        {
            SourceDocument theDocument = theCourseInfo.GetDocument();
            if (theDocument == null)
                return new Result(false, "课程中不存在相应的文档对象");
            else if (pageIndex >= theDocument.TotalPage)
                return new Result(false, "文档的页索引值超出文档页范围");

            
            SoundPage the = new SoundPage();
            the.MemberID = theMember.AutoID;
            the.DocumentID = theDocument.AutoID;
            the.CourseInfoID = theCourseInfo.AutoID;
            the.PageIndex = pageIndex ;
            the.SoundIndex = soundIndex;
            the.SType = sType;
            the.URL = url;
            the.FileID = theFile.AutoID;
            the.IsShare = isShare;


            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , the.MemberID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@DocumentID" , the.DocumentID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseInfoID" , the.CourseInfoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@PageIndex" , the.PageIndex ,  E_DbType.Int , 4 ) ,
                    new ParameterTag("@SoundIndex" , the.SoundIndex ,  E_DbType.Int , 4 ) ,
                    new ParameterTag("@SType" , the.SType ,  E_DbType.Int , 4 ) ,
                    new ParameterTag("@URL" , the.URL ,  E_DbType.VarChar , 200 ) ,
                    new ParameterTag("@IsShare" , the.IsShare ,  E_DbType.Bit , 4 ) ,
                    new ParameterTag("@FileID" , the.FileID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CTime" , the.CTime ,  E_DbType.DateTime , 8 ) 
            };
            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                theFile.ConvertTempFile();

                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;
            }
            return rs;
        }


        /// <summary>删除</summary>
        public static Result Delete(SoundPage theSound)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theSound.AutoID ,  E_DbType.VarChar , 50 ) ,
                };
            rs = theSound.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear(theSound.AutoID);

                MFile theFile = theSound.GetFile();
                if (theFile != null) {
                    MFile.Delete(theFile);
                }
            }
            return rs;
        }
        /// <summary>删除(某页的所有语音)</summary>
        public static Result Delete(SourceDocument theDocument, int pageIndex)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@DocumentID" , theDocument.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@PageIndex" , pageIndex ,  E_DbType.Int , 4 ) 
            };
            rs = SoundPage.NONE.EntityMaping_Excute("Delete_Page", ps);
            return rs;
        }

        #endregion

    }
}
