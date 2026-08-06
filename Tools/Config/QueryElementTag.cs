using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Collections;

namespace Tools.Config
{

    /// <summary>包含所有的查询项配置信息的集合类</summary>
    [Serializable]
    public sealed class QueryElementTagCollection : System.Configuration.ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new QueryElementTag();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((QueryElementTag)element).Key;
        }


        public new QueryElementTag this[string name]
        {
            get
            {
                return (QueryElementTag)BaseGet(name);
            }
        }

        public QueryElementTag this[int index]
        {
            get { return (QueryElementTag)BaseGet(index); }
        }

        public void  Add(QueryElementTag element  )
        {
            this.BaseAdd(element);
        }


    }
    /// <summary>配置中的类型的构造信息类</summary>
    [Serializable]
    public sealed class QueryElementTag : System.Configuration.ConfigurationElement
    {
        /// <summary>键的名称</summary>
        [ConfigurationProperty("Key", IsRequired = true, IsKey = true, DefaultValue = "")]
        public string Key
        {
            get
            {
                return this["Key"].ToString();
            }
            set { this["Key"] = value; }
        }

        /// <summary>查询程序过程名</summary>
        [ConfigurationProperty("ProcViewName", IsRequired = true , DefaultValue="")]
        public string ProcViewName
        {
            get
            {
                return this["ProcViewName"].ToString();
            }
            set { this["ProcViewName"] = value; }
        }

        /// <summary>更新存储过程名</summary>
        [ConfigurationProperty("ProcEditName", IsRequired = true, DefaultValue="")]
        public string ProcEditName
        {
            get
            {
                return this["ProcEditName"].ToString();
            }
            set { this["ProcEditName"] = value; }
        }

        /// <summary>应用于所在的类型</summary>
        [ConfigurationProperty("MapingType", IsRequired = true , DefaultValue=null) ]
        public string MapingType
        {
            get
            {
                return this["MapingType"] as string ;
            }
            set { this["MapingType"] = value; }
        }
         


        /// <summary>备注</summary>
        [ConfigurationProperty("Remark", IsRequired = false, DefaultValue = "")]
        public string Remark
        {
            get
            {
                return  this["Remark"].ToString() ;
            }
            set { this["Remark"] = value; }
        }


        /// <summary>查询分支信息的集合</summary>
        [ConfigurationProperty("QItems", IsRequired = false, DefaultValue = null)]
        public QItemCollection QItems
        {
            get
            {
                return (QItemCollection)this["QItems"];
            }
            set { this["QItems"] = value; }
        }

        ///<summary>设置每一个查询项已改变</summary>
        public void SetItemDataChanged()
        {
            foreach (QItem item in this.QItems)
            {
                item.IsDataChanged = true;
            }
        }
    }


    /// <summary>查询分支信息类</summary>
    [Serializable]
    public sealed class QItem : System.Configuration.ConfigurationSection
    {

        /// <summary>键的名称</summary>
        [ConfigurationProperty("Key", IsRequired = true, IsKey = true, DefaultValue = "")]
        public string Key
        {
            get
            {
                return this["Key"].ToString();
            }
            set { this["Key"] = value; }
        }

        /// <summary>查询分支编号</summary>
        [ConfigurationProperty("ViewType", IsRequired = false, DefaultValue = 0)]
        public int ViewType
        {
            get
            {
                return  (int)this["ViewType"]    ;
            }
            set { this["ViewType"] = value; }
        }

        /// <summary>查询条件字符串</summary>
        [ConfigurationProperty("SqlWhere", IsRequired = false, DefaultValue = "")]
        public string SqlWhere
        {
            get
            {
                return this["SqlWhere"].ToString();
            }
            set { this["SqlWhere"] = value; }
        }


        /// <summary>备注</summary>
        [ConfigurationProperty("Remark", IsRequired = false, DefaultValue = "")]
        public string Remark
        {
            get
            {
                return  this["Remark"].ToString() ;
            }
            set { this["Remark"] = value; }
        }


        private bool _IsDataChanged = false ;
        /// <summary>获取或设置一个值，指示数据是否已改变(该属性用于控制是否读取缓存数据)</summary>
        public bool IsDataChanged
        {
            get { return _IsDataChanged; }
            set { _IsDataChanged = value; }
        }
    }

    /// <summary>查询分支信息的集合类</summary>
    [Serializable]
    public sealed class QItemCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new QItem();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((QItem)element).Key ;
        }


        public new QItem this[string name]
        {
            get
            {
                return (QItem)BaseGet(name);
            }
        }

        public QItem this[int index]
        {
            get { return (QItem)BaseGet(index); }
        }

        public QItem Add()
        {
            QItem item = new QItem();
            item.Key = Guid.NewGuid().ToString();
            base.BaseAdd(item);
            return item;
        }

        
    }


}
