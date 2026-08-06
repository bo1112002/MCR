using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;

namespace MCR.Mods
{
    /// <summary>系统管理员</summary>
    public class SysAdmin : SchoolAdmin
    {
        /// <summary>构造方法</summary>
        protected SysAdmin(MemberType mType)
            : base(mType)
        { }





        #region 静态成员

        internal static new WX_Member New()
        {
            return new SysAdmin(MemberType.E_SysAdmin);
        }


        /// <summary>
        /// 获取管理员的页面
        /// </summary>
        public static IList<KeyValueClass> GetPageList(string key)
        {
            return KeyValueClass.Map_KVs[key].Childs;
        }

        #endregion

    }
}
