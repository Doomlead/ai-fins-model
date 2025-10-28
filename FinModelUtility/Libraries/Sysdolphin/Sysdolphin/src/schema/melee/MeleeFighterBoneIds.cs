﻿using schema.binary;

namespace sysdolphin.schema.melee;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/Melee/Pl/SBM_FighterBoneIDs.cs
/// </summary>
[BinarySchema]
public sealed partial class MeleeFighterBoneIds : IBinaryDeserializable {
  public int HeadBone { get; set; }
  public int RightArm { get; set; }
  public int LeftLeg { get; set; }
  public int RightLeg { get; set; }
  public int LeftArm { get; set; }
}