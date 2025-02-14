﻿using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Looting
{
  /// <summary>
  /// TODO: Keep track of roll-results etc
  /// Every Character has a LooterEntry which represents the interface between a Character and its current Loot
  /// </summary>
  public class LooterEntry
  {
    internal Character m_owner;
    private Loot m_loot;

    public LooterEntry(Character chr)
    {
      m_owner = chr;
    }

    /// <summary>The Looter</summary>
    public Character Owner
    {
      get { return m_owner; }
    }

    /// <summary>The Loot that the Character is currently looking at</summary>
    public Loot Loot
    {
      get { return m_loot; }
      internal set
      {
        if(m_owner == null)
        {
          m_loot = null;
        }
        else
        {
          if(m_loot == value)
            return;
          Loot loot = m_loot;
          m_loot = value;
          if(value == null)
          {
            m_owner.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 |
                                 UnitFlags.SelectableNotAttackable | UnitFlags.Influenced |
                                 UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 |
                                 UnitFlags.Preparation | UnitFlags.PlusMob |
                                 UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable |
                                 UnitFlags.Passive | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 |
                                 UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 |
                                 UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 |
                                 UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed |
                                 UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed |
                                 UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted |
                                 UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 |
                                 UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
            if(!loot.MustKneelWhileLooting)
              return;
            m_owner.StandState = StandState.Stand;
          }
          else
          {
            m_owner.UnitFlags |= UnitFlags.Looting;
            if(!value.MustKneelWhileLooting)
              return;
            m_owner.StandState = StandState.Kneeling;
          }
        }
      }
    }

    /// <summary>Requires loot to already be generated</summary>
    /// <param name="lootable"></param>
    public void TryLoot(ILootable lootable)
    {
    }

    /// <summary>
    /// Returns whether this Looter is entitled to loot anything from the given loot
    /// </summary>
    public bool MayLoot(Loot loot)
    {
      return m_owner != null && (loot.Looters.Count == 0 || loot.Looters.Contains(this) ||
                                 m_owner.GodMode || loot.Group != null &&
                                 m_owner.Group == loot.Group &&
                                 (loot.FreelyAvailableCount > 0 ||
                                  m_owner.GroupMember == loot.Group.MasterLooter));
    }

    /// <summary>
    /// Releases the current loot and (maybe) makes it available to everyone else.
    /// </summary>
    public void Release()
    {
      if(m_loot == null)
        return;
      if(m_owner != null)
        LootHandler.SendLootReleaseResponse(m_owner, m_loot);
      m_loot.RemoveLooter(this);
      if(m_loot.Looters.Count == 0)
        m_loot.IsReleased = true;
      Loot = null;
    }
  }
}