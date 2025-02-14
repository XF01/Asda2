﻿using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
  public class DisenchantingLootItemEntry : LootItemEntry
  {
    public static IEnumerable<DisenchantingLootItemEntry> GetAllDataHolders()
    {
      List<DisenchantingLootItemEntry> all = new List<DisenchantingLootItemEntry>(10000);
      AddItems(LootEntryType.Disenchanting, all);
      return all;
    }
  }
}