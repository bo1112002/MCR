using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools.AOP
{
    /// <summary>标识切点的特性</summary>
    public class AopAttribute : Attribute
    {
        readonly string _Key = string.Empty;
        public string Key
        {
            get { return _Key; }
        }

        public AopAttribute(string key)
        {
            this._Key = key;
        }
    }
}
