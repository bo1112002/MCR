using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Tools.Config
{
    /// <summary>配置信息的主结点</summary>
    [Serializable]
    public class InterfaceSection :  ConfigurationSection
    {
        [ConfigurationProperty("InterfaceElements", IsRequired = true)]
        public InterfaceElementCollection InterfaceElements
        {
            get
            {
                return (InterfaceElementCollection)this["InterfaceElements"];
            }
        }


        [ConfigurationProperty("QueryElementList", IsRequired = true)]
        public QueryElementTagCollection QueryElementList
        {
            get
            {
                return (QueryElementTagCollection)this["QueryElementList"];
            }
        }
    }

}
