using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Tools;

namespace MCR.Mods.VSTO
{
    class Other
    {
    }

    /// <summary>文档类别</summary>
    public enum PPT_FileType
    {
        /// <summary>课件</summary>
        Courseware=1,
        /// <summary>试题</summary>
        Question=2,
        /// <summary>通知</summary>
        Nofity=3,
        /// <summary>投票</summary>
        VoteQuestions=4,
        /// <summary>讨论</summary>
        Discuss=5,
        /// <summary>无类型</summary>
        NONE=100
    }

    /// <summary>文档页类别</summary>
    [EnumDescription("文档页类别")]
    public enum PPT_SlideType
    {
        /// <summary>其它</summary>
        [EnumDescription("其它")]
        NONE = 0 ,
        /// <summary>单选题</summary>
        [EnumDescription("单选题")]
        Question_One,
        /// <summary>多选题</summary>
        [EnumDescription("多选题")]
        Question_More,
        /// <summary>投票</summary>
        [EnumDescription("投票")]
        Vote,
        /// <summary>视频</summary>
        [EnumDescription("视频")]
        Vedio

    }



}
