﻿using fin.util.asserts;

using schema.binary;

namespace modl.schema.terrain.bw1;

public sealed class Bw1Terrain : IBwTerrain, IBinaryDeserializable {
  public IBwHeightmap Heightmap { get; private set; }
  public IList<BwHeightmapMaterial> Materials { get; private set; }

  public void Read(IBinaryReader br) {
    var sections = new Dictionary<string, BwSection>();
    while (!br.Eof) {
      SectionHeaderUtil.ReadNameAndSize(br, out var name, out var size);
      var offset = br.Position;

      sections[name] = new BwSection(name, (int) size, offset);

      br.Position += size;
    }

    // TODO: Handle COLM
    // TODO: Handle GPNF
    // TODO: Handle UWCT

    var terrSection = sections["TERR"];
    br.Position = terrSection.Offset;
    var terrData = br.ReadNew<TerrData>();

    var chnkSection = sections["CHNK"];
    br.Position = chnkSection.Offset;
    var tilesBytes = br.ReadBytes(chnkSection.Size);

    var cmapSection = sections["CMAP"];
    br.Position = cmapSection.Offset;
    var tilemapBytes = br.ReadBytes(cmapSection.Size);

    var matlSection = sections["MATL"];
    var expectedMatlSectionSize = terrData.MaterialCount * 48;
    Asserts.Equal(expectedMatlSectionSize, matlSection.Size);
    br.Position = matlSection.Offset;
    this.Materials = br.ReadNews<BwHeightmapMaterial>(terrData.MaterialCount);

    this.Heightmap = new HeightmapParser(terrData, tilemapBytes, tilesBytes);
  }
}