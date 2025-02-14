﻿using NLog;

namespace Cell.Core
{
  /// <summary>
  /// Defines a wrapper for a chunk of memory that may be split into smaller, logical segments.
  /// </summary>
  public class ArrayBuffer
  {
    protected static Logger log = LogManager.GetCurrentClassLogger();
    private BufferManager m_mgr;
    public readonly byte[] Array;

    /// <summary>
    /// Creates an ArrayBuffer that is wrapping a pre-existing buffer.
    /// </summary>
    /// <param name="arr">the buffer to wrap</param>
    internal ArrayBuffer(byte[] arr)
    {
      Array = arr;
    }

    /// <summary>
    /// Creates an ArrayBuffer and allocates a new buffer for usage.
    /// </summary>
    /// <param name="mgr">the <see cref="T:Cell.Core.BufferManager" /> which allocated this array</param>
    internal ArrayBuffer(BufferManager mgr, int bufferSize)
    {
      m_mgr = mgr;
      Array = new byte[bufferSize];
    }

    protected internal void CheckIn(BufferSegment segment)
    {
      if(m_mgr == null)
        return;
      m_mgr.CheckIn(segment);
    }
  }
}