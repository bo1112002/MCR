using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;
using System.Collections;

namespace Tools.AOP
{
    public interface IAopContext
    {
        IMessage Dispose(AopContextAttribute attr , IMethodCallMessage callMsg );
    }

    public class AopContextAttribute : ProxyAttribute
    {
        readonly string _Key = string.Empty;
        public string Key
        {
            get { return _Key; }
        }

        public AopContextAttribute(string key)
        {
            this._Key = key;
        }

        public override MarshalByRefObject CreateInstance(Type serverType)
        {
            AopContextProxy  proxy = new AopContextProxy(serverType) ;
            return proxy.GetTransparentProxy() as MarshalByRefObject;
        }
    }

    public class AopContextProxy : RealProxy
    {
        public AopContextProxy(Type typ) : base(typ)
        {
        }

        public override IMessage Invoke(IMessage msg)
        {
            if (msg is IConstructionCallMessage)
            {
                IConstructionCallMessage constructCallMsg = msg as IConstructionCallMessage;
                IConstructionReturnMessage constructionReturnMessage = this.InitializeServerObject((IConstructionCallMessage)msg);
                RealProxy.SetStubData(this, constructionReturnMessage.ReturnValue);
                Console.WriteLine("Call constructor");
                return constructionReturnMessage;
            }
            else  //if (myIMessage is IMethodCallMessage)
            {
                IMethodCallMessage callMsg = msg as IMethodCallMessage;
                IMessage message;
                try
                {
                    object[] objAttrs = this.GetProxiedType().GetMethod(callMsg.MethodName).GetCustomAttributes(typeof(AopAttribute), true);
                    if (objAttrs.Length > 0)
                    {
                        AopContextAttribute attr = objAttrs[0] as AopContextAttribute;
                        KeyValueClass theKV = KeyValueClass.Map_KVs["IMethodIntercept"][attr.Key];
                        if (theKV == null)
                        {
                            theKV = KeyValueClass.Map_KVs["IMethodIntercept"];
                        }
                        IAopContext aopHandler = theKV.CreateM_Object(null) as IAopContext;
                        message = aopHandler.Dispose(attr ,  callMsg );
                    }
                    else
                    {
                        object[] args = callMsg.Args;
                        object rtnVal = callMsg.MethodBase.Invoke(GetUnwrappedServer(), args);
                        message = new ReturnMessage(rtnVal, args, args.Length, callMsg.LogicalCallContext, callMsg);
                    }
                }
                catch (Exception e)
                {
                    message =  new ReturnMessage(e, callMsg);
                }
                return message;
            }
        }

    }

    class StopMessage : IMessage
    {

        #region IMessage 成员

        IDictionary IMessage.Properties
        {
            get 
            {
                return new Dictionary<string, object>();
            }
        }

        #endregion
    }
}
