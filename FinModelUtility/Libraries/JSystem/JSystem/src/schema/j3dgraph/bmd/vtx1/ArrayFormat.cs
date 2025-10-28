﻿using fin.schema;

using gx;
using gx.displayList;

using schema.binary;


namespace jsystem.schema.j3dgraph.bmd.vtx1;

[BinarySchema]
public sealed partial class ArrayFormat : IBinaryConvertible {
  public GxVertexAttribute ArrayType;
  public GxComponentCountType ComponentCountType { get; set; }
  public GxComponentType DataType;
  public byte DecimalPoint;

  [Unknown]
  public byte Unknown1;

  [Unknown]
  public ushort Unknown2;
}