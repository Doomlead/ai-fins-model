﻿using System.Numerics;

using schema.binary;

namespace pikmin1.schema.mod;

[BinarySchema]
public sealed partial class Nbt : IBinaryConvertible {
  public Vector3 Normal { get; private set; }
  public Vector3 Binormal { get; private set; }
  public Vector3 Tangent { get; private set; }
}