using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ID.Infrastructure.Enums
{
    public class CustomEnumBuilder
    {
        private EnumBuilder enumBuilder;
        private int index;
        private AssemblyBuilder _assemblyBuilder;
        private AssemblyName _name;
        public CustomEnumBuilder(string enumname)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            _name = new AssemblyName("Tms.DynamicTypes");
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(_name, AssemblyBuilderAccess.Run);
            ModuleBuilder mb = _assemblyBuilder.DefineDynamicModule("TMS.Dynamics");
            enumBuilder = mb.DefineEnum(enumname, TypeAttributes.Public, typeof(int));
        }
        /// <summary>
        /// adding one string to enum
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public FieldBuilder add(string s)
        {
            FieldBuilder f = enumBuilder.DefineLiteral(s, index);
            index++;
            return f;
        }

        /// <summary>
        /// adding array to enum
        /// </summary>
        /// <param name="s"></param>
        public void addRange(string[] s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                enumBuilder.DefineLiteral(s[i], i);
            }
        }

        /// <summary>
        /// getting index 0
        /// </summary>
        /// <returns></returns>
        public object getEnum()
        {
            Type finished = enumBuilder.CreateTypeInfo();
            //_assemblyBuilder.Save(_name.Name + ".dll");
            object o1 = Enum.Parse(finished, "0");
            return o1;
        }

        /// <summary>
        /// getting with index
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object getEnum(int i)
        {
            Type finished = enumBuilder.CreateTypeInfo();
            //_assemblyBuilder.Save(_name.Name + ".dll");
            object o1 = Enum.Parse(finished, i.ToString());
            return o1;
        }
    }

    public class FieldDescriptor
    {
        public FieldDescriptor(string fieldName, Type fieldType)
        {
            FieldName = fieldName;
            FieldType = fieldType;
        }
        public string FieldName { get; }
        public Type FieldType { get; }
    }

    public static class MyTypeBuilder
    {
        public static object CreateNewObject()
        {
            var myTypeInfo = CompileResultTypeInfo();
            var myType = myTypeInfo.AsType();
            var myObject = Activator.CreateInstance(myType);

            return myObject;
        }

        public static TypeInfo CompileResultTypeInfo()
        {
            TypeBuilder tb = GetTypeBuilder();
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            var yourListOfFields = new List<FieldDescriptor>()
            {
                new FieldDescriptor("YourProp1",typeof(string)),
                new FieldDescriptor("YourProp2", typeof(int))
            };
            foreach (var field in yourListOfFields)
                CreateProperty(tb, field.FieldName, field.FieldType);

            TypeInfo objectTypeInfo = tb.CreateTypeInfo();
            return objectTypeInfo;
        }

        private static TypeBuilder GetTypeBuilder()
        {
            var typeSignature = "MyDynamicType";
            var an = new AssemblyName(typeSignature);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);
            return tb;
        }

        private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
    }
}
