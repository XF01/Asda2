﻿using WCell.Constants.Skills;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Skills;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Adds a flat modifier to one Skill</summary>
  public class ModSkillTalentHandler : AuraEffectHandler
  {
    private Skill skill;

    protected override void Apply()
    {
      if(!(m_aura.Auras.Owner is Character))
        return;
      skill = ((Character) m_aura.Auras.Owner).Skills[(SkillId) m_spellEffect.MiscValue];
      if(skill == null)
        return;
      skill.Modifier += (short) EffectValue;
    }

    protected override void Remove(bool cancelled)
    {
      if(skill == null)
        return;
      skill.Modifier -= (short) EffectValue;
    }
  }
}