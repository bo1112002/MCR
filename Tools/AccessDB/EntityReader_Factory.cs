using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools.AccessDB;
using System.Data;
using System.Reflection;

namespace Tools.AccessDB
{
    /// <summary>数据读取对象的工厂类</summary>
    class EntityReader_Factory : IEntityReader
    {
        #region IEntityReader 成员

        EntityReader IEntityReader.New(IDataReader reader)
        {
            return new ImpEntityReader(reader);
        }

        #endregion
    }

    /// <summary>实现IDataReader对象的数据读取的处理类</summary>
    class ImpEntityReader : EntityReader
    {
        public ImpEntityReader(IDataReader reader)
            : base(reader)
        {
        }

        /// <summary>通过映射配置中的获取相应的结果值给予对应属性</summary>
        public override T GetValue<T>(object context, string name) 
        {
            name = name.Trim();
            EntityBase entity = context as EntityBase;
            if (entity != null)
            {
                EntityMaping em = entity.GetEntityMaping();
                if (em.Parames.ContainsKey(name))
                {
                    string sName = em.Parames[name].SourceColumn;
                    object obj = this.GetValue(sName);
                    if(obj is T )
                    {
                        return (T)obj ;
                    }
                    else
                    {
                        return (T)Convert.ChangeType(obj, typeof(T));
                    }
                    //throw new Exception(string.Format("映射属性转换失败：类型({0})->属性名({1})({2})", entity.GetTypeKey(), name,e.Message ));
                }
            }
            throw new Exception(string.Format("映射属性失败：类型({0})->属性名({1})", entity.GetTypeKey(), name));

            //return default(T);
        }
        /// <summary>通过反射属性的方式来加载结果值给对象属性</summary>
        public override bool SetValues(object context)
        {
            EntityBase entity = context as EntityBase;
            if (entity == null)
                return false;
            
            Type t = entity.GetTypeBase();
            EntityMaping em = entity.GetEntityMaping();

            PropertyInfo[] pInfos = t.GetProperties( BindingFlags.Public| BindingFlags.Instance| BindingFlags.SetProperty );
            foreach(PropertyInfo pInfo in pInfos )
            {
                string pName = pInfo.Name  ;
                if (em.Parames.ContainsKey(pName) == false)
                    continue;

                object tVal = this.GetValue( em.Parames[pName].SourceColumn  ) ;
                pInfo.SetValue(entity, tVal, null);
            }
            return true;
        }
    }
}
