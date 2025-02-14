﻿using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Misc
{
  public class CharmAuraHandler : AuraEffectHandler
  {
    protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterRef, Unit target,
      ref SpellFailedReason failReason)
    {
      Unit unit = creatingCast.CasterReference.Object as Unit;
      if(unit == null)
      {
        failReason = SpellFailedReason.BadTargets;
      }
      else
      {
        if(!(target is NPC))
        {
          LogManager.GetCurrentClassLogger()
            .Warn("{0} tried to Charm {1} which is not an NPC, but Player charming is not yet supported.",
              unit, target);
          failReason = SpellFailedReason.BadTargets;
        }

        if(unit.Charm != null)
          failReason = SpellFailedReason.AlreadyHaveCharm;
        else if(target.HasMaster)
          failReason = SpellFailedReason.CantBeCharmed;
        else if(unit.HasMaster)
        {
          failReason = SpellFailedReason.Charmed;
        }
        else
        {
          if(!(unit is Character) || ((Character) unit).ActivePet == null)
            return;
          failReason = SpellFailedReason.AlreadyHaveSummon;
        }
      }
    }

    protected override void Apply()
    {
      Unit casterUnit = m_aura.CasterUnit;
      if(casterUnit == null)
        return;
      Unit owner = m_aura.Auras.Owner;
      casterUnit.Charm = owner;
      owner.Charmer = casterUnit;
      int duration = m_aura.Duration;
      if(casterUnit is Character)
        ((Character) casterUnit).MakePet((NPC) owner, duration);
      else
        casterUnit.Enslave((NPC) owner, duration);
    }

    protected override void Remove(bool cancelled)
    {
      Unit casterUnit = m_aura.CasterUnit;
      Unit owner = m_aura.Auras.Owner;
      casterUnit.Charm = null;
      owner.Charmer = null;
      if(!(casterUnit is Character) || ((Character) casterUnit).ActivePet != m_aura.Auras.Owner)
        return;
      ((Character) casterUnit).ActivePet = null;
    }
  }
}