using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>成员浏览信息(关系)类(MRead)</summary>
    [Serializable]
    public class MemberReadInfo : EntityBase
    {
        #region 持久属性
        string _MemberID = string.Empty;
        /// <summary>成员ID</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }
        string _TagID = string.Empty;
        /// <summary>业务对象标识ID</summary>
        public string TagID
        {
            get { return _TagID; }
            set { _TagID = value; }
        }
        int _TagType = 0;
        /// <summary>标识ID类别(0:其它(默认),1:反馈,2:通知信息,3:文档,4:课程)</summary>
        public int TagType
        {
            get { return _TagType; }
            set { _TagType = value; }
        }
        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }
        #endregion

        protected MemberReadInfo() { }


        #region============= 重写成员=========>>>

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.TagID = reader.GetValue<string>(this, "TagID");
            this.TagType = reader.GetValue<int>(this, "TagType");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
        }

        public override Type GetTypeBase()
        {
            return typeof(MemberReadInfo);
        }

        protected override string GetPrefixName()
        {
            return "MRead";
        }
        #endregion=============END==========<<<



        /// <summary>获取当前成员</summary>
        public WX_Member GetMember()
        {
            return WX_Member.GetByID(this.MemberID);
        }

        /// <summary>是否已查看</summary>
        public bool IsRead(WX_Member theMember)
        {
            return MemberReadInfo.IsRead(theMember, this);
        }



        #region 静态成员
        public static readonly MemberReadInfo NONE = new MemberReadInfo();

        /// <summary>依据物理唯一标识获取对象(如果不存在，则返回null)</summary>
        public static MemberReadInfo GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            MemberReadInfo the = EntityBase.GetMyICache().Get(autoID) as MemberReadInfo;
            if (the == null)
            {
                ParameterTag[] ps =  
                { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 50 ) 
                };
                Result rs = MemberReadInfo.NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new MemberReadInfo();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }
        /// <summary>依据当前成员及业务对象返回相应的查看信息(不存在则返回null)</summary>
        public static MemberReadInfo GetByID(WX_Member theMember, EntityBase theEntity)
        {
            MemberReadInfo the = null ;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@TagID" , theEntity.AutoID ,  E_DbType.VarChar , 50 ) 
            };
            Result rs = MemberReadInfo.NONE.EntityMaping_Excute("GetByID2", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string id = readers[0].GetValue(0).ToString()  ;
                    the = MemberReadInfo.GetByID(id);
                }
            });
            return the;
        }

        /// <summary>是否已读(如果找不到相应的信息，则返回false)</summary>
        public static bool IsRead(WX_Member theMember, EntityBase theEntity)
        {
            MemberReadInfo the = GetByID( theMember ,  theEntity) ;
            return  ( the != null ) ;
        }


        /// <summary>获取浏览数</summary>
        public static int GetCountByMemberAndTagID(WX_Member theMember, EntityBase theEntity)
        {
            int rsCount = 0;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@TagID" , theEntity.AutoID ,  E_DbType.VarChar , 50 ) 
            };
            Result rs = MemberReadInfo.NONE.EntityMaping_Excute("GetCountByMemberAndTagID", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    rsCount = Convert.ToInt32(readers[0].GetValue(0));
                }
            });
            return rsCount;
        }

        //=========更新操作===============

        /// <summary>已读的事件</summary>
        public static event Action<MemberReadInfo> Evt_ReadChange;

        /// <summary>添加</summary>
        public static Result Insert(WX_Member theMember, EntityBase theEntity)
        {
            //先检查对象是否已存在,如果不存在则进行添加操作
            MemberReadInfo info = GetByID(theMember, theEntity);
            if (info != null)
            {
                return new Result(true, string.Empty, info);
            }
            else
            {
                int tagType = 0;
                if (theEntity is FeedbackInfo)
                    tagType = 1;
                else if (theEntity is NotifyInfo)
                    tagType = 2;
                else if (theEntity is SourceDocument)
                    tagType = 3;


                //添加处理
                info = new MemberReadInfo();
                info.MemberID = theMember.AutoID;
                info.TagID = theEntity.AutoID;
                info.TagType = tagType;


                ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , info.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , info.MemberID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@TagID" , info.TagID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@TagType" , info.TagType  ,  E_DbType.Int , 8 ) ,
                    new ParameterTag("@CTime" , info.CTime ,  E_DbType.DateTime, 8 ) 
                };

                Result rs = info.EntityMaping_Excute("Insert", ps);
                if (rs.IsOK)
                {
                    EntityBase.GetMyICache().Set(info.AutoID, info);
                    rs.Data =info;
                    
                    if (Evt_ReadChange != null)
                        Evt_ReadChange(info);

                }
                return rs;
            }
        }
        /// <summary>删除</summary>
        public static Result Delete(MemberReadInfo info)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , info.AutoID ,  E_DbType.VarChar , 50 ) ,
                };
            rs = info.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear(info.AutoID);

                if (Evt_ReadChange != null)
                    Evt_ReadChange(info);
            }
            return rs;
        }


        #endregion
    }
}
