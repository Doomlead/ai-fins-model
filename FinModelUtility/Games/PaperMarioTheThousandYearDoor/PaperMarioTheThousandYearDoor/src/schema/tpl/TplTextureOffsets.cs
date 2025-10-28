﻿using schema.binary;

namespace ttyd.schema.tpl;

[BinarySchema]
public sealed partial class TplTextureOffsets : IBinaryDeserializable {
  public uint HeaderOffset { get; set; }
  public uint PaletteOffset { get; set; }
}