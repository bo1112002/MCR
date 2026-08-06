using MCR.Mods.VSTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;
using Tools.Http;

namespace MCR.Mods
{
    /// <summary>资源文档(如：课件、 投票...)(SDOC)</summary>
    [Serializable]
    public class SourceDocument : EntityBase
    {
        #region 持久属性

        string _Name = string.Empty;
        /// <summary>名称</summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }


        string _MemberID = string.Empty;
        /// <summary>所属的成员ID</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }

        int _FileType = 0;
        /// <summary>文件类别(0:PPT,1:Word)</summary>
        public int FileType
        {
            get { return _FileType; }
            set { _FileType = value; }
        }

        int _TotalPage = 0;
        /// <summary>总页数</summary>
        public int TotalPage
        {
            get { return _TotalPage; }
            set { _TotalPage = value; }
        }


        PPT_FileType _FType = PPT_FileType.Courseware;
        /// <summary>当前文档的文档类型(课件,试题,通知,投票,讨论)</summary>
        public PPT_FileType FType
        {
            get { return _FType; }
            set { _FType = value; }
        }
        string _FileID = string.Empty;
        /// <summary>文件ID</summary>
        public string FileID
        {
            get { return _FileID; }
            set { _FileID = value; }
        }

        string _DocFileID = string.Empty;
        /// <summary>文档上的物理唯一标识</summary>
        public string DocFileID
        {
            get { return _DocFileID; }
            set { _DocFileID = value; }
        }

        bool _IsShare = false;
        /// <summary>当前资源是否共享(如果true则可以在资源库中显示，fals:只能在老师的私用资源中显示)</summary>
        public bool IsShare
        {
            get { return _IsShare; }
            set { _IsShare = value; }
        }
        bool _IsDisabe = false;
        /// <summary>是否禁用(只能是学校管理员能设置该值,如果为True则除管理员外，任何人不能对这个文档进行查看,包括下载)</summary>
        public bool IsDisabe
        {
            get { return _IsDisabe; }
            set { _IsDisabe = value; }
        }
        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }
        #endregion


        protected SourceDocument()
        {

            MemberReadInfo.Evt_ReadChange += (readInfo) =>
            {
                if (readInfo.TagID == this.AutoID)
                    _ReadCount = -1;
            };

            SourceDocument_Read.Evt_EntityChange += (info) =>
            {
                SourceDocument_Read theRead = info as SourceDocument_Read;
                if (theRead != null && theRead.DocumentID == this.AutoID)
                    _ReadOKCount = -1;
            };
        }

        /// <summary>创建者的名称</summary>


