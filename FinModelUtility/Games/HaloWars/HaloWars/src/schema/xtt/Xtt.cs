﻿using fin.image;
using fin.image.io.dxt;

using hw.schema.binary;

using schema.binary;

namespace hw.schema.xtt;

public sealed class Xtt : IBinaryDeserializable {
  public IImage AlbedoTexture { get; private set; }

  public void Read(IBinaryReader br) {
    var binaryResource = br.ReadNew<BinaryResource>();

    var albedoChunk = binaryResource.GetFirstChunkOfType(
        BinaryResourceChunkType.XTT_AtlasChunkAlbedo);
    this.AlbedoTexture = ExtractEmbeddedDxt1_(albedoChunk);
  }

  private static IImage ExtractEmbeddedDxt1_(BinaryResourceChunk chunk) {
    using var br = chunk.GetBinaryReader();

    // Decompress DXT1 texture and turn it into a Bitmap
    var unk0 = br.ReadUInt32();
    var width = br.ReadInt32();
    var height = br.ReadInt32();
    var unk1 = br.ReadUInt32();

    br.PushContainerEndianness(Endianness.LittleEndian);
    return new Dxt1ImageReader(width, height, 1, 4, false).ReadImage(br);
  }
}