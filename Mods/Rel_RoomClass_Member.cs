using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>关系类(多对多)：班级与成员(学生或老师)的关系(RALRM)</summary>
    [Serializable]
    internal class Rel_RoomClass_Member : EntityBase
    {
        #region 持久属性
        string _MemberID = string.Empty;
        /// <summary>学生或老师ID</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
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


        protected Rel_RoomClass_Member()
        {
        }


        #region============= 重写成员=========>>>

        public override Type GetTypeBase()
        {
            return typeof(Rel_RoomClass_Member);
        }

        protected override string GetPrefixName()
        {
            return "RALRM";
        }





        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.RoomClassID = reader.GetValue<string>(this, "RoomClassID");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
        }

        #endregion=============END==========<<<


        #region 静态成员
        public static readonly Rel_RoomClass_Member NONE = new Rel_RoomClass_Member();

        /// <summary>依据物理唯一标识获取对象</summary>
        public static Rel_RoomClass_Member GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            Rel_RoomClass_Member the = EntityBase.GetMyICache().Get(autoID) as Rel_RoomClass_Member;

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
                        the = new Rel_RoomClass_Member();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        public static Rel_RoomClass_Member GetByID(WX_Member theMember, RoomClass theRoomClass)
        {
            Rel_RoomClass_Member the = null;
            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@RoomClassID" , theRoomClass.AutoID  ,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = NONE.EntityMaping_Excute("GetByID2", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string id = readers[0].GetValue(0).ToString();
                    the = EntityBase.GetMyICache().Get(id) as Rel_RoomClass_Member;
                    if (the == null)
                    {
                        the = new Rel_RoomClass_Member();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                }
            });
            return the;
        }

        //=============更新操作============================
        public static Result Insert(WX_Member theMember, RoomClass theRoomClass)
        {
            Result rs = Result.NONE;

            School theSchoolA = theRoomClass.GetMySchool();
            School theSchoolB = theMember.GetSchool();
            if (theSchoolA == null || theSchoolB == null)
            {
                return new Result(false, "当前的成员没有设置所属的学校，请确认个人信息中的所属学校是否已设置");
            }
            else if (theSchoolA.AutoID != theSchoolB.AutoID)
            {
                return new Result(false, "当前的成员所属的学校与指定班级的所属学校不匹配");
            }



            Rel_RoomClass_Member the = GetByID(theMember, theRoomClass);
            if (the != null)
                return new Result(true, "对象已存在", the, 100);

            the = new Rel_RoomClass_Member();
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@RoomClassID" , theRoomClass.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CTime" , DateTime.Now ,  E_DbType.DateTime , 8 ) ,
            };

            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                the.CTime = DateTime.Now;
                the.MemberID = theMember.AutoID;
                the.RoomClassID = theRoomClass.AutoID;
                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;
            }
            return rs;
        }
        /// <summary>删除</summary>
        public static Result Delete(WX_Member theMember, RoomClass theRoomClass)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@RoomClassID" , theRoomClass.AutoID  ,  E_DbType.VarChar , 50 ) 
                                };
            rs = NONE.EntityMaping_Excute("Delete", ps);
            return rs;
        }

         /// <summary>删除</summary>
        public static Result Delete_All(RoomClass theRoomClass)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps =  { 
                new ParameterTag("@RoomClassID" , theRoomClass.AutoID  ,  E_DbType.VarChar , 50 ) 
                                };
            rs = NONE.EntityMaping_Excute("Delete_All", ps);
            return rs;
        }
        #endregion


    }
}
