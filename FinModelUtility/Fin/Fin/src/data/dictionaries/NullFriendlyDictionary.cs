﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using fin.util.asserts;

namespace fin.data.dictionaries;

/// <summary>
///   A dictionary that accepts null keys.
/// </summary>
public sealed class NullFriendlyDictionary<TKey, TValue> : IFinDictionary<TKey, TValue> {
  private readonly Dictionary<TKey, TValue> impl_ = new();

  private bool hasNull_;
  private TValue nullValue_;

  private readonly object lock_ = new();

  public int Count => this.Keys.Count();

  public void Clear() {
    lock (this.lock_) {
      this.impl_.Clear();
      this.hasNull_ = false;
    }
  }

  public IEnumerable<TKey?> Keys {
    get {
      foreach (var key in this.impl_.Keys) {
        yield return key;
      }
      if (this.hasNull_) {
        yield return default;
      }
    }
  }

  public IEnumerable<TValue> Values {
    get {
      foreach (var value in this.impl_.Values) {
        yield return value;
      }
      if (this.hasNull_) {
        yield return this.nullValue_;
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool ContainsKey(TKey key)
    => key == null ? this.hasNull_ : this.impl_.ContainsKey(key);

  public void Add(TKey key, TValue value) {
    lock (this.lock_) {
      if (key == null) {
        this.hasNull_ = true;
        this.nullValue_ = value;
      } else {
        this.impl_[key] = value;
      }
    }
  }

  public TValue GetOrAdd(TKey key, Func<TKey, TValue> createHandler) {
    lock (this.lock_) {
      if (key == null) {
        if (this.hasNull_) {
          return this.nullValue_;
        }

        this.hasNull_ = true;
        return this.nullValue_ = createHandler(default!);
      }

      ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(
          this.impl_,
          key,
          out var exists);
      if (exists) {
        return value!;
      }

      return value = createHandler(key);
    }
  }

  public TValue this[TKey key] {
    get {
      if (!this.ContainsKey(key)) {
        Asserts.Fail($"Expected to find key {key} in dictionary!");
      }

      if (key == null) {
        return this.nullValue_;
      }

      return this.impl_[key];
    }
    set => this.Add(key, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Remove(TKey key) => this.Remove(key, out _);

  public bool Remove(TKey key, out TValue value) {
    lock (this.lock_) {
      bool didRemove;
      if (key == null) {
        didRemove = this.hasNull_;
        value = this.nullValue_;
        this.hasNull_ = false;
      } else {
        didRemove = this.impl_.Remove(key, out value!);
      }

      return didRemove;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IEnumerator<(TKey, TValue)> GetEnumerator() {
    foreach (var (key, value) in this.impl_) {
      yield return (key, value);
    }

    if (this.hasNull_) {
      yield return (default!, this.nullValue_);
    }
  }
}