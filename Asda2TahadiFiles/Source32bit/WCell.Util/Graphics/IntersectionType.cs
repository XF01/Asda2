﻿using System;

namespace WCell.Util.Graphics
{
  [Flags]
  public enum IntersectionType
  {
    NoIntersection = 0,
    Intersects = 1,
    Contained = 2,
    Touches = Contained | Intersects // 0x00000003
  }
}