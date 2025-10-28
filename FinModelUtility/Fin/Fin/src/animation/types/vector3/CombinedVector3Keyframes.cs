﻿using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector3;

public sealed class CombinedVector3Keyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, Vector3> interpolator,
    IndividualInterpolationConfig<Vector3>? individualConfig = null)
    : ICombinedVector3Keyframes<TKeyframe>
    where TKeyframe : IKeyframe<Vector3> {
  private readonly InterpolatedKeyframes<TKeyframe, Vector3> impl_
      = new(sharedConfig, interpolator, individualConfig);

  public ISharedInterpolationConfig SharedConfig => sharedConfig;

  public IndividualInterpolationConfig<Vector3>? IndividualConfig
    => individualConfig;

  public IReadOnlyList<TKeyframe> Definitions => this.impl_.Definitions;
  public bool HasAnyData => this.impl_.HasAnyData;

  public void Add(TKeyframe keyframe) => this.impl_.Add(keyframe);

  public bool TryGetAtFrame(float frame, out Vector3 value)
    => this.impl_.TryGetAtFrame(frame, out value);

  public void GetAllFrames(Span<Vector3> dst) => this.impl_.GetAllFrames(dst);

  public bool TryGetSimpleKeyframes(
      out IReadOnlyList<(float frame, Vector3 value)> keyframes,
      out IReadOnlyList<(Vector3 tangentIn, Vector3 tangentOut)>? tangentKeyframes)
    => this.impl_.TryGetSimpleKeyframes(out keyframes, out tangentKeyframes);
}