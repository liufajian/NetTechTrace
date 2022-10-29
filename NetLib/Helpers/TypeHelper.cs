using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NetLib.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="propSetterDic"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Type CreateType(Dictionary<string, Type> properties, out Dictionary<string, MethodInfo> propSetterDic, Type? parent = null)
        {
            var newTypeName = Guid.NewGuid().ToString();
            var assemblyName = new AssemblyName(newTypeName);
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("Main");
            var dynamicType = dynamicModule.DefineType(newTypeName,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    parent);
            dynamicType.DefineDefaultConstructor(MethodAttributes.Public |
                                                MethodAttributes.SpecialName |
                                                MethodAttributes.RTSpecialName);

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    AddProperty(dynamicType, property.Key, property.Value);
                }
            }

            var ntype = dynamicType.CreateType();

            propSetterDic = new Dictionary<string, MethodInfo>();

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    var propertyInfo = ntype.GetProperty(property.Key);

                    var setMethod = propertyInfo.GetSetMethod();
                    if (setMethod == null) continue;
                    propSetterDic.Add(property.Key, setMethod);
                }
            }

            return ntype;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="propSetterDic"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Type CreateTypeFromPocoInterface(Type pocoInterface)
        {
            var newTypeName = Guid.NewGuid().ToString();
            var assemblyName = new AssemblyName(newTypeName);
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("Main");
            var dynamicType = dynamicModule.DefineType(newTypeName,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);

            dynamicType.DefineDefaultConstructor(MethodAttributes.Public |
                                                MethodAttributes.SpecialName |
                                                MethodAttributes.RTSpecialName);

            foreach (var property in pocoInterface.GetProperties())
            {
                AddProperty(dynamicType, property.Name, property.PropertyType);
            }

            var ntype = dynamicType.CreateType();

            return ntype;
        }

        private static void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            var getMethod = typeBuilder.DefineMethod("get_" + propertyName,
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getMethodIL = getMethod.GetILGenerator();
            getMethodIL.Emit(OpCodes.Ldarg_0);
            getMethodIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getMethodIL.Emit(OpCodes.Ret);

            var setMethod = typeBuilder.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });
            var setMethodIL = setMethod.GetILGenerator();
            Label modifyProperty = setMethodIL.DefineLabel();
            Label exitSet = setMethodIL.DefineLabel();

            setMethodIL.MarkLabel(modifyProperty);
            setMethodIL.Emit(OpCodes.Ldarg_0);
            setMethodIL.Emit(OpCodes.Ldarg_1);
            setMethodIL.Emit(OpCodes.Stfld, fieldBuilder);

            setMethodIL.Emit(OpCodes.Nop);
            setMethodIL.MarkLabel(exitSet);
            setMethodIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethod);
            propertyBuilder.SetSetMethod(setMethod);
        }
    }
}