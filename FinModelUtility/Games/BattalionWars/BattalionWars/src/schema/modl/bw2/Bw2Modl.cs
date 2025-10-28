﻿using fin.data.dictionaries;
using fin.schema;
using fin.util.asserts;

using modl.schema.modl.bw2.node;

using schema.binary;

namespace modl.schema.modl.bw2;

public sealed class Bw2Modl : IModl, IBinaryDeserializable {
  public List<IBwNode> Nodes { get; } = [];
  public ListDictionary<ushort, ushort> CnctParentToChildren { get; } = new();

  [Unknown]
  public void Read(IBinaryReader br) {
    {
      br.PushMemberEndianness(Endianness.LittleEndian);
      var filenameLength = br.ReadUInt32();
      br.Position += filenameLength;
      br.PopEndianness();
    }

    SectionHeaderUtil.AssertNameAndReadSize(br, "MODL", out var size);
    var expectedEnd = br.Position + size;

    var version = br.ReadUInt32s(2);

    var nodeCount = br.ReadUInt16();
    var additionalDataCount = br.ReadUInt16();

    var unkInt = br.ReadUInt32();
    var unknown0 = br.ReadSingles(4);

    var bgfNameLength = br.ReadInt32();
    var bgfName = br.ReadString(bgfNameLength);

    var additionalData = br.ReadUInt32s(additionalDataCount);

    this.SkipSection_(br, "XMEM");

    // Reads in nodes (bones)
    {
      this.Nodes.Clear();
      for (var i = 0; i < nodeCount; ++i) {
        var node = new Bw2Node(additionalDataCount);
        node.Read(br);
        this.Nodes.Add(node);
      }
    }

    // Reads in hierarchy, how nodes are "CoNneCTed" or "CoNCaTenated?"?
    {
      br.PushMemberEndianness(Endianness.LittleEndian);
      SectionHeaderUtil.AssertNameAndReadSize(br, "CNCT", out var cnctSize);
      var cnctCount = cnctSize / 4;

      this.CnctParentToChildren.Clear();
      for (var i = 0; i < cnctCount; ++i) {
        var parent = br.ReadUInt16();
        var child = br.ReadUInt16();

        this.CnctParentToChildren.Add(parent, child);
      }

      br.PopEndianness();
    }

    Asserts.Equal(expectedEnd, br.Position);
  }

  private void SkipSection_(IBinaryReader br, string sectionName) {
    SectionHeaderUtil.AssertNameAndReadSize(br, sectionName, out var size);
    var data = br.ReadBytes((int) size);
  }
}