using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace MCR.Mods.VSTO
{
    /*
            [0]: {[subscribe, 1]}
            [1]: {[openid, oTcA5wb9F2QqBc37Eiik2EYiGG0U]}
            [2]: {[nickname, bo1112002]}
            [3]: {[sex, 1]}
            [4]: {[language, zh_CN]}
            [5]: {[city, 广州]}
            [6]: {[province, 广东]}
            [7]: {[country, 中国]}
            [8]: {[headimgurl, http://wx.qlogo.cn/mmopen/PiajxSqBRaEKxUSkMA41KCayd6EUj58CJmHDicMtWINlHA52oDyr9IaAjLljIWNCK6PicbsGfaDnQVTDjJTy9KdXOe2YjWhJgx6MFtgqtibgFibM/0]}
            [9]: {[subscribe_time, 1489403552]}
            [10]: {[unionid, ]}
            [11]: {[remark, ]}
            [12]: {[groupid, 2]}
            [13]: {[errcode, 请求成功]}
            [14]: {[errmsg, ]}
            [15]: {[P2PData, ]}
         */

    /// <summary>微信用户原始信息类</summary>
    [Serializable]
    public class WX_UserInfo
    {
        /// <summary>是否已关注</summary>
        public bool Subscribe { get; private set; }
        /// <summary>微信用户号</summary>
        public string OpenID { get; private set; }
        /// <summary>用户昵称</summary>
        public string Nickname { get; private set; }
        /// <summary>性别（1:男，0：女）</summary>
        public int Sex { get; private set; }
        /// <summary>语言</summary>
        public string Language { get; private set; }
        /// <summary>城市</summary>
        public string City { get; private set; }
        /// <summary>省份</summary>
        public string Province { get; private set; }
        /// <summary>国家</summary>
        public string Country { get; private set; }
        /// <summary>用户头像</summary>
        public string HeadImgURL { get; private set; }
        /// <summary>用户分组ID</summary>
        public int GroupID { get; private set; }

        public WX_UserInfo(bool isSubcribe, string openID, string nickName, int sex, string language, 
            string city, string province, string country, string headUrl , int groupID )
        {
            this.Subscribe = isSubcribe;
            this.OpenID = openID;
            this.Nickname = nickName;
            this.Sex = sex;
            this.Language = language;
            this.City = city;
            this.Province = province;
            this.Country = country;
            this.HeadImgURL = headUrl;
            this.GroupID = groupID;
        }
        public WX_UserInfo(IDictionary<string, object> mapInfo)
        {
            this.Subscribe = (Convert.ToInt32( mapInfo["subscribe"] )  == 1 ) ;
            this.OpenID = mapInfo["openid"].ToString();
            this.Nickname = mapInfo["nickname"].ToString();
            this.Sex = Convert.ToInt32(mapInfo["sex"]);
            this.Language = mapInfo["language"].ToString();
            this.City = mapInfo["city"].ToString();
            this.Country = mapInfo["country"].ToString();
            this.Province = mapInfo["province"].ToString();
            this.HeadImgURL = mapInfo["headimgurl"].ToString();
            this.GroupID = Convert.ToInt32( mapInfo["groupid"] ) ;

            if (mapInfo.ContainsKey("MemberInfo") == true)  
            {
                _MemberInfo = mapInfo["MemberInfo"] as IDictionary<string, object>;
            }

        }

        /// <summary>获取当前用户头像的Image对象</summary>
        public Image GetHeadImg()
        {
            Image img = new Bitmap( 60 , 60 ) ;
            if (string.IsNullOrEmpty(this.HeadImgURL))
                return img;

            try
            {
                WebClient theWebClient = new WebClient();
                byte[] bsImg = theWebClient.DownloadData(this.HeadImgURL);
                MemoryStream ms = new MemoryStream(bsImg);
                img = Image.FromStream(ms);
                return img;
            }
            catch
            { 
            }
            return img;
        }

        IDictionary<string, object> _MemberInfo = null;
        /// <summary>当前对应成员的信息，如果不存在，则返回null</summary>
        public IDictionary<string, object> MemberInfo
        {
            get { return _MemberInfo; }
            //set { _MemberInfo = value; }
        }

        


    }
}
