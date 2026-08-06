using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>班级(RC)</summary>
    [Serializable]
    public class RoomClass :  EntityBase
    {
        #region 持久属性
        string _MemberID = string.Empty;
        /// <summary>成员ID(创建人)</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }
        string _SchoolID = string.Empty;
        /// <summary>学校ID</summary>
        public string SchoolID
        {
            get { return _SchoolID; }
            set { _SchoolID = value; }
        }
        string _Bind_ID = string.Empty;
        /// <summary>用于绑定该学校的物理班级</summary>
        public string Bind_ID
        {
            get { return _Bind_ID; }
            set { _Bind_ID = value; }
        }
        string _Name = string.Empty;
        /// <summary>班级名称</summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        string _InvitCode = string.Empty;
        /// <summary>加入班级的邀请码(由数字+大写字母组成的8位随机字符串)</summary>
        public string InvitCode
        {
            get { return _InvitCode; }
            set { _InvitCode = value; }
        }
        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }


        int _AuditingTag = -1;
        /// <summary>是否已审核(-1:待审核，0:未通过，1:已通过，)</summary>
        public int AuditingTag
        {
            get { return _AuditingTag; }
            set { _AuditingTag = value; }
        }

        string _AuditingMemberID = string.Empty;
        /// <summary>审核人</summary>
        public string AuditingMemberID
        {
            get { return _AuditingMemberID; }
            set { _AuditingMemberID = value; }
        }
        DateTime _AuditingTime = PublicMethod.NONE_DateTime;
        /// <summary>审核时间</summary>
        public DateTime AuditingTime
        {
            get { return _AuditingTime; }
            set { _AuditingTime = value; }
        }

        #endregion

        protected RoomClass()
        {
            EntityBase.Evt_EntityChange += (entity) => {
                if (entity.AutoID == this.AutoID) {
                    _MemberCount = -1;
                }
            };
        }
        
        /*
        protected RoomClass(Member myMember, School mySchool)
        {
        }
        */

        #region============= 重写成员=========>>>

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.Bind_ID = reader.GetValue<string>(this, "Bind_ID");
            this.SchoolID = reader.GetValue<string>(this, "SchoolID");
            this.Name = reader.GetValue<string>(this, "Name");
            this.InvitCode = reader.GetValue<string>(this, "InvitCode");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");

            this.AuditingTag = reader.GetValue<int>(this, "AuditingTag");
            this.AuditingMemberID = reader.GetValue<string>(this, "AuditingMemberID");
            this.AuditingTime = reader.GetValue<DateTime>(this, "AuditingTime");
        }

        public override Type GetTypeBase()
        {
            return typeof(RoomClass);
        }

        protected override string GetPrefixName()
        {
            return "RC";
        }

        #endregion=============END==========<<<


        /// <summary>获取当前创建班级的成员对象</summary>
        public WX_Member GetMyMember()
        {
            return WX_Member.GetByID(this.MemberID);
        }

        /// <summary>获取当前班级的所属的学校对象</summary>
        public School GetMySchool()
        {
            return School.GetByID(this.SchoolID);
        }

        /// <summary>获取当前班级下的所有成员</summary>
        public IList<WX_Member> GetAllMember()
        {
            return WX_Member.GetListByRoomClass_ALL(this);
        }
        /// <summary>获取当前班级下的学生</summary>
        public IList<WX_Member> GetStudents()
        {
            return WX_Member.GetStudents(this);
        }
        /// <summary>获取当前班级下的老师</summary>
        public IList<WX_Member> GetTeachers()
        {
            return WX_Member.GetTeachers(this);
        }


        
        int _MemberCount = -1;
        /// <summary>获取当前班级的成员数(包括学生及老师)</summary>
        public int MemberCount
        {
            get 
            {
                if (_MemberCount < 0)
                {
                    _MemberCount = GetAllMember().Count;
                }
                return _MemberCount; 
            }
        }

        /// <summary>当前班级的课件数</summary>
        public int CoursewareCount( Subject  theSubject )
        {
            Rel_Subject_RoomClass theRel = Rel_Subject_RoomClass.GetByID(theSubject, this);
            return theRel.CoursewareCount;
        }

        /// <summary>当前班级的试题数</summary>
        public int QuestionCount(Subject theSubject)
        {
            Rel_Subject_RoomClass theRel = Rel_Subject_RoomClass.GetByID(theSubject, this);
            return theRel.QuestionCount;
        }



        //===============更新操作=====================
        /// <summary>修改班级名称</summary>
        public Result Update(string name)
        {
            Result rs = Result.NONE;
            if (string.IsNullOrEmpty(name.Trim()) == true)
                return new Result(false, "名称不能为空");

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Name" , name ,  E_DbType.VarChar , 50 ) ,
            };
            rs = this.EntityMaping_Excute("UpdateName", ps);
            if (rs.IsOK)
            {
                this.Name = name;
            }
            return rs;
        }

        /// <summary>审核班级</summary>
        public Result Update(WX_Member theMember, int auditingTag)
        {
            Result rs = Result.NONE;
            if (auditingTag <-1 ||  auditingTag >=2)
                return new Result(false, "无效的审核类型");

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AuditingTag" , auditingTag ,  E_DbType.Int , 4 ) ,
                    new ParameterTag("@AuditingMemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@AuditingTime" , DateTime.Now ,  E_DbType.DateTime , 0 ) 
            };
            rs = this.EntityMaping_Excute("UpdateAuditing", ps);
            if (rs.IsOK)
            {
                this.AuditingTag = auditingTag;
                this.AuditingMemberID = theMember.AutoID;
                this.AuditingTime = DateTime.Now ;
            }
            return rs;
        }



        /// <summary>加入某个成员到当前班级</summary>
        public Result AddMember(WX_Member theMember , string qrCode)
        {
            if(this.GetMyQRCode() !=  qrCode)
                return new Result(false, "操作终止：加入班级邀请码无效或已过期");
            return Rel_RoomClass_Member.Insert(theMember, this); 
        }
        /// <summary>把指定成员从当前班级移除</summary>
        public Result RemoveMember(WX_Member theMember)
        {
            return Rel_RoomClass_Member.Delete(theMember , this);
        }

        /// <summary>获取指定成员的加入班级的验证码</summary>
        protected string GetInvitCode(WX_Member theMember )
        {
            long n1 = long.Parse( DateTime.Now.ToString("yyMMdd") ) ;
            long n2 = long.Parse(theMember.CTime.ToString("yyMMddHHmmss"));
            long n3 = long.Parse(this.CTime.ToString("yyMMddHHmmss"));

            long nnn = n3 - n2;
            if (nnn >= 0)
                nnn = nnn + n1;
            else
                nnn = (nnn * -1) + n1;
            return PublicMethod.To36String(nnn);
        }

        /// <summary>获取当前创建者的邀请码</summary>
        public string GetMyQRCode()
        { 
            WX_Member theMeber = this.GetMyMember() ;
            return GetInvitCode(theMeber);
        }

        /// <summary>从当前班级移除成员(如果：theMember==null则表示移除所有成员)</summary>
        public Result ClearMember( WX_Member theMember = null )
        {
            if (theMember != null)
            {
                return Rel_RoomClass_Member.Delete(theMember, this);
            }
            else
            {
                return Rel_RoomClass_Member.Delete_All(this);
            }
        }


        #region 静态成员
        public static readonly RoomClass NONE = new RoomClass();

        /// <summary>依据物理唯一标识获取对象</summary>
        public static RoomClass GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            RoomClass the = EntityBase.GetMyICache().Get(autoID) as RoomClass;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 30 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new RoomClass();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        public static RoomClass GetByMemberAndName(WX_Member theMember, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            RoomClass the = null;

            ParameterTag[] ps =  { 
                    new ParameterTag("@Name" , name ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , theMember.AutoID  ,  E_DbType.VarChar , 30 ) 
                                 };
            Result rs = NONE.EntityMaping_Excute("GetByMemberAndName", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string id = readers[0].GetValue(0).ToString();
                    the = EntityBase.GetMyICache().Get(id) as RoomClass;
                    if (the == null)
                    {
                        the = new RoomClass();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                }
            });
            return the;
        }

        /// <summary>获取与指定课程关联的班级对象</summary>
        public static RoomClass GetByCourseInfo(CourseInfo theCourseInfo)
        {
            RoomClass the = null;
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@CourseInfoID" , theCourseInfo.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetByCourseInfo", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string id = readers[0].GetValue(0).ToString();
                    the = EntityBase.GetMyICache().Get(id) as RoomClass;
                    if (the == null)
                    {
                        the = new RoomClass();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                }
            });
            return the;
        }


        /// <summary>获某个学校下的所有的班级集合</summary>
        public static IList<RoomClass> GetListBySchool(School theSchool)
        {
            List<RoomClass> list = new List<RoomClass>();

            ParameterTag[] ps =  
                { 
                    new ParameterTag("@SchoolID" , theSchool.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetListBySchool", ps, (readers) =>
            {
                EntityBase.AddToList<RoomClass>(list, readers, (r) => { return new RoomClass(); });
            });
            return list;
        }

        /// <summary>获取指定成员所创建的班级集合</summary>
        public static IList<RoomClass> GetListByMember(WX_Member theMember)
        {
            List<RoomClass> list = new List<RoomClass>();

            ParameterTag[] ps =  
                { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetListByMember", ps, (readers) =>
            {
                EntityBase.AddToList<RoomClass>(list, readers, (r) => { return new RoomClass(); });
            });
            return list;
        }

        /// <summary>获取与指定科目关联的班级集合</summary>
        public static IList<RoomClass> GetListBySubject(Subject theSubject)
        {
            List<RoomClass> list = new List<RoomClass>();

            ParameterTag[] ps =  
                { 
                    new ParameterTag("@SubjectID" , theSubject.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetListBySubject", ps, (readers) =>
            {
                EntityBase.AddToList<RoomClass>(list, readers, 
                    (r) => 
                    { 
                        return new RoomClass(); 
                    },
                    (theAdd, reader2) => 
                    { 
                        Rel_Subject_RoomClass theRel = Rel_Subject_RoomClass.GetByID(theSubject, theAdd); 
                        Dictionary<string,object> map = new Dictionary<string,object>() ;
                        map["RelSR_ID"] = theRel.AutoID;
                        map["CoursewareCount"] = theRel.CoursewareCount ;
                        map["QuestionCount"] = theRel.QuestionCount ;

                        map["QuestionCount"] = theRel.QuestionCount;
                        map["CourseCount"] = CourseInfo.GetCountByRoomClassAndSubject(theSubject, theAdd);
                        theAdd.Tag = map;
                        return null;
                    });
            });
            return list;
        }


        //===================更新==================
        /// <summary>新增</summary>
        public static Result Insert(WX_Member myMember, School mySchool, string name, string bindID = "")
        {
            name = name.Trim().Replace(" " , "");
            Result rs = Result.NONE;
            RoomClass the = GetByMemberAndName(myMember, name);
            if(the != null)
                return new Result(false , "该班级名称已存在", the , 100 ) ;

            the = new RoomClass();
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@SchoolID" , mySchool.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , myMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Name" , name ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Bind_ID" , bindID ,  E_DbType.VarChar , 1000 ) ,
                    new ParameterTag("@InvitCode" , the.InvitCode ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CTime" , the.CTime ,  E_DbType.DateTime , 8 ) 
            };

            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                the.SchoolID = mySchool.AutoID;
                the.MemberID = myMember.AutoID;
                the.Name = name;
                the.Bind_ID = bindID;
                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;
            }
            return rs;
        }
        /// <summary>删除</summary>
        public static Result Delete(RoomClass the)
        {
            WX_Member theMember  =the.GetMyMember() ;
            if(theMember == null )
                return new Result(false , "操作终止：找不到当前班级的创建成员" ) ;
            
            int count =  Subject.GetListByRoomClass_Count( the) ; 
            if(count >  0 )
                return new Result(false , "操作终止：当前班级已存在所属的课程");
            
            Result rs = Result.NONE;
            ParameterTag[] ps = 
            { 
                new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
            };
            rs = the.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear(the.AutoID);
            }
            return rs;
        }


        /// <summary>删除</summary>
        public static Result Delete(RoomClass theRoomClass, Subject theSubject)
        {
            WX_Member theMember = theRoomClass.GetMyMember();
            if (theMember == null)
                return new Result(false, "操作终止：找不到当前班级的创建成员");

            Result rs = Result.NONE;
            ParameterTag[] ps = 
            { 
                new ParameterTag("@AutoID" , theRoomClass.AutoID ,  E_DbType.VarChar , 50 ) ,
            };
            rs = theRoomClass.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                Rel_Subject_RoomClass.Delete(theSubject, theRoomClass); 
            }
            return rs;
        }


        #endregion



        
    }


    /*
    internal class RoomClass_ExtA : RoomClass
    {
        public RoomClass_ExtA(Subject theSubject)
        { 
        }
    }
    */


    
}
