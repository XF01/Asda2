﻿using System;
using System.Linq;
using System.Threading.Tasks;

namespace WCell.Util.Threading.TaskParallel
{
  /// <summary>Provides a grouping for common Parallel LINQ options.</summary>
  public sealed class ParallelLinqOptions : ParallelOptions
  {
    private ParallelExecutionMode _executionMode = ParallelExecutionMode.Default;
    private ParallelMergeOptions _mergeOptions = ParallelMergeOptions.Default;
    private bool _ordered;

    /// <summary>Gets or sets the execution mode.</summary>
    public ParallelExecutionMode ExecutionMode
    {
      get { return _executionMode; }
      set
      {
        if(value != ParallelExecutionMode.Default && value != ParallelExecutionMode.ForceParallelism)
          throw new ArgumentOutOfRangeException(nameof(ExecutionMode));
        _executionMode = value;
      }
    }

    /// <summary>Gets or sets the merge options.</summary>
    public ParallelMergeOptions MergeOptions
    {
      get { return _mergeOptions; }
      set
      {
        if(value != ParallelMergeOptions.AutoBuffered && value != ParallelMergeOptions.Default &&
           value != ParallelMergeOptions.FullyBuffered && value != ParallelMergeOptions.NotBuffered)
          throw new ArgumentOutOfRangeException(nameof(MergeOptions));
        _mergeOptions = value;
      }
    }

    /// <summary>Gets or sets whether the query should retain ordering.</summary>
    public bool Ordered
    {
      get { return _ordered; }
      set { _ordered = value; }
    }
  }
}