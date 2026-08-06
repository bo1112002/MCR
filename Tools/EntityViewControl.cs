using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools
{
    /// <summary>可自定义数据结构来提交给视图</summary>
    [Serializable]
    public abstract class EntityViewControl : EntityBase
    {
        public abstract void Serialize(IDictionary<string, object> map);

        /// <summary>用于对当前对象的序列(Serialize)结构进行扩展处理的事件</summary>
        public event Action<EntityViewControl, IDictionary<string, object>> Evt_SerializeExtend = null;

        /// <summary>触发Evt_SerializeExtend事件(参考Convert_IConvertObject类)</summary>
        public void Doing_SerializeExtend(IDictionary<string, object> map)
        {
            if (this.Evt_SerializeExtend != null)
            {
                //触发事件
                this.Evt_SerializeExtend(this, map);
                //移除当前事件的委托
                Delegate[] dlgts = Evt_SerializeExtend.GetInvocationList();
                foreach (Delegate dgt in dlgts)
                {
                    Delegate.Remove(this.Evt_SerializeExtend, dgt);
                }
                this.Evt_SerializeExtend = null;
            }
        }

    }
}
