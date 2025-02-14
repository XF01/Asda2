﻿using WCell.Util.Data;

namespace WCell.RealmServer.Mounts
{
  [DataHolder]
  public class MountTemplate : IDataHolder
  {
    public void FinalizeDataHolder()
    {
      if(!Asda2MountMgr.TemplatesByItemIDs.ContainsKey(ItemId))
        Asda2MountMgr.TemplatesByItemIDs.Add(ItemId, this);
      if(Asda2MountMgr.TemplatesById.ContainsKey(Id))
        return;
      Asda2MountMgr.TemplatesById.Add(Id, this);
    }

    public int Id { get; set; }

    public int ItemId { get; set; }

    public int Unk { get; set; }

    public int Time { get; set; }

    public int Unk2 { get; set; }

    public string Name { get; set; }

    public string ImageName { get; set; }

    public int Unk1 { get; set; }
  }
}