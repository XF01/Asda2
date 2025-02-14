﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace WCell.Util.DynamicAccess
{
  public static class AccessorMgr
  {
    private static readonly Dictionary<Type, Dictionary<MemberInfo, IGetterSetter>> m_accessors =
      new Dictionary<Type, Dictionary<MemberInfo, IGetterSetter>>();

    private static readonly Dictionary<Type, IProducer> m_defaultCtors = new Dictionary<Type, IProducer>();
    public static readonly Dictionary<Type, OpCode> PropTypesHashes = new Dictionary<Type, OpCode>();

    public const BindingFlags DefaultBindingFlags =
      BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private static int nextTypeId;
    private static ModuleBuilder _module;

    static AccessorMgr()
    {
      PropTypesHashes[typeof(sbyte)] = OpCodes.Ldind_I1;
      PropTypesHashes[typeof(byte)] = OpCodes.Ldind_U1;
      PropTypesHashes[typeof(char)] = OpCodes.Ldind_U2;
      PropTypesHashes[typeof(short)] = OpCodes.Ldind_I2;
      PropTypesHashes[typeof(ushort)] = OpCodes.Ldind_U2;
      PropTypesHashes[typeof(int)] = OpCodes.Ldind_I4;
      PropTypesHashes[typeof(uint)] = OpCodes.Ldind_U4;
      PropTypesHashes[typeof(long)] = OpCodes.Ldind_I8;
      PropTypesHashes[typeof(ulong)] = OpCodes.Ldind_I8;
      PropTypesHashes[typeof(bool)] = OpCodes.Ldind_I1;
      PropTypesHashes[typeof(double)] = OpCodes.Ldind_R8;
      PropTypesHashes[typeof(float)] = OpCodes.Ldind_R4;
    }

    /// <summary>
    /// Copies all public properties that have a setter and a getter and exist in the types of both objects from input to output.
    /// Ignores all properties that have the <see cref="T:WCell.Util.DynamicAccess.DontCopyAttribute" />.
    /// </summary>
    /// <returns></returns>
    public static IGetterSetter GetOrCreateAccessor(Type type, PropertyInfo info)
    {
      IGetterSetter getterSetter;
      if(!GetOrCreateAccessors(type).TryGetValue(info, out getterSetter))
        throw new Exception("Tried to get accessor for invalid Property: " + info);
      return getterSetter;
    }

    public static ModuleBuilder GetOrCreateModule()
    {
      if(_module == null)
        _module = CreateModule();
      return _module;
    }

    public static ModuleBuilder CreateModule()
    {
      return Thread.GetDomain().DefineDynamicAssembly(new AssemblyName
      {
        Name = "PropertyAccessorAssembly"
      }, AssemblyBuilderAccess.Run).DefineDynamicModule("Module");
    }

    public static Dictionary<MemberInfo, IGetterSetter> GetOrCreateAccessors<T>()
    {
      return GetOrCreateAccessors(typeof(T));
    }

    public static Dictionary<MemberInfo, IGetterSetter> GetOrCreateAccessors(Type type)
    {
      Dictionary<MemberInfo, IGetterSetter> accessors;
      if(!m_accessors.TryGetValue(type, out accessors))
        m_accessors.Add(type, accessors = CreateAccessors(type));
      return accessors;
    }

    public static IProducer GetOrCreateDefaultProducer(Type type)
    {
      IProducer producer;
      m_defaultCtors.TryGetValue(type, out producer);
      return CreateDefaultCtor(type);
    }

    private static Dictionary<MemberInfo, IGetterSetter> CreateAccessors(Type type)
    {
      Dictionary<MemberInfo, IGetterSetter> dictionary = new Dictionary<MemberInfo, IGetterSetter>();
      MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                             BindingFlags.NonPublic);
      ModuleBuilder module1 = GetOrCreateModule();
      foreach(MemberInfo key in members)
      {
        if(((DontCopyAttribute[]) key.GetCustomAttributes(typeof(DontCopyAttribute), false)).Length == 0)
        {
          IGetterSetter module2;
          if(key.MemberType == MemberTypes.Property)
            module2 = AddToModule(module1, (PropertyInfo) key);
          else if(key.MemberType == MemberTypes.Field)
            module2 = AddToModule(module1, (FieldInfo) key);
          else
            continue;
          dictionary.Add(key, module2);
        }
      }

      return dictionary;
    }

    private static IProducer CreateDefaultCtor(Type type)
    {
      IProducer module =
        AddToModule(GetOrCreateModule(), type.GetConstructor(new Type[0]));
      m_defaultCtors.Add(type, module);
      return module;
    }

    private static void Test()
    {
      X x1 = new X();
      FieldInfo field = x1.GetType().GetField("a");
      ConstructorInfo constructor = x1.GetType().GetConstructor(new Type[0]);
      EmitAssembly(field);
      IProducer producer = EmitAssembly(constructor);
      X x2;
      Utility.Measure("Create object", 2000000, () => x2 = new X());
      X x3;
      Utility.Measure("Dynamically create object", 2000000,
        () => x3 = (X) producer.Produce());
      Utility.Measure("Create object through reflection", 2000000,
        () => Activator.CreateInstance<X>());
    }

    private static IGetterSetter EmitAssembly(FieldInfo field)
    {
      return AddToModule(GetOrCreateModule(), field);
    }

    private static IProducer EmitAssembly(ConstructorInfo ctor)
    {
      return AddToModule(GetOrCreateModule(), ctor);
    }

    internal static IProducer AddToModule(ModuleBuilder module, ConstructorInfo ctor)
    {
      if(ctor.DeclaringType.IsValueType)
        throw new ArgumentException("Cannot emit Constructors of value-types (yet): " +
                                    ctor.DeclaringType);
      Type declaringType = ctor.DeclaringType;
      string str = NextTypeName(ctor.GetFullMemberName());
      TypeBuilder typeBuilder = module.DefineType(str, TypeAttributes.Public);
      typeBuilder.AddInterfaceImplementation(typeof(IProducer));
      Type[] parameterTypes = new Type[0];
      Type returnType = typeof(object);
      ILGenerator ilGenerator = typeBuilder.DefineMethod("Produce",
        MethodAttributes.Public | MethodAttributes.Virtual, returnType, parameterTypes).GetILGenerator();
      ilGenerator.DeclareLocal(declaringType);
      ilGenerator.Emit(OpCodes.Newobj, ctor);
      ilGenerator.Emit(OpCodes.Stloc_0);
      ilGenerator.Emit(OpCodes.Ldloc_0);
      ilGenerator.Emit(OpCodes.Ret);
      typeBuilder.CreateType();
      IProducer instance = module.Assembly.CreateInstance(str) as IProducer;
      if(instance == null)
        throw new Exception("Unable to create producer.");
      return instance;
    }

    private static string NextTypeName(string s)
    {
      s = s.Replace('.', '_').Replace(" ", "_").Replace(',', '_').Replace("`", "_").Replace("[", "_")
        .Replace("]", "_").Replace("=", "_");
      return "Dynamic" + s + nextTypeId++;
    }

    public static IGetterSetter AddToModule(ModuleBuilder module, FieldInfo field)
    {
      string str = NextTypeName(field.GetFullMemberName());
      Type declaringType = field.DeclaringType;
      Type fieldType = field.FieldType;
      TypeBuilder typeBuilder = module.DefineType(str, TypeAttributes.Public);
      typeBuilder.AddInterfaceImplementation(typeof(IGetterSetter));
      Type[] parameterTypes1 = new Type[1]
      {
        typeof(object)
      };
      Type returnType = typeof(object);
      ILGenerator ilGenerator1 = typeBuilder.DefineMethod("Get",
        MethodAttributes.Public | MethodAttributes.Virtual, returnType, parameterTypes1).GetILGenerator();
      ilGenerator1.DeclareLocal(typeof(object));
      ilGenerator1.Emit(OpCodes.Ldarg_1);
      ilGenerator1.Emit(OpCodes.Castclass, declaringType);
      ilGenerator1.Emit(OpCodes.Ldfld, field);
      if(fieldType.IsValueType)
        ilGenerator1.Emit(OpCodes.Box, fieldType);
      ilGenerator1.Emit(OpCodes.Stloc_0);
      ilGenerator1.Emit(OpCodes.Ldloc_0);
      ilGenerator1.Emit(OpCodes.Ret);
      Type[] parameterTypes2 = new Type[2]
      {
        typeof(object),
        typeof(object)
      };
      ILGenerator ilGenerator2 = typeBuilder.DefineMethod("Set",
        MethodAttributes.Public | MethodAttributes.Virtual, null, parameterTypes2).GetILGenerator();
      ilGenerator2.DeclareLocal(fieldType);
      ilGenerator2.Emit(OpCodes.Ldarg_1);
      ilGenerator2.Emit(OpCodes.Castclass, declaringType);
      ilGenerator2.Emit(OpCodes.Ldarg_2);
      if(fieldType.IsValueType)
        ilGenerator2.Emit(OpCodes.Unbox_Any, fieldType);
      ilGenerator2.Emit(OpCodes.Stfld, field);
      ilGenerator2.Emit(OpCodes.Ret);
      typeBuilder.CreateType();
      IGetterSetter instance = module.Assembly.CreateInstance(str) as IGetterSetter;
      if(instance == null)
        throw new Exception("Unable to create Field accessor.");
      return instance;
    }

    public static IGetterSetter AddToModule(ModuleBuilder module, PropertyInfo prop)
    {
      string str = NextTypeName(prop.GetFullMemberName());
      Type declaringType = prop.DeclaringType;
      TypeBuilder typeBuilder = module.DefineType(str, TypeAttributes.Public);
      typeBuilder.AddInterfaceImplementation(typeof(IGetterSetter));
      Type[] parameterTypes1 = new Type[1]
      {
        typeof(object)
      };
      Type returnType1 = typeof(object);
      ILGenerator ilGenerator1 = typeBuilder.DefineMethod("Get",
        MethodAttributes.Public | MethodAttributes.Virtual, returnType1, parameterTypes1).GetILGenerator();
      MethodInfo getMethod = prop.GetGetMethod();
      if(getMethod != null)
      {
        ilGenerator1.DeclareLocal(typeof(object));
        ilGenerator1.Emit(OpCodes.Ldarg_1);
        ilGenerator1.Emit(OpCodes.Castclass, declaringType);
        ilGenerator1.EmitCall(OpCodes.Call, getMethod, null);
        if(getMethod.ReturnType.IsValueType)
          ilGenerator1.Emit(OpCodes.Box, getMethod.ReturnType);
        ilGenerator1.Emit(OpCodes.Stloc_0);
        ilGenerator1.Emit(OpCodes.Ldloc_0);
      }
      else
        ilGenerator1.ThrowException(typeof(MissingMethodException));

      ilGenerator1.Emit(OpCodes.Ret);
      Type[] parameterTypes2 = new Type[2]
      {
        typeof(object),
        typeof(object)
      };
      Type returnType2 = null;
      ILGenerator ilGenerator2 = typeBuilder.DefineMethod("Set",
        MethodAttributes.Public | MethodAttributes.Virtual, returnType2, parameterTypes2).GetILGenerator();
      MethodInfo setMethod = prop.GetSetMethod();
      if(setMethod != null)
      {
        Type parameterType = setMethod.GetParameters()[0].ParameterType;
        ilGenerator2.DeclareLocal(parameterType);
        ilGenerator2.Emit(OpCodes.Ldarg_1);
        ilGenerator2.Emit(OpCodes.Castclass, declaringType);
        ilGenerator2.Emit(OpCodes.Ldarg_2);
        if(parameterType.IsValueType)
        {
          ilGenerator2.Emit(OpCodes.Unbox, parameterType);
          OpCode opcode;
          if(PropTypesHashes.TryGetValue(parameterType, out opcode))
            ilGenerator2.Emit(opcode);
          else
            ilGenerator2.Emit(OpCodes.Ldobj, parameterType);
        }
        else
          ilGenerator2.Emit(OpCodes.Castclass, parameterType);

        ilGenerator2.EmitCall(OpCodes.Callvirt, setMethod, null);
      }
      else
        ilGenerator2.ThrowException(typeof(MissingMethodException));

      ilGenerator2.Emit(OpCodes.Ret);
      typeBuilder.CreateType();
      IGetterSetter instance = module.Assembly.CreateInstance(str) as IGetterSetter;
      if(instance == null)
        throw new Exception("Unable to create property accessor for \"" + prop + "\" of type: " +
                            prop.DeclaringType.FullName);
      return instance;
    }

    public class X
    {
      public int a;
    }
  }
}