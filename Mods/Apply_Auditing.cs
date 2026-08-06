using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;

namespace MCR.Mods
{
    /// <summary>申请审核信息</summary>
    public class Apply_Auditing  
    {
        #region 持久属性
        string _ApplyMemberID = string.Empty;
        /// <summary>申请人</summary>
        public string ApplyMemberID
        {
            get { return _ApplyMemberID; }
            set { _ApplyMemberID = value; }
        }

        DateTime _ApplyTime = PublicMethod.NONE_DateTime;
        /// <summary>申请时间</summary>
        public DateTime ApplyTime
        {
            get { return _ApplyTime; }
            set { _ApplyTime = value; }
        }

        int _ApplyType = -1;
        /// <summary>申请类型</summary>
        public int ApplyType
        {
            get { return _ApplyType; }
            set { _ApplyType = value; }
        }

        string _ApplyRemark = string.Empty;
        /// <summary>申请事由</summary>
        public string ApplyRemark
        {
            get { return _ApplyRemark; }
            set { _ApplyRemark = value; }
        }


        int _AuditingTag = -1;
        /// <summary>是否已审核(-1:待审核，0:未通过，1:已通过，)</summary>
        public int AuditingTag
        {
            get { return _AuditingTag; }
            set { _AuditingTag = value; }
        }


        string _AuditingMemberID = string.Empty;
        /// <summary>审核人</summary>
        public string AuditingMemberID
        {
            get { return _AuditingMemberID; }
            set { _AuditingMemberID = value; }
        }


        string _AuditingRemark = string.Empty;
        /// <summary>审核回复</summary>
        public string AuditingRemark
        {
            get { return _AuditingRemark; }
            set { _AuditingRemark = value; }
        }


        DateTime _Auditing_Time = PublicMethod.NONE_DateTime;
        /// <summary>审核时间</summary>
        public DateTime Auditing_Time
        {
            get { return _Auditing_Time; }
            set { _Auditing_Time = value; }
        }

        #endregion


        protected Apply_Auditing()
        {
        }

    }
}
