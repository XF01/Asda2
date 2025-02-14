﻿using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModRangedAttackPowerByPercentOfStatHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      owner.ModRangedAPModByStat((StatType) m_spellEffect.MiscValue, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      owner.ModRangedAPModByStat((StatType) m_spellEffect.MiscValue, -EffectValue);
    }
  }
}