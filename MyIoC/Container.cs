using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MyIoC
{
    public class Container
    {
        private readonly HashSet<Type> registeredTypes = new HashSet<Type>();
        private readonly List<Assembly> dependencyAssemblys = new List<Assembly>();
        private readonly Dictionary<Type, Dictionary<object, int[]>> store = new Dictionary<Type, Dictionary<object, int[]>>();
        private static readonly Container.GetImportFieldType GetImportFieldInfo = (Container.GetImportFieldType)(type => ((IEnumerable<FieldInfo>)type.GetFields()).Where<FieldInfo>((Func<FieldInfo, bool>)(field => field.IsDefined(typeof(ImportAttribute), true))));
        private static readonly Container.GetImportPropertyType GetImportPropertyInfo = (Container.GetImportPropertyType)(type => ((IEnumerable<PropertyInfo>)type.GetProperties()).Where<PropertyInfo>((Func<PropertyInfo, bool>)(property => property.IsDefined(typeof(ImportAttribute), true))));
        private static readonly Container.GetImportConstructorType GetImportConstructorInfo = (Container.GetImportConstructorType)(type => ((IEnumerable<ConstructorInfo>)type.GetConstructors()).FirstOrDefault<ConstructorInfo>((Func<ConstructorInfo, bool>)(ctor => ctor.IsDefined(typeof(ImportConstructorAttribute), true))));

        public static bool IsHaveImportAttribute(Type type)
        {
            if (!Container.GetImportPropertyInfo(type).Any<PropertyInfo>() && !Container.GetImportFieldInfo(type).Any<FieldInfo>())
                return type.GetCustomAttribute<ImportConstructorAttribute>() != null;
            return true;
        }

        public Container()
        {
        }

        public Container(params Assembly[] assemblys)
        {
            this.RegistrateAssemblys(((IEnumerable<Assembly>)assemblys).ToArray<Assembly>());
        }

        public Container(params Type[] types)
        {
            this.RegistrateTypes(((IEnumerable<Type>)types).ToArray<Type>());
        }

        public Container(IEnumerable<Type> types, IEnumerable<Assembly> assemblys)
        {
            this.RegistrateTypes(types.ToArray<Type>());
            this.RegistrateAssemblys(assemblys.ToArray<Assembly>());
        }

        public void RegistrateAssembly(Assembly assembly)
        {
            this.RegistrateAssemblys(assembly);
        }

        public void RegistrateAssemblys(params Assembly[] assemblys)
        {
            if (assemblys == null || assemblys.Length == 0)
                throw new ArgumentException("Input parameter can not be null or empty", nameof(assemblys));
            foreach (Assembly assembly in assemblys)
                ;
        }

        public void RegistrateType(Type type)
        {
            this.RegistrateTypes(type);
        }

        public void RegistrateTypes(params Type[] types)
        {
            this.RegistrateTypes(false, types);
        }

        private void RegistrateTypes(bool recursion, params Type[] types)
        {
            if (types == null || types.Length == 0)
                throw new ArgumentException("Input parameter can not be null or empty", nameof(types));
            foreach (Type type in types)
            {
                if ((object)type == null)
                    throw new ArgumentException("Type can not be null");
                Dictionary<object, int[]> dictionary;
                if (!this.store.TryGetValue(type, out dictionary))
                {
                    ConstructorInfo constructorInfo = Container.GetImportConstructorInfo(type);
                    if ((object)constructorInfo == null)
                    {
                        this.store.Add(type, new Dictionary<object, int[]>()
            {
              {
                (object) type.GetConstructor(Type.EmptyTypes),
                (int[]) null
              }
            });
                        FieldInfo[] array1 = Container.GetImportFieldInfo(type).ToArray<FieldInfo>();
                        PropertyInfo[] array2 = Container.GetImportPropertyInfo(type).ToArray<PropertyInfo>();
                        Type[] array3 = ((IEnumerable<PropertyInfo>)array2).Select<PropertyInfo, Type>((Func<PropertyInfo, Type>)(prop => prop.PropertyType)).Union<Type>(((IEnumerable<FieldInfo>)array1).Select<FieldInfo, Type>((Func<FieldInfo, Type>)(field => field.FieldType))).Distinct<Type>().Except<Type>((IEnumerable<Type>)this.registeredTypes).ToArray<Type>();
                        this.registeredTypes.UnionWith((IEnumerable<Type>)array3);
                        if (((IEnumerable<Type>)array3).Any<Type>())
                        {
                            List<Type> tempList = new List<Type>((IEnumerable<Type>)this.registeredTypes);
                            foreach (PropertyInfo propertyInfo in array2)
                            {
                                Type pt = propertyInfo.PropertyType;
                                this.store[type].Add((object)propertyInfo, ((IEnumerable<Type>)array3).Select<Type, int>((Func<Type, int>)(item => tempList.IndexOf(pt))).ToArray<int>());
                            }
                            foreach (FieldInfo fieldInfo in array1)
                            {
                                Type pt = fieldInfo.FieldType;
                                this.store[type].Add((object)fieldInfo, ((IEnumerable<Type>)array3).Select<Type, int>((Func<Type, int>)(item => tempList.IndexOf(pt))).ToArray<int>());
                            }
                            this.RegistrateTypes(true, array3);
                        }
                    }
                    else
                    {
                        Type[] array = ((IEnumerable<ParameterInfo>)constructorInfo.GetParameters()).Select<ParameterInfo, Type>((Func<ParameterInfo, Type>)(param => param.ParameterType)).ToArray<Type>();
                        this.registeredTypes.UnionWith((IEnumerable<Type>)array);
                        List<Type> tempList = new List<Type>((IEnumerable<Type>)this.registeredTypes);
                        this.store.Add(type, new Dictionary<object, int[]>()
            {
              {
                (object) constructorInfo,
                ((IEnumerable<Type>) array).Select<Type, int>((Func<Type, int>) (item => tempList.IndexOf(item))).ToArray<int>()
              }
            });
                        this.RegistrateTypes(true, array);
                    }
                    this.registeredTypes.Add(type);
                    ExportAttribute customAttribute = type.GetCustomAttribute<ExportAttribute>();
                    if (customAttribute == null)
                    {
                        if (recursion)
                        {
                            this.registeredTypes.Remove(type);
                            this.store.Remove(type);
                        }
                    }
                    else if (customAttribute.Contract != null)
                    {
                        Type contract = customAttribute.Contract;
                    }
                }
            }
        }

        public IBindType<object> BindType(Type baseType)
        {
            return (IBindType<object>)null;
        }

        public void ToType(Type type)
        {
        }

        public object CreateInstance(Type type)
        {
            return (object)null;
        }

        public T CreateInstance<T>()
        {
            return default(T);
        }

        private delegate IEnumerable<FieldInfo> GetImportFieldType(Type targeType);

        private delegate IEnumerable<PropertyInfo> GetImportPropertyType(Type targeType);

        private delegate ConstructorInfo GetImportConstructorType(Type targeType);
    }
}
