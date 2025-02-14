﻿using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Core;
using WCell.Intercommunication.DataTypes;

namespace WCell.RealmServer.Privileges
{
  /// <summary>
  /// Handles the management of role groups, and their permissions.
  /// </summary>
  public class PrivilegeMgr : Manager<PrivilegeMgr>
  {
    private Dictionary<string, RoleGroup> m_roleGroups;

    /// <summary>Default constructor.</summary>
    private PrivilegeMgr()
    {
    }

    public void SetGroupInfo(RoleGroupInfo[] infos)
    {
      Dictionary<string, RoleGroup> dictionary =
        new Dictionary<string, RoleGroup>(
          StringComparer.InvariantCultureIgnoreCase);
      foreach(RoleGroupInfo info in infos)
        dictionary.Add(info.Name, new RoleGroup(info));
      m_roleGroups = dictionary;
    }

    /// <summary>Returns the RoleGroup with the highest Rank.</summary>
    public RoleGroup HighestRole
    {
      get
      {
        if(m_roleGroups == null)
          SetGroupInfo(RoleGroupInfo.CreateDefaultGroups().ToArray());
        return m_roleGroups[RoleGroupInfo.HighestRole.Name];
      }
    }

    /// <summary>Returns the RoleGroup with the highest Rank.</summary>
    public RoleGroup LowestRole
    {
      get { return m_roleGroups[RoleGroupInfo.LowestRole.Name]; }
    }

    /// <summary>All existing RoleGroups</summary>
    public Dictionary<string, RoleGroup> RoleGroups
    {
      get { return m_roleGroups; }
    }

    public bool IsInitialized { get; internal set; }

    /// <summary>Gets a role group by name.</summary>
    /// <returns>the RoleGroup if it exists; null otherwise</returns>
    public RoleGroup GetRoleOrDefault(string roleGroupName)
    {
      RoleGroup roleGroup;
      if(m_roleGroups.TryGetValue(roleGroupName, out roleGroup))
        return roleGroup;
      return m_roleGroups.Values.First();
    }

    public RoleGroup GetRole(string roleGroupName)
    {
      RoleGroup roleGroup;
      if(m_roleGroups.TryGetValue(roleGroupName, out roleGroup))
        return roleGroup;
      return null;
    }

    public void Setup()
    {
      int num = ServerApp<RealmServer>.Instance.AuthClient.IsConnected ? 1 : 0;
    }
  }
}