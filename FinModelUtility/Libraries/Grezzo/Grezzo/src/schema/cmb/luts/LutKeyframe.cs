﻿using schema.binary;

namespace grezzo.schema.cmb.luts;

[BinarySchema]
public sealed partial class LutKeyframe : IBinaryConvertible {
  public float InSlope;
  public float OutSlope;
  public int Frame;
  public float Value;
}