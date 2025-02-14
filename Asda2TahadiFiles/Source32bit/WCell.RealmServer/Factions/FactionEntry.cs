﻿using System;
using WCell.Constants.Factions;

namespace WCell.RealmServer.Factions
{
  [Serializable]
  public struct FactionEntry
  {
    public FactionId Id;

    /// <summary>m_reputationIndex</summary>
    public FactionReputationIndex FactionIndex;

    public Constants.RaceMask[] RaceMask;
    public Constants.ClassMask[] ClassMask;
    public int[] BaseRepValue;
    public FactionFlags[] BaseFlags;
    public FactionId ParentId;
    public string Name;
  }
}