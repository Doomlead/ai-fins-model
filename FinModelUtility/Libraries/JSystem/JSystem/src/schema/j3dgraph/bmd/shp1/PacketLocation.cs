﻿using schema.binary;


namespace jsystem.schema.j3dgraph.bmd.shp1;

[BinarySchema]
public sealed partial class PacketLocation : IBinaryDeserializable {
  public uint Size;
  public uint Offset;
}