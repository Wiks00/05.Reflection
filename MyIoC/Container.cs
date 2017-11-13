using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using MyIoC.Exceptions;

namespace MyIoC
{
    public class Container
    {
        private static readonly GetImportFieldType GetImportFieldInfo =
            type => 
                type.GetFields().Where(field => field.IsDefined(typeof(ImportAttribute), true));

        private static readonly GetImportPropertyType GetImportPropertyInfo =
            type => 
                type.GetProperties().Where(property => property.IsDefined(typeof(ImportAttribute), true));

        private static readonly GetImportConstructorType GetImportConstructorInfo =
            type =>
                type.GetConstructors().FirstOrDefault(ctor => ctor.IsDefined(typeof(ImportConstructorAttribute), true));

        private readonly List<Assembly> dependencyAssemblys = new List<Assembly>();
        private /*readonly*/ HashSet<Type> registeredTypes = new HashSet<Type>();

        private static Dictionary<Type, Func<object>> compiledCreators = new Dictionary<Type, Func<object>>();

        private readonly Dictionary<Type, Dictionary<object, int[]>> builderStore =
            new Dictionary<Type, Dictionary<object, int[]>>();

        public Container()
        {
        }

        public Container(params Assembly[] assemblys)
        {
            RegistrateAssemblys(assemblys.ToArray());
        }

        public Container(params Type[] types)
        {
            RegistrateTypes(types.ToArray());
        }

        public Container(IEnumerable<Type> types, IEnumerable<Assembly> assemblys)
        {
            RegistrateTypes(types.ToArray());
            RegistrateAssemblys(assemblys.ToArray());
        }

        public static bool IsHaveImportAttribute(Type type)
        {
            if (!GetImportPropertyInfo(type).Any() && !GetImportFieldInfo(type).Any())
                return type.GetCustomAttribute<ImportConstructorAttribute>() != null;
            return true;
        }

        public void RegistrateAssembly(Assembly assembly)
        {
            RegistrateAssemblys(assembly);
        }

        public void RegistrateAssemblys(params Assembly[] assemblys)
        {
            if (assemblys == null || assemblys.Length == 0)
            {
                throw new ArgumentException("Input parameter can not be null or empty", nameof(assemblys));
            }

            foreach (var assembly in assemblys);
        }

        public void RegistrateTypes(params Type[] types)
        {
            InnerRegistrateTypes(false, types);
        }

        private void InnerRegistrateTypes(bool recursion, params Type[] types)
        {
            if (types == null || types.Length == 0)
            {
                throw new TypeRegistrationException($"Input parameter {nameof(types)} can not be null or empty");
            }

            foreach (var type in types)
            {
                if (ReferenceEquals(type, null))
                {
                    throw new TypeRegistrationException("Type can not be null");
                }

                Dictionary<object, int[]> dictionary;

                if (builderStore.TryGetValue(type, out dictionary))
                {
                    continue;
                }

                var constructorInfo = GetImportConstructorInfo(type);

                if (ReferenceEquals(constructorInfo, null))
                {
                    try
                    {
                        builderStore.Add(type, new Dictionary<object, int[]>
                        {
                            {
                                type.GetConstructor(Type.EmptyTypes),
                                null
                            }
                        });
                    }
                    catch (ArgumentNullException)
                    {
                        throw new ImportConstructorException(
                            $"Error while registering type {type} - cannot access to default constructor. Use {nameof(ImportConstructorAttribute)} to register dependencies");
                    }

                    var fields = GetImportFieldInfo(type).ToArray();
                    var propertys = GetImportPropertyInfo(type).ToArray();

                    var nestedTypes =
                        propertys.Select(prop => prop.PropertyType)
                            .Union(fields.Select(field => field.FieldType))
                            .Distinct()
                            .Except(registeredTypes)
                            .ToArray();

                    if (fields.Any() || propertys.Any())
                    {
                        registeredTypes.UnionWith(nestedTypes);

                        var tempList = new List<Type>(registeredTypes);

                        foreach (var propertyInfo in propertys)
                        {
                            var pt = propertyInfo.PropertyType;
                            //ToDo : new [] { IndexOf } 
                            builderStore[type].Add(propertyInfo, new []{ tempList.IndexOf(pt) });
                        }

                        foreach (var fieldInfo in fields)
                        {
                            var pt = fieldInfo.FieldType;
                            //ToDo : new [] { IndexOf } 
                            builderStore[type].Add(fieldInfo, new[] { tempList.IndexOf(pt) });
                        }

                        if (nestedTypes.Any())
                        {                         
                            InnerRegistrateTypes(true, nestedTypes);
                        }
                    }
                }
                else
                {
                    var parameters = constructorInfo.GetParameters().Select(param => param.ParameterType).ToArray();

                    registeredTypes.UnionWith(parameters);
                    var tempList = new List<Type>(registeredTypes);

                    builderStore.Add(type, new Dictionary<object, int[]>
                    {
                        {
                            constructorInfo,
                            parameters.Select(item => tempList.IndexOf(item)).ToArray()
                        }
                    });

                    InnerRegistrateTypes(true, parameters);
                }

                registeredTypes.Add(type);
                var customAttribute = type.GetCustomAttribute<ExportAttribute>();

                if (ReferenceEquals(customAttribute, null))
                {
                    if (recursion)
                    {
                        registeredTypes.Remove(type);
                        builderStore.Remove(type);
                    }
                }
                else
                {
                    if (!ReferenceEquals(customAttribute.Contract, null))
                    {
                        var tempList = registeredTypes.ToList();
                        var contract = customAttribute.Contract;
                        var value = builderStore[type];

                        tempList[tempList.IndexOf(type)] = contract;

                        registeredTypes = new HashSet<Type>(tempList);

                        builderStore.Remove(type);
                        builderStore.Add(contract,value);
                    }
                }
            }
        }

