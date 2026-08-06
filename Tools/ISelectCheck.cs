using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools
{
    /// <summary>用于选取某个对象的接口</summary>
    public interface ISelectCheck
    {
        /// <summary>获取或设置选择</summary>
        bool Checked { get; set; }
        /// <summary>获取显示名</summary>
        string Name { get; }
        /// <summary>获取操作对象</summary>
        object CurrentObject { get; }
    }


    public class SelectCheckClass : ISelectCheck
    {
        bool _Checked = false;
        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                _Checked = value;
            }
        }

        string _Name = string.Empty ;
        public string Name
        {
            get { return _Name; }
        }

        object _CurrentObject = null;
        public object CurrentObject
        {
            get { return _CurrentObject; }
        }

        int _Val = 0;
        /// <summary>对应的值</summary>
        public int Val
        {
            get { return _Val; }
        }



        public SelectCheckClass(string name, int val , object obj= null)
        {
            this._Name = name;
            this._CurrentObject = obj;
            this._Val = val;
        }

    }
}
