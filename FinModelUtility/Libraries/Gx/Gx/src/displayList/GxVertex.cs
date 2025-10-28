﻿using fin.schema.color;

using OneOf;

namespace gx.displayList;

[GenerateOneOf]
public partial class IndexOrColor : OneOfBase<ushort, Rgba32>;

public sealed class GxVertex {
  public ushort PositionIndex { get; set; }
  public ushort? JointIndex { get; set; }
  public ushort? NormalIndex { get; set; }
  public ushort? NbtIndex { get; set; }
  public IndexOrColor? Color0IndexOrValue { get; set; }
  public IndexOrColor? Color1IndexOrValue { get; set; }
  public ushort? TexCoord0Index { get; set; }
  public ushort? TexCoord1Index { get; set; }
  public ushort? TexCoord2Index { get; set; }
  public ushort? TexCoord3Index { get; set; }
  public ushort? TexCoord4Index { get; set; }
  public ushort? TexCoord5Index { get; set; }
  public ushort? TexCoord6Index { get; set; }
  public ushort? TexCoord7Index { get; set; }

  public ushort? GetTexCoord(int i)
    => i switch {
        0 => this.TexCoord0Index,
        1 => this.TexCoord1Index,
        2 => this.TexCoord2Index,
        3 => this.TexCoord3Index,
        4 => this.TexCoord4Index,
        5 => this.TexCoord5Index,
        6 => this.TexCoord6Index,
        7 => this.TexCoord7Index,
    };

  public ushort?[] ColorIndices => [
      this.Color0IndexOrValue?.AsT0,
      this.Color1IndexOrValue?.AsT0,
  ];

  public ushort?[] TexCoordIndices => [
      this.TexCoord0Index,
      this.TexCoord1Index,
      this.TexCoord2Index,
      this.TexCoord3Index,
      this.TexCoord4Index,
      this.TexCoord5Index,
      this.TexCoord6Index,
      this.TexCoord7Index,
  ];
}