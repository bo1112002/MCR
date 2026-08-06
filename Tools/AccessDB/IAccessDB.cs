using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Tools.AccessDB
{
    /// <summary>数据持久的接口</summary>
    public interface IAccessDB : IBase
    {
        /// <summary>创建记录</summary>
        Result Insert();
        /// <summary>更新记录</summary>
        Result Update();
        /// <summary>删除记录</summary>
        Result Delete();
        /// <summary>把一个reader对象填充当前对象的属性值(默认实现为通过反射赋值，请尽可能的重写该方法)</summary>
        void ToEntity(EntityReader reader);


    }
}
