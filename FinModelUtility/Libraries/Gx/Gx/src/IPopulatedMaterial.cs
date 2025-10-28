﻿using System.Drawing;

using fin.model;

namespace gx;

public enum GxCullMode {
  None = 0,  // Do not cull any primitives
  Front = 1, // Cull front-facing primitives
  Back = 2,  // Cull back-facing primitives
  All = 3    // Cull all primitives
}

public static class GxCullModeExtensions {
  public static CullingMode ToFinCullingMode(this GxCullMode gxCullMode)
    => gxCullMode switch {
        GxCullMode.None  => CullingMode.SHOW_BOTH,
        GxCullMode.Front => CullingMode.SHOW_BACK_ONLY,
        GxCullMode.Back  => CullingMode.SHOW_FRONT_ONLY,
        GxCullMode.All   => CullingMode.SHOW_NEITHER,
        _                => throw new ArgumentOutOfRangeException(),
    };
}

public interface IPopulatedMaterial {
  string Name { get; }
  GxCullMode CullMode { get; }

  (int, Color)[] MaterialColors { get; }
  IColorChannelControl?[] ColorChannelControls { get; }
  (int, Color)[] AmbientColors { get; }
  Color?[] LightColors { get; }

  Color[] KonstColors { get; }
  IColorRegister?[] ColorRegisters { get; }

  ITevOrder?[] TevOrderInfos { get; }
  ITevStageProps?[] TevStageInfos { get; }

  ITevSwapMode?[] TevSwapModes { get; }
  ITevSwapModeTable?[] TevSwapModeTables { get; }

  IAlphaCompare AlphaCompare { get; }
  IBlendFunction BlendMode { get; }

  short[] TextureIndices { get; }
  ITexCoordGen?[] TexCoordGens { get; }
  ITextureMatrixInfo?[] TextureMatrices { get; }

  (GxWrapMode wrapModeS, GxWrapMode wrapModeT)[]? TextureWrapModeOverrides
    => null;

  IDepthFunction DepthFunction { get; }

  IIndirectTexture? IndirectTexture { get; }
}