using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools
{
    /// <summary>视图类</summary>
    public abstract class EntityView
    {
        readonly Dictionary<string, object> _Extend = new Dictionary<string, object>(50);
        /// <summary>获取扩展属性对象</summary>
        public Dictionary<string, object> Extend
        {
            get { return _Extend; }
        }
    }
}
