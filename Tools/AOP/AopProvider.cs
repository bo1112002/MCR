using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace Tools.AOP
{
    /// <summary>方法对象内容的实现接口</summary>
    public interface IMethodIntercept
    {
        /// <summary>方法对象内容实现方法,mberInfo->原方法对象， lig->新方法的指令对象</summary>
        void MethodBodyImp(AopAttribute aopAttr , MethodBuilder mBuilder , MemberInfo mberInfo , ILGenerator lig );
    }

    /// <summary>
    /// 通过创建代理类来实现切点的操作类(现在只适应物无返回参数的可重写的方法)
    /// </summary>
    /// <example>
    /// AopProvider apv = new AopProvider();
    /// object obj = apv.BuilderType(typeof(TestAop) ,  new Class1() );
    /// TestAop test = obj as TestAop;
    /// </example>
    /// </remarks>
    public class AopProvider
    {
        public static readonly AopProvider TheProvider = new AopProvider();

        private AopProvider()
        { }


        Type _TypeSource = null;
        /// <summary>原始类型对象</summary>
        public Type TypeSource
        {
            get { return _TypeSource; }
            private set
            {
                _TypeSource = value;
            }
        }
        /*
         * .method public hidebysig instance void  SetTag() cil managed
         * .method public hidebysig instance void  ShowText2(string A_1) cil managed
         * .method public hidebysig instance void  ShowText2(string str) cil managed
         *  atb =  MethodAttributes.Public| MethodAttributes.NewSlot; atb |= MethodAttributes.HasSecurity;
         */
        /// <summary>依据方法或构造对象返回其修饰标识</summary>
        public static MethodAttributes GetMethodAttributes(MethodBase mInfo)
        {
            if (mInfo.IsVirtual ) //override
            {
                return MethodAttributes.Public | MethodAttributes.Virtual;
            }
            else //new ->FamANDAssem | Family | HideBySig  MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            {
                return MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot ;
                //return MethodAttributes.Public | MethodAttributes.HideBySig  ;
            }
        }
        /// <summary>获取方法或构造对象的参数类型列表</summary>
        public static Type[] GetMethodInfoTypes(MethodBase tmpMethod)
        {
            ParameterInfo[] pams = tmpMethod.GetParameters();
            Type[] parameTypes = new Type[pams.Length];
            for (int i = 0; i < pams.Length; i++)
            {
                parameTypes[i] = pams[i].ParameterType;
            }
            return parameTypes;
        }

        /// <summary>主程序操作方法->创建目标target类型的代理对象dispose</summary>
        /// <param name="tSource">目标类型,它是放置切点(IAspectDispose)的类型</param>
        /// <param name="dispose">IAspectDispose接口对象</param>
        /// <returns>返回目标类型对象</returns>
        public object BuilderType(Type tSource, params object[] parames)
        {
            this.TypeSource = tSource;

            //创建一个程序集
            AssemblyName asmName = new AssemblyName("MyAssembly");
            AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly( asmName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule(asmName.Name , asmName.Name + ".dll");
            
            //创建自定义类型
            TypeAttributes tAttr = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed;
            TypeBuilder typeBuilder = modBuilder.DefineType(this._TypeSource.Name + "_Aop", tAttr, this.TypeSource );
            

            //创建成员变量
            //FieldBuilder fieldTarget = typTarget.DefineField("_MyDispose", typeof(IAspectDispose), FieldAttributes.Private);


            //ConstructorBuilder ctorTarget = typTarget.DefineDefaultConstructor(MethodAttributes.Public);
             //创建类型的构造器
            foreach( ConstructorInfo cstrInfo in this.TypeSource.GetConstructors())
            {
                Type[] tps = GetMethodInfoTypes(cstrInfo);
                MethodAttributes mAttr = cstrInfo.Attributes;
                ConstructorBuilder ctorTarget = typeBuilder.DefineConstructor( mAttr , cstrInfo.CallingConvention, tps);
                ILGenerator il = ctorTarget.GetILGenerator();

                if (tps.Length > 0)
                {
                    for (int i = 0; i < tps.Length; i++)
                    {
                        il.Emit(OpCodes.Ldarg, i);
                    }
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                }
                il.Emit(OpCodes.Call, cstrInfo );
                il.Emit(OpCodes.Nop);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);

            }
            
            

            //查找原类型的成员
            MemberInfo[] members = this.TypeSource.FindMembers(
                MemberTypes.All,
                BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public ,
                (mInfo, eObject) => {
                    //查找是否为目标方法
                    object[] obj = mInfo.GetCustomAttributes(typeof(AopAttribute), true);
                    return (obj != null && obj.Length > 0);
                }, null);
            foreach (MemberInfo mberInfo in members)
            {
                MethodInfo tmpMethod = mberInfo as MethodInfo;
                object[] obj = tmpMethod.GetCustomAttributes(typeof(AopAttribute), true);
                AopAttribute aopAttr = obj[0] as AopAttribute;
                MethodAttributes mAttr =   GetMethodAttributes(tmpMethod);

                Type[] tps = GetMethodInfoTypes(tmpMethod);

                


                //创建对应的方法
                MethodBuilder mBuilder = typeBuilder.DefineMethod(
                    tmpMethod.Name + "Vir", mAttr, tmpMethod.CallingConvention, tmpMethod.ReturnType, tps);
                ILGenerator il2 = mBuilder.GetILGenerator();

                KeyValueClass theKV = KeyValueClass.Map_KVs["IMethodIntercept"][aopAttr.Key];
                if (theKV == null)
                {
                    theKV = KeyValueClass.Map_KVs["IMethodIntercept"];
                }
                IMethodIntercept mInterceptHandler = theKV.CreateM_Object(null) as IMethodIntercept;
                mInterceptHandler.MethodBodyImp(aopAttr, mBuilder ,mberInfo, il2);

                if (tmpMethod.IsVirtual == false )
                {
                    typeBuilder.DefineMethodOverride(mBuilder, tmpMethod);
                }


            }
            typeBuilder.CreateType(); //表示完成该方法的创建
            asmBuilder.Save(asmName.Name + ".dll");

            Type rsType = typeBuilder.CreateType();
            object rsObject = Activator.CreateInstance(rsType, parames);
            return rsObject ;
        }
        

        /*
                il2.Emit(OpCodes.Ldarg_0);
                il2.Emit(OpCodes.Ldfld, fieldTarget);
                il2.Emit(OpCodes.Ldarg_0);
                il2.Emit(OpCodes.Callvirt, typeof(IAspectDispose).GetMethod("Befor", new Type[] { typeof(object) }));
                il2.Emit(OpCodes.Nop);

                //调用原始类型中指定的方法
                il2.Emit(OpCodes.Ldarg_0);
                for (int i = 0; i < tps.Length; i++)
                    il2.Emit(OpCodes.Ldarg, (i + 1));
                il2.EmitCall(OpCodes.Call, typTarget.BaseType.GetMethod(methodTarget.Name), tps);
                il2.Emit(OpCodes.Nop);


                LocalBuilder tmpObject = null;
                //依据方法的是否有返回值，创建栈位
                if (tmpMethod.ReturnType != typeof(void))
                {
                    tmpObject = il2.DeclareLocal(tmpMethod.ReturnType);
                    il2.Emit(OpCodes.Stloc, tmpObject);
                }

                il2.Emit(OpCodes.Ldarg_0);
                il2.Emit(OpCodes.Ldfld, fieldTarget);
                il2.Emit(OpCodes.Callvirt, typeof(IAspectDispose).GetMethod("After", Type.EmptyTypes));
                il2.Emit(OpCodes.Nop);

                if (tmpMethod.ReturnType != typeof(void))
                {
                    il2.Emit(OpCodes.Ldloc, tmpObject);
                }
                il2.Emit(OpCodes.Ret);

                
                if (mberInfo.MemberType == MemberTypes.Method) 
                { }
                else if (mberInfo.MemberType == MemberTypes.Property)
                { }
                else if (mberInfo.MemberType == MemberTypes.Constructor)
                { }
                else if (mberInfo.MemberType == MemberTypes.Event)
                { }
                else
                { }*/
        

    }


    /// <summary>
    /// 测试
    /// </summary>
    public class DefaultMIntercept_Test : IMethodIntercept
    {
        public DefaultMIntercept_Test()
        { }


        #region IMethodIntercept 成员

        void IMethodIntercept.MethodBodyImp(AopAttribute aopAttr, MethodBuilder mBuilder, MemberInfo mberInfo, ILGenerator ilg)
        {
            MethodInfo mInfo = mberInfo as MethodInfo;
            Type[] tps = AopProvider.GetMethodInfoTypes(mInfo);

            //ilg.Emit(OpCodes.Ldnull);
            //ilg.Emit(OpCodes.Ret);
            //return;
            //MethodInfo thisMehtod=null ;
            //byte[] bsIl = thisMehtod.GetMethodBody().GetILAsByteArray();
            //mBuilder.CreateMethodBody(bsIl, bsIl.Length );

            ilg.Emit(OpCodes.Ldarg_0);

            //调用原始类型中指定的方法
            for (int i = 0; i < tps.Length; i++)
            {
                ilg.Emit(OpCodes.Ldarg, i + 1);
            }
            ilg.Emit(OpCodes.Call, mInfo);

            //用局部变量存储源方法的返回值
            //LocalBuilder locReturn = ilg.DeclareLocal(mInfo.ReturnType);
            //ilg.Emit(OpCodes.Stloc , locReturn);
            //ilg.Emit(OpCodes.Nop);


            ilg.EmitWriteLine("MMMMMMMMMMM-->base->" + mInfo.Name);


            //ilg.Emit(OpCodes.Ldloc, locReturn);
            ilg.Emit(OpCodes.Ret);

        }

        #endregion


        //public Result Test()

    }
}
