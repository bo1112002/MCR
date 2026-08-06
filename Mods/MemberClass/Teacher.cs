using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;

namespace MCR.Mods
{

    /// <summary>老师的信息类</summary>
    [Serializable]
    public class Teacher : Student
    {
        /// <summary>构造方法</summary>
        protected Teacher(MemberType mType)
            : base(mType)
        {
        }


        AdminLogin _MyAdminLogin = null;
        /// <summary>获取当前成员的登录信息</summary>
        public AdminLogin GetAdminLogin()
        {
            if(_MyAdminLogin == null)
            {
                _MyAdminLogin = AdminLogin.GetByID(this);
            }
            return _MyAdminLogin;
        }



        #region 静态成员

        internal static new WX_Member New()
        {
            return new Teacher(MemberType.E_Teacher);
        }



        /// <summary>获取当前成员的登录信息</summary>
        public static Result GetAdminLogin( string loginName , string pwd )
        {
            Result rs = Result.NONE;
                 

            return rs;
        }


        #endregion

    }
}
