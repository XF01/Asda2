﻿using System;
using WCell.Constants.Factions;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Instances
{
  /// <summary>
  /// The base class for all WoW-style "Instances" (Dungeon, Heroic, Raid etc)
  /// 
  /// TODO:
  /// - SMSG_INSTANCE_RESET_FAILURE: The party leader has attempted to reset the instance you are in. Please zone out to allow the instance to reset.
  /// </summary>
  public abstract class BaseInstance : InstancedMap, IUpdatable
  {
    /// <summary>The timeout for normal Dungeon instances</summary>
    public static int DefaultInstanceTimeoutMillis = 1800000;

    private DateTime m_expiryTime = new DateTime();
    internal FactionGroup m_OwningFaction = FactionGroup.Invalid;
    private IInstanceHolderSet m_owner;
    internal MapDifficultyEntry difficulty;
    private DateTime m_lastReset;
    private TimerEntry m_timeoutTimer;
    private InstanceProgress progress;
    private InstanceSettings settings;

    protected internal override void InitMap()
    {
      base.InitMap();
      int resetTime = difficulty.ResetTime;
      m_lastReset = DateTime.Now;
      m_timeoutTimer = new TimerEntry(OnTimeout);
      RegisterUpdatableLater(this);
      settings = CreateSettings();
    }

    protected virtual InstanceSettings CreateSettings()
    {
      if(!Difficulty.IsDungeon)
        return new RaidInstanceSettings(this);
      return new DungeonInstanceSettings(this);
    }

    /// <summary>Whether this instance will ever expire</summary>
    public bool CanExpire
    {
      get { return m_expiryTime != new DateTime(); }
    }

    public DateTime ExpiryTime
    {
      get { return m_expiryTime; }
    }

    /// <summary>Difficulty of the instance</summary>
    public override MapDifficultyEntry Difficulty
    {
      get { return difficulty; }
    }

    public InstanceSettings Settings
    {
      get { return settings; }
    }

    public InstanceProgress Progress
    {
      get { return progress; }
    }

    public IInstanceHolderSet Owner
    {
      get { return m_owner; }
      set { m_owner = value; }
    }

    public override FactionGroup OwningFaction
    {
      get { return m_OwningFaction; }
    }

    public int TimeoutDelay
    {
      get { return DefaultInstanceTimeoutMillis; }
    }

    /// <summary>The last time this Instance was reset</summary>
    public DateTime LastReset
    {
      get { return m_lastReset; }
    }

    /// <summary>Whether this instance can be reset</summary>
    public override bool CanReset(Character chr)
    {
      if(chr.Role.IsStaff || chr == m_owner.InstanceLeader)
        return PlayerCount == 0;
      return false;
    }

    public override void Reset()
    {
      base.Reset();
      m_lastReset = DateTime.Now;
    }

    protected override void OnEnter(Character chr)
    {
      base.OnEnter(chr);
      if(m_timeoutTimer.IsRunning)
      {
        m_timeoutTimer.Stop();
        s_log.Debug("{0} #{1} timeout timer stopped by: {2}", Name,
          m_InstanceId, chr.Name);
      }

      if(chr.GodMode || Difficulty.BindingType != BindingType.Soft)
        return;
      Bind(chr);
    }

    protected void Bind(IInstanceHolderSet holder)
    {
      if(holder.InstanceLeader.Group != null)
        holder.InstanceLeader.Group.ForeachCharacter(chr =>
        {
          InstanceCollection instances = chr.Instances;
          if(instances == null)
            return;
          instances.BindTo(this);
        });
      else
        holder.InstanceLeaderCollection.BindTo(this);
    }

    protected override void OnLeave(Character chr)
    {
      if(PlayerCount > 1)
        return;
      if(TimeoutDelay > 0)
        m_timeoutTimer.Start(TimeoutDelay, 0);
      s_log.Debug("{0} #{1} timeout timer started.", Name, m_InstanceId);
    }

    public void Update(int dt)
    {
      m_timeoutTimer.Update(dt);
    }

    public override bool CanEnter(Character chr)
    {
      DateTime? lastLogout = chr.LastLogout;
      DateTime lastReset = m_lastReset;
      int num = lastLogout.HasValue ? (lastLogout.GetValueOrDefault() > lastReset ? 1 : 0) : 0;
      if(!base.CanEnter(chr))
        return false;
      if(Owner == null)
        return true;
      Character instanceLeader = Owner.InstanceLeader;
      if(instanceLeader == null || !chr.IsAlliedWith(instanceLeader))
        return chr.GodMode;
      return true;
    }

    public override void TeleportOutside(Character chr)
    {
      chr.TeleportToNearestGraveyard();
    }

    public override void DeleteNow()
    {
      InstanceMgr.Instances.RemoveInstance(MapId, InstanceId);
      base.DeleteNow();
    }

    protected override void Dispose()
    {
      base.Dispose();
      m_owner = null;
    }

    public override string ToString()
    {
      string str = "";
      if(Owner != null && Owner.InstanceLeader != null)
        str = " - Owned by: " + Owner.InstanceLeader.Name;
      return base.ToString() + (difficulty.IsHeroic ? " [Heroic]" : "") + str;
    }

    public override sealed void Save()
    {
      if(progress == null)
        progress = new InstanceProgress(MapId, InstanceId);
      progress.ResetTime =
        DateTime.Now.AddMilliseconds(m_timeoutTimer.RemainingInitialDelayMillis);
      progress.DifficultyIndex = DifficultyIndex;
      PerformSave();
      progress.Save();
    }

    /// <summary>Method is to be overridden by instance implementation</summary>
    protected virtual void PerformSave()
    {
    }
  }
}