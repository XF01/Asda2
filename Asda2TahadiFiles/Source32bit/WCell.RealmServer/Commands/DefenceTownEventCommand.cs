﻿using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Events.Asda2;
using WCell.RealmServer.Global;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class DefenceTownEventCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("defencetown", "dtown");
    }

    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.EventManager; }
    }

    public class StartCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("start");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Map map = trigger.Args.Character.Map;
        if(map.DefenceTownEvent != null)
        {
          trigger.Reply(string.Format("Defence town event in {0} is already started.", map.Name));
        }
        else
        {
          int minLevel = 1;
          int maxLevel = 80;
          float amountMod = trigger.Text.NextFloat(1f);
          int num1 = 100;
          float otherStatsMod = trigger.Text.NextFloat(1f);
          int num2 = 3;
          int num3 = 30;
          Asda2EventMgr.StartDeffenceTownEvent(map, minLevel, maxLevel, amountMod, num1,
            otherStatsMod, num2, num3);
          trigger.Reply("Ok, defence town event started. Town is {0}, dificulty is {1}. [{2}-{3}]Level",
            (object) map.Name, (object) num3, (object) minLevel, (object) maxLevel);
        }
      }
    }

    public class StopCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("stop");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Map map = trigger.Args.Character.Map;
        if(map.DefenceTownEvent == null)
        {
          trigger.Reply("Defence town event in not running.");
        }
        else
        {
          Asda2EventMgr.StopDeffenceTownEvent(map, trigger.Text.NextInt(0) != 0);
          trigger.Reply("Guess word event stoped.");
        }
      }
    }
  }
}