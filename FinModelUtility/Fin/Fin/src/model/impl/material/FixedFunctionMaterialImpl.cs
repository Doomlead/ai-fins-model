﻿using System.Collections.Generic;
using System.Linq;

using fin.color;
using fin.language.equations.fixedFunction;
using fin.image.util;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class MaterialManagerImpl {
    public IFixedFunctionMaterial AddFixedFunctionMaterial() {
      this.Registers ??= new FixedFunctionRegisters();
      var material = new FixedFunctionMaterialImpl(this.Registers);
      this.materials_.Add(material);
      return material;
    }
  }

  private class FixedFunctionMaterialImpl
      : BMaterialImpl, IFixedFunctionMaterial {
    private readonly List<IReadOnlyTexture> textures_ = [];

    private readonly IReadOnlyTexture?[] texturesSources_
        = new IReadOnlyTexture[8];

    private readonly IColor?[] colors_ = new IColor[2];
    private readonly float?[] alphas_ = new float?[2];

    public FixedFunctionMaterialImpl(IFixedFunctionRegisters registers) {
      this.Registers = registers;
      this.TransparencyType = TransparencyType.TRANSPARENT;
    }

    public override IEnumerable<IReadOnlyTexture> Textures => this.textures_;

    public IFixedFunctionEquations<FixedFunctionSource> Equations { get; } =
      new FixedFunctionEquations<FixedFunctionSource>();

    public IFixedFunctionRegisters Registers { get; }

    public IReadOnlyList<IReadOnlyTexture?> TextureSources
      => this.texturesSources_;

    public IFixedFunctionMaterial SetTextureSource(
        int textureIndex,
        IReadOnlyTexture texture) {
      if (!this.texturesSources_.Contains(texture)) {
        this.textures_.Add(texture);
      }

      this.texturesSources_[textureIndex] = texture;

      return this;
    }

    public IReadOnlyTexture? CompiledTexture { get; set; }


    public IFixedFunctionMaterial SetAlphaCompare(
        AlphaOp alphaOp,
        AlphaCompareType alphaCompareType0,
        float reference0,
        AlphaCompareType alphaCompareType1,
        float reference1) {
      this.AlphaOp = alphaOp;
      this.AlphaCompareType0 = alphaCompareType0;
      this.AlphaReference0 = reference0;
      this.AlphaCompareType1 = alphaCompareType1;
      this.AlphaReference1 = reference1;
      return this;
    }

    public AlphaOp AlphaOp { get; private set; }

    public AlphaCompareType AlphaCompareType0 { get; private set; } =
      AlphaCompareType.Always;

    public float AlphaReference0 { get; private set; }

    public AlphaCompareType AlphaCompareType1 { get; private set; } =
      AlphaCompareType.Always;

    public float AlphaReference1 { get; private set; }

    private IReadOnlyTexture? normalTexture_;

    public IReadOnlyTexture? NormalTexture {
      get => this.normalTexture_;
      set {
        this.normalTexture_ = value;
        if (value != null) {
          this.textures_.Add(value);
        }
      }
    }
  }
}