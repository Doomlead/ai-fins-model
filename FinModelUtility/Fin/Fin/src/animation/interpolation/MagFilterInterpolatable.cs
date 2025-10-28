﻿using System;
using System.Collections.Generic;

using fin.math.floats;
using fin.math.interpolation;

namespace fin.animation.interpolation;

/// <summary>
///   Helper class for interpolating a track with a variable "mag filter" for
///   higher framerates.
/// </summary>
public sealed class MagFilterInterpolatable<T>(IInterpolator<T> interpolator)
    : IInterpolatable<T> {
  public AnimationInterpolationMagFilter AnimationInterpolationMagFilter {
    get;
    set;
  } = AnimationInterpolationMagFilter.ANY_FRAME_RATE;

  public IInterpolatable<T>? Impl { get; set; }
  public bool HasAnyData => this.Impl?.HasAnyData ?? false;

  public bool TryGetAtFrame(float frame, out T value) {
    if (this.Impl == null) {
      value = default!;
      return false;
    }

    var intFrame = (int) frame;
    var frac = frame - intFrame;
    if (frac.IsRoughly0() ||
        this.AnimationInterpolationMagFilter ==
        AnimationInterpolationMagFilter.ORIGINAL_FRAME_RATE_NEAREST) {
      return this.Impl.TryGetAtFrame(intFrame, out value);
    }

    if (this.AnimationInterpolationMagFilter ==
        AnimationInterpolationMagFilter.ANY_FRAME_RATE) {
      return this.Impl.TryGetAtFrame(frame, out value);
    }

    if (this.Impl.TryGetAtFrame(intFrame, out var fromValue) &&
        this.Impl.TryGetAtFrame((int) Math.Ceiling(frame),
                                out var toValue)) {
      value = interpolator.Interpolate(fromValue, toValue, frac);
      return true;
    }

    value = default!;
    return false;
  }

  public void GetAllFrames(Span<T> dst) => this.Impl.GetAllFrames(dst);

  public bool TryGetSimpleKeyframes(
      out IReadOnlyList<(float frame, T value)> keyframes,
      out IReadOnlyList<(T tangentIn, T tangentOut)>? tangentKeyframes)
    => this.Impl.TryGetSimpleKeyframes(out keyframes, out tangentKeyframes);
}