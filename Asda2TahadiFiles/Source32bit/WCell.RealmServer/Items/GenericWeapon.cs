﻿using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
  /// <summary>A weapon that can be completely customized</summary>
  public class GenericWeapon : IAsda2Weapon
  {
    public static readonly DamageInfo[] FistDamage = new DamageInfo[1]
    {
      new DamageInfo(DamageSchoolMask.Physical, 1f, 2f)
    };

    public static readonly DamageInfo[] RangedDamage = new DamageInfo[1]
    {
      new DamageInfo(DamageSchoolMask.Physical, 1f, 2f)
    };

    public static readonly DamageInfo[] DefaultDamage = new DamageInfo[1]
    {
      new DamageInfo(DamageSchoolMask.Physical, 1f, 3f)
    };

    /// <summary>Default Fists</summary>
    public static GenericWeapon Fists = new GenericWeapon(InventorySlotTypeMask.WeaponMainHand,
      FistDamage, SkillId.Unarmed, 0.0f, Unit.DefaultMeleeAttackRange, 2000);

    /// <summary>Default Ranged Weapon</summary>
    public static GenericWeapon Ranged = new GenericWeapon(InventorySlotTypeMask.WeaponRanged,
      RangedDamage, SkillId.Bows, Unit.DefaultMeleeAttackRange, Unit.DefaultRangedAttackRange,
      2000);

    /// <summary>No damage weapon</summary>
    public static GenericWeapon Peace = new GenericWeapon(InventorySlotTypeMask.WeaponMainHand,
      FistDamage, SkillId.None, 0.0f, 0.0f, 10000);

    public GenericWeapon(InventorySlotTypeMask slot, int damageCount)
    {
      InventorySlotMask = slot;
      Damages = new DamageInfo[damageCount];
      IsWeapon = true;
    }

    public GenericWeapon(InventorySlotTypeMask slot, float minDmg, float maxDmg)
      : this(slot, minDmg, maxDmg, DamageSchoolMask.Physical)
    {
      IsWeapon = true;
    }

    public GenericWeapon(InventorySlotTypeMask slot, float minDmg, float maxDmg, DamageSchoolMask dmgType)
      : this(slot, minDmg, maxDmg, Fists.AttackTime, dmgType)
    {
      IsWeapon = true;
    }

    public GenericWeapon(InventorySlotTypeMask slot, float minDmg, float maxDmg, int attackTime)
      : this(slot, minDmg, maxDmg, attackTime, DamageSchoolMask.Physical)
    {
      IsWeapon = true;
    }

    public GenericWeapon(InventorySlotTypeMask slot, float minDmg, float maxDmg, int attackTime,
      DamageSchoolMask dmgType)
    {
      InventorySlotMask = slot;
      AttackTime = attackTime;
      MaxRange = Unit.DefaultMeleeAttackRange;
      Damages = new DamageInfo[1]
      {
        new DamageInfo(dmgType, minDmg, maxDmg)
      };
      IsWeapon = true;
    }

    public GenericWeapon(InventorySlotTypeMask slot, DamageInfo[] damages, SkillId skill, float minRange,
      float maxRange, int attackTime)
    {
      InventorySlotMask = slot;
      Damages = damages;
      Skill = skill;
      MinRange = minRange;
      MaxRange = maxRange;
      AttackTime = attackTime;
      IsWeapon = true;
    }

    public GenericWeapon(InventorySlotTypeMask slot, float minDmg, float maxDmg, int attackTime, float atackRange)
      : this(slot, minDmg, maxDmg, attackTime, DamageSchoolMask.Physical)
    {
      IsWeapon = true;
      MaxRange = atackRange;
      MinRange = 0.0f;
    }

    public DamageInfo[] Damages { get; set; }

    public bool IsWeapon { get; set; }

    public int BonusDamage
    {
      get { return 0; }
      set { }
    }

    public SkillId Skill { get; set; }

    public bool IsRanged
    {
      get { return InventorySlotMask == InventorySlotTypeMask.WeaponRanged; }
    }

    public bool IsMelee
    {
      get { return !IsRanged; }
    }

    /// <summary>The minimum Range of this weapon</summary>
    public float MinRange { get; set; }

    /// <summary>The maximum Range of this Weapon</summary>
    public float MaxRange { get; set; }

    /// <summary>The time in milliseconds between 2 attacks</summary>
    public int AttackTime { get; set; }

    public InventorySlotTypeMask InventorySlotMask { get; private set; }
  }
}