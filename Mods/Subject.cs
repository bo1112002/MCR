using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>科目(SBJ)</summary>
    [Serializable]
    public class Subject : EntityBase
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
        /// <summary>所属的学校ID</summary>
        public string SchoolID
        {
            get { return _SchoolID; }
            set { _SchoolID = value; }
        }
        string _Bind_ID = string.Empty;
        /// <summary>用于绑定该学校的物理科目</summary>
        public string Bind_ID
        {
            get { return _Bind_ID; }
            set { _Bind_ID = value; }
        }
        string _Name = string.Empty;
        /// <summary>科目名称</summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }
        #endregion

        protected Subject() { }

        #region============= 重写成员=========>>>

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.SchoolID = reader.GetValue<string>(this, "SchoolID");
            this.Bind_ID = reader.GetValue<string>(this, "Bind_ID");
            this.Name = reader.GetValue<string>(this, "Name");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
        }

        public override Type GetTypeBase()
        {
            return typeof(Subject);
        }

        protected override string GetPrefixName()
        {
            return "SBJ";
        }
        #endregion=============END==========<<<

        /// <summary>获取当前创建的成员</summary>
        public WX_Member GetMyMember()
        {
            return WX_Member.GetByID(this.MemberID);
        }


        /// <summary>获取当前科目下的班级集合</summary>
        public IList<RoomClass> GetMyRoomClasses()
        {
            return RoomClass.GetListBySubject(this);
        }


        /// <summary>创建者(老师)名</summary>
        public string CreateName
        {
            get
            {
                WX_Member theMember = this.GetMyMember();
                if (theMember == null)
                    return string.Empty;
                return theMember.Name;
            }
        }

        /// <summary>创建时间的字符串格式化</summary>
        public string CTimeString
        {
            get
            {
                return this.CTime.ToString("yyyy-MM-dd");
            }
        }


        //============更新操作============

        /// <summary>修改科目名称</summary>
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

        /// <summary>向当前科目添加所属的班级</summary>
        public Result AddClassRoom(RoomClass theRoomClass)
        {
            WX_Member theMember = this.GetMyMember();
            if (theMember == null)
                return new Result(false, "操作终止：无效的成员对象");

            School theSchool = theMember.GetSchool();
            if (theSchool == null)
                return new Result(false, "操作终止：当前成员需要设置所属的学校");

            Result rs = Rel_RoomClass_Member.Insert(theMember, theRoomClass);
            if (rs.IsOK)
            {

                if (Rel_Subject_RoomClass.GetByID(this, theRoomClass) != null)
                {
                    return Result.OK;
                }
                else
                {
                    return Rel_Subject_RoomClass.Insert(this, theRoomClass);
                }
            }
            return rs;
        }
        /// <summary>移除指定的班级</summary>
        public Result RemoveClassRoom(RoomClass theRoomClass)
        {
            return Rel_Subject_RoomClass.Delete(this, theRoomClass);
        }






        #region 静态成员
        public static readonly Subject NONE = new Subject();

        /// <summary>依据物理唯一标识获取对象</summary>
        public static Subject GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            Subject the = EntityBase.GetMyICache().Get(autoID) as Subject;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 50 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new Subject();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        /// <summary>依据物理唯一标识获取对象</summary>
        public static Subject GetByMemberAndName(WX_Member theMember, string subjectName)
        {
            if (string.IsNullOrEmpty(subjectName))
            {
                return null;
            }

            Subject the = null;
            ParameterTag[] ps =  { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Name" , subjectName ,  E_DbType.VarChar , 50 ) 
                                 };
            Result rs = NONE.EntityMaping_Excute("GetByMemberAndName", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string id = readers[0].GetValue(0).ToString();
                    the = EntityBase.GetMyICache().Get(id) as Subject;
                    if (the == null)
                    {
                        the = new Subject();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                }
            });
            return the;
        }

        /// <summary> 获取某个学校的所有科目</summary>
        public static IList<Subject> GetBySchool(School theSchool)
        {
            List<Subject> list = new List<Subject>();
            ParameterTag[] ps =  { 
                new ParameterTag("@AutoID" , theSchool.AutoID,  E_DbType.VarChar , 50 ) ,
                                };
            Subject the = null;
            Result rs = NONE.EntityMaping_Excute("GetItemBySchoolID", ps, (readers) =>
            {
                foreach (EntityReader reader in readers)
                {
                    string autoID = reader.GetValue("AutoID").ToString();
                    the = EntityBase.GetMyICache().Get(autoID) as Subject;
                    if (the == null)
                    {
                        the = new Subject();
                        the.ToEntity(reader);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    list.Add(the);
                }
            });
            return list;
        }

        /// <summary>获取指定班级的科目集合</summary>
        public static IList<Subject> GetListByRoomClass(RoomClass theRoomClass)
        {
            List<Subject> list = new List<Subject>();
            ParameterTag[] ps =  { 
                new ParameterTag("@RoomClassID" , theRoomClass.AutoID,  E_DbType.VarChar , 50 ) ,
                                };
            Result rs = NONE.EntityMaping_Excute("GetListByRoomClass", ps, (readers) =>
            {
                EntityBase.AddToList<Subject>(list, readers, (r) => new Subject());
            });
            return list;
        }

        /// <summary>获取指定班级的科目数</summary>
        public static int GetListByRoomClass_Count(RoomClass theRoomClass)
        {
            int count = 0;
            ParameterTag[] ps =  { 
                new ParameterTag("@RoomClassID" , theRoomClass.AutoID,  E_DbType.VarChar , 50 ) ,
                                };
            Result rs = NONE.EntityMaping_Excute("GetListByRoomClass_Count", ps, (readers) =>
            {
                if (readers.Count > 0)
                    count = Convert.ToInt32(readers[0].GetValue(0));
            });
            return count;
        }

        /// <summary>获取指定成员(老师)的所在学校的科目集合</summary>
        public static IList<Subject> GetListByMember_More(WX_Member theMember, string lastID = "")
        {
            if (string.IsNullOrEmpty(lastID) == true)
                lastID = "ZZZZZZZZ"; //表示最大值

            List<Subject> list = new List<Subject>();
            School theSchool = theMember.GetSchool();
            if (theSchool == null)
                return list;


            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theMember.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@SchoolID" , theSchool.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@LastID" , lastID ,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = NONE.EntityMaping_Excute("GetListByMember", ps, (readers) =>
            {
                EntityBase.AddToList<Subject>(list, readers, (r) => new Subject_Ext2(), (theAdd, reader2) =>
                {
                    if (theAdd is Subject_Ext2 == false)
                    {
                        Subject_Ext2 the2 = new Subject_Ext2();
                        the2.ToEntity(reader2);
                        return the2;
                    }
                    return null;
                });
            });
            return list;
        }


        /// <summary>获取指定成员(学生)的所在学校的科目集合</summary>
        public static IList<Subject> GetListByStudent_More(WX_Member theMember, string likeName, string lastID = "")
        {
            if (string.IsNullOrEmpty(lastID) == true)
                lastID = "ZZZZZZZZ"; //表示最大值

            List<Subject> list = new List<Subject>();
            School theSchool = theMember.GetSchool();
            if (theSchool == null)
                return list;


            if (string.IsNullOrEmpty(likeName))
            {
                likeName = "%";
            }
            else
            {
                likeName = "%" + likeName.Replace(" ", "%") + "%";
            }


            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theMember.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@SchoolID" , theSchool.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@LikeName" , likeName ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@LastID" , lastID ,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = NONE.EntityMaping_Excute("GetListByStudent", ps, (readers) =>
            {
                EntityBase.AddToList<Subject>(list, readers, (r) => new Subject_Ext3(theMember), (theAdd, reader2) =>
                {
                    if (theAdd is Subject_Ext3)
                    {
                        Subject_Ext3 theNew2 = new Subject_Ext3(theMember);
                        theNew2.ToEntity(reader2);
                        return theNew2;
                    }
                    return null;
                });
            });
            return list;
        }




        //============更新操作============
        /// <summary>新增</summary>
        public static Result Insert(WX_Member theMember, string name)
        {
            if (theMember.MType < MemberType.E_Teacher)
            {
                return new Result(false, "操作终止：该成员无权限");
            }
            else if (string.IsNullOrEmpty(name))
            {
                return new Result(false, "操作终止：名称能为空");
            }

            School theSchool = theMember.GetSchool();
            if (theSchool == null)
            {
                return new Result(false, "需要指定所属的学校");
            }

            Subject theSubject = GetByMemberAndName(theMember, name);
            if (theSubject != null)
                return new Result(true, "对象已存在", theSubject, 100);

            Result rs = Result.NONE;
            Subject the = new Subject();
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@SchoolID" , theSchool.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Name" , name ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Bind_ID" , the.Bind_ID  ,  E_DbType.VarChar ,  50) ,
                    new ParameterTag("@CTime" , DateTime.Now ,  E_DbType.DateTime , 8 ) 
            };
            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                the.MemberID = theMember.AutoID;
                the.CTime = DateTime.Now;
                the.SchoolID = theSchool.AutoID;
                the.Name = name;
                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;
            }
            return rs;
        }
        /// <summary>删除</summary>
        public static Result Delete(Subject the)
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


    /// <summary>Subject的扩展类</summary>
    internal class Subject_Ext2 : Subject
    {

        internal Subject_Ext2()
            : base()
        {
        }

        /// <summary>获取当前科目下的所属班级集合</summary>
        public IList<RoomClass> MyRoomClasses
        {
            get
            {
                return RoomClass.GetListBySubject(this);
            }
        }


    }

    /// <summary>Subject的扩展类</summary>
    internal class Subject_Ext3 : Subject
    {

        readonly WX_Member _TheMember = null;
        internal Subject_Ext3(WX_Member theMember)
            : base()
        {
            _TheMember = theMember;
        }


        private string _RoomClassID = string.Empty;
        /// <summary>获取当前成员所属的班级</summary>
        public string RoomClassID
        {
            get { return _RoomClassID; }
            set { _RoomClassID = value; }
        }

        int _CourseCount = 0;
        /// <summary>课程数</summary>
        public int CourseCount
        {
            get { return _CourseCount; }
            set { _CourseCount = value; }
        }


        protected override void ToEntity(EntityReader reader)
        {
            base.ToEntity(reader);

            this.RoomClassID = reader.GetValue("RoomClassID").ToString();
            this.CourseCount = Convert.ToInt32(reader.GetValue("CourseCount"));
        }

        /// <summary>获取当前所属的班级</summary>
        public RoomClass GetRoomClass()
        {
            return RoomClass.GetByID(this.RoomClassID);
        }

        /// <summary>获取当前所属的班级</summary>
        public string RoomClassName
        {
            get
            {
                RoomClass theRoomClass = this.GetRoomClass();
                if (theRoomClass == null)
                    return string.Empty;
                return theRoomClass.Name;
            }
        }
        /// <summary>获取</summary>
        public string FirstDocumentImgURL
        { 
            get
            {
                CourseInfo theCourseInfo = null;
                RoomClass theRoomClass = this.GetRoomClass();
                if (theRoomClass != null)
                {
                    theCourseInfo = CourseInfo.GetByRoomClassToFirst(theRoomClass, this);
                    SourceDocument theDocment = theCourseInfo.GetDocument();
                    if (theDocment != null)
                    {
                        return theDocment.FirstImgURL;
                    }
                }
                return string.Empty;
            }
        }


        /// <summary>未完成的课程</summary>
        public IList<CourseInfo> NotDoneCourse
        {
            get
            {
                RoomClass theRoomClass = this.GetRoomClass();
                if (theRoomClass == null)
                    return new List<CourseInfo>();
                return CourseInfo.GetListByNotRead(theRoomClass, this, _TheMember );
            }
        }


        /// <summary>获取科目与班级的关系ID</summary>
        public string RelSR_ID
        {
            get
            {
                 RoomClass theRoomClass = this.GetRoomClass();
                if (theRoomClass == null)
                    return string.Empty ;
                Rel_Subject_RoomClass the = Rel_Subject_RoomClass.GetByID(this, theRoomClass);
                if (the == null)
                    return string.Empty;
                return the.AutoID;
            }
        }

        /*
        /// <summary>获取当前科目下的课程集合</summary>
        public IList<CourseInfo> MyCourseInfo
        {
            get
            {
                RoomClass theRoomClass = this.GetRoomClass();
                if (theRoomClass == null)
                    return new List<CourseInfo>();
                return CourseInfo.GetByRoomClass(theRoomClass ,  this ); 
            }
        }
        */

    }


}
