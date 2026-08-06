using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>科目与班级的关系(RSJRM)</summary>
    public class Rel_Subject_RoomClass : EntityBase
    {
        #region 持久属性
        string _SubjectID = string.Empty;
        /// <summary>科目ID</summary>
        public string SubjectID
        {
            get { return _SubjectID; }
            set { _SubjectID = value; }
        }
        string _RoomClassID = string.Empty;
        /// <summary>班级ID</summary>
        public string RoomClassID
        {
            get { return _RoomClassID; }
            set { _RoomClassID = value; }
        }
        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }
        #endregion

        protected Rel_Subject_RoomClass()
        {
            EntityBase.Evt_EntityChange += (entity) =>
            {
                if (entity is SourceDocument)
                {
                    _QuestionCount = -1;
                    _CoursewareCount = -1;
                }
            };
        }


        #region============= 重写成员=========>>>

        public override Type GetTypeBase()
        {
            return typeof(Rel_Subject_RoomClass);
        }

        protected override string GetPrefixName()
        {
            return "RSJRM";
        }


        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.SubjectID = reader.GetValue<string>(this, "SubjectID");
            this.RoomClassID = reader.GetValue<string>(this, "RoomClassID");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
        }

        #endregion=============END==========<<<


        /// <summary>获取当前科目</summary>
        public Subject GetSubject()
        {
            return Subject.GetByID(this.SubjectID);
        }

        /// <summary>获取当前科目</summary>
        public RoomClass GetRoomClass()
        {
            return RoomClass.GetByID(this.RoomClassID);
        }

        int _CoursewareCount = -1;
        /// <summary>当前班级的课件数</summary>
        public int CoursewareCount
        {
            get
            {
                Subject theSubject = this.GetSubject();
                RoomClass theRoomClass = this.GetRoomClass();
                if (theSubject == null || theRoomClass == null)
                    return 0;
                if (_CoursewareCount < 0)
                {
                    _CoursewareCount = SourceDocument.GetSourceCountByFType(theSubject, theRoomClass, VSTO.PPT_FileType.Courseware);
                }
                return _CoursewareCount;
            }
        }

        int _QuestionCount = -1;
        /// <summary>当前班级的试题数</summary>
        public int QuestionCount
        {
            get
            {
                Subject theSubject = this.GetSubject();
                RoomClass theRoomClass = this.GetRoomClass();
                if (theSubject == null || theRoomClass == null)
                    return 0;
                if (_QuestionCount < 0)
                {
                    _QuestionCount = SourceDocument.GetSourceCountByFType(theSubject, theRoomClass, VSTO.PPT_FileType.Question);
                }
                return _QuestionCount;
            }
        }

        int _NofityCount = -1;
        /// <summary>当前班级的通知数</summary>
        public int NofityCount
        {
            get
            {
                Subject theSubject = this.GetSubject();
                RoomClass theRoomClass = this.GetRoomClass();
                if (theSubject == null || theRoomClass == null)
                    return 0;
                if (_NofityCount < 0)
                {
                    _NofityCount = SourceDocument.GetSourceCountByFType(theSubject, theRoomClass, VSTO.PPT_FileType.Nofity);
                }
                return _NofityCount;
            }
        }

        int _VoteQuestionsCount = -1;
        /// <summary>当前班级的投票数</summary>
        public int VoteQuestionsCount
        {
            get
            {
                Subject theSubject = this.GetSubject();
                RoomClass theRoomClass = this.GetRoomClass();
                if (theSubject == null || theRoomClass == null)
                    return 0;
                if (_VoteQuestionsCount < 0)
                {
                    _VoteQuestionsCount = SourceDocument.GetSourceCountByFType(theSubject, theRoomClass, VSTO.PPT_FileType.VoteQuestions);
                }
                return _VoteQuestionsCount;
            }
        }

        int _DiscussCount = -1;
        /// <summary>当前班级的讨论数</summary>
        public int DiscussCount
        {
            get
            {
                Subject theSubject = this.GetSubject();
                RoomClass theRoomClass = this.GetRoomClass();
                if (theSubject == null || theRoomClass == null)
                    return 0;
                if (_DiscussCount < 0)
                {
                    _DiscussCount = SourceDocument.GetSourceCountByFType(theSubject, theRoomClass, VSTO.PPT_FileType.Discuss);
                }
                return _DiscussCount;
            }
        }


        #region 静态成员
        public static readonly Rel_Subject_RoomClass NONE = new Rel_Subject_RoomClass();

        /// <summary>依据物理唯一标识获取对象</summary>
        public static Rel_Subject_RoomClass GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            Rel_Subject_RoomClass the = EntityBase.GetMyICache().Get(autoID) as Rel_Subject_RoomClass;

            if (the == null)
            {
                ParameterTag[] ps =  
                { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 50 ) 
                };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new Rel_Subject_RoomClass();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        public static Rel_Subject_RoomClass GetByID(Subject theSubject, RoomClass theRoomClass)
        {
            Rel_Subject_RoomClass the = null;
            ParameterTag[] ps =  { 
                new ParameterTag("@SubjectID" , theSubject.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@RoomClassID" , theRoomClass.AutoID  ,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = NONE.EntityMaping_Excute("GetByID2", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string id = readers[0].GetValue(0).ToString();
                    the = EntityBase.GetMyICache().Get(id) as Rel_Subject_RoomClass;
                    if (the == null)
                    {
                        the = new Rel_Subject_RoomClass();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                }
            });
            return the;
        }



        //=============更新操作============================
        public static Result Insert(Subject theSubject, RoomClass theRoomClass)
        {
            Result rs = Result.NONE;

            Rel_Subject_RoomClass the = GetByID(theSubject, theRoomClass);
            if (the != null)
                return new Result(true, "对象已存在", the, 100);


            the = new Rel_Subject_RoomClass();

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@SubjectID" , theSubject.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@RoomClassID" , theRoomClass.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CTime" , DateTime.Now ,  E_DbType.DateTime , 8 ) ,
            };

            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                the.CTime = DateTime.Now;
                the.SubjectID = theSubject.AutoID;
                the.RoomClassID = theRoomClass.AutoID;
                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;
            }
            return rs;
        }
        /// <summary>删除</summary>
        public static Result Delete(Subject theSubject, RoomClass theRoomClass)
        {

            Rel_Subject_RoomClass theRel = Rel_Subject_RoomClass.GetByID(theSubject, theRoomClass);
            if (theRel == null)
                return new Result(true, "记录不存在", -100);

            int rsCount =  CourseInfo.GetCountByRoomClassAndSubject(theSubject, theRoomClass);
            if (rsCount > 0)
                return new Result(false, "操作终止：当前班级已发布至一个课程，不可移除当前关联");

            Result rs = Result.NONE;
            ParameterTag[] ps =  { 
                new ParameterTag("@SubjectID" , theSubject.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@RoomClassID" , theRoomClass.AutoID  ,  E_DbType.VarChar , 50 ) 
                                };
            rs = NONE.EntityMaping_Excute("Delete", ps);
            return rs;
        }
        #endregion
    }
}
