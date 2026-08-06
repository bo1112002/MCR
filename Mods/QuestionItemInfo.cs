using MCR.Mods.VSTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>题项信息类(QITEM)</summary>
    [Serializable]
    public class QuestionItemInfo : EntityBase
    {
        #region 持久属性
        string _QuestionID = string.Empty;
        /// <summary>所属的题目ID</summary>
        public string QuestionID
        {
            get { return _QuestionID; }
            set { _QuestionID = value; }
        }
        bool _IsVal = false;
        /// <summary>是否为正确值</summary>
        public bool IsVal
        {
            get { return _IsVal; }
            set { _IsVal = value; }
        }
        string _ValText = string.Empty;
        /// <summary>题项内容</summary>
        public string ValText
        {
            get { return _ValText; }
            set { _ValText = value; }
        }
        string _ItemKey = Guid.NewGuid().ToString();
        /// <summary>题项的内置标号</summary>
        public string ItemKey
        {
            get { return _ItemKey; }
            set { _ItemKey = value; }
        }

        string _TagString = string.Empty;
        /// <summary>题项的外置标号</summary>
        public string TagString
        {
            get { return _TagString; }
            set { _TagString = value; }
        }

        #endregion


        protected QuestionItemInfo() { }

        #region============= 重写成员=========>>>


        public override Type GetTypeBase()
        {
            return typeof(QuestionItemInfo);
        }

        protected override string GetPrefixName()
        {
            return "QITEM";
        }

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.QuestionID = reader.GetValue<string>(this, "QuestionID");
            this.IsVal = reader.GetValue<bool>(this, "IsVal");
            this.ValText = reader.GetValue<string>(this, "ValText");
            this.ItemKey = reader.GetValue<string>(this, "ItemKey");
            this.TagString = reader.GetValue<string>(this, "TagString");

        }
        #endregion=============END==========<<<


        /// <summary>获取所属的题目对象</summary>
        public QuestionInfo GetQuestionInfo()
        {
            return QuestionInfo.GetByID(this.QuestionID);
        }

        //============================
        /// <summary>修改当前信息</summary>
        public Result Update(bool isVal, string valText)
        {
            Result rs = Result.NONE;
            if (string.IsNullOrEmpty(valText.Trim()) == true)
                return new Result(false, "题项内容不能为空");

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , this.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@IsVal" , isVal ,  E_DbType.Bit , 1 ) ,
                    new ParameterTag("@ValText" , valText ,  E_DbType.VarChar , 500 ) ,
            };
            rs = this.EntityMaping_Excute("Update", ps);
            if (rs.IsOK)
            {
                this.IsVal = isVal;
                this.ValText = valText;
            }
            return rs;
        }



        #region 静态成员
        public static readonly QuestionItemInfo NONE = new QuestionItemInfo();

        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static QuestionItemInfo GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
            {
                return null;
            }
            QuestionItemInfo the = EntityBase.GetMyICache().Get(autoID) as QuestionItemInfo;
            if (the == null)
            {
                ParameterTag[] ps =  { 
                    new ParameterTag("@AutoID" , autoID ,  E_DbType.VarChar , 50 ) 
                                 };
                Result rs = NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new QuestionItemInfo();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        /// <summary>依据物理唯一标识获取对象(不存在则返回null)</summary>
        public static QuestionItemInfo GetByItemKey(string itemKey)
        {
            QuestionItemInfo the = null;
            ParameterTag[] ps =  { 
                    new ParameterTag("@ItemKey" , itemKey ,  E_DbType.VarChar , 100 ) 
                                 };
            Result rs = NONE.EntityMaping_Excute("GetByItemKey", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    string id = readers[0].GetValue(0).ToString();
                    the = GetByID(id);
                }
            });
            return the;
        }

        ///<summary>获取指定题目的题项集合 </summary>
        public static IList<QuestionItemInfo> GetListByQuest(QuestionInfo theQuest)
        {
            List<QuestionItemInfo> list = new List<QuestionItemInfo>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@QuestionID" , theQuest.AutoID ,  E_DbType.VarChar , 50 ),
                };
            Result rs = NONE.EntityMaping_Excute("GetListByQuest", ps, (readers) =>
            {
                EntityBase.AddToList<QuestionItemInfo>(list, readers, (r) => { return new QuestionItemInfo(); });
            });
            return list;
        }


        ///<summary>获取某个成员的题项与相应答题结果信息集合 </summary>
        public static IList<QuestionItemInfo> GetListByMemberQResult(WX_Member theMember, CourseInfo theCourse, QuestionInfo theQuest)
        {
            List<QuestionItemInfo> list = new List<QuestionItemInfo>();
            ParameterTag[] ps =  
                { 
                    new ParameterTag("@MemberID" , theMember.AutoID ,  E_DbType.VarChar , 50 ),
                     new ParameterTag("@CourseInfoID" , theCourse.AutoID ,  E_DbType.VarChar , 50 ),
                      new ParameterTag("@QuestionID" , theQuest.AutoID ,  E_DbType.VarChar , 50 )
                };
            Result rs = NONE.EntityMaping_Excute("GetListByMemberQResult", ps, (readers) =>
            {
                EntityBase.AddToList<QuestionItemInfo>(list, readers, (r) => { return new QuestionItemInfo(); }, (theAdd, read) =>
                {
                    string qResultID = read.GetValue("QResultID").ToString();
                    theAdd.ExtPropertys["QResultID"] = qResultID;
                    if (theAdd.IsVal && string.IsNullOrEmpty(qResultID) == false)
                    {
                        theAdd.ExtPropertys["IsOK"] = true;
                    }
                    else
                    {
                        theAdd.ExtPropertys["IsOK"] = false;
                    }

                    return theAdd;
                });
            });
            return list;
        }

        

        //==========更新操作===========

        /// <summary>添加</summary>
        public static Result Insert(QuestionInfo quest, MC_QuestionItemClass item)
        {
            QuestionItemInfo info = QuestionItemInfo.GetByItemKey(item.ItemKey);
            if (info != null)
                return new Result(true, string.Empty, info, 100);

            Result rs = Result.NONE;
            QuestionItemInfo the = new QuestionItemInfo();
            the.QuestionID = quest.AutoID;
            the.IsVal = item.IsVal;
            the.ValText = item.ValText;
            the.ItemKey = item.ItemKey;
            the.TagString = item.TagString;

            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , the.AutoID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@QuestionID" , the.QuestionID ,  E_DbType.VarChar , 50 ) ,
                    new ParameterTag("@IsVal" , the.IsVal ,  E_DbType.Bit , 4 ) ,
                    new ParameterTag("@ValText" , the.ValText ,  E_DbType.VarChar , 500 ) ,
                    new ParameterTag("@ItemKey" , the.ItemKey ,  E_DbType.VarChar , 100 ) ,
                    new ParameterTag("@TagString" , the.TagString ,  E_DbType.VarChar , 50 ) 

            };
            rs = the.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                EntityBase.GetMyICache().Set(the.AutoID, the);
                rs.Data = the;
            }
            return rs;
        }
        /// <summary>批量添加 </summary>
        public static Result Insert_Batch(QuestionInfo quest, IList<MC_QuestionItemClass> items)
        {
            int count = 0;
            Result rs = new Result(true, "添加成功");
            foreach (MC_QuestionItemClass item in items)
            {
                rs = QuestionItemInfo.Insert(quest, item);
                if (rs.IsOK)
                {
                    count++;
                }
            }

            Result rs2 = new Result(true, string.Format("成功添加完成{0}个题项", count));
            return rs2;
        }

        /// <summary>删除(预留)</summary>
        public static Result Delete(QuestionItemInfo theItem)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@AutoID" , theItem.AutoID ,  E_DbType.VarChar , 50 ) ,
                };
            rs = NONE.EntityMaping_Excute("Delete", ps);
            return rs;
        }

        /// <summary>删除</summary>
        public static Result Delete(QuestionInfo theQuest)
        {
            Result rs = Result.NONE;
            ParameterTag[] ps = new ParameterTag[] { 
                    new ParameterTag("@QuestionID" , theQuest.AutoID ,  E_DbType.VarChar , 50 ) ,
                };
            rs = NONE.EntityMaping_Excute("DeleteByQuestionInfo", ps);
            return rs;
        }


        #endregion

    }
}
