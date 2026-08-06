using MCR.Mods.VSTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{

    /// <summary>题目信息类(QUES)</summary>
    /// <remarks>
    /// 这里的题目是一个泛意，不要误解为单纯的题目，它包含了：1：单选题, 2:多选题 , 3:投票题
    /// </remarks>
    [Serializable]
    public class QuestionInfo : EntityBase
    {
        #region 持久属性

        string _DocumentID = string.Empty;
        /// <summary>所关联的资源文件对象ID</summary>
        public string DocumentID
        {
            get { return _DocumentID; }
            set { _DocumentID = value; }
        }
        int _PageIndex = 0;
        /// <summary>所在的页数</summary>
        public int PageIndex
        {
            get { return _PageIndex; }
            set { _PageIndex = value; }
        }

        string _MemberID = string.Empty;
        /// <summary>创建题目的老师(需要验证)</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }
        string _GID = string.Empty;
        /// <summary>题目所在的物理页的唯一物理标识</summary>
        public string GID
        {
            get { return _GID; }
            set { _GID = value; }
        }
        PPT_SlideType _QType = PPT_SlideType.NONE;
        /// <summary>当前题的类别(0：其它选题，1：单选题, 2:多选题 , 3:投票题)</summary>
        public PPT_SlideType QType
        {
            get { return _QType; }
            set { _QType = value; }
        }
        string _Caption = string.Empty;
        /// <summary>标题</summary>
        public string Caption
        {
            get { return _Caption; }
            set { _Caption = value; }
        }
        float _Value = 0F;
        /// <summary>当前题分值</summary>
        public float Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        DateTime _CTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CTime
        {
            get { return _CTime; }
            set { _CTime = value; }
        }



        string _CourseDetaileID = string.Empty;
        /// <summary>所属的课程的章节ID</summary>
        public string CourseDetaileID
        {
            get { return _CourseDetaileID; }
            set { _CourseDetaileID = value; }
        }

        bool _IsSubmit = false;
        /// <summary>是否已发布</summary>
        public bool IsSubmit
        {
            get { return _IsSubmit; }
            set { _IsSubmit = value; }
        }

        #endregion

        protected QuestionInfo()
        {
        }


        #region============= 重写成员=========>>>
        /*
        public override void Serialize(IDictionary<string, object> map)
        {
            map.Add("AutoID", this.AutoID);
            map.Add("MemberID", GetMember());
            map.Add("DocumentID", this.DocumentID);
            map.Add("PageIndex", this.PageIndex);
            map.Add("GID", this.GID);
            map.Add("Caption", this.Caption);
            map.Add("Value", this.Value);
            map.Add("CTime", this.CTime);
        }
        */
        public override Type GetTypeBase()
        {
            return typeof(QuestionInfo);
        }

        protected override string GetPrefixName()
        {
            return "QUES";
        }

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.DocumentID = reader.GetValue<string>(this, "DocumentID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.PageIndex = reader.GetValue<int>(this, "PageIndex");
            this.GID = reader.GetValue<string>(this, "GID");
            this.QType = (PPT_SlideType)reader.GetValue<int>(this, "QType");
            this.Caption = reader.GetValue<string>(this, "Caption");
            this.Value = reader.GetValue<float>(this, "Value");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
            this.CourseDetaileID = reader.GetValue<string>(this, "CourseDetaileID");
            this.IsSubmit = reader.GetValue<bool>(this, "IsSubmit");


        }
        #endregion=============END==========<<<

        /// <summary>标题字符串格式化</summary>
        public string CaptionString
        {
            get
            {
                return string.Format("[{0}] {1}", this.QTypeString, this.Caption);
            }
        }

        /// <summary>获取题型的字符描述</summary>
        public string QTypeString
        {
            get
            {
                return EnumDescription.GetFieldText(this.QType);
            }
        }

        public int QTypeVal
        {
            get
            {
                return (int)this.QType;
            }
        }

        //==========================


        /// <summary>获取当前成员对象</summary>
        public WX_Member GetMember()
        {
            return WX_Member.GetByID(this.MemberID);
        }
        /// <summary>获取当前成员对象</summary>
        public SourceDocument GetDocment()
        {
            return SourceDocument.GetByID(this.DocumentID);
        }

        /// <summary>获取当前题目的所有的题项</summary>
        public IList<QuestionItemInfo> GetQuestItems()
        {
            return QuestionItemInfo.GetListByQuest(this);
        }

        /// <summary>获取当前题目及指定成员的答题结果项集合</summary>
        public IList<QuestionResult> GetQuestResultByMember(WX_Member theMember, CourseInfo theCourse)
        {
            return QuestionResult.GetListByQuestResult(theMember, theCourse, this);
        }
        /// <summary>某个成员是否答对</summary>
        public bool GetByMemberIsYes(WX_Member theMember, CourseInfo theCourse)
        {
            bool isYes = true;
            IList<QuestionResult> list = this.GetQuestResultByMember(theMember, theCourse);
            foreach (QuestionResult qResult in list)
            {
                if (qResult.IsYes() == false)
                {
                    isYes = false;
                    break;
                }
            }
            return isYes;
        }


        //===========================
        /// <summary>修改当前信息</summary>
        public Result Update(PPT_SlideType qType, string caption, float val)
        {
            Result rs = Result.NONE;
            if (this.IsSubmit)
                return new Result(false, "当前题目已发布，不可以进行修改");
            else if (string.IsNullOrEmpty(caption.Trim()) == true)
                return new Result(false, "题目内容不能为空");
            else if (val < 0F)
                return new Result(false, "分值不能为负数");

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@QType" , qType ,  E_DbType.Int , 8 ) ,
                    new ParameterTag("@Caption" , caption ,  E_DbType.VarChar , 300 ) ,
                    new ParameterTag("@Value" , val ,  E_DbType.Float , 0 ) 
            };
            rs = this.EntityMaping_Excute("Update", ps);
            if (rs.IsOK)
            {
                this.QType = qType;
                this.Caption = caption;
                this.Value = val;

                rs.Data = this;
            }
            return rs;
        }
        /// <summary>修改当前QST课程章节题为已发布</summary>
        public Result Update_IsSubmit()
        {
            if (this.IsSubmit)
                return new Result(true, string.Empty, this);

            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@IsSubmit" , true ,  E_DbType.Bit , 1 ) ,
            };
            rs = this.EntityMaping_Excute("Update_Submit", ps);
            if (rs.IsOK)
            {
                this.IsSubmit = true;
                rs.Data = this;
            }
            return rs;
        }


        #region 静态成员
        public static readonly QuestionInfo NONE = new QuestionInfo();
        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static QuestionInfo GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            QuestionInfo the = EntityBase.GetMyICache().Get(autoID) as QuestionInfo;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 30 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new QuestionInfo();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }


        /// <summary>获取文档的某一页下的题目对象</summary>
        public static QuestionInfo GetByDocumentAndPage(SourceDocument doc, int pageIndex)
        {
            QuestionInfo the = null;
            ParameterTag[] ps =  { 
                    new ParameterTag("@DocumentID" , doc.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@PageIndex" , pageIndex ,  E_DbType.Int , 4 ) 
                                 };
            Result rs = NONE.EntityMaping_Excute("GetByDocumentAndPage", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string id = readers[0].GetValue(0).ToString();
                    the = GetByID(id);
                }
            });
            return the;
        }

        ///<summary>获取指定成员(老师)的所有问题集合 </summary>
        public static IList<QuestionInfo> GetListByMember(WX_Member theMember, PPT_SlideType type = PPT_SlideType.NONE)
        {
            List<QuestionInfo> list = new List<QuestionInfo>();
            ParameterTag[] ps =  
            { 
                new ParameterTag("@MemberID" , theMember.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@QType" , (int)type,  E_DbType.Int, 8 )
            };
            Result rs = NONE.EntityMaping_Excute("GetListByMember", ps, (readers) =>
            {
                EntityBase.AddToList<QuestionInfo>(list, readers, (r) => { return new QuestionInfo(); });
            });
            return list;
        }
        ///<summary>获取指定资源文档的所有问题集合 </summary>
        public static IList<QuestionInfo> GetListByDocument(SourceDocument doc)
        {
            List<QuestionInfo> list = new List<QuestionInfo>();
            ParameterTag[] ps =  { 
                new ParameterTag("@DocumentID" , doc.AutoID,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = QuestionInfo.NONE.EntityMaping_Excute("GetListByDocument", ps, (readers) =>
            {
                EntityBase.AddToList<QuestionInfo>(list, readers, (r) => { return new QuestionInfo(); });
            });
            return list;
        }



        ///<summary>获取指定资源文档的所有问题集合 </summary>
        public static void GetCount_JoinQuestionByMember(WX_Member theMember, out int sumCount, out int okCount)
        {
            int t_sumCount = 0, t_okCount = 0;

            ParameterTag[] ps =  { 
                new ParameterTag("@MemberID" , theMember.AutoID,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = QuestionInfo.NONE.EntityMaping_Excute("GetCount_JoinQuestionByMember", ps, (readers) =>
            {
                if (readers.Count > 0 && readers.ReaderIndex == 0)
                {
                    t_sumCount = Convert.ToInt32(readers[0].GetValue(0));
                }
                else if (readers.Count > 0 && readers.ReaderIndex == 1)
                {
                    t_okCount = Convert.ToInt32(readers[0].GetValue(0));
                }
            });

            sumCount = t_sumCount;
            okCount = t_okCount;
        }




        ///<summary>获取某课程的有错题信息集合及每题的错误人数 </summary>
        public static IList<QuestionInfo> GetListByCourse_Error(CourseInfo theCourse)
        {
            List<QuestionInfo> list = new List<QuestionInfo>();
            ParameterTag[] ps =  { 
                new ParameterTag("@CourseInfoID" , theCourse.AutoID,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = QuestionInfo.NONE.EntityMaping_Excute("GetListByCourse_Error", ps, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string qid = r.GetValue(0).ToString();
                    QuestionInfo the = QuestionInfo.GetByID(qid);
                    if (the != null)
                    {
                        the.ExtPropertys["ErrorCount"] = Convert.ToInt32(r.GetValue(1));
                        list.Add(the);
                    }
                }
            });
            return list;
        }

        ///<summary>获取某课程的有错题信息集合及每题的错误人数 </summary>
        public static IList<QuestionInfo> GetListByCourse_Error(CourseInfo theCourse, WX_Member theSudent)
        {
            List<QuestionInfo> list = new List<QuestionInfo>();
            ParameterTag[] ps =  { 
                new ParameterTag("@CourseInfoID" , theCourse.AutoID,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@MemberID" , theSudent.AutoID,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = QuestionInfo.NONE.EntityMaping_Excute("GetListByCourse_Error2", ps, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string qid = r.GetValue(0).ToString();
                    QuestionInfo the = QuestionInfo.GetByID(qid);
                    if (the != null)
                    {
                        the.ExtPropertys["ErrorCount"] = Convert.ToInt32(r.GetValue(1));
                        list.Add(the);
                    }
                }
            });
            return list;
        }


        //================================
        /// <summary>获取课程章节的相关习题</summary>
        public static IList<QuestionInfo> GetList_CourseDetaile_QST(string detailID, bool? isSubmit = null)
        {
            if (isSubmit == null)
                return GetList_CourseDetaile_ALL_QST(detailID);

            List<QuestionInfo> list = new List<QuestionInfo>();
            ParameterTag[] ps =  { 
                new ParameterTag("@CourseDetaileID" , detailID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@IsSubmit" , isSubmit ,  E_DbType.Bit , 1 )
                                };
            Result rs = QuestionInfo.NONE.EntityMaping_Excute("GetList_CourseDetaile_QST", ps, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string qid = r.GetValue(0).ToString();
                    QuestionInfo the = QuestionInfo.GetByID(qid);
                    if (the != null)
                    {
                        list.Add(the);
                    }
                }
            });
            return list;
        }
        /// <summary>获取课程章节的相关习题</summary>
        static IList<QuestionInfo> GetList_CourseDetaile_ALL_QST(string detailID)
        {
            List<QuestionInfo> list = new List<QuestionInfo>();
            ParameterTag[] ps =  { 
                new ParameterTag("@CourseDetaileID" , detailID ,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = QuestionInfo.NONE.EntityMaping_Excute("GetList_CourseDetaile_ALL_QST", ps, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string qid = r.GetValue(0).ToString();
                    QuestionInfo the = QuestionInfo.GetByID(qid);
                    if (the != null)
                    {
                        list.Add(the);
                    }
                }
            });
            return list;
        }



        /// <summary>获取课程章节的相关习题</summary>
        public static IList<QuestionInfo> GetList_CourseDetaile_QST_Vote3(string detailID, bool? isSubmit = null)
        {
            if (isSubmit == null)
                return GetList_CourseDetaile_ALL_QST_Vote3(detailID);

            List<QuestionInfo> list = new List<QuestionInfo>();
            ParameterTag[] ps =  { 
                new ParameterTag("@CourseDetaileID" , detailID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@IsSubmit" , isSubmit ,  E_DbType.Bit , 1 )
                                };
            Result rs = QuestionInfo.NONE.EntityMaping_Excute("GetList_CourseDetaile_QST_Vote3", ps, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string qid = r.GetValue(0).ToString();
                    QuestionInfo the = QuestionInfo.GetByID(qid);
                    if (the != null)
                    {
                        list.Add(the);
                    }
                }
            });
            return list;
        }


        /// <summary>获取课程章节的相关投票</summary>
        static IList<QuestionInfo> GetList_CourseDetaile_ALL_QST_Vote3(string detailID)
        {
            List<QuestionInfo> list = new List<QuestionInfo>();
            ParameterTag[] ps =  { 
                new ParameterTag("@CourseDetaileID" , detailID ,  E_DbType.VarChar , 50 ) 
                                };
            Result rs = QuestionInfo.NONE.EntityMaping_Excute("GetList_CourseDetaile_ALL_QST_Vote3", ps, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string qid = r.GetValue(0).ToString();
                    QuestionInfo the = QuestionInfo.GetByID(qid);
                    if (the != null)
                    {
                        list.Add(the);
                    }
                }
            });
            return list;
        }


        /// <summary>获取课程章节的相关习题数</summary>
        public static int GetList_CourseDetaile_COUNT_QST(string detailID)
        {
            int count = 0;
            ParameterTag[] ps =  { 
                new ParameterTag("@CourseDetaileID" , detailID ,  E_DbType.VarChar , 50 ) ,
                new ParameterTag("@IsSubmit" , true ,  E_DbType.Bit , 1 )

                                };
            Result rs = QuestionInfo.NONE.EntityMaping_Excute("GetList_CourseDetaile_COUNT_QST", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    count = Convert.ToInt32(readers[0].GetValue(0));
                }
            });
            return count;
        }


        ///<summary>获取某课程的错误题项集合 </summary>
        public static IList<QuestionInfo> GetListByErrorItem_QST(WX_Member theMember, CourseInfo theCourseInfo)
        {
            List<QuestionInfo> list = new List<QuestionInfo>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ),
                    new ParameterTag("@CourseDetaileID" , theCourseInfo.DetaileID ,  E_DbType.VarChar , 50 )
                };
            Result rs = NONE.EntityMaping_Excute("GetListByErrorItem_QST", ps, (readers) =>
            {
                EntityBase.AddToList<QuestionInfo>(list, readers, (r) => { return new QuestionInfo(); });
            });
            return list;
        }


        //==========更新操作===========

        /// <summary>添加</summary>
        public static Result Insert(MC_QuestionClass mcQuestion, WX_Member theMember, SourceDocument doc, int pageIndex, CourseDetail theDetail = null)
        {
            Result rs = Result.NONE;
            QuestionInfo the = new QuestionInfo();
            the.DocumentID = doc.AutoID;
            the.MemberID = theMember.AutoID;
            the.PageIndex = pageIndex;
            the.GID = mcQuestion.GID;
            the.QType = mcQuestion.QType;
            the.Caption = mcQuestion.Caption;
            the.Value = mcQuestion.Value;

            if (theDetail != null)
                the.CourseDetaileID = theDetail.ID;

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@DocumentID" , the.DocumentID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , the.MemberID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@PageIndex" , (int)the.PageIndex  ,  E_DbType.Int , 8 ) ,
                    new ParameterTag("@GID" , the.GID ,  E_DbType.VarChar , 100 ) ,
                    new ParameterTag("@QType" , (int)the.QType ,  E_DbType.Int , 4 ) ,
                    new ParameterTag("@Caption" , the.Caption ,  E_DbType.VarChar , 300) ,
                    new ParameterTag("@Value" , the.Value ,  E_DbType.Float, 0 ) ,
                    new ParameterTag("@CTime" , the.CTime  ,  E_DbType.DateTime, 0 ),
                    new ParameterTag("@CourseDetaileID" , the.CourseDetaileID ,  E_DbType.VarChar , 50 ) 
            };
            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;
                QuestionItemInfo.Insert_Batch(the, mcQuestion.Items);
            }
            return rs;
        }

        /// <summary>添加</summary>
        public static Result Insert_QST(MC_QuestionClass mcQuestion, WX_Member theMember)
        {
            Result rs = Result.NONE;
            QuestionInfo the = new QuestionInfo();
            the.DocumentID = string.Empty;
            the.MemberID = theMember.AutoID;
            the.PageIndex = -1;
            the.GID = mcQuestion.GID;
            the.QType = mcQuestion.QType;
            the.Caption = mcQuestion.Caption;
            the.Value = mcQuestion.Value;
            the.CourseDetaileID = mcQuestion.CourseDetaileID;

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@DocumentID" , the.DocumentID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , the.MemberID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@PageIndex" , (int)the.PageIndex  ,  E_DbType.Int , 8 ) ,
                    new ParameterTag("@GID" , the.GID ,  E_DbType.VarChar , 100 ) ,
                    new ParameterTag("@QType" , (int)the.QType ,  E_DbType.Int , 4 ) ,
                    new ParameterTag("@Caption" , the.Caption ,  E_DbType.VarChar , 500) ,
                    new ParameterTag("@Value" , the.Value ,  E_DbType.Float, 0 ) ,
                    new ParameterTag("@CTime" , the.CTime  ,  E_DbType.DateTime, 0 ),
                    new ParameterTag("@CourseDetaileID" , the.CourseDetaileID ,  E_DbType.VarChar , 50 ) 
            };
            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;
                QuestionItemInfo.Insert_Batch(the, mcQuestion.Items);
            }
            return rs;
        }



        /// <summary>批量修改某个QST课程章节题为已发布 . qTypeMode=1为习题，qTypeMode=3为投票</summary>
        public static Result Update_IsSubmit_ALL(string detailID , int qTypeMode = 1 )
        {
            IList<QuestionInfo> list = null;
            if (qTypeMode == 1)
            {
                list = GetList_CourseDetaile_QST(detailID);
            }
            else if (qTypeMode == 3)
            {
                list = GetList_CourseDetaile_QST_Vote3(detailID);
            }
            else
            {
                return new Result(false , "找不到目标分类集合");
            }

            foreach (QuestionInfo the in list)
            {
                the.Update_IsSubmit();
            }
            return Result.OK;
        }

        /// <summary>删除(管理员权限)</summary>
        public static Result Delete(QuestionInfo theQuest)
        {

            if (theQuest.IsSubmit)
                return new Result(false, "该题已提交，不可删除");

            //如果删除当前问题，则其它所有关联的记录也要一并删除(QuestionItemInfo、QuestionResult)
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theQuest.AutoID ,  E_DbType.VarChar , 50 ) ,
            };
            rs = theQuest.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Clear(theQuest.AutoID);
            }
            return rs;
        }




        #endregion



    }















}
