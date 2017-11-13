// Decompiled with JetBrains decompiler
// Type: MyIoC.DynamicMethodBuilder
// Assembly: MyIoC, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6CFC37AA-7E42-420C-92FE-E4296AEBCF8C
// Assembly location: E:\Task_MyIoC\ConsoleTest\bin\Debug\MyIoC.dll

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MyIoC
{
    public static class DynamicMethodBuilder
    {
        private static DynamicMethod dynamicMethod;

        public static ILGenerator Init(ConstructorInfo constructorInfo)
        {
            dynamicMethod = new DynamicMethod("DM$OBJ_FACTORY_" + constructorInfo.DeclaringType,
                constructorInfo.DeclaringType,
                constructorInfo.GetParameters().Select(param => param.ParameterType).ToArray(),
                constructorInfo.DeclaringType);

            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Newobj, constructorInfo);
            return ilGenerator;
        }

        public static ILGenerator AddCtor(this ILGenerator generator, ConstructorInfo constructorInfo)
        {
            generator.Emit(OpCodes.Newobj, constructorInfo);
            return generator;
        }

        public static ILGenerator AddFiled(this ILGenerator generator, FieldInfo fieldInfo)
        {
            generator.Emit(OpCodes.Newobj, fieldInfo);
            return generator;
        }

        public static ILGenerator AddProp(this ILGenerator generator, PropertyInfo propertyInfo, Type value)
        {
            generator.EmitCall(OpCodes.Call, propertyInfo.SetMethod, new Type[1]
            {
                value
            });
            return generator;
        }

        public static Func<T> Compile<T>(this ILGenerator generator) where T : class
        {
            generator.Emit(OpCodes.Pop);
            generator.Emit(OpCodes.Ret);
            return (Func<T>) dynamicMethod.CreateDelegate(typeof(Func<T>));
        }
    }
}