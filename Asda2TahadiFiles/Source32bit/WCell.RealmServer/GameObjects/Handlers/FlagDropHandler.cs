﻿using NLog;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
  /// <summary>GO Type 26</summary>
  public class FlagDropHandler : GameObjectHandler
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    public override bool Use(Character user)
    {
      GOEntry entry = m_go.Entry;
      return true;
    }
  }
}