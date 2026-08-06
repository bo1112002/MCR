using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools
{
    /// <summary>指定对象用的接口</summary>
    public interface IAssignationValue<T>
    {
        T Value { get; set; }
    }
}
