﻿using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector3;

public sealed class Vector3KeyframeWithTangentsInterpolator
    : Vector3KeyframeWithTangentsInterpolator<KeyframeWithTangents<Vector3>> {
  public static Vector3KeyframeWithTangentsInterpolator Instance { get; }
    = new();

  private Vector3KeyframeWithTangentsInterpolator() { }
}

public class Vector3KeyframeWithTangentsInterpolator<TKeyframe>
    : IKeyframeInterpolator<TKeyframe, Vector3>
    where TKeyframe : IKeyframeWithTangents<Vector3> {
  public Vector3 Interpolate(
      TKeyframe from,
      TKeyframe to,
      float frame,
      ISharedInterpolationConfig sharedInterpolationConfig) {
    if (!InterpolationUtil.TryToGetHermiteCoefficients(
            from,
            to,
            frame,
            sharedInterpolationConfig,
            out var fromCoefficient,
            out var toCoefficient,
            out var oneCoefficient)) {
      return new Vector3KeyframeInterpolator<TKeyframe>().Interpolate(
          from,
          to,
          frame,
          sharedInterpolationConfig);
    }

    return fromCoefficient * from.ValueOut +
           toCoefficient * to.ValueIn +
           Vector3.One * oneCoefficient;
  }
}