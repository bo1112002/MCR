using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCR.Mods.VSTO
{

    /// <summary>选择题结构类</summary>
    [Serializable]
    public class MC_QuestionClass
    {
        string _GID = Guid.NewGuid().ToString();
        /// <summary>当前题的唯一物理标识</summary>
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
            get 
            { 
                return _Caption; 
            }
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
        /// <summary>课程章节ID</summary>
        public string CourseDetaileID
        {
            get { return _CourseDetaileID; }
            set { _CourseDetaileID = value; }
        }

        string _AutoID = string.Empty;
        /// <summary>当前题的物理标识ID[可选项或保留项]</summary>
        public string AutoID
        {
            get { return _AutoID; }
            set { _AutoID = value; }
        }


        public MC_QuestionClass() 
        { 
        }

        public MC_QuestionClass(PPT_SlideType slideType)
        {
            this.QType = slideType;
        }
        //==============================

        /// <summary>获取当前题项的文本描述</summary>
        public string GetQTypeString()
        {
            switch( this.QType )
            {
                case PPT_SlideType.Question_More:
                    return "多选";
                case PPT_SlideType.Question_One:
                    return "单选";
                case PPT_SlideType.Vote:
                    return "投票";
                case PPT_SlideType.Vedio:
                    return "视频";
                default:
                    return "其它";
            }
        }

        /// <summary>获取正确答案数</summary>
        public int GetResultOK_Count()
        {
            int count = 0;
            foreach (MC_QuestionItemClass theItem in this.Items)
            {
                if (theItem.IsVal == true)
                    count++;
            }
            return count;
        }


        //=================================
        IList<MC_QuestionItemClass> _Items = new List<MC_QuestionItemClass>();
        /// <summary>题项集合</summary>
        public IList<MC_QuestionItemClass> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }

        /// <summary>创建一个新题项,返回当前对象所在的集合索引</summary>
        public int AddItem(string txt, bool isVal)
        {
            MC_QuestionItemClass the = new MC_QuestionItemClass(this.GID, txt, isVal);
            Items.Add(the);
            return Items.Count - 1;
        }
        /// <summary>移除指定一个索引值（如果该索引值的对应项存在，则成功移除）</summary>
        public void RemoveItem(int index)
        {
            if (Items.Count > index)
            {
                Items.RemoveAt(index);
            }
        }



    }

    /// <summary>题项</summary>
    [Serializable]
    public class MC_QuestionItemClass
    {

        string _GID = string.Empty;
        /// <summary>所属题的GID</summary>
        public string GID
        {
            get { return _GID; }
            private set { _GID = value; }
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
            get 
            { 
                 return _ValText   ; 
            }
            set { 
                _ValText = value   ; 
            }
        }

        string _ItemKey = Guid.NewGuid().ToString();
        /// <summary>题项的标号</summary>
        public string ItemKey
        {
            get { return _ItemKey; }
            set { _ItemKey = value; }
        }


        string _TagString = string.Empty;
        /// <summary>ACB标识</summary>
        public string TagString
        {
            get { return _TagString; }
            set { _TagString = value; }
        }


        string _AutoID = string.Empty;
        /// <summary>当前题的物理标识ID[可选项或保留项]</summary>
        public string AutoID
        {
            get { return _AutoID; }
            set { _AutoID = value; }
        }

        /*======================*/

        public MC_QuestionItemClass() { }
        public MC_QuestionItemClass(string gid, string valText, bool isVal)
        {
            this.GID = gid;
            this.ValText = valText;
            this.IsVal = isVal;
        }

    }


}
