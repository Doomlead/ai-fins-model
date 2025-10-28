﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance.Helpers;

using Dxt;

using fin.image;
using fin.io;
using fin.model;
using fin.model.impl;


namespace HaloWarsTools;

public sealed class HWXtdResource : HWBinaryResource {
  public IModel Mesh { get; private set; }

  public IImage AmbientOcclusionTexture { get; private set; }
  public IImage OpacityTexture { get; private set; }

  public static HWXtdResource FromFile(HWContext context, string filename)
    => GetOrCreateFromFile(context, filename, HWResourceType.Xtd) as
        HWXtdResource;

  protected override void Load(byte[] bytes) {
    base.Load(bytes);

    this.Mesh = this.ImportMesh(bytes);

    this.AmbientOcclusionTexture = this.ExtractEmbeddedDXT5A(bytes,
      this.GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_AOChunk));
    this.OpacityTexture = this.ExtractEmbeddedDXT5A(
        bytes,
        this.GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_AlphaChunk));
  }

  private IImage ExtractEmbeddedDXT5A(byte[] bytes,
                                      HWBinaryResourceChunk chunk) {
    // Get raw embedded DXT5 texture from resource file
    var width = (int) Math.Sqrt(chunk.Size * 2);
    var height = width;

    // For some godforsaken reason, every pair of bytes is flipped so we need
    // to fix it here. This was really annoying to figure out, haha.
    for (var i = 0; i < chunk.Size; i += 2) {
      var offset = (int) chunk.Offset + i;

      var byte0 = bytes[offset + 0];
      var byte1 = bytes[offset + 1];

      bytes[offset + 0] = byte1;
      bytes[offset + 1] = byte0;
    }

    return DxtDecoder.DecompressDxt5a(bytes,
                                      (int) chunk.Offset,
                                      width,
                                      height);
  }

  private IModel ImportMesh(byte[] bytes) {
    HWBinaryResourceChunk headerChunk = this.GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_XTDHeader);
    float tileScale =
        BinaryUtils.ReadFloatBigEndian(bytes,
                                       (int) headerChunk.Offset + 12);
    HWBinaryResourceChunk atlasChunk = this.GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_AtlasChunk);

    int gridSize =
        (int) Math.Round(Math.Sqrt((atlasChunk.Size - 32) /
                                   8)); // Subtract the min/range vector sizes, divide by position + normal size, and sqrt for grid size
    int positionOffset = (int) atlasChunk.Offset + 32;
    int normalOffset = positionOffset + gridSize * gridSize * 4;

    // These are stored as ZYX, 4 bytes per component
    Vector3 posCompMin = BinaryUtils
                         .ReadVector3BigEndian(
                             bytes,
                             (int) atlasChunk.Offset)
                         .ReverseComponents();
    Vector3 posCompRange =
        BinaryUtils
            .ReadVector3BigEndian(bytes, (int) atlasChunk.Offset + 16)
            .ReverseComponents();

    var finModel = new ModelImpl<NormalUvVertexImpl>(
        gridSize * gridSize,
        (index, position) => new NormalUvVertexImpl(index, position)) {
        // TODO: Fix this
        FileBundle = null,
        Files = new HashSet<IReadOnlyGenericFile>(),
    };
    var finMesh = finModel.Skin.AddMesh();

    var finVertices = finModel.Skin.TypedVertices;

    // Read vertex offsets/normals and add them to the mesh
    ParallelHelper.For(0,
                       finVertices.Count,
                       new GridVertexGenerator(
                           bytes,
                           finVertices,
                           gridSize,
                           tileScale,
                           positionOffset,
                           normalOffset,
                           posCompMin,
                           posCompRange));

    // Generate faces based on terrain grid
    for (int x = 0; x < gridSize - 1; ++x) {
      var triangleStripVertices = new IReadOnlyVertex[2 * gridSize];

      for (int z = 0; z < gridSize; ++z) {
        var a = finVertices[GetVertexIndex(x, z, gridSize)];
        var b = finVertices[GetVertexIndex(x + 1, z, gridSize)];

        triangleStripVertices[2 * z + 0] = b;
        triangleStripVertices[2 * z + 1] = a;
      }

      finMesh.AddTriangleStrip(triangleStripVertices)
             .SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);
    }

    return finModel;
  }

  private readonly struct GridVertexGenerator(
      byte[] bytes,
      IReadOnlyList<NormalUvVertexImpl> vertices,
      int gridSize,
      float tileScale,
      int positionOffset,
      int normalOffset,
      Vector3 posCompMin,
      Vector3 posCompRange)
      : IAction {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke(int index) {
      var x = index % gridSize;
      var z = (index - x) / gridSize;

      int offset = index * 4;

      // Get offset position and normal for this vertex
      Vector3 position =
          ReadVector3Compressed(
              bytes.AsSpan(positionOffset + offset, 4)) *
          posCompRange -
          posCompMin;

      // Positions are relative to the terrain grid, so shift them by the grid position
      position += new Vector3(x, 0, z) * tileScale;

      Vector3 normal =
          ConvertDirectionVector(
              Vector3.Normalize(
                  ReadVector3Compressed(
                      bytes.AsSpan(normalOffset + offset, 4)) *
                  2.0f -
                  Vector3.One));

      // Simple UV based on original, non-warped terrain grid
      var texCoord = new Vector2(x / ((float) gridSize - 1),
                                 z / ((float) gridSize - 1));

      var vertex = vertices[index];
      vertex.SetLocalPosition(position);
      vertex.SetLocalNormal(normal);
      vertex.SetUv(texCoord);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int GetVertexIndex(int x, int z, int gridSize)
    => z * gridSize + x;

  private const uint K_BIT_MASK_10 = (1 << 10) - 1;

  private const float INVERSE_K_BIT_MASK_10 =
      1f / K_BIT_MASK_10;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Vector3 ReadVector3Compressed(ReadOnlySpan<byte> bytes) {
    // Inexplicably, position and normal vectors are encoded inside 4 bytes. ~10 bits per component
    // This seems okay for directions, but positions suffer from stairstepping artifacts
    uint v = BitConverter.ToUInt32(bytes);
    uint x = (v >> 0) & K_BIT_MASK_10;
    uint y = (v >> 10) & K_BIT_MASK_10;
    uint z = (v >> 20) & K_BIT_MASK_10;
    return new Vector3(x, y, z) * INVERSE_K_BIT_MASK_10;
  }

  // TODO: This might not be right
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Vector3 ConvertDirectionVector(Vector3 vector)
    => new(vector.Z, vector.X, vector.Y);
}