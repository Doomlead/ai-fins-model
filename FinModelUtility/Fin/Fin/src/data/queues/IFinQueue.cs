﻿using System;
using System.Collections.Generic;

using readOnly;

namespace fin.data.queues;

/// <summary>
///   Simpler interface for queues that is easier to implement.
/// </summary>
[GenerateReadOnly]
public partial interface IFinQueue<T> : IFinCollection<T> {
  void Enqueue(T first, params ReadOnlySpan<T> rest);
  void Enqueue(IEnumerable<T> values);

  T Dequeue();
  bool TryDequeue(out T value);

  [Const]
  new T Peek();

  [Const]
  new bool TryPeek(out T value);
}