﻿using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
  public class SummonAllTotemsHandler : SpellEffectHandler
  {
    public SummonAllTotemsHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override void Apply()
    {
      Character casterObject = Cast.CasterObject as Character;
      if(casterObject == null)
        return;
      int buttonIndex = Effect.MiscValue + 132;
      int miscValueB = Effect.MiscValueB;
      while(miscValueB != 0)
      {
        if(casterObject.GetTypeFromActionButton(buttonIndex) == 0)
        {
          Spell spell = SpellHandler.Get(casterObject.GetActionFromActionButton(buttonIndex));
          if(spell != null)
          {
            SpellCast spellCast = casterObject.SpellCast;
            if(spellCast != null)
              spellCast.Trigger(spell);
          }
        }

        --miscValueB;
        ++buttonIndex;
      }
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