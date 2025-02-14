﻿using System.Reflection;
using WCell.Intercommunication.DataTypes;
using WCell.Util.ReflectionUtil;

namespace WCell.Core
{
  public class ReflectUtil : MemberAccessor<IRoleGroup>
  {
    public static readonly ReflectUtil Instance = new ReflectUtil();

    public override bool CanRead(MemberInfo member, IRoleGroup user)
    {
      return true;
    }

    public override bool CanWrite(MemberInfo member, IRoleGroup user)
    {
      return true;
    }
  }
}