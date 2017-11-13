// Decompiled with JetBrains decompiler
// Type: MyIoC.DynamicMethodBuilder
// Assembly: MyIoC, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6CFC37AA-7E42-420C-92FE-E4296AEBCF8C
// Assembly location: E:\Task_MyIoC\ConsoleTest\bin\Debug\MyIoC.dll

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace MyIoC
{
    public static class DynamicMethodBuilder
    {
        private static DynamicMethod dynamicMethod;

        public static ILGenerator Init(ConstructorInfo constructorInfo, Type returnType = null)
        {
            returnType = returnType ?? constructorInfo.DeclaringType;

            dynamicMethod = new DynamicMethod("DM$OBJ_FACTORY_" + returnType, returnType, Type.EmptyTypes);

            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Newobj, constructorInfo);

            return ilGenerator;
        }

        public static ILGenerator AddCtor(this ILGenerator generator, ConstructorInfo constructorInfo, Type initType = null)
        {
            if (ReferenceEquals(generator, null))
            {
                return Init(constructorInfo, initType);
            }

            generator.Emit(OpCodes.Newobj, constructorInfo);
            
            return generator;
        }

        public static ILGenerator AddField(this ILGenerator generator, FieldInfo fieldInfo)
        {
            generator.Emit(OpCodes.Stfld, fieldInfo);

            return generator;
        }

        public static ILGenerator AddProperty(this ILGenerator generator, PropertyInfo propertyInfo)
        {
            generator.Emit(OpCodes.Callvirt, propertyInfo.SetMethod);

            return generator;
        }

        public static ILGenerator Dup(this ILGenerator generator)
        {
            generator.Emit(OpCodes.Dup);

            return generator;
        }

        public static Func<T> Compile<T>(this ILGenerator generator)
        {
            generator.Emit(OpCodes.Ret);

            return (Func<T>)dynamicMethod.CreateDelegate(typeof(Func<T>));
        }

        public static Func<object> Compile(this ILGenerator generator)
        {
            generator.Emit(OpCodes.Ret);

            return (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));
        }
    }
}