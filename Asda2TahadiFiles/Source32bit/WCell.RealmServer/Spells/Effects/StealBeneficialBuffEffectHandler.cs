﻿using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells.Effects
{
  /// <summary>
  /// Steals a positive Aura -which has a timeout and is not channeled- off the target and applies it on oneself
  /// </summary>
  public class StealBeneficialBuffEffectHandler : SpellEffectHandler
  {
    private Aura toSteal;

    public StealBeneficialBuffEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override SpellFailedReason InitializeTarget(WorldObject target)
    {
      ObjectReference sharedReference = m_cast.CasterObject.SharedReference;
      AuraCollection auras = m_cast.CasterUnit.Auras;
      foreach(Aura aura in ((Unit) target).Auras)
      {
        if(aura.IsBeneficial && aura.CanBeStolen &&
           (aura.TimeLeft > 100 && auras.GetAura(sharedReference, aura.Id, aura.Spell) == null))
        {
          toSteal = aura;
          return SpellFailedReason.Ok;
        }
      }

      return SpellFailedReason.NothingToSteal;
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      SpellCast cast = m_cast;
      if(!toSteal.IsAdded)
        return;
      toSteal.Remove(true);
      if(toSteal.TimeLeft > 120000)
        toSteal.TimeLeft = 120000;
      cast.CasterUnit.Auras.AddAura(toSteal, true);
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }

    public override ObjectTypes CasterType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}