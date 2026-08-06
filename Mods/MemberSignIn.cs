using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>成员签到信息类(MSNI)</summary>
    public class MemberSignIn : EntityBase
    {

        #region 持久属性

        int _LaunchType = 0;
        /// <summary>签到记录类型, 1:老师发起签到,2:学生签到</summary>
        public int LaunchType
        {
            get { return _LaunchType; }
            set { _LaunchType = value; }
        }

        string _MemberID = string.Empty;
        /// <summary>成员ID</summary>
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

        DateTime _CTime = DateTime.Now;
        /// <summary>创建签到时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }

        string _ParentID = string.Empty;
        /// <summary>父级ID</summary>
        public string ParentID
        {
            get { return _ParentID; }
            set { _ParentID = value; }
        }

        double _X = 0;
        /// <summary>坐标X</summary>
        public double X
        {
            get { return _X; }
            set { _X = value; }
        }
        double _Y = 0;
        /// <summary>坐标Y</summary>
        public double Y
        {
            get { return _Y; }
            set { _Y = value; }
        }



        #endregion


        #region============= 重写成员=========>>>
        public override Type GetTypeBase()
        {
            return typeof(MemberSignIn);
        }

        protected override string GetPrefixName()
        {
            return "MSNI";
        }

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.LaunchType = reader.GetValue<int>(this, "LaunchType");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.CourseInfoID = reader.GetValue<string>(this, "CourseInfoID");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
            this.ParentID = reader.GetValue<string>(this, "ParentID");
            this.X = reader.GetValue<double>(this, "X");
            this.Y = reader.GetValue<double>(this, "Y");
        }
        #endregion=============END==========<<<


        /// <summary>结束签到</summary>
        public Result Update_EndSign()
        {
            if (this.LaunchType != 1)
                return new Result(false, "该操作只针对签到的发起者");

            ParameterTag[] ps =  
                { 
                    new ParameterTag("@CTime" , DateTime.Now ,  E_DbType.DateTime , 0 ) ,
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) 

                };

            Result rs = this.EntityMaping_Excute("Update_CTime", ps);
            if (rs.IsOK)
            {
                this.CTime = DateTime.Now;
            }
            return rs;
        }


        /// <summary>获取当前的发起签到对象，如果不存在，则返回null</summary>
        public MemberSignIn GetParent()
        {
            return MemberSignIn.GetByID(this.ParentID);
        }

        /// <summary>对于发起签到对象则为：是否签到结束;对于签到对象则为：是否延迟签到</summary>
        public bool IsEnd
        {
            get
            {
                if (this.LaunchType == 1)
                {
                    return DateTime.Now > this.CTime;
                }
                else //是否延迟签到
                {
                    MemberSignIn theParent = this.GetParent();
                    if (theParent == null)
                        return false;

                    return this.CTime > theParent.CTime;
                }
            }
        }



        #region 静态成员
        /// <summary>无效对象</summary>
        public static readonly MemberSignIn NONE = new MemberSignIn();

        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static MemberSignIn GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
                return null;

            MemberSignIn the = EntityBase.GetMyICache().Get(autoID) as MemberSignIn;
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
                        the = new MemberSignIn();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }


        public static MemberSignIn GetByID(WX_Member theMember, CourseInfo theCourse)
        {
            MemberSignIn the = null;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@CourseInfoID" , theCourse.AutoID ,  E_DbType.VarChar , 50 ) 
            };

            Result rs = NONE.EntityMaping_Excute("GetByID2", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string id = readers[0].GetValue(0).ToString();
                    the = MemberSignIn.GetByID(id);
                }
            });
            return the;
        }



        public static MemberSignIn GetByID(WX_Member theMember, MemberSignIn theParent)
        {
            MemberSignIn the = null;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@ParentID" , theParent.AutoID ,  E_DbType.VarChar , 50 ) 
            };

            Result rs = NONE.EntityMaping_Excute("GetByID3", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string id = readers[0].GetValue(0).ToString();
                    the = MemberSignIn.GetByID(id);
                }
            });
            return the;
        }


        /// <summary>获取签到集合</summary>
        public static IList<MemberSignIn> GetListByChilds(MemberSignIn theParent)
        {
            List<MemberSignIn> list = new List<MemberSignIn>();
            if (theParent.LaunchType != 1)
                return list;

            ParameterTag[] ps =  
            { 
                new ParameterTag("@ParentID" , theParent.AutoID ,  E_DbType.VarChar , 50 ) 
            };
            Result rs = NONE.EntityMaping_Excute("GetListByChilds", ps, (readers) =>
            {
                EntityBase.AddToList<MemberSignIn>(list, readers, (r) =>
                {
                    return new MemberSignIn();
                });
            });
            return list;
        }


        //==========更新操作===========

        /// <summary>记录微信用户的信息</summary>
        public static Result Insert(WX_Member theMember, CourseInfo theCourse, int type, MemberSignIn theParent = null, double x = 0, double y = 0)
        {
            if (type != 1 && type != 2)
                return new Result(false, "签到记录类型值不在范围内");

            MemberSignIn theNew = new MemberSignIn();

            if (type == 1) //如果发起签到，则增加三小时
            {
                theNew.CTime = DateTime.Now.AddHours(3);
            }
            else if (type == 2 && theParent == null )
            {
                return new Result(false, "需要指定所属的发起签到对象");
            }
            else if (type == 2 && theParent.IsEnd == true)
            {
                return new Result(false, "签到已结束");
            }

            theNew.LaunchType = type;
            theNew.MemberID = theMember.AutoID;
            //theNew.CourseInfoID = theCourse.AutoID;

            if (theParent != null)
                theNew.ParentID = theParent.AutoID;

            theNew.X = x;
            theNew.Y = y;


            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theNew.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@LaunchType" , theNew.LaunchType,  E_DbType.Int , 4 ) ,
                    new ParameterTag("@MemberID" , theNew.MemberID,  E_DbType.VarChar , 100 ) ,
                    new ParameterTag("@CourseInfoID" , theNew.CourseInfoID,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CTime" , theNew.CTime   ,  E_DbType.DateTime , 8 ) ,
                    new ParameterTag("@ParentID" , theNew.ParentID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@X" , theNew.X ,  E_DbType.Float , 0 ) ,
                    new ParameterTag("@Y" , theNew.Y ,  E_DbType.Float , 0 ) 
                };

            Result rs = theNew.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK == true)
            {
                rs.Data = theNew;
                EntityBase.GetMyICache().Set(theNew.AutoID, theNew);
            }
            return rs;
        }

        /// <summary>删除</summary>
        public static Result Delete(MemberSignIn the)
        {
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) 
                };

            Result rs = the.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                rs.Data = the;
                GetMyICache().Clear(the.AutoID);
            }
            return rs;
        }

        #endregion
    }

}
