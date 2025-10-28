﻿using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector3;

public sealed class Vector3KeyframeInterpolator
    : Vector3KeyframeInterpolator<Keyframe<Vector3>> {
  public static Vector3KeyframeInterpolator Instance { get; } = new();
  private Vector3KeyframeInterpolator() { }
}

public class Vector3KeyframeInterpolator<TKeyframe>
    : IKeyframeInterpolator<TKeyframe, Vector3>
    where TKeyframe : IKeyframe<Vector3> {
  public Vector3 Interpolate(
      TKeyframe from,
      TKeyframe to,
      float frame,
      ISharedInterpolationConfig sharedInterpolationConfig) {
    InterpolationUtil.GetTAndDuration(from,
                                      to,
                                      frame,
                                      sharedInterpolationConfig,
                                      out var t,
                                      out _);

    return from.ValueOut * (1 - t) + to.ValueIn * t;
  }
}