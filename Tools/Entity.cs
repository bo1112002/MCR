using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools
{
    /// <summary>
    /// 实体的基类
    /// <remarks> </remarks>
    /// </summary>
    public abstract class Entity : IComparable
    {
        protected Entity()
        {
        }

        /// <summary>获取当前对象的标识值</summary>
        public abstract string GetMainID();
        /// <summary>后期验证</summary>
        public abstract Result Validate();


        bool _IsReadOnly = false;
        /// <summary>获取或设置当前对象是否为只读的标识</summary>
        public bool IsReadOnly
        {
            get { return _IsReadOnly; }
            set { _IsReadOnly = value; }
        }

        public override string ToString()
        {
            return GetMainID();
        }

        #region IComparable 成员

        int IComparable.CompareTo(object obj)
        {
            Entity en = obj as Entity;
            if (en != null)
            {
                return this.GetMainID().CompareTo(en.GetMainID());
            }
            return -1;
        }

        public override bool Equals(object obj)
        {
            Entity en = obj as Entity;
            if (en != null)
            {
                return this.GetMainID().Equals(en.GetMainID());
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.GetMainID().GetHashCode();
        }

        #endregion
    }
}
