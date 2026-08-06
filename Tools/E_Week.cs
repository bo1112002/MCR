using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools
{
    public enum E_Week
    {
        星期日, 星期一, 星期二, 星期三, 星期四, 星期五, 星期六
    }

    public enum E_Week2
    {
        [EnumDescription_Bool("星期日")]
        W0,
        [EnumDescription_Bool("星期一")]
        W1,
        [EnumDescription_Bool("星期二")]
        W2,
        [EnumDescription_Bool("星期三")]
        W3,
        [EnumDescription_Bool("星期四")]
        W4,
        [EnumDescription_Bool("星期五")]
        W5,
        [EnumDescription_Bool("星期六")]
        W6
    }
    public enum E_Month
    {
        [EnumDescription_Bool("一月")]
        M1 =1 ,
        [EnumDescription_Bool("二月")]
        M2,
        [EnumDescription_Bool("三月")]
        M3,
        [EnumDescription_Bool("四月")]
        M4,
        [EnumDescription_Bool("五月")]
        M5,
        [EnumDescription_Bool("六月")]
        M6,
        [EnumDescription_Bool("七月")]
        M7,
        [EnumDescription_Bool("八月")]
        M8,
        [EnumDescription_Bool("九月")]
        M9,
        [EnumDescription_Bool("十月")]
        M10,
        [EnumDescription_Bool("十一月")]
        M11,
        [EnumDescription_Bool("十二月")]
        M12
    }

    /// <summary>日期格式化类</summary>
    public class DateTimeFormater
    {
        private DateTimeFormater()
        { }

        /// <summary>带星期的时间格式化方法</summary>
        /// <param name="date">指定时间</param>
        public static string ToString( DateTime date )
        {
            return string.Format("{0} {1} " , 
                date.ToString("yyyy年MM月dd日") , ((E_Week)date.DayOfWeek).ToString() );
        }
    }

}