        #region BindType

        public BindType<object> BindType(Type type)
        {
            registeredTypes.Add(type);

            return new BindType<object>(this);
        }

        public BindType<T1> BindType<T1>() where T1 : class
        {
            registeredTypes.Add(typeof(T1));

            return new BindType<T1>(this);
        }

        public BindType<object> BindTypes(params Type[] baseTypes)
        {
            registeredTypes.UnionWith(baseTypes);

            return new BindType<object>(this);
        }

        public BindType<T1, T2> BindTypes<T1,T2>() where T1 : class
                                                   where T2 : class
        {
            registeredTypes.UnionWith(new [] {typeof(T1), typeof(T2)});

            return new BindType<T1, T2>(this);
        }

        public BindType<T1, T2, T3> BindTypes<T1, T2, T3>() where T1 : class
                                                            where T2 : class
                                                            where T3 : class
        {
            registeredTypes.UnionWith(new[] { typeof(T1), typeof(T2), typeof(T3) });

            return new BindType<T1, T2, T3>(this);
        }

        public BindType<T1, T2, T3, T4> BindTypes<T1, T2, T3, T4>() where T1 : class
                                                                    where T2 : class
                                                                    where T3 : class
                                                                    where T4 : class
        {
            registeredTypes.UnionWith(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });

            return new BindType<T1, T2, T3, T4>(this);
        }

        #endregion

        public object CreateInstance(Type type)
        {
            Func<object> constructor;

            if (!compiledCreators.TryGetValue(type, out constructor))
            {
                ILGenerator methodBuilder = Create(type);

                constructor = methodBuilder.Compile();

                compiledCreators.Add(type, constructor);
            }

            return constructor();
        }

        public T CreateInstance<T>() where T : class
        {
            Func<object> constructor;

            Type type = typeof(T);

            if (!compiledCreators.TryGetValue(type, out constructor))
            {
                ILGenerator methodBuilder = Create(type);

                constructor = methodBuilder.Compile<T>();

                compiledCreators.Add(type, (Func<T>)constructor);
            }

            return (T)constructor();
        }

        private ILGenerator Create(Type type)
        {
            ILGenerator methodBuilder = null;

            var initCtor = builderStore[type].First();
            var parameters = initCtor.Value;

            if (ReferenceEquals(parameters, null))
            {
                methodBuilder = DynamicMethodBuilder.Init((ConstructorInfo)initCtor.Key, type);

                if (builderStore[type].Count > 1)
                {
                    foreach (var dependence in builderStore[type].Skip(1))
                    {
                        var info = dependence.Key;

                        methodBuilder = methodBuilder.Dup();

                        methodBuilder = InitDependence(methodBuilder, registeredTypes.ElementAt(dependence.Value[0]));

                        if (info is PropertyInfo)
                        {
                            methodBuilder = methodBuilder.AddProperty((PropertyInfo)info);
                        }

                        if (info is FieldInfo)
                        {
                            methodBuilder = methodBuilder.AddField((FieldInfo)info);
                        }
                    }
                }
            }
            else
            {
                foreach (var dependence in parameters)
                {
                    methodBuilder = InitDependence(methodBuilder, registeredTypes.ElementAt(dependence), type);
                }

                methodBuilder = methodBuilder.AddCtor((ConstructorInfo)initCtor.Key);
            }

            return methodBuilder;
        }

        private ILGenerator InitDependence(ILGenerator methodBuilder, Type type, Type initType = null)
        {
            var instructions = builderStore[type];

            var ctor = instructions.First();

            if (ReferenceEquals(ctor.Value, null))
            {
                methodBuilder = methodBuilder.AddCtor((ConstructorInfo)ctor.Key, initType);

                if (builderStore[type].Count > 1)
                {
                    foreach (var dependence in builderStore[type].Skip(1))
                    {
                        var info = dependence.Key;

                        methodBuilder = methodBuilder.Dup();

                        methodBuilder = InitDependence(methodBuilder, registeredTypes.ElementAt(dependence.Value[0]), initType);

                        if (info is PropertyInfo)
                        {
                            methodBuilder = methodBuilder.AddProperty((PropertyInfo)info);
                        }

                        if (info is FieldInfo)
                        {
                            methodBuilder = methodBuilder.AddField((FieldInfo)info);
                        }
                    }
                }
            }
            else
            {
                foreach (var dependence in ctor.Value)
                {
                    methodBuilder = InitDependence(methodBuilder, registeredTypes.ElementAt(dependence));
                }

                methodBuilder = methodBuilder.AddCtor((ConstructorInfo)ctor.Key);
            }

            return methodBuilder;
        }

        private delegate IEnumerable<FieldInfo> GetImportFieldType(Type targeType);

        private delegate IEnumerable<PropertyInfo> GetImportPropertyType(Type targeType);

        private delegate ConstructorInfo GetImportConstructorType(Type targeType);
    }
}