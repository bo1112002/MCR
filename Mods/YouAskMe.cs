using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>提问-你问我答(YAME)</summary>
    [Serializable]
    public class YouAskMe : EntityBase
    {
        #region 持久属性
        string _CourseInfoID = string.Empty;
        /// <summary>所属的课程ID</summary>
        public string CourseInfoID
        {
            get { return _CourseInfoID; }
            set { _CourseInfoID = value; }
        }
        string _MemberID = string.Empty;
        /// <summary>成员ID(提问人或回答人)</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }
        string _FirstID = string.Empty;
        /// <summary>第一个发起问题的记录ID</summary>
        public string FirstID
        {
            get { return _FirstID; }
            set { _FirstID = value; }
        }
        string _Contents = string.Empty;
        /// <summary>提问或回答的内容</summary>
        public string Contents
        {
            get { return _Contents; }
            set { _Contents = value; }
        }
        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间(提问时间)</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }
        #endregion

        protected YouAskMe() { }

        #region============= 重写成员=========>>>

        protected override void ToEntity(EntityReader reader)
        {
            this.CourseInfoID = reader.GetValue<string>(this, "CourseInfoID");
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.FirstID = reader.GetValue<string>(this, "FirstID");
            this.Contents = reader.GetValue<string>(this, "Contents");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
        }
        /*
        public override void Serialize(IDictionary<string, object> map)
        {
            map.Add("AutoID", this.AutoID);
            map.Add("MemberID", GetMember());
            map.Add("CourseInfoID", this.CourseInfoID);
            map.Add("FirstID", this.FirstID);
            map.Add("Contents", this.Contents);
            map.Add("CTime", this.CTime);
        }
        */
        public override Type GetTypeBase()
        {
            return typeof(YouAskMe);
        }

        protected override string GetPrefixName()
        {
            return "YAME";
        }
        #endregion=============END==========<<<



        /// <summary>获取当前成员</summary>
        public WX_Member GetMember()
        {
            return WX_Member.GetByID(this.MemberID);
        }
        /// <summary>获取当前课程</summary>
        public CourseInfo GetCourseInfo()
        {
            return CourseInfo.GetByID(this.CourseInfoID);
        }
        /// <summary>当前是否为问题发起对象</summary>
        public bool IsFirst
        {
            get
            {
                return this.AutoID == this.FirstID;
            }
        }


        /// <summary>获取当前发起问题的回复</summary>
        public IList<YouAskMe> Childs()
        {
            return GetFirstOfChilds(this);
        }


        //============更新操作=================
        /*
        /// <summary>用户回答</summary>
        public  Result Update(Member theMember, string CourseInfoID, string FirstID, string Contents)
        {
            Result rs = Result.NONE;
            return rs;
        }*/


        #region 静态成员
        public static readonly YouAskMe NONE = new YouAskMe();

        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static YouAskMe GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            YouAskMe the = EntityBase.GetMyICache().Get(autoID) as YouAskMe;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 30 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new YouAskMe();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }


        /// <summary>获取某个发起问题的所有回复的集合</summary>
        public static IList<YouAskMe> GetFirstOfChilds(YouAskMe theYouAskMe)
        {
            List<YouAskMe> list = new List<YouAskMe>();
            if (theYouAskMe.IsFirst == false)
                return list;
            ParameterTag[] ps =  { 
                new ParameterTag("@AutoID" , theYouAskMe.AutoID,  E_DbType.VarChar , 50 ) ,
                                };
            YouAskMe the = null;
            Result rs = NONE.EntityMaping_Excute("GetFirstItemByAutoID", ps, (readers) =>
            {
                foreach (EntityReader reader in readers)
                {
                    string autoID = reader.GetValue("AutoID").ToString();
                    the = EntityBase.GetMyICache().Get(autoID) as YouAskMe;
                    if (the == null)
                    {
                        the = new YouAskMe();
                        the.ToEntity(reader);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    list.Add(the);
                }
            });
            return list;
        }

        /// <summary>获取某个成员发起问题的集合</summary>
        public static IList<YouAskMe> GetAllFirstByMember(WX_Member theMember, int pageNo, int pageSize)
        {
            List<YouAskMe> list = new List<YouAskMe>();
            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theMember.AutoID,  E_DbType.VarChar , 50 ) 
                                };
            YouAskMe the = null;
            Result rs = NONE.EntityMaping_Excute("GetRequestItemByMemberID", ps, (readers) =>
            {
                foreach (EntityReader reader in readers)
                {
                    string autoID = reader.GetValue("AutoID").ToString();
                    the = EntityBase.GetMyICache().Get(autoID) as YouAskMe;
                    if (the == null)
                    {
                        the = new YouAskMe();
                        the.ToEntity(reader);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    list.Add(the);
                }
            });
            return list;
        }
        /// <summary>获取某个成员参与回复问题的集合</summary>
        public static IList<YouAskMe> GetAllFirstToAskByMember(WX_Member theMember, int pageNo, int pageSize)
        {
            List<YouAskMe> list = new List<YouAskMe>();
            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theMember.AutoID,  E_DbType.VarChar , 50 ) 
                                };
            YouAskMe the = null;
            Result rs = NONE.EntityMaping_Excute("GetResponseItemByMemberID", ps, (readers) =>
            {
                foreach (EntityReader reader in readers)
                {
                    string autoID = reader.GetValue("AutoID").ToString();
                    the = EntityBase.GetMyICache().Get(autoID) as YouAskMe;
                    if (the == null)
                    {
                        the = new YouAskMe();
                        the.ToEntity(reader);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    list.Add(the);
                }
            });
            return list;
        }


        /// <summary>获取课堂上的某成员发起问题的集合</summary>
        public static IList<YouAskMe> GetAllFirstToAskByCourseInfo(WX_Member theMember, CourseInfo theCourseInfo)
        {
            List<YouAskMe> list = new List<YouAskMe>();
            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theMember.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@CourseInfoID" , theCourseInfo.AutoID,  E_DbType.VarChar , 50 ) 
                                };
            YouAskMe the = null;
            Result rs = NONE.EntityMaping_Excute("GetItemByMCID", ps, (readers) =>
            {
                foreach (EntityReader reader in readers)
                {
                    string autoID = reader.GetValue("AutoID").ToString();
                    the = EntityBase.GetMyICache().Get(autoID) as YouAskMe;
                    if (the == null)
                    {
                        the = new YouAskMe();
                        the.ToEntity(reader);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    list.Add(the);
                }
            });
            return list;
        }


        //=============更新操作==================


        /// <summary>用户发起问题或回复</summary>
        public static Result Insert(WX_Member theMember, CourseInfo theCourseInfo, string Contents, YouAskMe theFirst = null)
        {
            Result rs = Result.NONE;
            YouAskMe the = new YouAskMe(); //AutoID属性会在new 的时候就会被赋值
            if (theFirst != null && theFirst.IsFirst)
            {
                //回复
                //如果是问题的回复，则FirstID为需要保存发起对象的theFirst.AutoID
                the.FirstID = theFirst.AutoID;
            }
            else if (theFirst == null)
            {
                //发起问题
                //如果是发起问题，则FirstID为当前对象的AutoID
                the.FirstID = the.AutoID;
            }
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseInfoID" , theCourseInfo.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@FirstID" , the.FirstID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Contents" , Contents ,  E_DbType.VarChar , 1000 ) ,
                    new ParameterTag("@CTime" , DateTime.Now ,  E_DbType.DateTime , 50 ) ,
            };
            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                the.Contents = Contents;
                the.CTime = DateTime.Now;
                the.MemberID = theMember.AutoID;
                the.CourseInfoID = theCourseInfo.AutoID;
                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;
            }
            return rs;
        }

        /// <summary>删除(老师或管理员权限),如果删除是的发起问题的对象，则所有的回复记录都要全部删除</summary>
        public static Result Delete(YouAskMe info)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , info.AutoID ,  E_DbType.VarChar , 50 ) ,
                };
            if (info.IsFirst == true)
            {
                rs = info.EntityMaping_Excute("DeleteTheByID", ps);
            }
            else
            {
                rs = info.EntityMaping_Excute("DeleteAllByID", ps);
            }
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear(info.AutoID);
            }
            return rs;
        }

        #endregion
    }
}
