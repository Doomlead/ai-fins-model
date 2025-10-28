﻿using fin.util.asserts;

using gx.displayList;

using schema.binary;

using GxPrimitive = gx.displayList.GxPrimitive;


namespace gx;

public sealed class GxDisplayListReader {
  public GxPrimitive? Read(IBinaryReader br,
                           IVertexDescriptor vertexDescriptor) {
    this.ReadOpcode(br, vertexDescriptor, out var primitive);
    return primitive;
  }

  public GxOpcode ReadOpcode(IBinaryReader br,
                             IVertexDescriptor vertexDescriptor,
                             out GxPrimitive? primitive) {
    var opcode = (GxOpcode) br.ReadByte();
    primitive = null;

    if (opcode == GxOpcode.NOP) {
      return opcode;
    }

    if (opcode == GxOpcode.LOAD_CP_REG) {
      var command = br.ReadByte();
      var value = br.ReadUInt32();

      if (command == 0x50) {
        vertexDescriptor.Value
            = (vertexDescriptor.Value & ~(uint) 0x1FFFF) | value;
      } else if (command == 0x60) {
        vertexDescriptor.Value
            = (vertexDescriptor.Value & 0x1FFFF) | (value << 17);
      } else {
        throw new NotImplementedException();
      }
      return opcode;
    }

    if (opcode == GxOpcode.LOAD_XF_REG) {
      var lengthMinusOne = br.ReadUInt16();
      var length = lengthMinusOne + 1;

      // http://hitmen.c02.at/files/yagcd/yagcd/chap5.html#sec5.11.4
      var firstXfRegisterAddress = br.ReadUInt16();

      var values = br.ReadUInt32s(length);
      // TODO: Implement
      return opcode;
    }

    // TODO: Implement this to support loading matrices.
    if (opcode is GxOpcode.LOAD_INDX_A
                  or GxOpcode.LOAD_INDX_B
                  or GxOpcode.LOAD_INDX_C
                  or GxOpcode.LOAD_INDX_D) {
      var arrayIndex = br.ReadUInt16();
      var addrLen = br.ReadUInt16();
      return opcode;
    }

    // TODO: Other bits are vertex format, what is this used for?
    opcode &= (GxOpcode) 0xF8;

    if (opcode is GxOpcode.DRAW_QUADS
                  or GxOpcode.DRAW_TRIANGLES
                  or GxOpcode.DRAW_TRIANGLE_STRIP
                  or GxOpcode.DRAW_TRIANGLE_FAN
                  or GxOpcode.DRAW_LINES
                  or GxOpcode.DRAW_LINE_STRIP
                  or GxOpcode.DRAW_POINTS) {
      var vertices = new GxVertex[br.ReadUInt16()];

      for (var i = 0; i < vertices.Length; ++i) {
        var vertex = vertices[i] = new GxVertex();

        foreach (var (vertexAttribute, vertexFormat, colorComponentType) in
                 vertexDescriptor) {
          if (vertexAttribute == GxVertexAttribute.Color0 &&
              vertexFormat == GxAttributeType.DIRECT) {
            Asserts.True(colorComponentType.HasValue);
            vertex.Color0IndexOrValue = GxAttributeUtil.ReadColor(
                br,
                colorComponentType!.Value);
            continue;
          }

          if (vertexAttribute == GxVertexAttribute.Color1 &&
              vertexFormat == GxAttributeType.DIRECT) {
            Asserts.True(colorComponentType.HasValue);
            vertex.Color1IndexOrValue = GxAttributeUtil.ReadColor(
                br,
                colorComponentType!.Value);
            continue;
          }

          var value = vertexFormat switch {
              GxAttributeType.DIRECT or null => br.ReadByte(),
              GxAttributeType.INDEX_8 => br.ReadByte(),
              GxAttributeType.INDEX_16 => br.ReadUInt16(),
              _ => throw new NotImplementedException(),
          };

          switch (vertexAttribute) {
            case GxVertexAttribute.PosMatIdx: {
              Asserts.Equal(0, value % 3);
              vertex.JointIndex = (ushort) (value / 3);
              break;
            }
            case GxVertexAttribute.Position: {
              vertex.PositionIndex = value;
              break;
            }
            case GxVertexAttribute.Normal: {
              vertex.NormalIndex = value;
              break;
            }
            case GxVertexAttribute.NBT: {
              vertex.NbtIndex = value;
              break;
            }
            case GxVertexAttribute.Color0: {
              vertex.Color0IndexOrValue = value;
              break;
            }
            case GxVertexAttribute.Color1: {
              vertex.Color1IndexOrValue = value;
              break;
            }
            case GxVertexAttribute.Tex0Coord: {
              vertex.TexCoord0Index = value;
              break;
            }
            case GxVertexAttribute.Tex1Coord: {
              vertex.TexCoord1Index = value;
              break;
            }
            case GxVertexAttribute.Tex2Coord: {
              vertex.TexCoord2Index = value;
              break;
            }
            case GxVertexAttribute.Tex3Coord: {
              vertex.TexCoord3Index = value;
              break;
            }
            case GxVertexAttribute.Tex4Coord: {
              vertex.TexCoord4Index = value;
              break;
            }
            case GxVertexAttribute.Tex5Coord: {
              vertex.TexCoord5Index = value;
              break;
            }
            case GxVertexAttribute.Tex6Coord: {
              vertex.TexCoord6Index = value;
              break;
            }
            case GxVertexAttribute.Tex7Coord: {
              vertex.TexCoord7Index = value;
              break;
            }
          }
        }
      }

      primitive = new GxPrimitive((GxPrimitiveType) opcode, vertices);
      return opcode;
    }

    throw new NotImplementedException();
  }
}