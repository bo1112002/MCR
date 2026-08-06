using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Tools.Config
{
    /// <summary>配置中的类型的构造信息</summary>
    [Serializable]
    public class InterfaceElement : System.Configuration.ConfigurationElement
    {
        /// <summary>
        /// 标识名
        /// </summary>
        [ConfigurationProperty("Key", IsRequired = true, IsKey = true, DefaultValue = "")]
        public string Key
        {
            get
            {
                return this["Key"].ToString();
            }
        }

        /// <summary>
        /// 实现类的Dll文件的对路径
        /// </summary>
        [ConfigurationProperty("DllPath", IsRequired = true, DefaultValue = "")]
        public string DllPath
        {
            get
            {
                return this["DllPath"].ToString();
            }
        }

        /// <summary>
        /// 实现类的完整类型名
        /// </summary>
        [ConfigurationProperty("ImpTypeFullName", IsRequired = true, DefaultValue = "")]
        public string ImpTypeFullName
        {
            get
            {
                return this["ImpTypeFullName"].ToString();
            }
        }

        /// <summary>
        /// 获取构造函数的参数集
        /// </summary>
        [ConfigurationProperty("Parames", IsRequired = false, DefaultValue = null)]
        public ParameItemCollection Parames
        {
            get
            {
                return (ParameItemCollection)this["Parames"];
            }
        }
    }
    [Serializable]
    public class InterfaceElementCollection : System.Configuration.ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new InterfaceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((InterfaceElement)element).Key;
        }


        public new InterfaceElement this[string name]
        {
            get
            {
                return (InterfaceElement)BaseGet(name);
            }
        }

        public InterfaceElement this[int index]
        {
            get { return (InterfaceElement)BaseGet(index); }
        }


    }



    /// <summary>
    /// 在配置信息中的构造参数信息
    /// </summary>
    [Serializable]
    public class ParameItem : System.Configuration.ConfigurationSection
    {
        /// <summary>在配置中指定构造参数的类型</summary>
        public enum ElementKeyToParamterType
        {
            [EnumDescription("System.String")]
            String,
            [EnumDescription("System.Boolean")]
            Bool,
            [EnumDescription("System.Int32")]
            Int,
            [EnumDescription("System.UInt32")]
            UInt,
            [EnumDescription("System.Decimal")]
            Decimal,
            [EnumDescription("System.DateTime")]
            DateTime,
            [EnumDescription("System.Object")]
            ElementKey

        }

        /// <summary>参数的名称</summary>
        [ConfigurationProperty("Name", IsRequired = true, IsKey = true, DefaultValue = "")]
        public string Name
        {
            get
            {
                return this["Name"].ToString();
            }
        }

        /// <summary>获取ElementKeyToParamterType值之一,用于指定参数的类型</summary>
        [ConfigurationProperty("ParamterType", IsRequired = true, DefaultValue = ElementKeyToParamterType.String)]
        public ElementKeyToParamterType ParamterType
        {
            get
            {
                return (ElementKeyToParamterType)this["ParamterType"];
            }
        }

        /// <summary>参数的值</summary>
        [ConfigurationProperty("Value", IsRequired = true, DefaultValue = "")]
        public string Value
        {
            get
            {
                return this["Value"].ToString();
            }
            set
            {
                this["Value"] = value;
            }
        }
    }

    [Serializable]
    public class ParameItemCollection : System.Configuration.ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ParameItem();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ParameItem)element).Name;
        }


        public new ParameItem this[string name]
        {
            get
            {
                return (ParameItem)BaseGet(name);
            }
        }

        public ParameItem this[int index]
        {
            get { return (ParameItem)BaseGet(index); }
        }


    }
}
