using MCR.Mods.VSTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>答题项结果信息类(QREST)</summary>
    /// <remarks>统计(分数、答错人数、课程错题数等)、错题本等等记录的原始数据</remarks>
    [Serializable]
    public class QuestionResult : EntityBase
    {
        #region 持久属性
        string _MemberID = string.Empty;
        /// <summary>学生ID</summary>
        public string MemberID
        {
            get { return _MemberID; }
            set { _MemberID = value; }
        }
        string _CourseInfoID = string.Empty;
        /// <summary>所属的课程ID</summary>
        public string CourseInfoID
        {
            get { return _CourseInfoID; }
            set { _CourseInfoID = value; }
        }
        string _QuestionInfoID = string.Empty;
        /// <summary>题目ID</summary>
        public string QuestionInfoID
        {
            get { return _QuestionInfoID; }
            set { _QuestionInfoID = value; }
        }
        string _QuestionItemID = string.Empty;
        /// <summary>所选择的题项ID</summary>
        public string QuestionItemID
        {
            get { return _QuestionItemID; }
            set { _QuestionItemID = value; }
        }
        string _Value = string.Empty;
        /// <summary>结果值(备用)</summary>
        public string Value
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
        /// <summary>章节ID</summary>
        public string CourseDetaileID
        {
            get { return _CourseDetaileID; }
            set { _CourseDetaileID = value; }
        }



        #endregion

        protected QuestionResult() { }

        #region============= 重写成员=========>>>
        public override Type GetTypeBase()
        {
            return typeof(QuestionResult);
        }

        protected override string GetPrefixName()
        {
            return "QREST";
        }


        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.MemberID = reader.GetValue<string>(this, "MemberID");
            this.CourseInfoID = reader.GetValue<string>(this, "CourseInfoID");
            this.QuestionInfoID = reader.GetValue<string>(this, "QuestionInfoID");
            this.QuestionItemID = reader.GetValue<string>(this, "QuestionItemID");
            this.Value = reader.GetValue<string>(this, "Value");
            this.CTime = reader.GetValue<DateTime>(this, "CTime");
            this.CourseDetaileID = reader.GetValue<string>(this, "CourseDetaileID");
        }

        #endregion=============END==========<<<


        /// <summary>获取当前对应的课程对象</summary>
        public CourseInfo GetCourseInfo()
        {
            return CourseInfo.GetByID(this.CourseInfoID);
        }
        /// <summary>获取当前对应的题项对象</summary>
        public QuestionItemInfo GetQuestionItemInfo()
        {
            return QuestionItemInfo.GetByID(this.QuestionItemID);
        }

        /// <summary>获取当前题项的标识</summary>
        public string ItemKey
        {
            get
            {
                QuestionItemInfo item = this.GetQuestionItemInfo();
                if (item != null)
                    return item.ItemKey;
                return string.Empty;
            }
        }


        /// <summary>当前选项结果是否正确</summary>
        public bool IsYes()
        {
            QuestionItemInfo qItem = this.GetQuestionItemInfo();
            if (qItem == null)
                return false;

            return qItem.IsVal;
        }



        #region 静态成员

        public static readonly QuestionResult NONE = new QuestionResult();
        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static QuestionResult GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            QuestionResult the = EntityBase.GetMyICache().Get(autoID) as QuestionResult;
            if (the == null)
            {
                ParameterTag[] ps =  
                { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 50 ) 
                };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new QuestionResult();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }
        /// <summary>获取某成员某课程某题项的结果信息,如果不存在则返回null</summary>
        public static QuestionResult GetByID(WX_Member theMember, CourseInfo theCourse, QuestionItemInfo qItem)
        {
            ParameterTag[] ps = new ParameterTag[] 
            { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseInfoID" , theCourse.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@QuestionItemID" , qItem.AutoID ,  E_DbType.VarChar , 50 ) 
            };

            QuestionResult the = null;
            Result rs = NONE.EntityMaping_Excute("GetByID2", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string id = readers[0].GetValue(0).ToString();
                    the = EntityBase.GetMyICache().Get(id) as QuestionResult;
                    if (the == null)
                    {
                        the = new QuestionResult();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                }
            });
            return the;
        }



        ///<summary>获取某个课程下的指定题目及成员的答题项 </summary>
        public static IList<QuestionResult> GetListByQuestResult(WX_Member theMember, CourseInfo theCourse, QuestionInfo theQuest)
        {
            List<QuestionResult> list = new List<QuestionResult>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseInfoID" , theCourse.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@QuestionInfoID" , theQuest.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetListByQuestResult", ps, (readers) =>
            {
                EntityBase.AddToList<QuestionResult>(list, readers, (r) => { return new QuestionResult(); });
            });
            return list;
        }

        ///<summary>获取某个课程下的指定题目及成员的答题项 </summary>
        public static IList<QuestionResult> GetListByQuestResult_QST(WX_Member theMember, string courseDetailID )
        {
            List<QuestionResult> list = new List<QuestionResult>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseDetaileID" , courseDetailID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetListByQuestResult_QST", ps, (readers) =>
            {
                EntityBase.AddToList<QuestionResult>(list, readers, (r) => { return new QuestionResult(); });
            });
            return list;
        }



        ///<summary>获取某个课程下的指定投票结果 </summary>
        public static Dictionary<string, int> GetListByQuestResult_QST_Vote3(string courseDetailID)
        {
            Dictionary<string, int> dicResult = new Dictionary<string, int>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@CourseDetaileID" , courseDetailID ,  E_DbType.VarChar , 50 ) 
                };
            Result rs = NONE.EntityMaping_Excute("GetListByQuestResult_QST_Vote3", ps, (readers) =>
            {
                foreach( EntityReader r in readers )
                {
                    string itemID = r.GetValue("QuestionItemID").ToString() ;
                    dicResult[itemID] = Convert.ToInt32(r.GetValue(1));
                }
            });
            return dicResult ;
        }



        //==========更新操作===========

        /// <summary>添加</summary>
        public static Result Insert(WX_Member theMember, CourseInfo theCourse, QuestionItemInfo qItem, string val = "0")
        {
            SourceDocument theDocument = theCourse.GetDocument();
            if (theDocument == null)
                return new Result(false, "无效的课程，找不到相关文档对象");

            QuestionInfo theQuest = qItem.GetQuestionInfo();
            if (theQuest == null)
                return new Result(false, "无效的题目，找不到当前题项所属的题目对象");


            Result rs = Result.NONE;
            if (theQuest.QType == PPT_SlideType.Question_One)
            {
                rs = QuestionResult.Delete_Question(theMember, theCourse, theQuest);
                //if (rs.IsOK == false) return rs;
            }
            QuestionResult the = GetByID(theMember, theCourse, qItem);
            if (the != null)
                return new Result(true, string.Empty, the);

            QuestionResult theNew = new QuestionResult();
            theNew.MemberID = theMember.AutoID;
            theNew.CourseInfoID = theCourse.AutoID;
            theNew.QuestionInfoID = qItem.QuestionID;
            theNew.QuestionItemID = qItem.AutoID;
            theNew.Value = val;

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theNew.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , theNew.MemberID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseInfoID" , theNew.CourseInfoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@QuestionInfoID" , theNew.QuestionInfoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@QuestionItemID" , theNew.QuestionItemID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Value" , theNew.Value ,  E_DbType.VarChar , 100 ) ,
                    new ParameterTag("@CTime" , theNew.CTime ,  E_DbType.DateTime , 8 ),
                     new ParameterTag("@CourseDetaileID" , string.Empty ,  E_DbType.VarChar , 50 ) 
            };
            rs = theNew.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Set(theNew.AutoID, theNew);
                rs.Data = theNew;
            }
            return rs;
        }
        /// <summary>批量添加 </summary>
        public static Result Insert_Batch(WX_Member theMember, CourseInfo theCourse, IList<QuestionItemInfo> qItems)
        {
            foreach (QuestionItemInfo qItem in qItems)
            {
                Result rs2 = Insert(theMember, theCourse, qItem);
            }
            return new Result(true, "成功完成所有操作");
        }

        /// <summary>删除(预留)</summary>
        public static Result Delete(QuestionResult theQuestItemResult)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theQuestItemResult.AutoID ,  E_DbType.VarChar , 50 ) ,
                };
            rs = NONE.EntityMaping_Excute("Delete", ps);
            return rs;
        }
        /// <summary>删除(批量删除--某成员某课程某题项的结果)</summary>
        public static Result Delete(WX_Member theMember, CourseInfo theCourse, QuestionItemInfo qItem)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseInfoID" , theCourse.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@QuestionItemID" , qItem.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            rs = NONE.EntityMaping_Excute("Delete2", ps);
            return rs;
        }
        /// <summary>删除(批量删除--某成员某课程某题所有结果项)</summary>
        public static Result Delete_Question(WX_Member theMember, CourseInfo theCourse, QuestionInfo theQuestion)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseInfoID" , theCourse.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@QuestionInfoID" , theQuestion.AutoID ,  E_DbType.VarChar , 50 ) 
                };
            rs = NONE.EntityMaping_Excute("Delete3", ps);
            return rs;
        }




        /// <summary>添加</summary>
        public static Result Insert_QST(WX_Member theMember, QuestionItemInfo qItem ,  string detailID  ,  string val="")
        {
            QuestionInfo theQuest = qItem.GetQuestionInfo();
            if (theQuest == null)
                return new Result(false, "无效的题目，找不到当前题项所属的题目对象");
            else if (theQuest.IsSubmit== false)
                return new Result(false, "该题目还未发布，操作被终止");

            CourseInfo theCourseInfo = CourseInfo.GetByDetaileID_QST(detailID);
            if (theCourseInfo == null)
                return new Result(false , "找不到有效的所属课程信息");


            Result rs = Result.NONE;

            QuestionResult theNew = new QuestionResult();
            theNew.MemberID = theMember.AutoID;
            theNew.CourseInfoID = theCourseInfo.AutoID;
            theNew.QuestionInfoID = theQuest.AutoID ;
            theNew.QuestionItemID = qItem.AutoID;
            theNew.CourseDetaileID = detailID;
            theNew.Value = val;

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theNew.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@MemberID" , theNew.MemberID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseInfoID" , theNew.CourseInfoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@QuestionInfoID" , theNew.QuestionInfoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@QuestionItemID" , theNew.QuestionItemID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@Value" , theNew.Value ,  E_DbType.VarChar , 100 ) ,
                    new ParameterTag("@CTime" , theNew.CTime ,  E_DbType.DateTime , 8 ),
                    new ParameterTag("@CourseDetaileID" , theNew.CourseDetaileID ,  E_DbType.VarChar , 50 ) 
            };
            rs = theNew.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Set(theNew.AutoID, theNew);
                rs.Data = theNew;
            }
            return rs;
        }


        /// <summary>删除(预留)</summary>
        public static Result Delete_QST(QuestionInfo theQuestionInfo)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@QuestionInfoID" , theQuestionInfo.AutoID ,  E_DbType.VarChar , 50 ) ,
                };
            rs = NONE.EntityMaping_Excute("Delete_QST", ps);
            return rs;
        }
        /// <summary>删除某人某章节习题的所有答题结果</summary>
        public static Result Delete_QST2(WX_Member theMember ,  string detaileID )
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@CourseDetaileID" ,detaileID ,  E_DbType.VarChar , 50 ) 
                };
            rs = NONE.EntityMaping_Excute("Delete_QST2", ps);
            return rs;
        }


        #endregion


    }




    /**/
    /// <summary>记录成员与答题结果的键值对结构类</summary>
    public class QuestionResultToMembers : Dictionary<string, IList<QuestionResult>>
    {
    }
}
