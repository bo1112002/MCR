using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>学校(SCL)</summary>
    [Serializable]
    public class School : EntityBase
    {
        #region 持久属性
        string _Name = string.Empty;
        /// <summary>学校名称</summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        string _Bind_Key = string.Empty;
        /// <summary>用于绑定接口标识(通过第三方接口获取学校的信息(学校外部ID)</summary>
        public string Bind_Key
        {
            get { return _Bind_Key; }
            set { _Bind_Key = value; }
        }
        string _Remark = string.Empty;
        /// <summary>备注</summary>
        public string Remark
        {
            get { return _Remark; }
            set { _Remark = value; }
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

        protected School()
        {
            EntityBase.Evt_EntityChange += (entityInfo) =>
            {
                if (entityInfo is SourceDocument)
                {
                    _FileTypeOfStatistical = null;
                }
            };

        }

        #region============= 重写成员=========>>>

        public override Type GetTypeBase()
        {
            return typeof(School);
        }

        protected override string GetPrefixName()
        {
            return "SCL";
        }

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.Name = reader.GetValue<string>(this, "Name");
            this.Remark = reader.GetValue<string>(this, "Remark");
            this.IsDisable = reader.GetValue<bool>(this, "IsDisable");
            this.Bind_Key = reader.GetValue<string>(this, "Bind_Key");
        }

        #endregion=============END==========<<<


        Dictionary<string, int> _FileTypeOfStatistical = null;
        /// <summary>获取当前学校的资源文档各分类数量</summary>
        public Dictionary<string, int> GetFileTypeOfStatistical()
        {
            int count = 0;
            if (_FileTypeOfStatistical == null)
            {
                _FileTypeOfStatistical = new Dictionary<string, int>();

                count = SourceDocument.GetSourceCountBySchoolAndFType(this, VSTO.PPT_FileType.Courseware);
                _FileTypeOfStatistical.Add(VSTO.PPT_FileType.Courseware.ToString(), count);

                count = SourceDocument.GetSourceCountBySchoolAndFType(this, VSTO.PPT_FileType.Discuss);
                _FileTypeOfStatistical.Add(VSTO.PPT_FileType.Discuss.ToString(), count);

                count = SourceDocument.GetSourceCountBySchoolAndFType(this, VSTO.PPT_FileType.Nofity);
                _FileTypeOfStatistical.Add(VSTO.PPT_FileType.Nofity.ToString(), count);

                count = SourceDocument.GetSourceCountBySchoolAndFType(this, VSTO.PPT_FileType.Question);
                _FileTypeOfStatistical.Add(VSTO.PPT_FileType.Question.ToString(), count);

                count = SourceDocument.GetSourceCountBySchoolAndFType(this, VSTO.PPT_FileType.VoteQuestions);
                _FileTypeOfStatistical.Add(VSTO.PPT_FileType.VoteQuestions.ToString(), count);

                //count = SourceDocument.GetSourceCountBySchoolAndFType(this, VSTO.PPT_FileType.NONE);
                //_FileTypeOfStatistical.Add(VSTO.PPT_FileType.NONE.ToString(), count);
            }
            return _FileTypeOfStatistical;
        }


        //====================更新========================

        /// <summary>修改当前学校名称及备注</summary>
        public Result Update(string name, string remark)
        {
            name = name.Trim();
            if (string.IsNullOrEmpty(name) == true)
            {
                return new Result(false, "操作终止：学校名不能为空");
            }

            ParameterTag[] ps =  
             { 
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@Name" , name ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@Remark" , remark ,  E_DbType.VarChar , 200 ) 
             };

            Result rs = this.EntityMaping_Excute("Update", ps);
            if (rs.IsOK == true)
            {
                this.Name = name;
                this.Remark = remark;
            }
            return rs;
        }
        /// <summary>是否禁用当前学校</summary>
        public Result Update_IsDisable(bool isDisable)
        {
            ParameterTag[] ps =  
             { 
                 new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                 new ParameterTag("@IsDisable" , isDisable  ,  E_DbType.Bit , 1 ) 
             };

            Result rs = this.EntityMaping_Excute("Update_IsDisable", ps);
            if (rs.IsOK == true)
            {
                this.IsDisable = isDisable;
            }
            return rs;
        }


        #region 静态成员

        /// <summary>无效对象</summary>
        public static readonly School NONE = new School() { AutoID = "SCL-001", Name = "其它" };

        /// <summary>依据物理唯一标识获取对象</summary>
        public static School GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
                return null;
            School the = EntityBase.GetMyICache().Get(autoID) as School;
            if (the == null)
            {
                ParameterTag[] ps =  
                { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 50 ) 
                };
                Result rs = School.NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new School();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }
        /// <summary>获取所有学校对象集合</summary>
        public static IList<School> GetAll()
        {
            List<School> list = new List<School>();
            Result rs = School.NONE.EntityMaping_Excute("GetList_All", null, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string autoID = r.GetValue(0).ToString();
                    School the = EntityBase.GetMyICache().Get(autoID) as School;
                    if (the == null)
                    {
                        the = new School();
                        the.ToEntity(r);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    list.Add(the);
                }
            });
            return list;
        }

        public static IList<School> GetAllByKeyWord(string schoolName, string schoolEmail, string schoolPhone, int pageSize, int pageNo, ref int totalCount, int disable = -1)
        {
            List<School> list = new List<School>();
            string keyWord = string.Empty;

            if (!string.IsNullOrEmpty(schoolName))
            {
                keyWord += " And Name like '% " + schoolName + " %' ";
            }
            if (!string.IsNullOrEmpty(schoolEmail))
            {
                keyWord += " And Email like '% " + schoolEmail + " %' ";
            }
            if (!string.IsNullOrEmpty(schoolPhone))
            {
                keyWord += " And Phone like '% " + schoolPhone + " %' ";
            }
            if (disable > -1)
            {
                keyWord += " And IsDisable = " + disable;
            }

            ParameterTag[] ps =
            {
                new ParameterTag("@keyWord" , keyWord ,  E_DbType.VarChar , 500 ) ,
                new ParameterTag("@pageSize" ,pageSize ,  E_DbType.Int , 4 ) ,
                new ParameterTag("@pageNo" , pageNo ,  E_DbType.Int , 4 ) 
            };

            Result rs = School.NONE.EntityMaping_Excute("GetAllByKeyWord", ps, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string autoID = r.GetValue(0).ToString();
                    School the = GetByID(autoID);
                    if (the == null)
                    {
                        the.ToEntity(r);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    if (the != null)
                    {
                        list.Add(the);
                    }
                }
            });
            int sum = 0;
            Result rs2 = School.NONE.EntityMaping_Excute("GetAllByKeyWord", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    sum = Convert.ToInt32(readers[0]);
                }
            });
            totalCount = sum;
            return list;
        }




        //==========更新=========
        /// <summary>新增</summary>
        public static Result Insert(string name, string remark, string bind_key = "")
        {
            name = name.Trim();
            if (string.IsNullOrEmpty(name) == true)
            {
                return new Result(false, "操作终止：学校名不能为空");
            }


            School theNew = new School();
            theNew.Name = name;
            theNew.Remark = remark;
            theNew.Bind_Key = bind_key;

            ParameterTag[] ps = 
            { 
                new ParameterTag("@AutoID" , theNew.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@Name" , theNew.Name,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@CTime" , theNew.CTime ,  E_DbType.DateTime , 0 ) ,
                new ParameterTag("@Bind_Key" , theNew.Bind_Key  ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@IsDisable" , theNew.IsDisable  ,  E_DbType.Bit , 1 ) ,
                new ParameterTag("@Remark" , theNew.Remark ,  E_DbType.VarChar , 200 ) 
                
            };

            Result rs = theNew.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK == true)
            {
                rs.Data = theNew;
                EntityBase.GetMyICache().Set(theNew.AutoID, theNew);
            }
            return rs;
        }
        /// <summary>删除</summary>
        public static Result Delete(School the)
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




    public class School_QST : School
    {
        protected School_QST()
        { }

        /// <summary>获取所有学校信息集合</summary>
        public static IList<School> GetAll()
        {
            List<School> list = EntityBase.GetMyICache().Get("School_QST::GetAll") as List<School> ;
            if (list == null)
            {
                list = new List<School>();
                Dictionary<string, object> dicData = QST_Interface.GetList_School();
                if (dicData.Keys.Contains("ERR") == false && dicData["code"].ToString() == "200")
                {
                    IList infos = dicData["data"] as IList;
                    foreach (object obj in infos)
                    {
                        Dictionary<string, object> info = obj as Dictionary<string, object>;
                        if (info != null)
                        {
                            School_QST theSchool = new School_QST();
                            theSchool.AutoID = info["id"].ToString();
                            theSchool.Bind_Key = info["code"].ToString();
                            theSchool.CTime = Convert.ToDateTime(info["createTime"]);
                            theSchool.Name = info["name"].ToString();
                            theSchool.Remark = info["remark"].ToString();
                            theSchool.IsDisable = true;
                            list.Add(theSchool);
                        }
                    }
                    EntityBase.GetMyICache().Set("School_QST::GetAll",  list , DateTime.Now.AddHours(5) );
                }
            }
            return list;
        }

        /// <summary>获取某学校信息</summary>
        public static School GetFindID_QST( string id )
        {
            School_QST theSchool = null;
            Dictionary<string, object> dicData = QST_Interface.GetSchoolByID(id);
            if (dicData.Keys.Contains("ERR") == false && dicData["code"].ToString() == "200")
            {
                Dictionary<string, object> info = dicData["data"] as Dictionary<string, object>;
                if (info != null)
                {
                    theSchool = new School_QST();
                    theSchool.AutoID = info["id"].ToString();
                    theSchool.Bind_Key = info["code"].ToString();
                    theSchool.CTime = Convert.ToDateTime(info["createTime"]);
                    theSchool.Name = info["name"].ToString();
                    theSchool.Remark = info["remark"].ToString();
                    theSchool.IsDisable = true;
                    return theSchool;
                }
            }

            if (theSchool == null)
            {
                IList<School> list = GetAll();
                foreach (School the in list)
                {
                    if (the.AutoID == id)
                        return the;
                }
            }
            return null;
        }


    }











}
