﻿using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  /// <summary>
  /// For BeastMastery.  Adds 4 talent points to all controlled pets.
  /// </summary>
  public class ModPetTalentPointsHandler : AuraEffectHandler
  {
    protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
      Unit target, ref SpellFailedReason failReason)
    {
      if(!(target is Character) || target.Class == ClassId.THS)
        return;
      failReason = SpellFailedReason.BadTargets;
    }

    protected override void Apply()
    {
      Character owner = m_aura.Auras.Owner as Character;
      if(owner == null)
        return;
      owner.PetBonusTalentPoints += BonusPoints;
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = m_aura.Auras.Owner as Character;
      if(owner == null)
        return;
      owner.PetBonusTalentPoints -= BonusPoints;
    }

    public int BonusPoints
    {
      get { return m_aura.Spell.Effects[0].BasePoints + 1; }
    }
  }
}