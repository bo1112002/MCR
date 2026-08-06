using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools
{
    /// <summary>计算标识</summary>
    public enum E_PriceBillingMode
    {

        [EnumDescription("未知")]
        None = 0,
        /// <summary>百份比(%)</summary>
        [EnumDescription("百份比(%)")]
        Percentage,
        /// <summary>代替价格</summary>
        [EnumDescription("代替价格")]
        Replace,
        /// <summary>优惠金额(-+)</summary>
        [EnumDescription("优惠金额(-+)")]
        DealsMoney
    }
}
