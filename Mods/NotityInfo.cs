using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>通知信息类(NOTI)</summary>
    [Serializable]
    public class NotifyInfo : EntityBase
    {
        #region 持久属性
        string _MemberID = string.Empty;
        /// <summary>成员ID(发布人,如果当前发布是一个老师则为班级通知，如果为系统管理员则为系统通知)</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }
        string _MsgID = string.Empty;
        /// <summary>通知内容ID</summary>
        public string MsgID
        {
            get { return _MsgID; }
            set { _MsgID = value; }
        }
        int _NotityType = 0;
        /// <summary>消息类别:0:系统通知(所有成员) , 1:班级通知</summary>
        public int NotityType
        {
            get { return _NotityType; }
            set { _NotityType = value; }
        }
        bool _IsDisable = false;
        /// <summary>是否禁用</summary>
        public bool IsDisable
        {
            get { return _IsDisable; }
            set { _IsDisable = value; }
        }

        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }
        #endregion


        #region============= 重写成员=========>>>

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.MsgID = reader.GetValue<string>(this, "MsgID");
            this.NotityType = reader.GetValue<int>(this, "NotityType");
            this.IsDisable = reader.GetValue<bool>(this, "IsDisable");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
        }

        /*
        public override void Serialize(IDictionary<string, object> map)
        {
            map.Add("AutoID", this.AutoID);
            map.Add("MemberID", GetMember());
            map.Add("TheMsg", GetMsgCntent());
            map.Add("NotityType", this.NotityType);
            map.Add("IsDisable", this.IsDisable);
            map.Add("CTime", this.CTime);
        }
        */

        public override Type GetTypeBase()
        {
            return typeof(NotifyInfo);
        }

        protected override string GetPrefixName()
        {
            return "NOTI";
        }
        #endregion=============END==========<<<



        /// <summary>获取当前成员</summary>
        public WX_Member GetMember()
        {
            return WX_Member.GetByID(this.MemberID);
        }

        /// <summary>是否已查看(学校管理员)</summary>
        public bool IsRead(WX_Member theMember)
        {
            return MemberReadInfo.IsRead(theMember, this);
        }

        /// <summary>获取当前消息内容对象</summary>
        public MsgContentInfo GetMsgCntent()
        {
            return MsgContentInfo.GetByID(this.MsgID);
        }

        /// <summary>当前某个成员打开当前文档的处理</summary>
        public Result OpenOfHandle(WX_Member theMember)
        {
            return MemberReadInfo.Insert(theMember, this);
        }


        #region 静态成员
        public static readonly NotifyInfo NONE = new NotifyInfo();


        /// <summary>依据物理唯一标识获取对象(如果不存在，则返回null)</summary>
        public static NotifyInfo GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            NotifyInfo the = EntityBase.GetMyICache().Get(autoID) as NotifyInfo;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 30 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new NotifyInfo();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }


        /// <summary>获取某个成员的通知信息</summary>
        public static IList<NotifyInfo> GetByMember(WX_Member theMember,int pageNo,int pageSize)
        {
            List<NotifyInfo> list = new List<NotifyInfo>();
            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theMember.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@pageNo" , pageNo,  E_DbType.Int , 4 ) ,
                new ParameterTag("@pageSize" , pageSize,  E_DbType.Int , 4 ) 
                                };
            NotifyInfo the = null;
            Result rs = NONE.EntityMaping_Excute("GetItemByMemberID", ps, (readers) =>
            {
                foreach (EntityReader reader in readers)
                {
                    string autoID = reader.GetValue("AutoID").ToString();
                    the = EntityBase.GetMyICache().Get(autoID) as NotifyInfo;
                    if (the == null)
                    {
                        the = new NotifyInfo();
                        the.ToEntity(reader);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    list.Add(the);
                }
            });
            return list;
        }


        /// <summary>获取某个成员的未读通知数</summary>
        public static int GetNotReadCount(WX_Member theMember)
        {
            int count = 0;
            ParameterTag[] ps =  { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@TagType" , 2 ,  E_DbType.Int , 4 ) //标识ID类别(0:其它(默认),1:反馈,2:通知信息)
                                 };
            Result rs = NONE.EntityMaping_Excute("GetMemberReadInfoByID", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    count = Convert.ToInt32(readers[0]);
                }
            });
            return count;
        }

        //=========更新操作===============

        /// <summary>添加</summary>
        public static Result Insert(WX_Member theMember, int notityType, MsgContentInfo contentInfo)
        {
            //先添加MsgContentInfo对象，成功后，再添加当月通知对象
            Result rs = Result.NONE;
            if (contentInfo == null)
            {
                return new Result(false, "通知不能为空");
            }
            else
            {
                rs = MsgContentInfo.Insert(contentInfo);
                if (rs.IsOK)
                {
                    NotifyInfo the = new NotifyInfo() ;
                    ParameterTag[] ps = new ParameterTag[] { 
                                        new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                                        new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                                        new ParameterTag("@MsgID" , contentInfo.AutoID ,  E_DbType.VarChar , 50 ) ,
                                        new ParameterTag("@NotityType" , notityType ,  E_DbType.Int , 4 ) ,
                                        new ParameterTag("@IsDisable" , false ,  E_DbType.Bit , 4 ) ,
                                        new ParameterTag("@CTime" , DateTime.Now ,  E_DbType.DateTime , 50 ) ,
                                        };
                    rs = the.EntityMaping_Excute("Insert", ps);
                    if (rs.IsOK)
                    {
                        the.MemberID = theMember.AutoID;
                        the.MsgID = contentInfo.AutoID;
                        the.NotityType = notityType;
                        the.IsDisable = false;
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                        rs.Data = the;
                    }
                    return rs;
                }
            }


            return rs;
        }

        /// <summary>删除反馈信息</summary>
        public static Result Delete(FeedbackInfo info)
        {
            //同时还要删除关联的MsgContentInfo消息记录
            return Result.NONE;
        }




        #endregion

    }

    /// <summary>通知的消息内容类(MSGC)</summary>
     [Serializable]
    public class MsgContentInfo : EntityBase
    {
        #region 持久属性
        string _NotifyID = string.Empty;
        /// <summary>所属的通知ID</summary>
        public string NotifyID
        {
            get { return _NotifyID; }
            set { _NotifyID = value; }
        }
        int _ContentType = 0;
        /// <summary>消息内容类型：0:文本格式,1:文档(投票),2:文档(通知),3:文档(讨论)</summary>
        public int ContentType
        {
            get { return _ContentType; }
            set { _ContentType = value; }
        }
        string _Caption = string.Empty;
        /// <summary>标题</summary>
        public string Caption
        {
            get { return _Caption; }
            set { _Caption = value; }
        }
        string _Content = string.Empty;
        /// <summary>消息文件内容(可选)</summary>
        public string Content
        {
            get { return _Content; }
            set { _Content = value; }
        }
        string _DocumentID = string.Empty;
        /// <summary>文档ID(可选)</summary>
        public string DocumentID
        {
            get { return _DocumentID; }
            set { _DocumentID = value; }
        }

        //========================

        #endregion

        #region============= 重写成员=========>>>

        public override Type GetTypeBase()
        {
            return typeof(MsgContentInfo);
        }

        protected override string GetPrefixName()
        {
            return "MSGC";
        }
        

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.NotifyID = reader.GetValue<string>(this, "NotifyID");
            this.ContentType = reader.GetValue<int>(this, "ContentType");
            this.Caption = reader.GetValue<string>(this, "Caption");
            this.Content = reader.GetValue<string>(this, "Content");
            this.DocumentID = reader.GetValue<string>(this, "DocumentID");
        }

        #endregion=============END==========<<<
        /*
       public override void Serialize(IDictionary<string, object> map)
       {
           map.Add("AutoID", this.AutoID);
           map.Add("NotifyID", this.NotifyID);
           map.Add("ContentType", this.ContentType);
           map.Add("Caption", this.Caption);
           map.Add("Content", this.Content);
           map.Add("DocumentID", this.DocumentID);
       }
        */
        /// <summary>是否文档消息</summary>
        public bool IsDocument
        {
            get
            {
                return (string.IsNullOrEmpty(this.DocumentID) == false);
            }
        }


        #region 静态成员
        public static readonly MsgContentInfo NONE = new MsgContentInfo();
        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static MsgContentInfo GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            MsgContentInfo the = EntityBase.GetMyICache().Get(autoID) as MsgContentInfo;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 30 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new MsgContentInfo();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }


        //==========更新操作===========

        /// <summary>添加(记录登录信息)</summary>
        public static Result Insert(NotifyInfo theNotify, int contentType, string caption, string content, SourceDocument doc)
        {
            if (doc != null)
            { }
            else if (content != null)
            { }
            else
            {
                return new Result(false, "操作终止：缺少消息的内容");
            }
            return Result.NONE;
        }

        /// <summary>
        /// 发布通知
        /// </summary>
        public static Result Insert(MsgContentInfo contentInfo)
        {
            Result rs = Result.NONE;
            if (contentInfo == null)
            {
                return new Result(false, "通知不能为空");
            }
            if (string.IsNullOrEmpty(contentInfo.NotifyID))
            {
                return new Result(false, "通知人不能为空");
            }
            if (string.IsNullOrEmpty(contentInfo.Caption))
            {
                return new Result(false, "标题不能为空");
            }
            if (string.IsNullOrEmpty(contentInfo.Content))
            {
                return new Result(false, "内容不能为空");
            }
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , contentInfo.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@NotifyID" , contentInfo.NotifyID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@ContentType" , contentInfo.ContentType ,  E_DbType.Int , 4 ) ,
                    new ParameterTag("@Caption" , contentInfo.Caption ,  E_DbType.VarChar , 100 ) ,
                    new ParameterTag("@Content" , contentInfo.Content ,  E_DbType.VarChar , 5000 ) ,
                    new ParameterTag("@DocumentID" , contentInfo.DocumentID ,  E_DbType.VarChar , 50 ) ,
            };
            rs = contentInfo.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Set(contentInfo.AutoID, contentInfo);
                rs.Data = contentInfo;
            }
            return rs;
        }


        /// <summary>删除(管理员权限)</summary>
        public static Result Delete(WX_Member theMember, MsgContentInfo contentInfo)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , contentInfo.AutoID ,  E_DbType.VarChar , 50 ) ,
            };
            rs = contentInfo.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear(contentInfo.AutoID);
            }
            return rs;
        }
        #endregion


    }

    /*
    /// <summary>通知阅读信息类</summary>
    public class NotifyInfo_Read
    {
        string _NotifyID = string.Empty; //所属的通知ID
        string _MemberID = string.Empty; //成员ID(阅读人)

        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }
    }
     * 
     * */

}
