using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>系统反馈信息类(FEE)</summary>
    [Serializable]
    public class FeedbackInfo : EntityViewControl 
    {

        #region 持久属性

        string _SchoolID = string.Empty;
        /// <summary>所属的学校ID</summary>
        public string SchoolID
        {
            get { return _SchoolID; }
            set { _SchoolID = value; }
        }

        string _MemberID = string.Empty;
        /// <summary>成员ID(反馈人)</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }
        string _Content = string.Empty;
        /// <summary>反馈内容</summary>
        public string Content
        {
            get { return _Content; }
            set { _Content = value; }
        }
        string _Remark = string.Empty;
        /// <summary>备注(学校管理员)</summary>
        public string Remark
        {
            get { return _Remark; }
            set { _Remark = value; }
        }

        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }
        
        #endregion

        protected FeedbackInfo()
        { 


        }

        #region============= 重写成员=========>>>

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.SchoolID = reader.GetValue<string>(this, "SchoolID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.Content = reader.GetValue<string>(this, "Content");
            this.Remark = reader.GetValue<string>(this, "Remark");
        }

        public override void Serialize(IDictionary<string, object> map)
        {
            map.Add("AutoID", this.AutoID);
            map.Add("SchoolID", this.SchoolID);
            map.Add("MemberID", this.MemberID);
            map.Add("Content", this.Content);
            map.Add("Remark", this.Remark);
        }

        public override Type GetTypeBase()
        {
            return typeof(FeedbackInfo);
        }

        protected override string GetPrefixName()
        {
            return "FEE";
        }
        #endregion=============END==========<<<
       


        /// <summary>获取当前成员</summary>
        public WX_Member GetMember()
        {
            return WX_Member.GetByID(this.MemberID);
        }

        /// <summary>是否已查看(学校管理员)</summary>
        public bool IsRead(WX_Member theAdmin)
        {
            return  MemberReadInfo.IsRead(theAdmin, this);
        }   
            

        //=========更新操作===============

        #region 更新备注(学校管理员)
        /// <summary>更新备注(学校管理员)</summary>
        public Result Update(string remark)
        {
            Result rs = Result.NONE;

            if (string.IsNullOrEmpty(remark.Trim()) == true)
                return new Result(false, "操作终止：备注信息不能为空");

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Remark" , remark ,  E_DbType.VarChar , 500 ) ,
            };
            rs = this.EntityMaping_Excute("UpdateRemark", ps);
            if (rs.IsOK)
            {
                this.Remark = remark;
            }
            return rs;
        }

        /// <summary>标识当前管理员已查看过当前信息</summary>
        public Result Read( WX_Member theAdmin )
        {
            if (this.IsRead(theAdmin) == true)
            {
                return Result.OK;
            }
            else if (theAdmin.MType < MemberType.E_SchoolAdmin)
            {
                return new Result(false, "操作终止：当前用户权限无效");
            }
            else
            {
                Result rs = MemberReadInfo.Insert(theAdmin, this);
                return rs;
            }
            
        }

        #endregion



        /// <summary>当前某个成员打开当前文档的处理</summary>
        public Result OpenOfHandle(WX_Member theMember)
        {
            return MemberReadInfo.Insert(theMember, this);
        }



        #region 静态成员
        public static readonly FeedbackInfo NONE = new FeedbackInfo();

        //static readonly Dictionary<string, FeedbackInfo> _Feed_Cache = new Dictionary<string, FeedbackInfo>();//缓存

        #region 依据物理唯一标识获取对象(如果不存在，则返回null)
        /// <summary>依据物理唯一标识获取对象(如果不存在，则返回null)</summary>
        public static FeedbackInfo GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            FeedbackInfo the =  EntityBase.GetMyICache().Get(autoID)  as FeedbackInfo ;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 50 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new FeedbackInfo();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        } 
        #endregion

        #region 获取某个学校的反馈信息
        /// <summary>获取某个学校的反馈信息</summary>
        public static IList<FeedbackInfo> GetBySchool(School theSchool)
        {
            List<FeedbackInfo> list = new List<FeedbackInfo>();
            ParameterTag[] ps =  { 
                new ParameterTag("@SchoolID" , theSchool.AutoID,  E_DbType.VarChar , 50 ) 
                                };
            FeedbackInfo the = null;
            Result rs = NONE.EntityMaping_Excute("GetItemByScoolID", ps, (readers) =>
            {
                foreach (EntityReader reader in readers)
                {
                    string autoID = reader.GetValue("AutoID").ToString();
                    the = EntityBase.GetMyICache().Get(autoID) as FeedbackInfo;
                    if (the == null)
                    {
                        the = new FeedbackInfo();
                        the.ToEntity(reader);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    list.Add(the);
                }
            });
            return list;
        } 
        #endregion

        #region 获取某个成员的反馈信息
        /// <summary>获取某个成员的反馈信息</summary>
        public static IList<FeedbackInfo> GetByMember(WX_Member theMember)
        {
            List<FeedbackInfo> list = new List<FeedbackInfo>();
            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = FeedbackInfo.NONE.EntityMaping_Excute("GetItemByMemberID", ps, (reader) =>
            {
                EntityBase.AddToList<FeedbackInfo>(list, reader, (r) => new FeedbackInfo());
            });
            return list;
        }  
        #endregion


        /// <summary>获取某个管理员的未读数</summary>
        public static int GetNotReadCount(WX_Member theAdmin)
        {
            int count = 0;
            ParameterTag[] ps =  { 
                    new ParameterTag("@MemberID" , theAdmin.AutoID ,  E_DbType.VarChar , 30 ) ,
                    new ParameterTag("@TagType" , 1 ,  E_DbType.Int , 4 ) //标识ID类别(0:其它(默认),1:反馈,2:通知信息)
                                 };
            Result rs = NONE.EntityMaping_Excute("GetMemberReadInfoByID", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    count = Convert.ToInt32(readers[0]);
                }
            });
            return 0;
        }

        //=========更新操作===============

        #region 添加反馈信息
        /// <summary>添加反馈信息</summary>
        public static Result Insert(WX_Member theMember,  string content)
        {
            Result rs = Result.NONE;

            if( string.IsNullOrEmpty( content ) )
            {
                return new Result( false , "反馈内容不能为空" ) ;
            }

            if( theMember.GetSchool() == null )
                return new Result(false , "请选择一个学校，才可再提交反馈信息");


            FeedbackInfo info = new FeedbackInfo() ;
            info.MemberID = theMember.AutoID ;
            info.Content = content ;
            info.SchoolID = theMember.SchoolID ;


            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , info.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@SchoolID" , info.SchoolID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , info.MemberID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Content" , info.Content ,  E_DbType.VarChar , 1000 ) 
            };
            rs = FeedbackInfo.NONE.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Set(info.AutoID, info);
                rs.Data = info; 
            }
            return rs;
        } 
        #endregion

        #region 删除反馈信息
        /// <summary>删除反馈信息</summary>
        public static Result Delete(FeedbackInfo info)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , info.AutoID ,  E_DbType.VarChar , 50 ) ,
            };
            rs = info.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear(info.AutoID);
            }
            return rs;
        } 
        #endregion
        
        

       
        #endregion  

    }
}
