using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>某成员阅读资源文件信息类(SDOCR)</summary>
    [Serializable]
    public class SourceDocument_Read : EntityBase
    {
        #region 持久属性
        string _DocumentID = string.Empty;
        /// <summary>资源文件ID</summary>
        public string DocumentID
        {
            get { return _DocumentID; }
            set { _DocumentID = value; }
        }

        string _CourseID = string.Empty;
        /// <summary>所属的课程ID</summary>
        public string CourseID
        {
            get { return _CourseID; }
            set { _CourseID = value; }
        }

        string _MemberID = string.Empty;
        /// <summary>所属成员ID(阅读者)</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }
        int _PageIndex = 0;
        /// <summary>当前页索引</summary>
        public int PageIndex
        {
            get { return _PageIndex; }
            set { _PageIndex = value; }
        }
        int _MiuToal = 0;
        /// <summary>当前页阅读时间长(秒)</summary>
        public int MiuToal
        {
            get { return _MiuToal; }
            set { _MiuToal = value; }
        }
        string _ReadLog = string.Empty;
        /// <summary>阅读笔记</summary>
        public string ReadLog
        {
            get { return _ReadLog; }
            set { _ReadLog = value; }
        }
        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间(开始阅读时间)</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }
        #endregion

        protected SourceDocument_Read() { }


        #region============= 重写成员=========>>>

        public override Type GetTypeBase()
        {
            return typeof(SourceDocument_Read);
        }

        protected override string GetPrefixName()
        {
            return "SDOCR";
        }

        protected override void ToEntity(Tools.AccessDB.EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.DocumentID = reader.GetValue<string>(this, "DocumentID");
            this.CourseID = reader.GetValue<string>(this, "CourseID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.MiuToal = reader.GetValue<int>(this, "MiuToal");
            this.ReadLog = reader.GetValue<string>(this, "ReadLog");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");

        }
        #endregion=============END==========<<<

        /// <summary>获取当前成员对象</summary>
        public WX_Member GetMember()
        {
            return WX_Member.GetByID(this.MemberID);
        }
        /// <summary>获取当前文档对象</summary>
        public SourceDocument GetDocument()
        {
            return SourceDocument.GetByID(this.DocumentID);
        }
        /// <summary>获取当前课程对象</summary>
        public CourseInfo GetCourseInfo()
        {
            return CourseInfo.GetByID(this.CourseID);
        }

        //============更新操作====================
        /// <summary>修改阅读时间</summary>
        public Result Update_MiuToal(int miu)
        {
            if (miu <= this.MiuToal) //如果需修改的阅读时间小于当前阅读时间,则不修改
            {
                return Result.OK;
            }

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MiuToal" , miu ,  E_DbType.Int , 8 ) ,
            };
            Result rs = this.EntityMaping_Excute("Update_MiuToal", ps);
            if (rs.IsOK)
            {
                this.MiuToal = miu;
            }
            return rs;
        }
        /// <summary>修改阅读日志（允许保存空字符串）</summary>
        public Result Update_ReadLog(string log)
        {
            if (log == null)
                log = string.Empty;

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@ReadLog" , log ,  E_DbType.VarChar  , 5000 ) ,
            };
            Result rs = this.EntityMaping_Excute("Update_ReadLog", ps);
            if (rs.IsOK)
            {
                this.ReadLog = log;
            }
            return rs;
        }



        #region 静态成员
        public static readonly SourceDocument_Read NONE = new SourceDocument_Read();
        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static SourceDocument_Read GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            SourceDocument_Read the = EntityBase.GetMyICache().Get(autoID) as SourceDocument_Read;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 50 ) 
                                     };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new SourceDocument_Read();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        /// <summary>获取某个成员对某个文档的阅读信息集合</summary>
        public static IList<SourceDocument_Read> GetByDocument(SourceDocument theDocument, WX_Member theMember)
        {
            List<SourceDocument_Read> list = new List<SourceDocument_Read>();

            ParameterTag[] ps =  { 
                new ParameterTag("@DocumentID" , theDocument.AutoID ,  E_DbType.VarChar , 50 )  ,
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = SourceDocument_Read.NONE.EntityMaping_Excute("GetByDocument", ps, (reader) =>
            {
                EntityBase.AddToList<SourceDocument_Read>(list, reader, (r) => new SourceDocument_Read());
            });
            return list;
        }
        /// <summary>获取某个成员对某个文档的是否完成阅读信息</summary>
        public static bool GetByDocumentToRead(SourceDocument theDocument, WX_Member theMember)
        {
            int readCount = 0;
            ParameterTag[] ps =  { 
                new ParameterTag("@DocumentID" , theDocument.AutoID ,  E_DbType.VarChar , 50 )  ,
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = SourceDocument_Read.NONE.EntityMaping_Excute("GetByDocumentToRead", ps, (reader) =>
            {
                if (reader.Count > 0)
                    readCount = Convert.ToInt32(reader[0].GetValue(0));
            });
            return readCount >= theDocument.TotalPage;
        }

        /// <summary>获取某个成员对某个文档的及某一页的阅读信息，如果没有则返回null</summary>
        public static SourceDocument_Read GetByPageToRead(SourceDocument theDocument, WX_Member theMember, int pageIndex)
        {
            SourceDocument_Read the = null;
            ParameterTag[] ps =  { 
                new ParameterTag("@DocumentID" , theDocument.AutoID ,  E_DbType.VarChar , 50 )  ,
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@PageIndex" , pageIndex ,  E_DbType.Int , 8 ) 
                                 };
            Result rs = SourceDocument_Read.NONE.EntityMaping_Excute("GetByPageToRead", ps, (reader) =>
            {
                if (reader.Count > 0)
                {
                    string id = reader[0].GetValue(0).ToString();
                    the = GetByID(id);
                }
            });
            return the;
        }
        /// <summary>获取某个成员对某个课程的及某一页的阅读信息，如果没有则返回null</summary>
        public static SourceDocument_Read GetByPageToRead(CourseInfo theCourseInfo, WX_Member theMember, int pageIndex)
        {
            SourceDocument_Read the = null;
            ParameterTag[] ps =  { 
                new ParameterTag("@CourseID" , theCourseInfo.AutoID ,  E_DbType.VarChar , 50 )  ,
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@PageIndex" , pageIndex ,  E_DbType.Int , 8 ) 
                                 };
            Result rs = SourceDocument_Read.NONE.EntityMaping_Excute("GetByPageToRead2", ps, (reader) =>
            {
                if (reader.Count > 0)
                {
                    string id = reader[0].GetValue(0).ToString();
                    the = GetByID(id);
                }
            });
            return the;
        }


        /// <summary>获取某文档已全部阅读人数</summary>
        public static int GetCountByDocumentToReadOK(SourceDocument theDocument)
        {
            int readCount = 0;
            ParameterTag[] ps =  { 
                new ParameterTag("@DocumentID" , theDocument.AutoID ,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = SourceDocument_Read.NONE.EntityMaping_Excute("GetCountByDocumentToReadOK", ps, (reader) =>
            {
                if (reader.Count > 0)
                    readCount = Convert.ToInt32(reader[0].GetValue(0));
            });
            return readCount;
        }

        /// <summary>获取某课程的阅读人数</summary>
        public static int GetCountByCourseInfoToReadOK(CourseInfo theCourseInfo)
        {
            int readCount = 0;
            ParameterTag[] ps =  { 
                new ParameterTag("@CourseID" , theCourseInfo.AutoID ,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = SourceDocument_Read.NONE.EntityMaping_Excute("GetCountByDocumentToReadOK2", ps, (reader) =>
            {
                if (reader.Count > 0)
                    readCount = Convert.ToInt32(reader[0].GetValue(0));
            });
            return readCount;
        }

        /// <summary>获取某成员对某文档已预习的页数</summary>
        public static int GetCountByRead(SourceDocument theDocument, WX_Member theMember)
        {
            int readCount = 0;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@DocumentID" , theDocument.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) 
            };
            Result rs = SourceDocument_Read.NONE.EntityMaping_Excute("GetCountByRead", ps, (reader) =>
            {
                if (reader.Count > 0)
                    readCount = Convert.ToInt32(reader[0].GetValue(0));
            });
            return readCount;
        }



        //==========更新操作===========

        /// <summary>添加(记录登录信息)</summary>
        public static Result Insert2(WX_Member theMember, SourceDocument theDocument, int pageIndex, int miuCount)
        {
            Result rs = Result.NONE;

            if (pageIndex >= theDocument.TotalPage)
                return new Result(false, "阅读页不在当前的文档范围内");


            SourceDocument_Read theNew = SourceDocument_Read.GetByPageToRead(theDocument, theMember, pageIndex);
            if (theNew != null)
            {
                if (miuCount > theNew.MiuToal)
                {
                    return theNew.Update_MiuToal(miuCount);
                }
                return Result.OK;
            }

            theNew = new SourceDocument_Read();
            theNew.MemberID = theMember.AutoID;
            theNew.DocumentID = theDocument.AutoID;
            theNew.PageIndex = pageIndex;
            theNew.MiuToal = miuCount;
            theNew.CTime = DateTime.Now;


            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theNew.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@DocumentID" , theNew.DocumentID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseID" , theNew.CourseID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , theNew.MemberID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@PageIndex" , theNew.PageIndex ,  E_DbType.Int , 8 ) ,
                    new ParameterTag("@MiuToal" , theNew.MiuToal ,  E_DbType.Int , 8 ) ,
                    new ParameterTag("@ReadLog" , theNew.ReadLog ,  E_DbType.VarChar , 5000 ) ,
                     new ParameterTag("@CTime" , theNew.CTime ,  E_DbType.DateTime , 8 ) 
            };
            rs = theNew.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Set(theNew.AutoID, theNew);
                rs.Data = theNew;
            }
            return rs;
        }


        /// <summary>添加(记录登录信息)</summary>
        public static Result Insert(WX_Member theMember, CourseInfo theCourse , int pageIndex, int miuCount)
        {
            Result rs = Result.NONE;

            SourceDocument theDocument = theCourse.GetDocument();
            if (theDocument == null)
                return new Result(false, "无效的文档对象");
            else if (pageIndex >= theDocument.TotalPage)
                return new Result(false, "阅读页不在当前的文档范围内");


            SourceDocument_Read theNew = SourceDocument_Read.GetByPageToRead(theCourse, theMember, pageIndex);
            if (theNew != null)
            {
                if (miuCount > theNew.MiuToal)
                {
                    return theNew.Update_MiuToal(miuCount);
                }
                return Result.OK;
            }

            theNew = new SourceDocument_Read();
            theNew.MemberID = theMember.AutoID;
            theNew.DocumentID = theDocument.AutoID;
            theNew.CourseID = theCourse.AutoID;
            theNew.PageIndex = pageIndex;
            theNew.MiuToal = miuCount;
            theNew.CTime = DateTime.Now;


            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theNew.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@DocumentID" , theNew.DocumentID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseID" , theNew.CourseID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , theNew.MemberID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@PageIndex" , theNew.PageIndex ,  E_DbType.Int , 8 ) ,
                    new ParameterTag("@MiuToal" , theNew.MiuToal ,  E_DbType.Int , 8 ) ,
                    new ParameterTag("@ReadLog" , theNew.ReadLog ,  E_DbType.VarChar , 5000 ) ,
                     new ParameterTag("@CTime" , theNew.CTime ,  E_DbType.DateTime , 8 ) 
            };
            rs = theNew.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Set(theNew.AutoID, theNew);
                rs.Data = theNew;
            }
            return rs;
        }

        /// <summary>删除</summary>
        public static Result Delete(SourceDocument_Read theDocumentRead)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theDocumentRead.AutoID ,  E_DbType.VarChar , 50 ) ,
            };
            rs = theDocumentRead.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear(theDocumentRead.AutoID);
            }
            return rs;
        }
        /// <summary>删除(某个文档的所有阅读信息)</summary>
        public static Result Delete(SourceDocument theDocument)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                new ParameterTag("@DocumentID" , theDocument.AutoID ,  E_DbType.VarChar , 50 ) ,
            };
            rs = NONE.EntityMaping_Excute("Delete2", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear();
            }
            return rs;
        }

        #endregion

    }

}
