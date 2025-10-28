﻿using System;
using System.Runtime.CompilerServices;

namespace fin.math.rotations;

public static class FinTrig {
  public const float DEG_2_RAD = MathF.PI / 180;
  public const float RAD_2_DEG = 1 / DEG_2_RAD;

  // - At this point, native C# approach is faster than FastMath.
  // - Math version is used instead of MathF because there are bit-level
  //   differences in MathF across machines that cause flakiness when comparing
  //   model files byte-by-byte. This was a fucking bitch to solve.

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Cos(float radians) => (float) Math.Cos(radians);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Sin(float radians) => (float) Math.Sin(radians);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Acos(float value) => (float) Math.Acos(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Asin(float value) => (float) Math.Asin(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Atan2(float y, float x) => (float) Math.Atan2(y, x);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void FromPitchYawRadians(float pitchRadians,
                                         float yawRadians,
                                         out float xNormal,
                                         out float yNormal,
                                         out float zNormal) {
    var horizontalNormal = Cos(pitchRadians);
    var verticalNormal = Sin(pitchRadians);

    xNormal = horizontalNormal * Cos(yawRadians);
    yNormal = horizontalNormal * Sin(yawRadians);
    zNormal = verticalNormal;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void FromPitchYawDegrees(float pitchDegrees,
                                         float yawDegrees,
                                         out float xNormal,
                                         out float yNormal,
                                         out float zNormal)
    => FromPitchYawRadians(pitchDegrees * DEG_2_RAD,
                                   yawDegrees * DEG_2_RAD,
                                   out xNormal,
                                   out yNormal,
                                   out zNormal);
}