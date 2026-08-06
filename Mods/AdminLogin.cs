using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{

    /// <summary>记录指定的用户的登录信息(ADML)</summary>
    [Serializable]
    public class AdminLogin : EntityBase
    {
        #region 持久属性
        /// <summary>所属的成员ID(只能是老师及管理员)</summary>
        public string MemberID { get; set; }
        /// <summary>登录名</summary>
        public string LoginName { get; set; }
        /// <summary>登录密码</summary>
        public string Password { get; set; }

        private DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }

        #endregion

        protected AdminLogin() { }

        #region============= 重写成员=========>>>

        public override Type GetTypeBase()
        {
            return typeof(AdminLogin);
        }

        protected override string GetPrefixName()
        {
            return "ADML";
        }
        


        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
            this.LoginName = reader.GetValue<string>(this, "LoginName");
            this.Password = reader.GetValue<string>(this, "Password");
        }

        #endregion=============END==========<<<

        WX_Member _MyMember = null;
        /// <summary>获取当前成员对象</summary>
        public WX_Member GetMember()
        {
            if (_MyMember == null)
            {
                _MyMember = WX_Member.GetByID(this.MemberID);
            }
            return _MyMember;
        }

        //============更新操作====================

        /// <summary>修改密码(pwd：为明码，该方法会对其进行MD5加密后再保存)</summary>
        public Result Update_PWD(string pwd)
        {
            pwd = pwd.Trim();
            if (string.IsNullOrEmpty(pwd) == true || pwd.Length < 6 || pwd.Length > 20)
            {
                return new Result(false, "操作终止：密码设置无效,密码不能为空，并且密码个数应在6至20个字符以内");
            }

            string strMD5 = PublicMethod.GetMd5Hash(pwd);
            ParameterTag[] ps =  
             { 
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@Password" , strMD5 ,  E_DbType.VarChar , 50 ) 
             };

            Result rs = this.EntityMaping_Excute("Update_PWD", ps);
            if (rs.IsOK == true)
            {
                this.Password = strMD5;
            }
            return rs;
        }


        #region 静态成员

        /// <summary>无效对象</summary>
        public static readonly AdminLogin NONE = new AdminLogin();

        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static AdminLogin GetByID(string autoID)
        {
            AdminLogin the = EntityBase.GetMyICache().Get(autoID) as AdminLogin;
            if (the == null)
            {
                ParameterTag[] ps =  
                { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 50 ) 
                };
                Result rs = AdminLogin.NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new AdminLogin();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static AdminLogin GetByID(string loginName, string pwd_md5)
        {
            AdminLogin the = null;

            //string strMD5 = PublicMethod.GetMd5Hash(pwd);
            ParameterTag[] ps =  
            { 
                new ParameterTag("@LoginName" , loginName ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@Password" , pwd_md5 ,  E_DbType.VarChar , 50 ) 
            };
            Result rs = AdminLogin.NONE.EntityMaping_Excute("GetByID_Login", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string autoID = readers[0].GetValue(0).ToString();
                    the = EntityBase.GetMyICache().Get(autoID) as AdminLogin;
                    if (the == null)
                    {
                        the = new AdminLogin();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }

                }
            });
            return the;
        }


        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static AdminLogin GetByID(WX_Member theMember)
        {
            AdminLogin the = null;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) 
            };
            Result rs = AdminLogin.NONE.EntityMaping_Excute("GetByID_Member", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string autoID = readers[0].GetValue(0).ToString();
                    the = EntityBase.GetMyICache().Get(autoID) as AdminLogin;
                    if (the == null)
                    {
                        the = new AdminLogin();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                }
            });
            return the;
        }




        //==========更新操作===========

        /// <summary>添加(记录登录信息)</summary>
        public static Result Insert(WX_Member theMember, string loginName, string pwd)
        {
            Teacher theTeacher = theMember as Teacher;
            if (theTeacher == null )
                return new Result(false, "操作终止：当前成员不具备相应权限");
            else if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(pwd))
                return new Result(false, "操作终止：登录名及密码不能为空");
            else if (Regexs.IsPassword(pwd) == false)
                return new Result(false, "操作终止：密码要求以字母开头，长度在6~20之间，只能包含字符、数字和下划线");


            AdminLogin theNew = theTeacher.GetAdminLogin();
            if (theNew != null)
            {
                Result rs2 = AdminLogin.Delete(theNew);
                if (rs2.IsOK == false)
                    return rs2;
            }

            string strMD5 = PublicMethod.GetMd5Hash(pwd);
            theNew = new AdminLogin();
            theNew.MemberID = theMember.AutoID;
            theNew.LoginName = loginName;
            theNew.Password = strMD5;

            ParameterTag[] ps = 
            { 
                new ParameterTag("@AutoID" , theNew.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@MemberID" , theNew.MemberID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@CTime" , theNew.CTime ,  E_DbType.DateTime , 0 ) ,
                new ParameterTag("@LoginName" , theNew.LoginName  ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@Password" , theNew.Password ,  E_DbType.VarChar , 50 ) 
                
            };

            Result rs = theNew.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK == true)
            {
                rs.Data = theNew;
                EntityBase.GetMyICache().Set(theNew.AutoID, theNew);
            }
            return rs;

        }


        /// <summary>删除(管理员权限)</summary>
        public static Result Delete(AdminLogin the)
        {
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) 
                };

            Result rs = the.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                GetMyICache().Clear(the.AutoID);
            }
            return rs;
        }

        #endregion

    }
}
