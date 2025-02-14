﻿using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class HealthCommand : RealmServerCommand
  {
    protected HealthCommand()
    {
    }

    protected override void Initialize()
    {
      Init("Health");
      EnglishParamInfo = "<amount>";
      EnglishDescription = "Sets Basehealth to the given value and fills up Health.";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      Unit target = trigger.Args.Target;
      if(target == null)
        return;
      int num = trigger.Text.NextInt(1);
      target.BaseHealth = num;
      target.Heal(target.MaxHealth - target.Health, null, null);
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Unit; }
    }
  }
}