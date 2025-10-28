﻿using System.Numerics;

using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema.gx;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Arisotura/SM64DSe/blob/master/SM64DSFormats/BMD.cs
/// </summary>
public interface IOpcode : IBinaryDeserializable {
  OpcodeType Type { get; }
}

[BinarySchema]
public sealed partial class NoopOpcode : IOpcode {
  [Skip]
  public OpcodeType Type => OpcodeType.NOOP;
}

public sealed class UnhandledOpcode(OpcodeType type) : IOpcode {
  public OpcodeType Type => type;

  public void Read(IBinaryReader br) {
    br.Position += type.GetLength();
  }
}

public sealed class MatrixRestoreOpcode : IOpcode {
  public OpcodeType Type => OpcodeType.MATRIX_RESTORE;
  public byte TransformId { get; set; }

  public void Read(IBinaryReader br) {
    this.TransformId = (byte) (br.ReadUInt32() & 0x1F);
  }
}

public sealed class ColorOpcode : IOpcode {
  public OpcodeType Type => OpcodeType.COLOR;

  public Vector3 Color { get; set; }

  public void Read(IBinaryReader br) {
    var raw = br.ReadUInt32();
    var r = (byte) ((raw << 3) & 0xF8);
    var g = (byte) ((raw >> 2) & 0xF8);
    var b = (byte) ((raw >> 7) & 0xF8);
    this.Color = new Vector3(r, g, b) / 255;
  }
}

public sealed class NormalOpcode : IOpcode {
  public OpcodeType Type => OpcodeType.NORMAL;
  public Vector3 Normal { get; set; }

  public void Read(IBinaryReader br) {
    uint param = br.ReadUInt32();
    short x = (short) ((param << 6) & 0xFFC0);
    short y = (short) ((param >> 4) & 0xFFC0);
    short z = (short) ((param >> 14) & 0xFFC0);
    this.Normal = new Vector3(x, y, z) / 32768;
  }
}

public sealed class TexCoordOpcode : IOpcode {
  public OpcodeType Type => OpcodeType.TEXCOORD;
  public Vector2 TexCoord { get; set; }

  public void Read(IBinaryReader br) {
    uint param = br.ReadUInt32();
    short s = (short) (param & 0xFFFF);
    short t = (short) (param >> 16);
    this.TexCoord = new Vector2(s, t) / 16;
  }
}

public interface IVertexOpcode : IOpcode {
  float? X { get; set; }
  float? Y { get; set; }
  float? Z { get; set; }
}

public sealed class Vertex0x23Opcode : IVertexOpcode {
  public OpcodeType Type => OpcodeType.VERTEX_0x23;

  public float? X { get; set; }
  public float? Y { get; set; }
  public float? Z { get; set; }

  public void Read(IBinaryReader br) {
    uint param1 = br.ReadUInt32();
    uint param2 = br.ReadUInt32();

    short x = (short) (param1 & 0xFFFF);
    short y = (short) (param1 >> 16);
    short z = (short) (param2 & 0xFFFF);

    this.X = x / 4096f;
    this.Y = y / 4096f;
    this.Z = z / 4096f;
  }
}

public sealed class Vertex0x24Opcode : IVertexOpcode {
  public OpcodeType Type => OpcodeType.VERTEX_0x24;
  public Vector3 Position { get; set; }

  public float? X { get; set; }
  public float? Y { get; set; }
  public float? Z { get; set; }

  public void Read(IBinaryReader br) {
    uint param = br.ReadUInt32();

    short x = (short) ((param << 6) & 0xFFC0);
    short y = (short) ((param >> 4) & 0xFFC0);
    short z = (short) ((param >> 14) & 0xFFC0);

    this.X = x / 4096f;
    this.Y = y / 4096f;
    this.Z = z / 4096f;
  }
}

public sealed class Vertex0x25Opcode : IVertexOpcode {
  public OpcodeType Type => OpcodeType.VERTEX_0x25;

  public float? X { get; set; }
  public float? Y { get; set; }
  public float? Z { get; set; }

  public void Read(IBinaryReader br) {
    uint param = br.ReadUInt32();

    short x = (short) (param & 0xFFFF);
    short y = (short) (param >> 16);

    this.X = x / 4096f;
    this.Y = y / 4096f;
  }
}

public sealed class Vertex0x26Opcode : IVertexOpcode {
  public OpcodeType Type => OpcodeType.VERTEX_0x26;

  public float? X { get; set; }
  public float? Y { get; set; }
  public float? Z { get; set; }

  public void Read(IBinaryReader br) {
    uint param = br.ReadUInt32();

    short x = (short) (param & 0xFFFF);
    short z = (short) (param >> 16);

    this.X = x / 4096f;
    this.Z = z / 4096f;
  }
}

public sealed class Vertex0x27Opcode : IVertexOpcode {
  public OpcodeType Type => OpcodeType.VERTEX_0x27;
  public Vector3 Position { get; set; }

  public float? X { get; set; }
  public float? Y { get; set; }
  public float? Z { get; set; }

  public void Read(IBinaryReader br) {
    uint param = br.ReadUInt32();

    short y = (short) (param & 0xFFFF);
    short z = (short) (param >> 16);

    this.Y = y / 4096f;
    this.Z = z / 4096f;
  }
}

public sealed class Vertex0x28Opcode : IOpcode {
  public OpcodeType Type => OpcodeType.VERTEX_0x28;

  public Vector3 DeltaPosition { get; set; }

  public void Read(IBinaryReader br) {
    uint param = br.ReadUInt32();

    short x = (short) ((param << 6) & 0xFFC0);
    short y = (short) ((param >> 4) & 0xFFC0);
    short z = (short) ((param >> 14) & 0xFFC0);

    this.DeltaPosition = new Vector3(x, y, z) / 262144;
  }
}

[BinarySchema]
public sealed partial class BeginVertexListOpcode : IOpcode {
  [Skip]
  public OpcodeType Type => OpcodeType.BEGIN_VERTEX_LIST;

  private uint flags_;

  [Skip]
  public PolygonType PolygonType => (PolygonType) (this.flags_ & 0x3);
}

[BinarySchema]
public sealed partial class EndVertexListOpcode : IOpcode {
  [Skip]
  public OpcodeType Type => OpcodeType.END_VERTEX_LIST;
}