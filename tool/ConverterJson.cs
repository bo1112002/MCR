using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using System.Diagnostics;
using Tools.Config;
using System.Web.Script.Serialization;
using fastJSON;

namespace MCR.tool
{
    /// <summary>进行JSON格式转换的抽象类</summary>
    public abstract class ConverterJson
    {
        /// <summary></summary>
        public abstract string ToJson(object obj);
        /// <summary></summary>
        public abstract object ToObject(string strJson, Type targetType);
        /// <summary>获取基础处理对象</summary>
        public abstract object GetBaseHandler();


        static ConverterJson _Instace = null;
        /// <summary>创建一个ConverterJson的子类实例对象</summary>
        public static ConverterJson CInstace()
        {
            if (_Instace == null)
            {
                ConverterJson theConverter =
                    KeyValueClass.Map_KVs["ConverterJson"].CreateM_Object(null) as ConverterJson;
                if (theConverter != null)
                {
                    _Instace =  theConverter;
                }
                else
                {
                    ConverterJson_Imp theImp = new ConverterJson_Imp();
                    _Instace = theImp;
                }
            }
            return _Instace;
        }   
    }




    /// <summary>JSON格式转换的默认实现类</summary>
    public class ConverterJson_Imp : ConverterJson
    {
        readonly JavaScriptSerializer _Jserialize = new JavaScriptSerializer() ;

        public ConverterJson_Imp()
        {
            _Jserialize.MaxJsonLength = int.MaxValue;
        }

        public override string ToJson(object obj)
        {
            return _Jserialize.Serialize(obj);
        }
        public override object ToObject(string strJson , Type targetType )
        {
            return _Jserialize.Deserialize(strJson, targetType );
        }
        public override object GetBaseHandler()
        {
            return _Jserialize ;
        }
    }


    /// <summary>JSON格式转换的默认实现类</summary>
    public class ConverterJson_FastJson : ConverterJson
    {
        //readonly JavaScriptSerializer _Jserialize = new JavaScriptSerializer();
        readonly JSONParameters _JsonP = null;
        public ConverterJson_FastJson()
        {
            _JsonP = new JSONParameters();
            _JsonP.EnableAnonymousTypes = false ;
            _JsonP.ParametricConstructorOverride = false;
            _JsonP.ShowReadOnlyProperties = true;
            _JsonP.SerializerMaxDepth = 1024;
            _JsonP.UseValuesOfEnums = false ;
            _JsonP.UseEscapedUnicode = false;
            _JsonP.InlineCircularReferences = false ;
            _JsonP.UsingGlobalTypes = false;
            _JsonP.UseExtensions = false;
            _JsonP.UseFastGuid = false;
            _JsonP.UseOptimizedDatasetSchema = false;
            _JsonP.UseUTCDateTime = false;


            
            fastJSON.JSON.RegisterCustomType(typeof(EntityViewControl), 
                (obj) => {
                    EntityViewControl the = obj as EntityViewControl;
                    if (the != null) {
                        Dictionary<string, object> map = new Dictionary<string, object>();
                        the.Serialize(map);
                        the.Doing_SerializeExtend(map);

                        return  fastJSON.JSON.ToJSON(map);
                    }
                    return string.Empty;
                } , 
                (strJson)=>{
                    return null;
                }
             );

            fastJSON.JSON.RegisterCustomType(typeof(EntityBase),
                (obj) =>
                {
                    EntityBase the = obj as EntityBase;
                    if (the != null)
                    {
                        string strJson = fastJSON.JSON.ToJSON(the);
                        the.Serialize_After_Doing();
                        return strJson; 
                    }
                    return string.Empty;
                },
                (strJson) =>
                {
                    return null;
                }
             );


        }


        public override string ToJson(object obj)
        {
            string ss =  fastJSON.JSON.ToJSON(obj, _JsonP );
            return ss;
        }


        readonly JavaScriptSerializer _Jserialize = new JavaScriptSerializer();
        public override object ToObject(string strJson, Type targetType )
        {
            //return  fastJSON.JSON.ToObject(strJson , targetType );
            return _Jserialize.Deserialize(strJson, targetType);
        }

        public override object GetBaseHandler()
        {
            return null ;
        }
    }







}
