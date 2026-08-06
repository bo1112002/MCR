using MCR.Mods.VSTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>课程(班级+科目)</summary>
    [Serializable]
    public class CourseInfo : EntityBase
    {
        #region 持久属性
        string _RoomClassID = string.Empty;
        /// <summary>班级ID</summary>
        public string RoomClassID
        {
            get { return _RoomClassID; }
            set { _RoomClassID = value; }
        }
        string _SubjectID = string.Empty;
        /// <summary>科目ID</summary>
        public string SubjectID
        {
            get { return _SubjectID; }
            set { _SubjectID = value; }
        }
        string _DocumentID = string.Empty;
        /// <summary>资源文件ID</summary>
        public string DocumentID
        {
            get { return _DocumentID; }
            set { _DocumentID = value; }
        }

        string _MemberID = string.Empty;
        /// <summary>所属的成员ID(创建人:老师)</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }

        string _Name = string.Empty;
        /// <summary>名称</summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        string _Remark = string.Empty;
        /// <summary>教学备注</summary>
        public string Remark
        {
            get { return _Remark; }
            set { _Remark = value; }
        }
        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }
        DateTime _LastTime = DateTime.Now.AddDays(3D);
        /// <summary>到期时间(如果在此时间后前,不需要对文档阅读行为进行记录)</summary>
        public DateTime LastTime
        {
            get { return _LastTime; }
            set { _LastTime = value; }
        }
        bool _IsOpenSpeak = false;
        /// <summary>是否开启讨论(弹幕)</summary>
        public bool IsOpenSpeak
        {
            get { return _IsOpenSpeak; }
            set { _IsOpenSpeak = value; }
        }
        bool _IsShareRecording = false;
        /// <summary>是否公开录音</summary>
        public bool IsShareRecording
        {
            get { return _IsShareRecording; }
            set { _IsShareRecording = value; }
        }
        bool _IsQuestionResult = false;
        /// <summary>是否公开问题答案</summary>
        public bool IsQuestionResult
        {
            get { return _IsQuestionResult; }
            set { _IsQuestionResult = value; }
        }
        bool _IsPageRemark = false;
        /// <summary>是否公开课件页备注</summary>
        public bool IsPageRemark
        {
            get { return _IsPageRemark; }
            set { _IsPageRemark = value; }
        }
        string _DetaileID = string.Empty;
        /// <summary>章节ID</summary>
        public string DetaileID
        {
            get { return _DetaileID; }
            set { _DetaileID = value; }
        }

        string _QST_CourseID = string.Empty;
        /// <summary>章节所属的QST课程ID</summary>
        public string QST_CourseID
        {
            get { return _QST_CourseID; }
            set { _QST_CourseID = value; }
        }
        #endregion


        protected CourseInfo()
        {
            Evt_EntityChange += (entity) =>
            {
                if (entity is QuestionResult)
                {
                    QuestionResult the = entity as QuestionResult;
                    if (the.CourseInfoID == this.AutoID)
                    {
                        _Count_SumMember = -1;
                        _Count_ERR_SumMember = -1;
                    }
                }

                if (entity is QuestionInfo)
                {
                    QuestionInfo the = entity as QuestionInfo;
                    if (the.CourseDetaileID == this.DetaileID)
                    {
                        _ExercisesCount = -1;
                    }
                }


                SourceDocument_Read theRead = entity as SourceDocument_Read;
                if (theRead != null && theRead.CourseID == this.AutoID)
                    _ReadOKCount = -1;


            };
        }

        /// <summary>获取邀请码的二维码的URL(加入班级)</summary>
        public string GetMyQR_URL()
        {
            return null;
        }
        #region============= 重写成员=========>>>
        public override Type GetTypeBase()
        {
            return typeof(CourseInfo);
        }

        protected override string GetPrefixName()
        {
            return "CIF";
        }

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.RoomClassID = reader.GetValue<string>(this, "RoomClassID");
            this.SubjectID = reader.GetValue<string>(this, "SubjectID");
            this.DocumentID = reader.GetValue<string>(this, "DocumentID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.Name = reader.GetValue<string>(this, "Name");
            this.Remark = reader.GetValue<string>(this, "Remark");
            this.LastTime = reader.GetValue<DateTime>(this, "LastTime");
            this.IsOpenSpeak = reader.GetValue<bool>(this, "IsOpenSpeak");
            this.IsShareRecording = reader.GetValue<bool>(this, "IsShareRecording");
            this.IsQuestionResult = reader.GetValue<bool>(this, "IsQuestionResult");
            this.IsPageRemark = reader.GetValue<bool>(this, "IsPageRemark");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
            this.DetaileID = reader.GetValue<string>(this, "DetaileID");
            this.QST_CourseID = reader.GetValue<string>(this, "QST_CourseID");
        }
        #endregion=============END==========<<<


        /// <summary>创建时间字符串格式化</summary>
        public string CTimeString
        {
            get
            {
                //return string.Format("{0} {1}", this.CTime.ToString("yyyy年MM月dd HH:mm") , PublicMethod.); 
                return DateTimeFormater.ToString(this.CTime);
            }
        }

        int _Count_SumMember = -1;
        /// <summary>获取参与答题的学生总数</summary>
        public int Count_SumMember
        {
            get
            {
                if (_Count_SumMember <= 0)
                {
                    _Count_SumMember = WX_Member.GetCount_SumMember(this);
                }
                return _Count_SumMember;
            }
        }

        int _Count_ERR_SumMember = -1;
        /// <summary>获取答错题的学生总数</summary>
        public int Count_ERR_SumMember
        {
            get
            {
                if (_Count_ERR_SumMember <= 0)
                {
                    _Count_ERR_SumMember = WX_Member.GetCount_ERR_SumMember(this);
                }
                return _Count_ERR_SumMember;
            }
        }

        /// <summary>当前创建者的名称</summary>
        public string MemberName
        {
            get
            {
                WX_Member theMember = this.GetMember();
                if (theMember != null)
                    return theMember.Name;
                return string.Empty;
            }

        }


        //===========================

        /// <summary>获取当前所属的班级对象</summary>
        public RoomClass GetRoomClass()
        {
            return RoomClass.GetByID(this.RoomClassID);
        }
        /// <summary>获取成员对象(创建人)</summary>
        public WX_Member GetMember()
        {
            return WX_Member.GetByID(this.MemberID);
        }
        /// <summary>获取当前所属的科目对象</summary>
        public Subject GetSubject()
        {
            return Subject.GetByID(this.SubjectID);
        }
        /// <summary>获取当前课堂的资源文档对象</summary>
        public SourceDocument GetDocument()
        {
            return SourceDocument.GetByID(this.DocumentID);
        }

        /// <summary>获取当前课程的文档URL</summary>
        public string URL
        {
            get
            {
                SourceDocument theDoc = this.GetDocument();
                if (theDoc == null)
                    return AppSettings.NONE404_URL;
                return theDoc.URL + "&CourseID=" + this.AutoID;
            }
        }
 //===================================================


        /// <summary>获取当前课程的作业集合</summary>
        public IList<WorkInfo> GetListByWord_QST(MemberType mtype)
        {
            if (mtype == MemberType.E_Student)
            {
                string key = "WorkInfos2_" + this.DetaileID;
                IList<WorkInfo> list = GetMyICache().Get(key) as IList<WorkInfo>;
                if (list == null || list.Count == 0)
                {
                    list = QST_Interface.GetWorksByCourseID_ToStduent(this.DetaileID);
                    GetMyICache().Set(key, list, DateTime.Now.AddMinutes(30));
                }
                return list;
            }
            else
            {
                string key = "WorkInfos_" + this.DetaileID;
                IList<WorkInfo> list = GetMyICache().Get(key) as IList<WorkInfo>;
                if (list == null || list.Count == 0)
                {
                    list = QST_Interface.GetWorksByCourseID(this.DetaileID);
                    GetMyICache().Set(key, list, DateTime.Now.AddMinutes(30));
                }
                return list;
            }
        }

        /// <summary>当前作业数</summary>
        public int WorkInfoCount
        {
            get
            {
                IList<WorkInfo> list = this.GetListByWord_QST(MemberType.E_Teacher);
               return list.Count;
            }
 
        }
             

        //===================================================

        /// <summary>
        /// 获取某成员的已阅读(预习)页数(如果该数大于等于文档的实际页数则表示当前成员已完成的预习)
        /// </summary>
        /// <remarks>
        /// 请参考：SourceDocument_Read
        /// </remarks>
        public int GetReadPageCount(WX_Member theMember)
        {
            SourceDocument theDoc = this.GetDocument();
            if (theDoc == null)
                return 0;
            int cc = SourceDocument_Read.GetCountByRead(theDoc, theMember);
            return cc;
        }

        int _ReadOKCount = -1;
        /// <summary>获取当前课程的阅读完成人数</summary>
        public int ReadOKCount
        {
            get
            {
                if (_ReadOKCount < 0)
                {
                    _ReadOKCount = SourceDocument_Read.GetCountByCourseInfoToReadOK(this);
                }
                return _ReadOKCount;
            }
        }
        /// <summary>获取所属文档的类别</summary>
        public PPT_FileType FileType
        {
            get
            {
                SourceDocument theDoc = this.GetDocument();
                if (theDoc != null)
                    return theDoc.FType;
                return PPT_FileType.Courseware;
            }
        }


        /// <summary>获取当前课程对应文档的URL</summary>
        public string DocumentURL
        {
            get
            {
                SourceDocument theDocument = this.GetDocument();
                if (theDocument == null)
                    return AppSettings.NONE_DOC_ImgURL;
                return theDocument.URL + "&CourseID=" + this.AutoID;
            }
        }

        int _ExercisesCount = -1;
        /// <summary>习题数</summary>
        public int ExercisesCount
        {
            get
            {
                if (_ExercisesCount < 0)
                {
                    _ExercisesCount = QuestionInfo.GetList_CourseDetaile_COUNT_QST(this.DetaileID);
                }
                return _ExercisesCount;
            }
        }


        //=============更新操作=================
        /// <summary>修改</summary>
        public Result Update(string name, string remark, DateTime lastTime)
        {
            Result rs = Result.NONE;
            if (string.IsNullOrEmpty(name.Trim()) == true)
                return new Result(false, "名称不能为空");

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Name" , name ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Remark" , remark ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@LastTime" , lastTime ,  E_DbType.DateTime , 50 ) ,
            };
            rs = this.EntityMaping_Excute("Update", ps);
            if (rs.IsOK)
            {
                this.Name = name;
                this.Remark = remark;
                this.LastTime = lastTime;
            }
            return rs;
        }
        ///<summary>是否开启讨论(弹幕)</summary>
        public Result Updaet_IsOpenSpeak(bool isOpenSpeak)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@IsOpenSpeak" , isOpenSpeak ,  E_DbType.Bit , 50 ) ,
            };
            rs = this.EntityMaping_Excute("Updaet_IsOpenSpeak", ps);
            if (rs.IsOK)
            {
                this.IsOpenSpeak = isOpenSpeak;
            }
            return rs;
        }
        ///<summary>是否公开录音</summary>
        public Result Updaet_IsShareRecording(bool isShareRecording)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@IsShareRecording" , isShareRecording ,  E_DbType.Bit , 50 ) ,
            };
            rs = this.EntityMaping_Excute("Updaet_IsShareRecording", ps);
            if (rs.IsOK)
            {
                this.IsShareRecording = isShareRecording;
            }
            return rs;
        }
        ///<summary>是否公开问题答案</summary>
        public Result Updaet_IsQuestionResult(bool isQuestionResult)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@isQuestionResult" , isQuestionResult ,  E_DbType.Bit , 50 ) ,
            };
            rs = this.EntityMaping_Excute("Updaet_IsQuestionResult", ps);
            if (rs.IsOK)
            {
                this.IsQuestionResult = isQuestionResult;
            }
            return rs;
        }
        ///<summary>是否公开课件页备注</summary>
        public Result Updaet_IsPageRemark(bool isPageRemark)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@IsPageRemark" , isPageRemark ,  E_DbType.Bit , 1 ) ,
            };
            rs = this.EntityMaping_Excute("Updaet_IsPageRemark", ps);
            if (rs.IsOK)
            {
                this.IsPageRemark = isPageRemark;
            }
            return rs;
        }


        ///<summary>是否公开课件页备注</summary>
        public Result Updaet_Info_QST(CourseDetail detail , SourceDocument theDocment)
        {

            CourseInfo_QST cInfo = detail.GetCourseInfo_QST();
            if (cInfo == null)
            {
                return new Result(false, "当前章节所属的课程信息无效");

            }

            string cName = cInfo.Name + detail.Name;

            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Name" , cName ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@DocumentID" , theDocment.AutoID ,  E_DbType.VarChar , 50 ) 
            };
            rs = this.EntityMaping_Excute("Updaet_Info", ps);
            if (rs.IsOK)
            {
                this.Name = cName;
                this.DocumentID = theDocment.AutoID;
            }
            return rs;
        }

        #region 静态成员
        public static readonly CourseInfo NONE = new CourseInfo();

        /// <summary>依据物理唯一标识获取对象</summary>
        public static CourseInfo GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            CourseInfo the = EntityBase.GetMyICache().Get(autoID) as CourseInfo;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 30 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new CourseInfo();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        /// <summary>依据章节，获取课程信息对象(不存在则返回null)</summary>
        public static CourseInfo GetByDetaileID_QST(string detaileID)
        {
            CourseInfo the = null;
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@DetaileID" , detaileID,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetByDetaileID", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    EntityReader r = readers[0];
                    string autoID = r.GetValue(0).ToString();
                    the = GetByID(autoID);
                }
            });
            return the;
        }


        /// <summary>获取某个班级某个科目下的所有班级的课程集合(按时间降序)</summary>
        public static IList<CourseInfo> GetByRoomClass(RoomClass theRoomClass, Subject theSubject)
        {
            List<CourseInfo> list = new List<CourseInfo>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@RoomClassID" , theRoomClass.AutoID ,  E_DbType.VarChar , 50 ),
                    new ParameterTag("@SubjectID" , theSubject.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetByRoomClass", ps, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string autoID = r.GetValue(0).ToString();
                    CourseInfo the = GetByID(autoID);
                    if (the == null)
                    {
                        the.ToEntity(r);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    if (the != null)
                    {
                        list.Add(the);
                    }
                }
            });
            return list;
        }
        /// <summary>获取某个班级某个科目下的所有班级的课程集合(按时间降序)</summary>
        public static IList<CourseInfo> GetByRoomClass(Rel_Subject_RoomClass theRelSR)
        {
            RoomClass theRoomClass = theRelSR.GetRoomClass();
            Subject theSubject = theRelSR.GetSubject();
            if (theRoomClass != null || theSubject != null)
            {
                return CourseInfo.GetByRoomClass(theRoomClass, theSubject);
            }
            return new List<CourseInfo>();
        }



        /// <summary>获取某班级及某科目的课程数</summary>
        public static int GetCountByRoomClassAndSubject(Subject theSubject, RoomClass theRoomClass)
        {
            int rsCount = 0;
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@RoomClassID" , theRoomClass.AutoID ,  E_DbType.VarChar , 50 ),
                    new ParameterTag("@SubjectID" , theSubject.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetCountByRoomClassAndSubject", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    rsCount = Convert.ToInt32(readers[0].GetValue(0));
                }
            });
            return rsCount;
        }



        /// <summary>获取某成员的所属的课程集合</summary>
        public static IList<CourseInfo> GetByRoomClassMember(WX_Member theMember)
        {
            List<CourseInfo> list = new List<CourseInfo>();
            ParameterTag[] ps =  
            { 
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 )
            };
            Result rs = NONE.EntityMaping_Excute("GetByRoomClassMember", ps, (readers) =>
            {
                EntityBase.AddToList<CourseInfo>(list, readers, (r) => new CourseInfo());
            });
            return list;
        }


        /// <summary>获取某科目的每一个课程信息对象(不存在则返回null)</summary>
        public static CourseInfo GetByRoomClassToFirst(RoomClass theRoomClass, Subject theSubject)
        {
            CourseInfo the = null;
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@RoomClassID" , theRoomClass.AutoID ,  E_DbType.VarChar , 50 ),
                    new ParameterTag("@SubjectID" , theSubject.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetByRoomClassToFirst", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    EntityReader r = readers[0];
                    string autoID = r.GetValue(0).ToString();
                    the = GetByID(autoID);
                    if (the == null)
                    {
                        the.ToEntity(r);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                }
            });
            return the;
        }


        /// <summary>获取某成员某科目未预习的课程集合</summary>
        public static IList<CourseInfo> GetListByNotRead(RoomClass theRoomClass, Subject theSubject, WX_Member theMember)
        {
            List<CourseInfo> list = new List<CourseInfo>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@RoomClassID" , theRoomClass.AutoID ,  E_DbType.VarChar , 50 ),
                    new ParameterTag("@SubjectID" , theSubject.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetListByNotRead", ps, (readers) =>
            {
                EntityBase.AddToList<CourseInfo>(list, readers, (r) => new CourseInfo_Ext_Member(theMember), (theAdd, reader) =>
                {
                    if (theAdd is CourseInfo_Ext_Member == false)
                    {
                        CourseInfo_Ext_Member the2 = new CourseInfo_Ext_Member(theMember);
                        the2.ToEntity(reader);
                        return the2;
                    }
                    return null;
                });
            });
            return list;
        }
        /// <summary>获取某成员某科目未预习的课程总数</summary>
        public static int GetListByNotRead_Count(RoomClass theRoomClass, Subject theSubject, WX_Member theMember)
        {
            int count = 0;
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@RoomClassID" , theRoomClass.AutoID ,  E_DbType.VarChar , 50 ),
                    new ParameterTag("@SubjectID" , theSubject.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetListByNotRead_Count", ps, (readers) =>
            {
                if (readers.Count > 0)
                    count = Convert.ToInt32(readers[0].GetValue(0));
            });
            return count;
        }


        /// <summary>获取某成员所有未预习(包括预习了一部分页)的课程集合</summary>
        public static IList<CourseInfo> GetListByMember_NotRead(WX_Member theMember)
        {
            List<CourseInfo> list = new List<CourseInfo>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetListByMember_NotRead", ps, (readers) =>
            {
                EntityBase.AddToList<CourseInfo>(list, readers, (r) => new CourseInfo_Ext_Member(theMember), (theAdd, reader) =>
                {
                    if (theAdd is CourseInfo_Ext_Member == false)
                    {
                        CourseInfo_Ext_Member the2 = new CourseInfo_Ext_Member(theMember);
                        the2.ToEntity(reader);
                        return the2;
                    }
                    return null;
                });
            });
            return list;
        }

        /// <summary>获取某成员所有未预习(包括预习了一部分页)的课程集合</summary>
        public static IList<CourseInfo> GetListByMember_NotRead_QST(WX_Member theMember ,  string  qstCourseIDs = null )
        {
            SetReplaceSql setReplace = null;
            if (string.IsNullOrEmpty(qstCourseIDs) == false)
            {
                setReplace = new SetReplaceSql("[WhereIN]", qstCourseIDs);
            }


            List<CourseInfo> list = new List<CourseInfo>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetListByMember_NotRead_QST", ps, (readers) =>
            {
                EntityBase.AddToList<CourseInfo>(list, readers, (r) => new CourseInfo_Ext_Member(theMember), (theAdd, reader) =>
                {
                    if (theAdd is CourseInfo_Ext_Member == false)
                    {
                        CourseInfo_Ext_Member the2 = new CourseInfo_Ext_Member(theMember);
                        the2.ToEntity(reader);
                        return the2;
                    }
                    return null ;
                });
            }, setReplace);
            return list;
        }

        /// <summary>获取某成员某科目未预习的课程总数</summary>
        public static int GetCountByMember_NotRead(WX_Member theMember)
        {
            int count = 0;
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetCountByMember_NotRead", ps, (readers) =>
            {
                if (readers.Count > 0)
                    count = Convert.ToInt32(readers[0].GetValue(0));
            });
            return count;
        }


        /// <summary>老师：学生的错题课程</summary>
        public static IList<CourseInfo> GetListByQuestionResult_T_More(WX_Member theMember, string lastID)
        {
            if (string.IsNullOrEmpty(lastID))
            {
                lastID = "ZZZZZZZZ";
            }

            List<CourseInfo> list = new List<CourseInfo>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@LastID" , lastID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetListByQuestionResult_T", ps, (readers) =>
            {
                EntityBase.AddToList<CourseInfo>(list, readers, (r) => new CourseInfo());
            });
            return list;
        }
        /// <summary>学生：学生的错题课程</summary>
        public static IList<CourseInfo> GetListByQuestionResult_S(WX_Member theMember)
        {
            List<CourseInfo> list = new List<CourseInfo>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetListByQuestionResult_S", ps, (readers) =>
            {
                EntityBase.AddToList<CourseInfo>(list, readers, (r) => new CourseInfo());
            });
            return list;
        }



        /// <summary>老师：获取老师的课程(DetaileID)</summary>
        public static IList<CourseInfo> GetListInDetails_QST( string inDetails )
        {
            List<CourseInfo> list = new List<CourseInfo>();
            ParameterTag[] ps =   {  };

            SetReplaceSql setInSql  = new SetReplaceSql( "{InStr}" , inDetails ) ;

            Result rs = NONE.EntityMaping_Excute("GetListInDetails_QST", null, (readers) =>
            {
                EntityBase.AddToList<CourseInfo>(list, readers, (r) => new CourseInfo());
            }, setInSql );
            return list;
        }


        /// <summary>获取老师的课程</summary>
        public static IList<CourseInfo> GetListByMember_QST(WX_Member theMember , string lastID = "")
        {
            List<CourseInfo> list = new List<CourseInfo>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@LastID" , lastID ,  E_DbType.VarChar , 50 ) ,
                };

            Result rs = NONE.EntityMaping_Excute("GetListByMember_QST", ps, (readers) =>
            {
                EntityBase.AddToList<CourseInfo>(list, readers, (r) => new CourseInfo());
            });
            return list;
        }
        /// <summary>依据QST课程ID获取的课程集合</summary>
        public static IList<CourseInfo> GetListByQST_CourseID(string courseID_QST)
        {
            List<CourseInfo> list = new List<CourseInfo>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@QST_CourseID" , courseID_QST,  E_DbType.VarChar , 50 ) 
                };

            Result rs = NONE.EntityMaping_Excute("GetListByQST_CourseID", ps, (readers) =>
            {
                EntityBase.AddToList<CourseInfo>(list, readers, (r) => new CourseInfo());
            });
            return list;
        }



        


        //=========更新==============
        /// <summary>添加</summary>
        public static Result Insert(WX_Member theMember, RoomClass theRoom, Subject theSubject, SourceDocument theDoc,
            string name, string remark, DateTime lastTime)
        {

            if (theDoc.IsDisabe == true)
                return new Result(false, "操作终止：该文档已被禁用");
            else if (string.IsNullOrEmpty(name))
                return new Result(false, "操作终止：请填写课程发布名");
            else if (lastTime < DateTime.Now)
                return new Result(false, "操作终止：截止时间应该是一个将来的某个时间值");
            else if (theMember.MType < MemberType.E_Teacher)
                return new Result(false, "操作终止：创建者没有相应的权限");


            Result rs = Result.NONE;
            CourseInfo the = new CourseInfo();
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@RoomClassID" , theRoom.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@SubjectID" , theSubject.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@DocumentID" , theDoc.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Name" , name ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Remark" , remark ,  E_DbType.VarChar , 1000 ) ,
                    new ParameterTag("@IsOpenSpeak" , false ,  E_DbType.Bit , 1 ) ,
                    new ParameterTag("@IsShareRecording" , false ,  E_DbType.Bit , 1 ) ,
                    new ParameterTag("@IsQuestionResult" , false ,  E_DbType.Bit , 1 ) ,
                    new ParameterTag("@IsPageRemark" , false ,  E_DbType.Bit , 1 ) ,
                    new ParameterTag("@LastTime" , lastTime ,  E_DbType.DateTime , 8 ) ,
                    new ParameterTag("@CTime" , DateTime.Now ,  E_DbType.DateTime , 8 ) ,
                    new ParameterTag("@DetaileID" , string.Empty ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@QST_CourseID" , string.Empty ,  E_DbType.VarChar , 50 )
            };
            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                the.MemberID = theMember.AutoID;
                the.RoomClassID = theRoom.AutoID;
                the.SubjectID = theSubject.AutoID;
                the.DocumentID = theDoc.AutoID;
                the.RoomClassID = theRoom.AutoID;
                the.IsOpenSpeak = false;
                the.IsShareRecording = false;
                the.IsQuestionResult = false;
                the.IsPageRemark = false;
                the.CTime = DateTime.Now;
                the.Name = name;
                the.Remark = remark;
                the.LastTime = lastTime;
                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;

                //自动设置当前文档为共享
                if (theDoc.IsShare == false)
                    theDoc.Update_IsShare(true);

            }
            return rs;
        }


        /// <summary>添加2</summary>
        public static Result Insert_QST(WX_Member theMember, CourseDetail detail, SourceDocument theDoc, DateTime? lastTime = null)
        {
            if (lastTime == null)
                lastTime = DateTime.Now.AddDays(3);

            if (theDoc.IsDisabe == true)
                return new Result(false, "操作终止：该文档已被禁用");
            else if (lastTime < DateTime.Now)
                return new Result(false, "操作终止：截止时间应该是一个将来的某个时间值");
            else if (theMember.MType < MemberType.E_Teacher)
                return new Result(false, "操作终止：创建者没有相应的权限");

            CourseInfo_QST cInfo = detail.GetCourseInfo_QST();
            if (cInfo == null)
            {
                return new Result(false, "当前章节所属的课程信息无效");

            }

            string cName =  string.Format("[{0}] {1}" , detail.GetParentName() , detail.Name) ;

            Result rs = Result.NONE;
            CourseInfo the = new CourseInfo();
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@RoomClassID" , string.Empty ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@SubjectID" , string.Empty ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@DocumentID" , theDoc.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Name" , cName ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Remark" , string.Empty  ,  E_DbType.VarChar , 1000 ) ,
                    new ParameterTag("@IsOpenSpeak" , false ,  E_DbType.Bit , 1 ) ,
                    new ParameterTag("@IsShareRecording" , false ,  E_DbType.Bit , 1 ) ,
                    new ParameterTag("@IsQuestionResult" , false ,  E_DbType.Bit , 1 ) ,
                    new ParameterTag("@IsPageRemark" , false ,  E_DbType.Bit , 1 ) ,
                    new ParameterTag("@LastTime" , lastTime.Value ,  E_DbType.DateTime , 8 ) ,
                    new ParameterTag("@CTime" , DateTime.Now ,  E_DbType.DateTime , 8 ) ,
                    new ParameterTag("@DetaileID" , detail.ID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@QST_CourseID" , cInfo.AutoID ,  E_DbType.VarChar , 50 )
            };
            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                the.MemberID = theMember.AutoID;
                the.DocumentID = theDoc.AutoID;
                the.IsOpenSpeak = false;
                the.IsShareRecording = false;
                the.IsQuestionResult = false;
                the.IsPageRemark = false;
                the.CTime = DateTime.Now;
                the.Name = cName;
                the.Remark = string.Empty;
                the.LastTime = lastTime.Value;
                the.DetaileID = detail.ID;
                the.QST_CourseID = cInfo.AutoID;

                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;

                //自动设置当前文档为共享
                if (theDoc.IsShare == false)
                    theDoc.Update_IsShare(true);

            }
            return rs;
        }


        /// <summary>删除</summary>
        public static Result Delete(CourseInfo the)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                };
            rs = NONE.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear(the.AutoID);
            }
            return rs;
        }

        #endregion



    }

    [Serializable]
    internal class CourseInfo_Ext_Member : CourseInfo
    {
        WX_Member _TheMember = null;
        public CourseInfo_Ext_Member(WX_Member theMember)
            : base()
        {
            _TheMember = theMember;
        }

        /// <summary>获取当前文档页数与已预习页数比的字符串格式</summary>
        public string DocumentReadRatioString
        {
            get
            {
                SourceDocument theDoc = GetDocument();
                if (theDoc == null)
                    return "0/0";

                int readCount = SourceDocument_Read.GetCountByRead(theDoc, _TheMember);

                return string.Format("{0}/{1}", readCount, theDoc.TotalPage);
            }
        }
    }



    //==================================================================


}
