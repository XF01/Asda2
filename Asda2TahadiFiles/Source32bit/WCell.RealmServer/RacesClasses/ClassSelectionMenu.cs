﻿using WCell.RealmServer.Gossips;

namespace WCell.RealmServer.RacesClasses
{
  public class ClassSelectionMenu : GossipMenu
  {
    public readonly BaseClass Class;

    public ClassSelectionMenu(BaseClass clss, ArchetypeSelectionHandler handler, uint textId)
      : base(textId)
    {
      Class = clss;
      foreach(Archetype archetype in ArchetypeMgr.Archetypes[(int) clss.Id])
      {
        if(archetype != null)
        {
          Archetype arche = archetype;
          AddItem(new GossipMenuItem(archetype.ToString(),
            convo => handler(convo, arche)));
        }
      }
    }
  }
}