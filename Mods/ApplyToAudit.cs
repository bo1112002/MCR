using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>老师角色的申请审核(AAUDIT)</summary>
    /// <remarks>
    /// 两种认证方式：
    /// 1.通过主动申请,由学校管理员通过
    /// 2.由学校管理员主动认证(由管理员扫描该老师二维码直接通过审核)
    /// </remarks>
    [Serializable]
    public class ApplyToAudit :EntityBase
    {
        #region 持久属性
        string _MemberID = string.Empty; 
        /// <summary>申请成员ID</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }
        string _SchoolAdminID = string.Empty; 
        /// <summary>审核成员ID</summary>
        public string SchoolAdminID
        {
            get { return _SchoolAdminID; }
            set { _SchoolAdminID = value; }
        }
        int _IsPass = -1; 
        /// <summary>是否通过审核流程标识：-1:等待审核, 0:不通过, 1:通过(变更后同时需要修改Member的MType值)</summary>
        public int IsPass
        {
            get { return _IsPass; }
            set { _IsPass = value; }
        }
        string _Remark_Apply = string.Empty; 
        /// <summary>申请备注</summary>
        public string Remark_Apply
        {
            get { return _Remark_Apply; }
            set { _Remark_Apply = value; }
        }
        string _Remark_Audit = string.Empty; 
        /// <summary>审核备注</summary>
        public string Remark_Audit
        {
            get { return _Remark_Audit; }
            set { _Remark_Audit = value; }
        }
        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        } 
        #endregion


        protected ApplyToAudit()
        {}

        #region============= 重写成员=========>>>
        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.SchoolAdminID = reader.GetValue<string>(this, "SchoolAdminID");
            this.IsPass = reader.GetValue<int>(this, "IsPass");
            this.Remark_Apply = reader.GetValue<string>(this, "Remark_Apply");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
        }

        public override Type GetTypeBase()
        {
            return typeof(ApplyToAudit);
        }

        protected override string GetPrefixName()
        {
            return "AAUDIT";
        }
        #endregion=============END==========<<<


        WX_Member _MyMember = null;
        /// <summary>获取当前成员对象(老师对象)</summary>
        public WX_Member GetMember()
        {
            if (_MyMember == null)
            {
                _MyMember = WX_Member.GetByID(this.MemberID);
            }
            return _MyMember;
        }
        

        /// <summary>获取当前成员对象(管理对象)</summary>
        public WX_Member GetAdmin()
        {
            return WX_Member.GetByID(this.MemberID) ;
        }



        #region 静态成员
        public static readonly ApplyToAudit NONE = new ApplyToAudit();

        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static ApplyToAudit GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            ApplyToAudit the = EntityBase.GetMyICache().Get(autoID) as ApplyToAudit;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 30 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new ApplyToAudit();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }


        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static ApplyToAudit GetByID(WX_Member theTeacher)
        {
            if (theTeacher == null)
            {
                return null;
            }
            ApplyToAudit the = null;

            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theTeacher.AutoID ,  E_DbType.VarChar , 30 ) 
                                };
            Result rs = NONE.EntityMaping_Excute("GetByID2", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    the = EntityBase.GetMyICache().Get(readers[0].ToString()) as ApplyToAudit;
                    if (the != null)
                    {
                        the = new ApplyToAudit();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                        
                }
            });
            return the;
        }



        //==========更新操作===========

        /// <summary>添加(提交老师角色申请,由管理审核)</summary>
        public static Result Insert_SubmitApply( WX_Member theTeacher, string remarkApply )
        {
            Result rs = Result.NONE;
            ApplyToAudit theApply = new ApplyToAudit(); //AutoID属性会在new 的时候就会被赋值

            if (theTeacher.MType <= MemberType.E_Student)
                return new Result(false, "操作终止：当前申请成员不符合申请条件");

           ApplyToAudit the =  GetByID(theTeacher);
           if (the != null)
               return new Result(false, "操作终止：当前成员已申请，不可重复申请");

           if (string.IsNullOrEmpty(remarkApply.Trim()) == true)
               return new Result(false, "申请备注不能为空");

           ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theApply.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@SchoolAdminID" , theTeacher.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@IsPass" , -1 ,  E_DbType.Int , 4 ) ,
                    new ParameterTag("@Remark_Apply" ,remarkApply ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CTime" , DateTime.Now ,  E_DbType.DateTime , 50 ) 
            };
           rs = theApply.EntityMaping_Excute("Insert", ps);
           if (rs.IsOK)
           {
               theApply.SchoolAdminID = theTeacher.AutoID;
               theApply.CTime = DateTime.Now;
               theApply.IsPass = -1;
               theApply.Remark_Apply = remarkApply;
               EntityBase.GetMyICache().Set(theApply.AutoID, theApply);
               rs.Data = theApply;
           }
           return rs;
        }


        /// <summary>添加(由管理主动审核通过某个成员为老师角色)</summary>
        public static Result Insert_Pass(WX_Member theTeacher, string remarkApply)
        {
            Result rs = Result.NONE;
            ApplyToAudit theApply = new ApplyToAudit(); //AutoID属性会在new 的时候就会被赋值

            if (theTeacher.MType <= MemberType.E_Student)
                return new Result(false, "操作终止：当前申请成员不符合申请条件");

            if (string.IsNullOrEmpty(remarkApply.Trim()) == true)
                return new Result(false, "申请备注不能为空");

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theApply.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@SchoolAdminID" , theTeacher.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@IsPass" , 1 ,  E_DbType.Int , 4 ) ,
                    new ParameterTag("@Remark_Apply" ,remarkApply ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CTime" , DateTime.Now ,  E_DbType.DateTime , 50 ) 
            };
            rs = theApply.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                theApply.SchoolAdminID = theTeacher.AutoID;
                theApply.CTime = DateTime.Now;
                theApply.IsPass = 1;
                theApply.Remark_Apply = remarkApply;
                EntityBase.GetMyICache().Set(theApply.AutoID, theApply);
                rs.Data = theApply;
            }
            return rs;
        }



        /// <summary>删除(管理员权限)</summary>
        public static Result Delete(ApplyToAudit the)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                };
            if (the.IsPass == 1)
            {
                rs = the.EntityMaping_Excute("ChangeRoleByID", ps);//如果已审核通过(==1)，删除后，则指定的老师角色要变成学生
            }
            else
            {
                rs = the.EntityMaping_Excute("Delete", ps);//其它情况下直接删除记录
            }
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear(the.AutoID);
            }
            return rs;
        }

        #endregion



    }
}
