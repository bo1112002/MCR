using MCR.Mods.VSTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;
using Tools.Http;

namespace MCR.Mods
{

    /// <summary>成员角色类别(学生:0 ， 老师:1， 学校管理员:2， 系统管理员:3)</summary>
    public enum MemberType
    {
        /// <summary>学生</summary>
        [EnumDescription("学生")]
        E_Student = 0,
        /// <summary>老师</summary>
        [EnumDescription("老师")]
        E_Teacher,
        /// <summary>学校管理员</summary>
        [EnumDescription("学校管理员")]
        E_SchoolAdmin,
        /// <summary>系统管理员</summary>
        [EnumDescription("系统管理员")]
        E_SysAdmin

    }



    /// <summary>微信用户信息类（MEM）</summary>
    [Serializable]
    public abstract class WX_Member : EntityBase
    {
        #region 持久属性
        /// <summary>微信ID</summary>
        public string OpenID { get; set; }
        /// <summary>昵称</summary>
        public string NickName { get; set; }

        string _HeadImgURL = string.Empty;
        /// <summary>头像的URL</summary>
        public string HeadImgURL
        {
            get 
            {
                if (string.IsNullOrEmpty(_HeadImgURL))
                {
                    return AppSettings.NONE_DOC_ImgURL;
                }
                return _HeadImgURL; 
            }
            set { _HeadImgURL = value; }
        }

        /// <summary>性别(1:男)</summary>
        public int Sex { get; set; }
        /// <summary>省份</summary>
        public string Province { get; set; }
        /// <summary>城市</summary>
        public string City { get; set; }
        /// <summary>区</summary>
        public string Country { get; set; }


