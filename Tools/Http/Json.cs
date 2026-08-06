using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Web.Script.Serialization;
using System.Reflection;
using System.Web;

namespace Tools.Http
{
    /// <summary>
    /// JsonObject : IReturnJson接口的对象类->Val:String
    /// </summary>
    public class JsonObject : IReturnJson
    {
        readonly object _Val;

        public object Val
        {
            get { return _Val; }
        }

        public JsonObject(object o)
        {
            _Val = o;
        }

        #region IReturnJson 成员

        string IReturnJson.GetJsonString( JavaScriptSerializer jSerialize)
        {
            return "{Val:\"" + _Val + "\"}";
        }

        #endregion
    }

    /// <summary>
    ///StringJson:IReturnJson接口列表类
    /// </summary>
    public class JsonList<T> : List<T>, IReturnJson
    {
        public JsonList()
        {
        }

        #region IReturnJson 成员

        string IReturnJson.GetJsonString( JavaScriptSerializer jSerialize)
        {
            return jSerialize.Serialize(this);
        }

        #endregion


    }


    /// <summary>
    ///StringJson:IReturnJson接口的Hashtable类
    /// </summary>
    public class JsonHashtable : Hashtable, IReturnJson
    {
        public JsonHashtable()
        {
        }

        public JsonHashtable(IDictionary dic)
        {
            foreach (object o in dic.Keys)
            {
                this[o] = dic[o];
            }
        }

        #region IReturnJson 成员

        string IReturnJson.GetJsonString(JavaScriptSerializer jSerialize)
        {
            return jSerialize.Serialize(this);
        }

        #endregion

    }

   


    public abstract class EntityProxy : IReturnJson
    {
        #region IReturnJson 成员

        string IReturnJson.GetJsonString(System.Web.Script.Serialization.JavaScriptSerializer jSerialize)
        {
            return jSerialize.Serialize(this);
        }

        #endregion
    }


    class JsonDictionary : Dictionary<string, EntityProxy>, IReturnJson
    {
        #region IReturnJson 成员

        string IReturnJson.GetJsonString(System.Web.Script.Serialization.JavaScriptSerializer jSerialize)
        {
            return jSerialize.Serialize(this);
        }

        #endregion
    }


    /*===================================================================*/
    /*===================================================================*/
    /// <summary>
    ///JavaScriptConverter_Me 的摘要说明
    /// </summary>
    public class JavaScriptConverter_Me
    {
        public JavaScriptConverter_Me()
        {
        }
    }


    public class DateTimeConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new Type[] { typeof(DateTime) };
            }
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            if (obj is DateTime)
            {
                DateTime d = (DateTime)obj;
                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("Value", d.Ticks);
                result.Add("Text", d.ToString("yyyy-MM-dd HH:mm"));
                return result;
            }
            return new Dictionary<string, object>();
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary != null && type == typeof(DateTime))
            {
                DateTime d = DateTime.FromBinary((long)dictionary["Value"]);
                return d;
            }
            return DateTime.MinValue;
        }

    }

    public class MethodObjectConverter : JavaScriptConverter
    {

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            MethodObject method = new MethodObject();
            method.ClassName = dictionary["ClassName"].ToString();
            method.CommandName = dictionary["CommandName"].ToString();
            method.CommandValue = dictionary["CommandValue"].ToString();
            method.MethodName = dictionary["MethodName"].ToString();
            method.Tag = dictionary["Tag"].ToString();
            ArrayList array = dictionary["ParamterArray"] as ArrayList;
            if (array != null && array.Count > 0)
            {
                method.Paramters.Clear();
                foreach (IDictionary dic in array)
                {
                    method.Paramters.Add(dic["ParamterName"].ToString(), dic["Data"].ToString());
                    
                    //ParamterObject p = new ParamterObject();
                    //p.ParamterName = dic["ParamterName"].ToString();
                    //p.ParamterType = dic["ParamterType"].ToString();
                    //if (p.ParamterType == "object")
                    //{
                    //    string str = HttpContext.Current.Server.UrlDecode(dic["Data"].ToString());
                    //    object o = serializer.DeserializeObject(str);
                    //    p.Data = o;
                    //}
                    //else
                    //{
                    //    p.Data = dic["Data"];
                    //}
                    //method.ParamterArray.Add(p);
                }
            }
            return method;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            MethodObject mObject = obj as MethodObject;
            if (mObject == null)
                return null;

            PropertyInfo[] infos = typeof(MethodObject).GetProperties();
            foreach (PropertyInfo the in infos)
            {
                //the.
                map.Add(the.Name, the.GetValue(mObject, null));
            }
            serializer.Serialize(obj);

            return map;
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return new Type[] { typeof(MethodObject) }; }
        }
    }

    /*
    public class HouseShoppingCartConverter : JavaScriptConverter
    {
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            string houseAutoID = (dictionary["HouseAutoID"]??"").ToString() ;
            HouseShoppingCart cart = new HouseShoppingCart(houseAutoID);

            ArrayList array =  dictionary["ListMerchandise"] as ArrayList ;
            foreach( IDictionary d in array )
            {
                ShoppingInfo info = new ShoppingInfo();
                info.M_AutoID = d["M_AutoID"].ToString();
                info.M_Base64Pic = d["M_Base64Pic"].ToString();
                info.M_Enable = Convert.ToBoolean( d["M_Enable"] ) ;
                info.M_IsStroe = Convert.ToBoolean( d["M_IsStroe"] ) ;
                info.M_MerchandiseTypeID = d["M_MerchandiseTypeID"].ToString();
                info.M_Name = d["M_Name"].ToString();
                info.M_Price = Convert.ToDouble( d["M_Price"] ) ;
                info.M_ProviderID = d["M_ProviderID"].ToString();
                info.M_Remark = d["M_Remark"].ToString();
                info.M_Unit = d["M_Unit"].ToString();
                long lg = (long)(d["M_UpdateTime"] as IDictionary)["Value"];
                info.M_UpdateTime = new DateTime( lg )  ;

                //long lg2 = (long)(d["ShoppingTime"] as IDictionary)["Value"];
                info.ShoppingTime = Convert.ToDateTime(d["ShoppingTime"]);
                cart.ListMerchandise.Add(  new ShoppingInfo()  );
            }
            return cart;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {

            Dictionary<string, object> result = new Dictionary<string, object>();
            HouseShoppingCart cart = obj as HouseShoppingCart;
            if (cart != null)
            {
                result.Add("HouseAutoID", cart.HouseAutoID);
                result.Add("ListMerchandise", cart.ListMerchandise.ToArray() );
            }
            return result;
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return new Type[] { typeof(HouseShoppingCart) }; }
        }
    }
    */
}
