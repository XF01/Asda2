﻿using System;
using System.IO;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Groups
{
  /// <summary>
  /// Represents the relationship between a Character and its Group/SubGroup.
  /// GroupMembers are also a singly linked list where the head is the person who joined first and the tail is the person who joined last.
  /// </summary>
  public class GroupMember : INamed
  {
    private uint m_lowId;
    private string m_characterName;
    protected internal SubGroup m_subGroup;
    protected internal GroupMember m_nextMember;
    private Character m_chr;

    /// <summary>Is set when player logs out</summary>
    private Vector3 m_lastPos;

    /// <summary>Is set when player logs out</summary>
    private Map m_Map;

    public GroupMember(Character character, GroupMemberFlags flags)
    {
      m_lowId = character.EntityId.Low;
      m_characterName = character.Name;
      Flags = flags;
      Character = character;
    }

    /// <summary>
    /// The low part of the Character's EntityId. Use EntityId.GetPlayerId(Id) to get a full EntityId
    /// </summary>
    public uint Id
    {
      get { return m_lowId; }
    }

    /// <summary>The name of this GroupMember</summary>
    public string Name
    {
      get { return m_characterName; }
    }

    /// <summary>The SubGroup of this Member (not null)</summary>
    public SubGroup SubGroup
    {
      get { return m_subGroup; }
      internal set { m_subGroup = value; }
    }

    /// <summary>The Group of this Member (not null)</summary>
    public Group Group
    {
      get { return m_subGroup.Group; }
    }

    public GroupMemberFlags Flags { get; internal set; }

    /// <summary>
    /// Returns the Character or null, if this member is offline
    /// </summary>
    public Character Character
    {
      get { return m_chr; }
      internal set { m_chr = value; }
    }

    public bool IsOnline
    {
      get { return m_chr != null; }
    }

    /// <summary>The current or last Map in which this GroupMember was</summary>
    public Map Map
    {
      get
      {
        if(m_chr != null)
          return m_chr.Map;
        return m_Map;
      }
      internal set { m_Map = value; }
    }

    /// <summary>
    /// The current position of this GroupMember or the last position if member already logged out
    /// </summary>
    public Vector3 Position
    {
      get
      {
        if(m_chr != null)
          return m_chr.Position;
        return m_lastPos;
      }
      internal set { m_lastPos = value; }
    }

    /// <summary>The GroupMember who joined after this GroupMember</summary>
    public GroupMember Next
    {
      get { return m_nextMember; }
      internal set { m_nextMember = value; }
    }

    /// <summary>Whether this member is the leader of the Group</summary>
    public bool IsLeader
    {
      get { return m_subGroup.Group.Leader == this; }
    }

    /// <summary>Whether this member is the MasterLooter of the Group</summary>
    public bool IsMasterLooter
    {
      get { return m_subGroup.Group.MasterLooter == this; }
    }

    /// <summary>Whether this member is an Assistant</summary>
    public bool IsAssistant
    {
      get { return Flags.HasFlag(GroupMemberFlags.Assistant); }
      set
      {
        if(value)
          Flags |= GroupMemberFlags.Assistant;
        else
          Flags &= ~GroupMemberFlags.Assistant;
      }
    }

    /// <summary>
    /// Whether this member is the Leader, MainAssistant or Assistant
    /// </summary>
    public bool IsAtLeastAssistant
    {
      get
      {
        if(!IsLeader)
          return Flags.HasAnyFlag(GroupMemberFlags.Assistant | GroupMemberFlags.MainAssistant);
        return true;
      }
    }

    /// <summary>Whether this member is a MainAssistant</summary>
    public bool IsMainAssistant
    {
      get { return Flags.HasAnyFlag(GroupMemberFlags.MainAssistant); }
    }

    /// <summary>Whether this member is the MainTank</summary>
    public bool IsMainTank
    {
      get { return Flags.HasAnyFlag(GroupMemberFlags.MainTank); }
    }

    /// <summary>Whether this member is MainAssistant or leader</summary>
    public bool IsAtLeastMainAssistant
    {
      get
      {
        if(!IsLeader)
          return Flags.HasAnyFlag(GroupMemberFlags.MainAssistant);
        return true;
      }
    }

    public void WriteIdPacked(RealmPacketOut packet)
    {
      new EntityId(Id, HighId.Player).WritePacked(packet);
    }

    /// <summary>Lets this Member leave the group.</summary>
    public void LeaveGroup()
    {
      if(m_subGroup == null)
        return;
      m_subGroup.Group.RemoveMember(this);
    }

    /// <summary>
    /// Calls the given handler over all members of the same Group within the given radius
    /// </summary>
    public void IterateMembersInRange(float radius, Action<GroupMember> handler)
    {
      foreach(GroupMember groupMember in Group)
      {
        float sqDistance = Position.DistanceSquared(groupMember.Position);
        if(groupMember == this || groupMember.Map == Map && Utility.IsInRange(sqDistance, radius))
          handler(groupMember);
      }
    }

    public override string ToString()
    {
      return "GroupMember: " + Name;
    }
  }
}