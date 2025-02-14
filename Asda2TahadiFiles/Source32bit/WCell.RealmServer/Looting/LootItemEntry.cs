﻿using System.Collections.Generic;
using WCell.Constants.Looting;
using WCell.Util.Data;

namespace WCell.RealmServer.Looting
{
  public abstract class LootItemEntry : LootEntity, IDataHolder
  {
    public LootEntryType LootType;
    public uint GroupId;
    public int MinAmountOrRef;
    public uint ReferencedEntryId;

    public object GetId()
    {
      return EntryId;
    }

    public void FinalizeDataHolder()
    {
      if(MinAmountOrRef < 0)
        ReferencedEntryId = (uint) -MinAmountOrRef;
      else
        MinAmount = MinAmountOrRef;
      if(MinAmount < 1)
        MinAmount = 1;
      if(MinAmount > MaxAmount)
        MaxAmount = MinAmount;
      if(DropChance < 0.0)
        DropChance = -DropChance;
      LootMgr.AddEntry(this);
    }

    protected static void AddItems<T>(LootEntryType t, List<T> all) where T : LootItemEntry
    {
      foreach(ResolvedLootItemList entry in LootMgr.GetEntries(t))
      {
        if(entry != null)
        {
          foreach(LootEntity lootEntity in entry)
            all.Add((T) lootEntity);
        }
      }
    }

    public override string ToString()
    {
      return ItemTemplate + " (" + DropChance + "%)";
    }
  }
}