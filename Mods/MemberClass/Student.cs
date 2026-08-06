using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCR.Mods
{
    /// <summary>学生的信息类</summary>
    public class Student : WX_Member
    {

        /// <summary>构造方法</summary>
        protected Student(MemberType mType) :base( mType )
        {
        }

        /// <summary>获取认证的二维码的URL(用于认证、加入班级)</summary>
        public string GetMyQR_URL()
        {
            return null;
        }



        #region 静态成员

        internal static WX_Member New()
        {
            return new Student(MemberType.E_Student);
        }

        #endregion
    }
}
