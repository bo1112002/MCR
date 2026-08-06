using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
    /// <summary>对操作后的结果信息的描述类</summary>
    [Serializable]
    public class Result  
    {
        /// <summary>无操作->false:无权限的操作或无操作(NONE)</summary>
        public static readonly Result NONE = new Result(false , "无权限的操作或无操作(NONE)");
        /// <summary>无操作->true:无权限的操作或无操作(NONE)</summary>
        public static readonly Result NONE_Ture = new Result(true, "无权限的操作或无操作(NONE)");
        /// <summary>正确操作->true</summary>
        public static readonly Result OK = new Result(true, "完成操作(OK)");
        /// <summary>发生未知错误->false</summary>
        public static readonly Result ERR = new Result(false, "发生未知错误(ERR)");
        /// <summary>不允许的操作->false</summary>
        public static readonly Result NotAllow = new Result(false, "不允许的操作(NotAllow)");
        /// <summary>对象无效->false</summary>
        public static readonly Result Invalid = new Result(false, "对象无效(Invalid)");

        public Result()
        { 
        }

        public Result(bool isOK) : this(isOK, string.Empty , null )
        {
        }

        public Result(bool isOK, string description , object data , int rType =0  )
        {
            this.IsOK = isOK;
            this.Description = description;
            this.Data = data;
            this.RType = rType ;
        }
        public Result(bool isOK, string description):this(isOK,description, null)
        {
            this.IsOK = isOK;
            this.Description = description;
        }


        bool _IsOK = false;
        /// <summary>表示结果</summary>
        public bool IsOK
        {
            get { return _IsOK; }
            set { _IsOK = value; }
        }

        private string _Description = string.Empty;
        /// <summary>结果描述</summary>
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        object _Data = null;
        /// <summary>获取或设置返回的数据</summary>
        public object Data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        int _RType = 0;
        /// <summary>获取结果对象的类别</summary>
        public int RType
        {
            get { return _RType; }
            private set { _RType = value; }
        }

        public override string ToString()
        {
            return string.Format("{0}->{1}", this.IsOK, this.Description);
        }
    }
}
