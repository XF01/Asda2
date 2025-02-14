﻿using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class TalentCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("Talents");
    }

    public class TalentsClearCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Reset", "Clear", "C");
        EnglishDescription = "Resets all talents for free";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Unit target = trigger.Args.Target;
        if(target == null || target.Talents == null)
          trigger.Reply(RealmLangKey.NoValidTarget);
        else
          target.Talents.ResetAllForFree();
      }
    }
  }
}