        #region============= 重写成员=========>>>
        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.Name = reader.GetValue<string>(this, "Name");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.TotalPage = reader.GetValue<int>(this, "TotalPage");
            this.FileType = reader.GetValue<int>(this, "FileType");
            this.FType = (PPT_FileType)reader.GetValue<int>(this, "FType");
            this.FileID = reader.GetValue<string>(this, "FileID");
            this.DocFileID = reader.GetValue<string>(this, "DocFileID");
            this.IsShare = reader.GetValue<bool>(this, "IsShare");
            this.IsDisabe = reader.GetValue<bool>(this, "IsDisabe");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
        }

        public override Type GetTypeBase()
        {
            return typeof(SourceDocument);
        }

        protected override string GetPrefixName()
        {
            return "SDOC";
        }
        #endregion=============END==========<<<

        /// <summary>获取当前文件对象</summary>
        public MFile GetMFile()
        {
            return MFile.GetByID(this.FileID);
        }

        /// <summary>通过FileType获取相应的文件后缀名</summary>
        public string GetExtNameFromFileType()
        {

            if (this.FileType == 0)
                return "pptx";
            else if (this.FileType == 1)
                return "docx";
            else return
                string.Empty;
        }
        /// <summary>通过FileType获取相应的网络文件类别名</summary>
        public string GetContextTypeFromFileType()
        {
            if (this.FileType == 0)
                return "application/x-ppt";
            else if (this.FileType == 1)
                return "application/msword";
            else
                return "application/octet-stream";
        }


        /// <summary>获取当前文档的URL（浏览）</summary>
        public string URL
        {
            get
            {
                return AppSettings.GetDocument_URL(this.FileID, this.AutoID);
            }
        }

        /// <summary>获取第一页的图</summary>
        public string FirstImgURL
        {
            get
            {
                MFile theFile = this.GetMFile();
                if (theFile != null)
                {
                    string url = AppSettings.GetDocument_URL(theFile.AutoID, this.AutoID, 1);
                    return url;
                }
                else
                {
                    return AppSettings.NONE_DOC_ImgURL;
                }
            }
        }

        /// <summary>获取当前成员对象</summary>
        public WX_Member GetMember()
        {
            return WX_Member.GetByID(this.MemberID);
        }

        /// <summary>当前创建者的名字</summary>
        public string MemberName
        {
            get
            {
                WX_Member the = this.GetMember();
                if (the == null)
                    return string.Empty;
                return the.Name;
            }
        }

        /// <summary>创建者的名称</summary>
        public string CTimeString
        {
            get
            {
                return this.CTime.ToString("yyyy-MM-dd");
            }
        }


        int _ReadCount = -1;
        /// <summary>浏览数</summary>
        public int ReadCount
        {
            get
            {
                if (_ReadCount < 0)
                {
                    WX_Member theMember = this.GetMember();
                    if (theMember == null)
                        return 0;
                    _ReadCount = MemberReadInfo.GetCountByMemberAndTagID(theMember, this);
                }
                return _ReadCount;
            }
        }

        int _ReadOKCount = -1;
        /// <summary>阅读完成数</summary>
        public int ReadOKCount
        {
            get
            {
                if (_ReadOKCount < 0)
                {
                    _ReadOKCount = SourceDocument_Read.GetCountByDocumentToReadOK(this);
                }
                return _ReadOKCount;
            }
        }




        /*========================================================================*/

        PPT_FileClass _MyPPT = null;
        /// <summary>获取文件的结构信息对象</summary>
        /// <remarks>
        /// 把这个部分数据以二进制序列，并保存在文件中(PagesBuffer)
        /// </remarks>
        public PPT_FileClass GetPageInfo()
        {
            if (_MyPPT == null)
            {
                MFile theMFile = this.GetMFile();
                if (theMFile == null)
                    return _MyPPT;
                FileStream fs = File.OpenRead(theMFile.GetFullPath());
                fs.Position = 0;
                _MyPPT = PPT_FileClass.Deserialzable(fs);
                fs.Close();
            }
            return _MyPPT;
        }

        /// <summary>获取所有页的备注信息集合</summary>
        public IList<RemarkInfo> PagesRemark()
        {
            List<RemarkInfo> list = new List<RemarkInfo>();
            PPT_FileClass theFileClass = GetPageInfo();
            if (theFileClass != null)
            {
                foreach (PPT_PageClass p in theFileClass.Pages)
                {
                    if (string.IsNullOrEmpty(p.Remark) == false)
                    {
                        list.Add(new RemarkInfo(p.Remark, p.Index));
                    }
                }
            }
            return list;
        }


        /// <summary>某个成员是否完成阅读</summary>
        public bool IsOK_Read(WX_Member theMember)
        {
            return false;
        }

        /// <summary>获取下载当前资源文件的URL</summary>
        public string DownFileRUL
        {
            get
            {
                return AppSettings.WebURL + "MFile.aspx?DownFile_Document=" + this.AutoID;
            }
        }

        /// <summary>发送邮件(当前文件)给指定成员</summary>
        public Result Email_Doument(WX_Member theMember, CourseInfo theCourseInfo = null)
        {
            if (Regexs.IsEmail(theMember.Email) == false)
                return new Result(false, "用户的邮箱地址无效");

            string tName = this.Name;
            if (theCourseInfo != null)
            {
                tName = theCourseInfo.Name;
            }
            string body = "<br/><br/><b>{0},您好,<a href='{2}'>请点击这里下载课程资源:{1}</a><br/>";
            body = string.Format(body, theMember.NickName, tName, this.DownFileRUL);
            IMailServer mser = AppSettings.Base as IMailServer;
            Result rs = MyMail.Send(mser.UserName, theMember.NickName, theMember.Email, "获取课程资源", body, null, mser);
            if (rs.IsOK == false)
            {
                return new Result(false, "电子邮箱地址无效,请在个人信息中设置正确的邮箱地址");
            }
            return rs;
        }



        //======= 更新操作=================
        /// <summary>是否禁用当前文档</summary>
        public Result Update_IsDisabe(bool isDisabe)
        {
            Result rs = Result.NONE;

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@IsDisabe" , isDisabe ,  E_DbType.Bit ,1 ) ,
            };
            rs = this.EntityMaping_Excute("Update_IsDisabe", ps);
            if (rs.IsOK)
            {
                this.IsDisabe = isDisabe;
            }
            return rs;
        }
        /// <summary>是否公开当前文档</summary>
        public Result Update_IsShare(bool isShare)
        {
            Result rs = Result.NONE;

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@IsShare" , isShare ,  E_DbType.Bit , 1 ) ,
            };
            rs = this.EntityMaping_Excute("Update_IsShare", ps);
            if (rs.IsOK)
            {
                this.IsShare = isShare;
            }
            return rs;
        }



        /// <summary>修改前文档</summary>
        public Result Update_File(MFile theFile)
        {
            Result rs = Result.NONE;

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@FileID" , theFile.AutoID ,  E_DbType.VarChar , 50 ) ,
            };
            rs = this.EntityMaping_Excute("Update_File", ps);
            if (rs.IsOK)
            {
                this.FileID = theFile.AutoID;
            }
            return rs;
        }



        /// <summary>当前某个成员打开当前文档的处理</summary>
        public Result OpenOfHandle(WX_Member theMember)
        {
            return MemberReadInfo.Insert(theMember, this);
        }


        #region 静态成员
        public static readonly SourceDocument NONE = new SourceDocument();

        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static SourceDocument GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            SourceDocument the = EntityBase.GetMyICache().Get(autoID) as SourceDocument;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 30 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new SourceDocument();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        /// <summary>通过文件ID获取相应的对象</summary>
        public static SourceDocument GetByFileID(string fileID)
        {
            if (string.IsNullOrEmpty(fileID))
            {
                return null;
            }

            SourceDocument the = null;
            ParameterTag[] ps =  { 
                    new ParameterTag("@FileID" , fileID ,  E_DbType.VarChar , 50 ) 
                                 };
            Result rs = NONE.EntityMaping_Excute("GetByFileID", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string id = readers[0].GetValue(0).ToString();
                    the = SourceDocument.GetByID(id);
                }
            });
            return the;
        }



        /// <summary>获取某个成员的资料文档对象 , isShare!=null , 则返回相应的文档对象集合</summary>
        public static IList<SourceDocument> GetListByMember(WX_Member theMember, bool? isShare = null)
        {
            string strIsShare = isShare == null ? "%" : isShare.ToString();

            List<SourceDocument> list = new List<SourceDocument>();
            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theMember.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@IsShare" , strIsShare ,  E_DbType.VarChar , 10 ) ,
                                };
            //SourceDocument the = null;
            Result rs = NONE.EntityMaping_Excute("GetSourceByMID", ps, (readers) =>
            {
                EntityBase.AddToList<SourceDocument>(list, readers, (r) => new SourceDocument());
            });
            return list;
        }

        /// <summary>获取某个成员的资料文档对象集合(分页加载)</summary>
        public static IList<SourceDocument> GetListByMember_More(WX_Member theMember, string lastID, string docLikeName = null)
        {
            if (string.IsNullOrEmpty(docLikeName) == true)
                docLikeName = "%";
            docLikeName.Trim();
            docLikeName = docLikeName.Replace(" ", "%");
            docLikeName = string.Format("%{0}%", docLikeName);

            List<SourceDocument> list = new List<SourceDocument>();
            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theMember.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@LastID" , lastID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@Name" , docLikeName ,  E_DbType.VarChar , 50 ) 
                                 };
            Result rs = NONE.EntityMaping_Excute("GetListByMember_More", ps, (readers) =>
            {
                EntityBase.AddToList<SourceDocument>(list, readers, (r) => new SourceDocument());
            });
            return list;
        }


        /// <summary>获取所有已共享的资料文档对象集合(分页加载)</summary>
        public static IList<SourceDocument> GetListAllShare_More(WX_Member theMember, string lastID, string docLikeName = null, PPT_FileType? fType = null, bool isPublic = true)
        {
            School theSchool = theMember.GetSchool();

            if (string.IsNullOrEmpty(docLikeName) == true)
                docLikeName = "%";
            else
            {
                docLikeName.Trim();
                docLikeName = docLikeName.Replace(" ", "%");
                docLikeName = string.Format("%{0}%", docLikeName);
            }

            string strFType = (fType == null ? "%" : ((int)fType).ToString());



            List<SourceDocument> list = new List<SourceDocument>();
            ParameterTag[] ps =  { 
                new ParameterTag("@LastID" , lastID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@Name" , docLikeName ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@SchoolID" , theSchool.AutoID  ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@FType" ,strFType ,  E_DbType.VarChar , 50 )
                                 };

            string sqlKey = isPublic ? "GetListAllShare_More" : "GetListAllShare_More2";
            Result rs = NONE.EntityMaping_Excute(sqlKey , ps, (readers) =>
            {
                EntityBase.AddToList<SourceDocument>(list, readers, (r) => new SourceDocument(),
                    (theDoc, theReader) =>
                    {
                        theDoc.ExtPropertys["IsMe"] = (theDoc.MemberID == theMember.AutoID);
                        return theDoc;
                    });
            });
            return list;
        }




        /// <summary>获取某个学校的资源文档 , fType!=null 则返回相应的分类文档对象集合</summary>
        public static IList<SourceDocument> GetListBySchool(School theSchool, PPT_FileType? fType = null)
        {

            string strFType = fType == null ? "%" : ((int)fType).ToString();

            List<SourceDocument> list = new List<SourceDocument>();
            ParameterTag[] ps =  { 
                new ParameterTag("@SchoolID" , theSchool.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@FType" ,strFType ,  E_DbType.VarChar , 50 ) ,
                                };
            //SourceDocument the = null;
            Result rs = NONE.EntityMaping_Excute("GetSourceBySID", ps, (readers) =>
            {
                EntityBase.AddToList<SourceDocument>(list, readers, (r) => new SourceDocument());
            });
            return list;
        }






        /// <summary>获取某学校某文档类别的数量</summary>
        public static int GetSourceCountBySchoolAndFType(School theSchool, PPT_FileType fType)
        {
            int rsCount = 0;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@SchoolID" , theSchool.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@FType" ,(int)fType ,  E_DbType.Int , 4) 
            };
            Result rs = SourceDocument.NONE.EntityMaping_Excute("GetSourceCountBySchoolAndFType", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    rsCount = Convert.ToInt32(readers[0].GetValue(0));
                }
            });
            return rsCount;
        }
        /// <summary>获取某学校某文档类别的数量</summary>
        public static IList<SourceDocument> GetSourceListBySchoolAndFType(School theSchool, PPT_FileType fType)
        {
            List<SourceDocument> list = new List<SourceDocument>();
            ParameterTag[] ps =  
            { 
                new ParameterTag("@SchoolID" , theSchool.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@FType" ,(int)fType ,  E_DbType.Int , 4) 
            };
            //SourceDocument the = null;
            Result rs = NONE.EntityMaping_Excute("GetSourceListBySchoolAndFType", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    EntityBase.AddToList<SourceDocument>(list, readers, (r) => new SourceDocument());
                }
            });
            return list;
        }



        /// <summary>获取某科目及班级下的文档类别的数量</summary>
        public static int GetSourceCountByFType(Subject theSubject, RoomClass theRoomClass, PPT_FileType fType)
        {
            int rsCount = 0;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@SubjectID" , theSubject.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@RoomClassID" , theRoomClass.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@FType" ,(int)fType ,  E_DbType.Int , 4) 
            };
            Result rs = SourceDocument.NONE.EntityMaping_Excute("GetSourceCountByFType", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    rsCount = Convert.ToInt32(readers[0].GetValue(0));
                }
            });
            return rsCount;
        }
        /// <summary>获取某科目及班级下的文档类别的集合</summary>
        public static IList<SourceDocument> GetSourceListByFType(Subject theSubject, RoomClass theRoomClass, PPT_FileType fType)
        {
            List<SourceDocument> list = new List<SourceDocument>();
            ParameterTag[] ps =  
            { 
                new ParameterTag("@SubjectID" , theSubject.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@RoomClassID" , theRoomClass.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@FType" ,(int)fType ,  E_DbType.Int , 4) 
            };
            Result rs = SourceDocument.NONE.EntityMaping_Excute("GetSourceListByFType2", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    EntityBase.AddToList<SourceDocument>(list, readers, (r) => new SourceDocument());
                }
            });
            return list;
        }





        /// <summary>获取某个类别的资源文档</summary>
        public static IList<SourceDocument> GetSourceListByFType(PPT_FileType fType)
        {
            List<SourceDocument> list = new List<SourceDocument>();
            ParameterTag[] ps =  
            { 
                new ParameterTag("@FType" ,(int)fType ,  E_DbType.Int , 4) 
            };
            Result rs = NONE.EntityMaping_Excute("GetSourceListByFType", ps, (readers) =>
            {
                EntityBase.AddToList<SourceDocument>(list, readers, (r) => new SourceDocument());
            });
            return list;
        }


        //==========更新操作===========

        /// <summary>添加(记录登录信息)</summary>
        public static Result Insert(WX_Member theMember, PPT_FileClass file, MFile theFile)
        {
            Result rs = Result.NONE;
            SourceDocument the = new SourceDocument();
            the.Name = file.Name;
            the.MemberID = theMember.AutoID;
            the.FType = file.FType;
            the.TotalPage = file.Pages.Count;
            the.FileType = 0;
            the.FileID = theFile.AutoID;
            the.DocFileID = file.FileID;
            the.IsShare = false;
            the.IsDisabe = false;

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Name" , the.Name ,  E_DbType.VarChar , 100 ) ,
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@FType" , (int)file.FType  ,  E_DbType.Int , 8 ) ,
                    new ParameterTag("@TotalPage" , the.TotalPage ,  E_DbType.Int , 8 ) ,
                    new ParameterTag("@FileType" , the.FileType ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@FileID" , the.FileID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@DocFileID" , the.DocFileID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@IsShare" , the.IsShare ,  E_DbType.Bit , 1) ,
                    new ParameterTag("@IsDisabe" , the.IsDisabe ,  E_DbType.Bit, 1) ,
                    new ParameterTag("@CTime" , the.CTime ,  E_DbType.DateTime, 8 ) ,
            };

            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                the._MyPPT = file;
                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;

                foreach (PPT_PageClass page in the._MyPPT.Pages)
                {
                    if (page.MyQuestion != null)
                    {
                        Result rs2 = QuestionInfo.Insert(page.MyQuestion, theMember, the, page.Index);
                    }
                }

            }
            return rs;
        }


        /// <summary>删除</summary>
        public static Result Delete(SourceDocument theDocument)
        {
            if (theDocument.IsShare)
                return new Result(false, "操作终止：当前资源文档已分享，公共资源不可删除");

            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theDocument.AutoID ,  E_DbType.VarChar , 50 ) ,
                };
            rs = theDocument.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear(theDocument.AutoID);
            }
            return rs;
        }

        #endregion
    }




    /// <summary>描述文档页备注结构类</summary>
    public class RemarkInfo
    {
        /// <summary>备注</summary>
        public string Remark { get; private set; }
        /// <summary>所属的页索引</summary>
        public int IndexPage { get; private set; }

        public RemarkInfo(string remark, int indexPage)
        {
            this.Remark = remark;
            this.IndexPage = indexPage;
        }
    }










}
