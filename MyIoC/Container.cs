using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                throw new ArgumentException("Input parameter can not be null or empty", nameof(assemblys));
            foreach (var assembly in assemblys)
                ;
        }

        public void RegistrateType(Type type)
        {
            RegistrateTypes(type);
        }

        public void RegistrateTypes(params Type[] types)
        {
            RegistrateTypes(false, types);
        }

        private void RegistrateTypes(bool recursion, params Type[] types)
        {
            if (types == null || types.Length == 0)
            {
                throw new ArgumentException("Input parameter can not be null or empty", nameof(types));
            }

            foreach (var type in types)
            {
                if ((object) type == null)
                {
                    throw new ArgumentException("Type can not be null");
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
                            $"Error while registering type {type} - no default constructor. Or use {nameof(ImportConstructorAttribute)} to register dependencies");
                    }

                    var fields = GetImportFieldInfo(type).ToArray();
                    var propertys = GetImportPropertyInfo(type).ToArray();

                    var nestedTypes =
                        propertys.Select(prop => prop.PropertyType)
                            .Union(fields.Select(field => field.FieldType))
                            .Distinct()
                            .Except(registeredTypes)
                            .ToArray();

                    registeredTypes.UnionWith(nestedTypes);

                    if (nestedTypes.Any())
                    {
                        var tempList = new List<Type>(registeredTypes);

                        foreach (var propertyInfo in propertys)
                        {
                            var pt = propertyInfo.PropertyType;

                            builderStore[type].Add(propertyInfo, nestedTypes.Select(item => tempList.IndexOf(pt)).ToArray());
                        }

                        foreach (var fieldInfo in fields)
                        {
                            var pt = fieldInfo.FieldType;

                            builderStore[type].Add(fieldInfo, nestedTypes.Select(item => tempList.IndexOf(pt)).ToArray());
                        }

                        RegistrateTypes(true, nestedTypes);
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
                    RegistrateTypes(true, parameters);
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

        //public IBindType<object> BindType(Type baseType)
        //{
        //    return (IBindType<object>) null;
        //}

        public void ToType(Type type)
        {
        }

        public object CreateInstance(Type type)
        {
            return null;
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