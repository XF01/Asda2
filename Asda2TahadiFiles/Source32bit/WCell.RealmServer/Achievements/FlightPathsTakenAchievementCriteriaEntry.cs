﻿using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
  [StructLayout(LayoutKind.Sequential)]
  public class FlightPathsTakenAchievementCriteriaEntry : AchievementCriteriaEntry
  {
    public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
    {
      return achievementProgressRecord.Counter >= 1U;
    }

    public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
    {
      achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, 1U, ProgressType.ProgressAccumulate);
    }
  }
}