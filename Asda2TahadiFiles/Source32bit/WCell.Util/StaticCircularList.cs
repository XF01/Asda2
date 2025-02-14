﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace WCell.Util
{
  /// <summary>
  /// A simple array that is indexed in a circular fashion.
  /// Elements always start at Tail and end at Head.
  /// 
  /// Tail greater Head:
  /// | x | x | H |   |   |   | T | x | x |
  /// 
  /// Head greater Tail:
  /// |   |   | T | x | x | x | H |   |   |
  /// </summary>
  public class StaticCircularList<T> : IEnumerable<T>, IEnumerable
  {
    private int tail;
    private int head;
    private T[] arr;

    public StaticCircularList(int capacity, Action<T> deleteCallback)
    {
      DeleteCallback = deleteCallback;
      arr = new T[capacity];
    }

    public Action<T> DeleteCallback { get; set; }

    /// <summary>The index of the tail</summary>
    public int Tail
    {
      get { return tail; }
      set { tail = value; }
    }

    /// <summary>The index of the head</summary>
    public int Head
    {
      get { return head; }
      set { head = value; }
    }

    public int Capacity
    {
      get { return arr.Length; }
    }

    public int Count
    {
      get { return Tail > Head ? Head + Capacity - Tail : Head - Tail; }
    }

    public bool IsFull
    {
      get { return Count == Capacity; }
    }

    public bool IsEmpty
    {
      get { return Tail == Head; }
    }

    /// <summary>The Tail item</summary>
    public T TailItem
    {
      get
      {
        if(IsEmpty)
          return default(T);
        return arr[tail];
      }
    }

    /// <summary>The Head item</summary>
    public T HeadItem
    {
      get
      {
        if(IsEmpty)
          return default(T);
        return arr[head];
      }
    }

    /// <summary>
    /// Increases the given number by one and wrapping it around, if it's &gt;= Capacity-1
    /// </summary>
    private int IncreaseAndWrap(int num)
    {
      return (num + 1) % Capacity;
    }

    private int DecreaseAndWrap(int num)
    {
      return (num - 1) % Capacity;
    }

    /// <summary>
    /// Sets the Head item.
    /// Also moves Tail forward, if it was already full, thus replacing the Tail item.
    /// </summary>
    public void Insert(T item)
    {
      if(Count + 1 == Capacity)
      {
        Action<T> deleteCallback = DeleteCallback;
        if(deleteCallback != null)
          deleteCallback(TailItem);
        tail = IncreaseAndWrap(tail);
      }

      head = IncreaseAndWrap(head);
      arr[head] = item;
    }

    /// <summary>
    /// Iterates over all items, starting at Tail and stopping at Head
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
      if(!IsEmpty)
      {
        if(Tail < Head)
        {
          for(int i = Tail; i <= Head; ++i)
            yield return arr[i];
        }
        else
        {
          for(int i = Tail; i < Capacity; ++i)
            yield return arr[i];
          for(int i = 0; i <= Head; ++i)
            yield return arr[i];
        }
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}