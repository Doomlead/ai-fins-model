﻿using System;
using System.Runtime.CompilerServices;

namespace fin.math.floats;

public static class FloatsExtensions {
  public const float ROUGHLY_EQUAL_ERROR = .001f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsRoughly(this float a, float b)
    => a.IsRoughly(b, ROUGHLY_EQUAL_ERROR);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsRoughly(this float a, float b, float tolerance)
    => MathF.Abs(a - b) < tolerance;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsRoughly0(this float a) => a.IsRoughly(0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsRoughly1(this float a) => a.IsRoughly(1);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int GetRoughHashCode(this float a)
    => (int) MathF.Round(a / ROUGHLY_EQUAL_ERROR);
}