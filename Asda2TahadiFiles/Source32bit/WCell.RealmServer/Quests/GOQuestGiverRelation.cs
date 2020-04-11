﻿using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;

namespace WCell.RealmServer.Quests
{
  public class GOQuestGiverRelation : QuestGiverRelation
  {
    public override ObjectTemplate ObjectTemplate
    {
      get { return GOMgr.GetEntry(QuestGiverId); }
    }
  }
}