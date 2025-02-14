﻿using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class WarehouseyCommand : RealmServerCommand
  {
    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.Player; }
    }

    protected override void Initialize()
    {
      Init("Warehouse");
      EnglishDescription = "Used for manipulation warehouse";
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Player; }
    }

    public class UnlockCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Unlock", "u");
        EnglishDescription = "Unlocks your warehouse";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        if(!trigger.Args.Character.IsWarehouseLocked)
        {
          trigger.Reply("Warehouse is not locked.");
        }
        else
        {
          string warehousePassword = trigger.Args.Character.Record.WarehousePassword;
          if(warehousePassword == null)
            trigger.Reply("Password not seted yet. Warehouse is not locked.");
          else if(trigger.Text.NextWord() != warehousePassword)
          {
            trigger.Reply("Wrong password.");
          }
          else
          {
            trigger.Args.Character.IsWarehouseLocked = false;
            trigger.Reply("Warehouse unlocked.");
          }
        }
      }
    }

    public class SetPasswordCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Set");
        EnglishDescription = "Sets warehouse password";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        if(trigger.Args.Character.Record.WarehousePassword != null)
        {
          trigger.Reply(
            "Password is already seted. Use <#warehouse reset [oldpass] [newpass]> command to reset it.");
        }
        else
        {
          string str = trigger.Text.NextWord();
          if(string.IsNullOrWhiteSpace(str))
          {
            trigger.Reply("Enter not empty password.");
          }
          else
          {
            trigger.Args.Character.Record.WarehousePassword = str;
            trigger.Reply(string.Format("Password was seted to [{0}].", str));
          }
        }
      }
    }

    public class ResetPasswordCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Reset");
        EnglishDescription = "Resets warehouse password";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        if(trigger.Args.Character.Record.WarehousePassword == null)
        {
          trigger.Reply("Password is not seted yet. Use <#warehouse set [pass]> command to set it.");
        }
        else
        {
          string str1 = trigger.Text.NextWord();
          if(string.IsNullOrWhiteSpace(str1))
            trigger.Reply("Enter not empty old password.");
          else if(str1 != trigger.Args.Character.Record.WarehousePassword)
          {
            trigger.Reply("Wrong password.");
          }
          else
          {
            string str2 = null;
            if(trigger.Text.HasNext)
              str2 = trigger.Text.NextWord();
            if(string.IsNullOrWhiteSpace(str2))
            {
              trigger.Reply("You enter empty new password. Clearing your warehouse password.");
              trigger.Args.Character.Record.WarehousePassword = null;
            }
            else
            {
              trigger.Args.Character.Record.WarehousePassword = str2;
              trigger.Reply(string.Format("Password was seted to [{0}].", str2));
            }
          }
        }
      }
    }
  }
}