        string _Name = string.Empty;
        /// <summary>真实姓名</summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_Name))
                {
                    return NickName;
                }
                else
                {
                    return _Name;
                }
            }
            set { _Name = value.Trim(); }
        }

        private bool _IsFollowed = false;
        /// <summary>是否已关注</summary>
        public bool IsFollowed
        {
            get { return _IsFollowed; }
            set { _IsFollowed = value; }
        }

        private bool _IsDisable = false;
        /// <summary>是否禁用</summary>
        public bool IsDisable
        {
            get { return _IsDisable; }
            set { _IsDisable = value; }
        }


        MemberType _MType = MemberType.E_Student;
        /// <summary>成员角色类别</summary>
        public MemberType MType
        {
            get { return _MType; }
            set { _MType = value; }
        }

        private string _InnerID = string.Empty;
        /// <summary>内部ID(学号、工作号)</summary>
        public string InnerID
        {
            get { return _InnerID; }
            set { _InnerID = value; }
        }

        string _SchoolID = string.Empty;
        /// <summary>所属的学校ID</summary>
        public string SchoolID
        {
            get { return _SchoolID; }
            set { _SchoolID = value; }
        }

        bool _IsAuth_School = false;
        /// <summary>是否被认证为该学校成员</summary>
        public bool IsAuth_School
        {
            get { return _IsAuth_School; }
            set { _IsAuth_School = value; }
        }


        private string _Email = string.Empty;
        /// <summary>电子邮箱</summary>
        public string Email
        {
            get { return _Email; }
            set { _Email = value; }
        }

        private DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }

        #endregion


        /// <summary>构造方法</summary>
        protected WX_Member(MemberType mType)
        {
            this.MType = MType;
        }

        /// <summary>性别字符串格式化</summary>
        public string SexString
        {
            get
            {
                return (this.Sex == 1 ? "男" : "女");
            }
        }
        /// <summary>获取角色名字符串</summary>
        public string MTypeString
        {
            get
            {
                return EnumDescription.GetFieldText(this.MType);
            }
        }
        /// <summary>获取角色的整数型</summary>
        public int MTypeNum
        {
            get
            {
                return (int)this.MType;
            }
        }


        /// <summary>获取当前学校名</summary>
        public string SchoolName
        {
            get
            {
                School the = GetSchool();
                if (the == null)
                    return "未知";
                return the.Name;
            }
        }



        /// <summary>获取当前成员所在学校(如果不存在则返回null)</summary>
        /// <remarks>
        /// 如果调用老师或学校管理员及相关的功能时，需要判断是否为null,如果为null需要提示填写所属学校
        /// </remarks>
        public School GetSchool()
        {
            //School the = School.GetByID(this.SchoolID);
            School the = School_QST.GetFindID_QST(this.SchoolID);
            if (the != null)
                return the;

            return School.NONE;
        }


        #region============= 重写成员=========>>>

        public override Type GetTypeBase()
        {
            return typeof(WX_Member);
        }

        protected override string GetPrefixName()
        {
            return "MEM";
        }


        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.OpenID = reader.GetValue<string>(this, "OpenID");
            this.NickName = reader.GetValue<string>(this, "NickName");
            this.Name = reader.GetValue<string>(this, "Name");
            this.HeadImgURL = reader.GetValue<string>(this, "HeadImgURL");
            this.Sex = reader.GetValue<int>(this, "Sex");
            this.Province = reader.GetValue<string>(this, "Province");
            this.City = reader.GetValue<string>(this, "City");
            this.Country = reader.GetValue<string>(this, "Country");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
            this.Email = reader.GetValue<string>(this, "Email");
            this.IsFollowed = reader.GetValue<bool>(this, "IsFollowed");
            this.IsDisable = reader.GetValue<bool>(this, "IsDisable");
            this.MType = (MemberType)reader.GetValue<int>(this, "MType");
            this.InnerID = reader.GetValue<string>(this, "InnerID");
            this.SchoolID = reader.GetValue<string>(this, "SchoolID");
            this.IsAuth_School = reader.GetValue<bool>(this, "IsAuth_School");




        }

        #endregion=============END==========<<<

        //============更新操作============
        /// <summary>修改(完善个人信息)</summary>
        public Result Update(string name, string email, string innerID)
        {
            if (Regexs.IsEmail(email) == false)
                return new Result(false, "操作终止：电子邮箱地址无效");

            ParameterTag[] ps =  
            { 
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) , 
                 new ParameterTag("@Name" , name ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@Email" , email ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@InnerID" , innerID ,  E_DbType.VarChar , 50 ) 
            };

            Result rs = this.EntityMaping_Excute("Update", ps);
            if (rs.IsOK == true)
            {
                this.Name = name;
                this.Email = email;
                this.InnerID = innerID;
            }
            return rs;
        }

        /// <summary>修改(第三方标识号)</summary>
        public Result Update_InnerID(string innerID, string schoolID)
        {
            if (string.IsNullOrWhiteSpace(innerID) == true)
                return new Result(false, "标识ID不能为空");
            
            WX_Member theM = WX_Member.GetByInnerID(innerID);
            if ( theM != null &&   theM.AutoID != this.AutoID )
                return new Result(false, "该成员已绑定过，不可重复绑定!", 101);

            ParameterTag[] ps =  
            { 
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) , 
                 new ParameterTag("@InnerID" , innerID ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@SchoolID" , schoolID ,  E_DbType.VarChar , 50 ) 
            };

            Result rs = this.EntityMaping_Excute("Update_InnerID2", ps);
            if (rs.IsOK == true)
            {
                this.InnerID = innerID;
            }
            return rs;
        }

        /// <summary>清空属性InnerID（取消绑定）</summary>
        public Result Update_InnerID_Clear()
        {
            ParameterTag[] ps =  
            { 
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) , 
                 new ParameterTag("@InnerID" , string.Empty ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@SchoolID" , string.Empty ,  E_DbType.VarChar , 50 ) 
            };

            Result rs = this.EntityMaping_Excute("Update_InnerID2", ps);
            if (rs.IsOK == true)
            {
                this.InnerID = string.Empty;
            }
            return rs;
        }


        /// <summary>成员微信信息的变更</summary>
        public Result Update_WX(string nickName, string headUrl, int sex, string city, string province, string country)
        {
            if (string.IsNullOrEmpty(nickName) == true ||
                string.IsNullOrEmpty(headUrl) == true ||
                string.IsNullOrEmpty(city) == true ||
                string.IsNullOrEmpty(nickName) == true ||
                string.IsNullOrEmpty(province) == true ||
                string.IsNullOrEmpty(country) == true)
            {
                return new Result(false, "操作终止：信息不填写不完整");
            }

            ParameterTag[] ps =  
            { 
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) , 
                 new ParameterTag("@NickName" , nickName ,  E_DbType.VarChar , 100 ) ,
                 new ParameterTag("@HeadImgURL" , headUrl ,  E_DbType.VarChar , 200 ) ,
                 new ParameterTag("@Sex" , sex ,  E_DbType.Int, 0 ) ,
                 new ParameterTag("@Province" , province ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@City" , city ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@Country" , country ,  E_DbType.VarChar , 50 ) 
            };

            Result rs = this.EntityMaping_Excute("Update_WX", ps);
            if (rs.IsOK == true)
            {
                this.NickName = nickName;
                this.HeadImgURL = headUrl;
                this.Sex = sex;
                this.Province = province;
                this.City = city;
                this.Country = Country;
            }
            return rs;
        }

        /// <summary>修改用户信息</summary>
        public Result UpdateMemberInfo(string nickName, string name, int sex, string email)
        {
            if (string.IsNullOrEmpty(nickName) == true ||
                string.IsNullOrEmpty(name) == true )
                //string.IsNullOrEmpty(email) == true   
            {
                return new Result(false, "操作终止：信息不填写不完整");
            }

            ParameterTag[] ps =
            {
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@NickName" , nickName ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@Name" , name ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@Sex" , sex ,  E_DbType.Int, 0 ) ,
                 new ParameterTag("@Email" , email ,  E_DbType.VarChar , 50 ) 
            };

            Result rs = this.EntityMaping_Excute("UpdateMemberInfo", ps);
            if (rs.IsOK == true)
            {
                this.NickName = nickName;
                this.Name = name;
                this.Sex = sex;
                this.Email = email;
            }
            return rs;
        }

        public Result UpdateMemberInfo_QST(Dictionary<string,object> dicInfo)
        {

            string nickName = this.NickName ;
            string name = dicInfo["name"].ToString() ;

            int sex = 1 ; //男
            if (dicInfo.ContainsKey("sex") == true && dicInfo["sex"].ToString().Contains("女") == true)
            {
                sex = 2 ;
            }
            string email =  dicInfo["email"].ToString() ;
            string uID =  dicInfo["id"].ToString() ;
            string schoolID = dicInfo["orgId"].ToString();

            Result rs =  Update_InnerID(uID , schoolID );
            if (rs.IsOK == true)
            {
                MemberType memberType = Convert.ToBoolean(dicInfo["teacher"]) ? MemberType.E_Teacher : MemberType.E_Student;
                rs = this.Update_MType(memberType);

                rs = this.UpdateMemberInfo(nickName, name, sex, email);
            }
            return rs;
        } 


        /// <summary>修改(设置当前成员的角色 , 并发送登录信息到邮箱)</summary>
        public Result Update_MType(MemberType mType)
        {
            if (string.IsNullOrEmpty(this.Email) == true)
            {
                return new Result(false, "操作终止：请完善你的个人信息，必须填写有效的电子邮箱地址，系统将后台登录发送该邮箱");
            }
            else if (this.MType == mType)
            {
                return Result.OK;
            }

            ParameterTag[] ps =  
            { 
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@MType" , (int)mType ,  E_DbType.Int, 0 ) 
            };

            Result rs = this.EntityMaping_Excute("Update_MType", ps);
            if (rs.IsOK == true)
            {
                this.MType = MType;
                //由于当前对象类型需要与MType匹配,所以需要重新加载对象
                GetMyICache().Clear(this.AutoID);
                WX_Member theMemberNew = GetByID(this.AutoID); //可能是学生，也可能是老师及以上角色

                if (theMemberNew is Teacher)
                {
                    char[] CODES = AppSettings.CheckCodes;
                    string pwd = CheckCodeImgString64.GenerateCheckCode(CODES, 6);

                    Result rs2 = AdminLogin.Insert(theMemberNew, this.Email, pwd);
                    if (rs.IsOK)
                    {
                        string body = "<br/><br/><b>{0},您好,你的登录信息：登录名:{1}，密码:{2}；请点击<a href='{3}'>登录</a> 修改密码 <br/>";
                        body = string.Format(body, this.NickName, this.Email, pwd, AppSettings.WebURL_Login);
                        IMailServer mser = AppSettings.Base as IMailServer;
                        Result rs3 = MyMail.Send(mser.UserName, this.NickName, this.Email, "获取登录信息", body, null, mser);
                    }
                }
            }
            return rs;
        }
        /// <summary>修改所属学校</summary>
        public Result Update_School(School theSchool)
        {
            if (theSchool.AutoID == this.SchoolID)
                return Result.OK;

            School beSchool = this.GetSchool();
            if (this.SchoolID == theSchool.AutoID)
                return Result.OK;
            else if (beSchool != null && string.IsNullOrEmpty(this.InnerID) == false)
                return new Result(false, "操作终止：学校已被绑定,不可再修改所属的学校");

            ParameterTag[] ps =  
            { 
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@SchoolID" , theSchool.AutoID ,  E_DbType.VarChar , 50 ) 
            };

            Result rs = this.EntityMaping_Excute("Update_School", ps);
            if (rs.IsOK == true)
            {
                this.SchoolID = theSchool.AutoID;
                this.Update_IsAuth_School(false);
            }
            return rs;
        }
        /// <summary>修改(是否禁用当前成员)</summary>
        public Result Update_IsDisable(bool isVal)
        {
            if (this.IsDisable == isVal)
                return Result.OK;

            ParameterTag[] ps =  
            { 
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@IsDisable" , isVal ,  E_DbType.Bit, 1 ) 
            };

            Result rs = this.EntityMaping_Excute("Update_IsDisable", ps);
            if (rs.IsOK == true)
            {
                this.IsDisable = isVal;
            }
            return rs;
        }

        /// <summary>修改(是否认证成员)</summary>
        public Result Update_IsAuth_School(bool isAuth)
        {
            if (this.IsDisable == IsDisable)
                return Result.OK;

            ParameterTag[] ps =  
            { 
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@IsAuth_School" , isAuth ,  E_DbType.Bit, 1 ) 
            };

            Result rs = this.EntityMaping_Excute("Update_IsAuth", ps);
            if (rs.IsOK == true)
            {
                this.IsAuth_School = isAuth;
            }
            return rs;
        }


        /// <summary>修改(当前成员关注微信号)</summary>
        public Result Update_IsFollowed(bool isVal)
        {
            if (this.IsDisable == isVal)
                return Result.OK;

            ParameterTag[] ps =  
            { 
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@IsFollowed" , isVal ,  E_DbType.Bit, 1 ) 
            };

            Result rs = this.EntityMaping_Excute("Update_IsFollowed", ps);
            if (rs.IsOK == true)
            {
                this.IsDisable = isVal;
            }
            return rs;
        }



        //==================================

        /// <summary>获取当前成员的课程目录信息集合</summary>
        /// <returns></returns>
        public IList<CourseInfo_QST> GetList_CourseInfo_QST()
        {
            return CourseInfo_QST.GetAllByMember(this);
        }







        #region 静态成员


        /// <summary>获取相应的成员实例对象</summary>
        internal static WX_Member New(MemberType mType)
        {
            if (mType == MemberType.E_SysAdmin)
                return SysAdmin.New();
            else if (mType == MemberType.E_SchoolAdmin)
                return SchoolAdmin.New();
            else if (mType == MemberType.E_Teacher)
                return Teacher.New();
            else  //if (mType == MemberType.E_Student)
                return Student.New();
        }
        /// <summary>无效对象</summary>
        public static readonly WX_Member NONE = Student.New();

        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static WX_Member GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
                return null;

            WX_Member the = EntityBase.GetMyICache().Get(autoID) as WX_Member;
            if (the == null)
            {
                ParameterTag[] ps =  
                { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 50 ) 
                };
                Result rs = WX_Member.NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        MemberType mType = (MemberType)Convert.ToInt32(readers[0].GetValue("MType"));
                        the = WX_Member.New(mType);
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static WX_Member GetByOpenID(string openID)
        {
            WX_Member the = null;
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@OpenID" , openID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = WX_Member.NONE.EntityMaping_Excute("GetByOpenID", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string autoID = readers[0].GetValue(0).ToString();
                    the = GetByID(autoID);
                    if (the == null)
                    {
                        MemberType mType = readers[0].GetValue<MemberType>(WX_Member.NONE, "MType");
                        the = WX_Member.New(mType);
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                }
            });
            return the;
        }
        
        /// <summary>依据第三方标识号获取某成员对象</summary>
        public static WX_Member GetByInnerID(string innerID)
        {
            WX_Member the = null;
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@InnerID" , innerID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = WX_Member.NONE.EntityMaping_Excute("GetByInnerID", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string autoID = readers[0].GetValue(0).ToString();
                    the = GetByID(autoID);
                }
            });
            return the;
        }


        /// <summary>获取某个学校的成员</summary>
        public static IList<WX_Member> GetListBySchool(School theSchool, MemberType mType)
        {
            List<WX_Member> list = new List<WX_Member>();
            if (mType == MemberType.E_SysAdmin)
                return list;

            ParameterTag[] ps =  
                { 
                    new ParameterTag("@SchoolID" , theSchool.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MType" , (int)mType ,  E_DbType.Int , 0 ) 
                };
            Result rs = WX_Member.NONE.EntityMaping_Excute("GetList_Shhool", ps, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string autoID = r.GetValue(0).ToString();
                    WX_Member the = GetByID(autoID);
                    if (the == null)
                    {
                        the = WX_Member.New(mType);
                        the.ToEntity(r);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    if (the != null)
                    {
                        list.Add(the);
                    }
                }
            });
            return list;
        }

        /// <summary>获取所有的系统管理员</summary>
        public static IList<WX_Member> GetListBySysAdmin()
        {
            List<WX_Member> list = new List<WX_Member>();

            Result rs = WX_Member.NONE.EntityMaping_Excute("GetList_SysAdmin", null, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string autoID = r.GetValue(0).ToString();
                    WX_Member the = GetByID(autoID);
                    if (the == null)
                    {
                        the = WX_Member.New(MemberType.E_SysAdmin);
                        the.ToEntity(r);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    if (the != null)
                    {
                        list.Add(the);
                    }
                }
            });
            return list;
        }


        /// <summary>多条件分布查询</summary>
        public static IList<WX_Member> GetList_More(int? mType, School theSchool, string name, string email, string lastID, int num)
        {
            List<WX_Member> list = new List<WX_Member>();

            string strMType = "%";
            string strSchoolID = "%";
            string strName = "%";
            string strEmail = "%";

            if (mType != null) strMType = mType.Value.ToString();
            if (theSchool != null) strSchoolID = theSchool.AutoID;
            if (string.IsNullOrEmpty(name) == false) strName = "%" + name + "%";
            if (string.IsNullOrEmpty(email) == false) strEmail = "%" + email + "%";

            ParameterTag[] ps =  
            { 
                new ParameterTag("@MType" , strMType ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@SchoolID" ,strSchoolID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@Name" , strName ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@Email" , strEmail ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@LastID" ,lastID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@N" , num ,  E_DbType.Int, 1 ) 
            };


            Result rs = WX_Member.NONE.EntityMaping_Excute("GetList_More", ps, (readers) =>
            {
                EntityBase.AddToList<WX_Member>(list, readers,
                    (r) =>
                    {
                        MemberType mType2 = (MemberType)Convert.ToInt32(r.GetValue("MType"));
                        return WX_Member.New(mType2);
                    });
            });
            return list;
        }

        /// <summary>获取当前班级下的指定的类型成员集合</summary>
        public static IList<WX_Member> GetListByRoomClass(RoomClass theRoom, MemberType mType)
        {
            List<WX_Member> list = new List<WX_Member>();
            ParameterTag[] ps =  
            { 
                    new ParameterTag("@RoomClassID" , theRoom.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MType" , (int)mType ,  E_DbType.Int , 4 ) 
            };
            Result rs = WX_Member.NONE.EntityMaping_Excute("GetListByRoomClass", ps, (readers) =>
            {
                EntityBase.AddToList<WX_Member>(list, readers, (r) =>
                {
                    MemberType mType2 = (MemberType)Convert.ToInt32(r.GetValue("MType"));
                    return WX_Member.New(mType2);
                });
            });
            return list;
        }

        /// <summary>获取当前班级下的所有成员</summary>
        public static IList<WX_Member> GetListByRoomClass_ALL(RoomClass theRoom)
        {
            List<WX_Member> listRS = new List<WX_Member>();
            IList<WX_Member> list = null;

            list = GetStudents(theRoom);
            listRS.AddRange(list);

            list = GetTeachers(theRoom);
            listRS.AddRange(list);

            return listRS;
        }

        /// <summary>获取当前班级下的学生</summary>
        public static IList<WX_Member> GetStudents(RoomClass theRoom)
        {
            IList<WX_Member> list = GetListByRoomClass(theRoom, MemberType.E_Student);
            return list;
        }



        /// <summary>获取当前班级下的老师</summary>
        public static IList<WX_Member> GetTeachers(RoomClass theRoom)
        {
            IList<WX_Member> list = GetListByRoomClass(theRoom, MemberType.E_Teacher);
            return list;
        }


        /// <summary>获取当前课程下参与答题的学生总人数</summary>
        public static int GetCount_SumMember(CourseInfo theCourseInfo)
        {
            int rsCount = 0;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@CourseInfoID" , theCourseInfo.AutoID ,  E_DbType.VarChar , 50 ) 
            };


            Result rs = WX_Member.NONE.EntityMaping_Excute("GetCount_SumMember", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    rsCount = Convert.ToInt32(readers[0].GetValue(0));
                }
            });
            return rsCount;
        }
        /// <summary>获取当前课程下参与答题学生信息集合</summary>
        public static IList<WX_Member> GetList_MemberByQuestionResult(CourseInfo theCourseInfo)
        {
            List<WX_Member> list = new List<WX_Member>();
            ParameterTag[] ps =  
            { 
                new ParameterTag("@CourseInfoID" , theCourseInfo.AutoID ,  E_DbType.VarChar , 50 ) 
            };

            Result rs = WX_Member.NONE.EntityMaping_Excute("GetList_MemberByQuestionResult", ps, (readers) =>
            {
                EntityBase.AddToList<WX_Member>(list, readers, (r) =>
                {
                    MemberType mType2 = (MemberType)Convert.ToInt32(r.GetValue("MType"));
                    return WX_Member.New(mType2);
                });
            });
            return list;
        }


        /// <summary>获取当前课程下参与答题学生信息集合</summary>
        public static IList<WX_Member> GetList_MemberByQuestionResult_QST(string courseDetailID )
        {
            List<WX_Member> list = new List<WX_Member>();
            ParameterTag[] ps =  
            { 
                new ParameterTag("@Value" , courseDetailID ,  E_DbType.VarChar , 50 ) 
            };

            Result rs = WX_Member.NONE.EntityMaping_Excute("GetList_MemberByQuestionResult_QST", ps, (readers) =>
            {
                EntityBase.AddToList<WX_Member>(list, readers, (r) =>
                {
                    MemberType mType2 = (MemberType)Convert.ToInt32(r.GetValue("MType"));
                    return WX_Member.New(mType2);
                });
            });
            return list;
        }


        /// <summary>获取当前课程下参与答题的学生总人数</summary>
        public static int GetCount_ERR_SumMember(CourseInfo theCourseInfo)
        {
            int rsCount = 0;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@CourseInfoID" , theCourseInfo.AutoID ,  E_DbType.VarChar , 50 ) 
            };


            Result rs = WX_Member.NONE.EntityMaping_Excute("GetCount_ERR_SumMember", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    rsCount = Convert.ToInt32(readers[0].GetValue(0));
                }
            });
            return rsCount;
        }
        /// <summary>获取当前课程下答错题的学生信息集合</summary>
        public static IList<WX_Member> GetListMember_ERR_SumMember(CourseInfo theCourseInfo)
        {
            List<WX_Member> list = new List<WX_Member>();
            ParameterTag[] ps =  
            { 
                new ParameterTag("@CourseInfoID" , theCourseInfo.AutoID ,  E_DbType.VarChar , 50 ) 
            };

            Result rs = WX_Member.NONE.EntityMaping_Excute("GetListMember_ERR_SumMember", ps, (readers) =>
            {
                EntityBase.AddToList<WX_Member>(list, readers, (r) =>
                {
                    MemberType mType2 = (MemberType)Convert.ToInt32(r.GetValue("MType"));
                    return WX_Member.New(mType2);
                });
            });
            return list;
        }



        /// <summary>获取某老师所有课程 ，参与答题的学生总数</summary>
        public static int GetCountAllMember_QuestionResult(WX_Member theTeacher)
        {
            int rsCount = 0;

            ParameterTag[] ps =  
            { 
                new ParameterTag("@MemberID" , theTeacher.AutoID ,  E_DbType.VarChar , 50 ) 
            };


            Result rs = WX_Member.NONE.EntityMaping_Excute("GetCountAllMember_QuestionResult", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    rsCount = Convert.ToInt32(readers[0].GetValue(0));
                }
            });
            return rsCount;
        }
        /// <summary>获取某老师所有课程 ，答错题的学生总数</summary>
        public static int GetCountAllMember_ERR_QuestionResult(WX_Member theTeacher)
        {
            int rsCount = 0;

            ParameterTag[] ps =  
            { 
                new ParameterTag("@MemberID" , theTeacher.AutoID ,  E_DbType.VarChar , 50 ) 
            };


            Result rs = WX_Member.NONE.EntityMaping_Excute("GetCountAllMember_ERR_QuestionResult", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    rsCount = Convert.ToInt32(readers[0].GetValue(0));
                }
            });
            return rsCount;
        }


        /// <summary>获取某老师所有课程 ，参与答题的学生总数</summary>
        public static int GetCountAllMember_QuestionResult_Time(WX_Member theTeacher, DateTime begin, DateTime end)
        {
            int rsCount = 0;

            ParameterTag[] ps =  
            { 
                new ParameterTag("@MemberID" , theTeacher.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@Begin" , begin ,  E_DbType.DateTime , 8 ),
                new ParameterTag("@End" , end ,  E_DbType.DateTime , 8 )
            };
            Result rs = WX_Member.NONE.EntityMaping_Excute("GetCountAllMember_QuestionResult_Time", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    rsCount = Convert.ToInt32(readers[0].GetValue(0));
                }
            });
            return rsCount;
        }
        /// <summary>获取某老师所有课程 ，答错题的学生总数</summary>
        public static int GetCountAllMember_ERR_QuestionResult_Time(WX_Member theTeacher, DateTime begin, DateTime end)
        {
            int rsCount = 0;
            ParameterTag[] ps =  
            { 
                new ParameterTag("@MemberID" , theTeacher.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@Begin" , begin ,  E_DbType.DateTime , 8 ),
                new ParameterTag("@End" , end ,  E_DbType.DateTime , 8 )
            };
            Result rs = WX_Member.NONE.EntityMaping_Excute("GetCountAllMember_ERR_QuestionResult_Time", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    rsCount = Convert.ToInt32(readers[0].GetValue(0));
                }
            });
            return rsCount;
        }


        /// <summary>获取某老师所有课程 ，答错题的学生总数</summary>
        public static IList<WX_Member> GetListMember_ERR_ByQuestionAndCouser(CourseInfo theCourse, QuestionInfo theQuest)
        {
            List<WX_Member> list = new List<WX_Member>();
            ParameterTag[] ps =  
            { 
                new ParameterTag("@CourseInfoID" , theCourse.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@QuestionInfoID" , theQuest.AutoID ,  E_DbType.VarChar , 50 ) 
            };
            Result rs = WX_Member.NONE.EntityMaping_Excute("GetListMember_ERR_ByQuestionAndCouser", ps, (readers) =>
            {
                EntityBase.AddToList<WX_Member>(list, readers, (r) =>
                {
                    MemberType mType2 = (MemberType)Convert.ToInt32(r.GetValue("MType"));
                    return WX_Member.New(mType2);
                });
            });
            return list;
        }


        /// <summary>获取某课程已预习的成员集合</summary>
        public static IList<WX_Member> GetListMember_ReadOKByCourseID(CourseInfo theCourse)
        {
            List<WX_Member> list = new List<WX_Member>();
            ParameterTag[] ps =  
            { 
                new ParameterTag("@CourseID" , theCourse.AutoID ,  E_DbType.VarChar , 50 ) 
            };
            Result rs = WX_Member.NONE.EntityMaping_Excute("GetListMember_ReadOKByCourseID", ps, (readers) =>
            {
                EntityBase.AddToList<WX_Member>(list, readers, (r) =>
                {
                    MemberType mType2 = (MemberType)Convert.ToInt32(r.GetValue("MType"));
                    return WX_Member.New(mType2);
                });
            });
            return list;
        }

        /// <summary>获取签到人的成员集合</summary>
        public static IList<WX_Member> GetListMember_SignInByParent(MemberSignIn theParent)
        {
            List<WX_Member> list = new List<WX_Member>();
            ParameterTag[] ps =  
            { 
                new ParameterTag("@ParentID" , theParent.AutoID ,  E_DbType.VarChar , 50 ) 
            };
            Result rs = WX_Member.NONE.EntityMaping_Excute("GetListMember_SignInByParent", ps, (readers) =>
            {
                EntityBase.AddToList<WX_Member>(list, readers, (r) =>
                {
                    MemberType mType2 = (MemberType)Convert.ToInt32(r.GetValue("MType"));
                    return WX_Member.New(mType2);
                });
            });
            return list;
        }



        //==========更新操作===========

        /// <summary>记录微信用户的信息</summary>
        public static Result Insert(WX_UserInfo wxInfo)
        {
            WX_Member theNew = WX_Member.New(MemberType.E_Student);
            theNew.OpenID = wxInfo.OpenID;
            theNew.NickName = wxInfo.Nickname;
            theNew.City = wxInfo.City;
            theNew.Country = wxInfo.Country;
            theNew.Province = wxInfo.Province;
            theNew.HeadImgURL = wxInfo.HeadImgURL;
            theNew.Sex = wxInfo.Sex;
            theNew.IsFollowed = wxInfo.Subscribe;


            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theNew.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@OpenID" , theNew.OpenID,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@NickName" , theNew.NickName,  E_DbType.VarChar , 100 ) ,
                    new ParameterTag("@Name" , theNew.Name,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@City" , theNew.City  ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Province" , theNew.Province ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@HeadImgURL" , theNew.HeadImgURL ,  E_DbType.VarChar , 200 ) ,
                    new ParameterTag("@Sex" , theNew.Sex ,  E_DbType.Int , 0 ) ,
                    new ParameterTag("@Country" , theNew.Country ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MType" , (int)theNew.MType ,  E_DbType.Int , 0 ) ,
                    new ParameterTag("@CTime" , theNew.CTime ,  E_DbType.DateTime , 0 ) ,
                    new ParameterTag("@Email" , theNew.Email ,  E_DbType.VarChar , 100 ) ,
                    new ParameterTag("@IsFollowed" , theNew.IsFollowed ,  E_DbType.Bit , 1 ) ,
                    new ParameterTag("@IsDisable" , theNew.IsDisable ,  E_DbType.Bit , 1 ) ,
                    new ParameterTag("@InnerID" , theNew.InnerID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@SchoolID" , theNew.SchoolID ,  E_DbType.VarChar , 50 ) 
                    
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
        public static Result Delete(WX_Member the)
        {
            if (the.MType == MemberType.E_SysAdmin)
                return new Result(false, "操作终止：不能删除系统管理员");

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
