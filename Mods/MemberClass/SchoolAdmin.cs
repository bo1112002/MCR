using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCR.Mods
{
    /// <summary>学校管理员</summary>
    [Serializable]
    public class SchoolAdmin : Teacher
    {
        /// <summary>构造方法</summary>
        protected SchoolAdmin(MemberType mType)
            : base(mType)
        {

            MemberReadInfo.Evt_ReadChange += (info) =>
            {
                if (info.MemberID == this.AutoID && info.TagType == 1  )
                    _FeedbackInfo_NotRead_Count = -1;
            };
        
        }



        int _FeedbackInfo_NotRead_Count = -1;
        /// <summary>获取当前用户的反馈未读数</summary>
        public int GetFeedbackInfo_NotRead_Count()
        {
            if (_FeedbackInfo_NotRead_Count < 0)
            {
                _FeedbackInfo_NotRead_Count = FeedbackInfo.GetNotReadCount(this);
            }
            return _FeedbackInfo_NotRead_Count;
        }




        #region 静态成员

        internal static new SchoolAdmin New()
        {
            return new SchoolAdmin(MemberType.E_SchoolAdmin);
        }

        #endregion

    }
}
