﻿using System;
using System.Runtime.CompilerServices;

using fin.image.formats;
using fin.math;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;


namespace fin.image.io.tile;

/// <summary>
///   Stolen from:
///   https://github.com/xdanieldzd/Scarlet/blob/master/Scarlet/Drawing/Compression/ETC1.cs
/// </summary>
public readonly struct Etc1TileReader(bool hasAlpha) : ITileReader<Rgba32> {
  /* Specs: https://www.khronos.org/registry/gles/extensions/OES/OES_compressed_ETC1_RGB8_texture.txt */

  /* Other implementations:
   * https://github.com/richgel999/rg-etc1/blob/master/rg_etc1.cpp
   * https://github.com/Gericom/EveryFileExplorer/blob/master/3DS/GPU/Textures.cs
   * https://github.com/gdkchan/Ohana3DS-Rebirth/blob/master/Ohana3DS%20Rebirth/Ohana/TextureCodec.cs */

  private static readonly int[] ETC1_MODIFIER_TABLES_ = [
      2, 8, -2, -8,
      5, 17, -5, -17,
      9, 29, -9, -29,
      13, 42, -13, -42,
      18, 60, -18, -60,
      24, 80, -24, -80,
      33, 106, -33, -106,
      47, 183, -47, -183
  ];

  public IImage<Rgba32> CreateImage(int width, int height)
    => new Rgba32Image(hasAlpha ? PixelFormat.ETC1A : PixelFormat.ETC1,
                       width,
                       height);

  public int TileWidth => 8;
  public int TileHeight => 8;

  public void Decode(IBinaryReader br,
                     Span<Rgba32> scan0,
                     int tileX,
                     int tileY,
                     int imageWidth,
                     int imageHeight) {
    var x = tileX * this.TileWidth;
    var y = tileY * this.TileHeight;

    var blockXCount = Math.Min((imageWidth - x) / 4, 2);
    var blockYCount = Math.Min((imageHeight - y) / 4, 2);

    Span<Rgb24> colors = stackalloc Rgb24[4 * 4];

    var valuesPerBlock = hasAlpha ? 2 : 1;
    var valueCount = (int) FinMath.Clamp(
        blockXCount * blockYCount * valuesPerBlock,
        0,
        (br.Length - br.Position) / 8);
    if (valueCount == 0) {
      return;
    }

    Span<ulong> blocksSpan = stackalloc ulong[valueCount];
    br.ReadUInt64s(blocksSpan);

    for (int by = 0; by < 4 * blockXCount; by += 4) {
      for (int bx = 0; bx < 4 * blockYCount; bx += 4) {
        if (valueCount-- <= 0) {
          return;
        }

        var blockSpan =
            blocksSpan.Slice(
                (blockXCount * (by / 4) + (bx / 4)) * valuesPerBlock,
                valuesPerBlock);

        var alpha = 0xFFFFFFFFFFFFFFFF;
        if (hasAlpha) {
          alpha = blockSpan[0];
        }

        var block = blockSpan[valuesPerBlock - 1];

        DecodeETC1Block_(colors, block);
        for (int py = 0; py < 4; py++) {
          if (y + by + py >= imageHeight) {
            break;
          }

          for (int px = 0; px < 4; px++) {
            if (x + bx + px >= imageWidth) {
              break;
            }

            var offsetInTile = py * 4 + px;
            var color = colors[offsetInTile];
            byte pixelAlpha =
                (byte) (((alpha >> (((px * 4) + py) * 4)) & 0xF) * 17);

            var offsetInImage = (y + by + py) * imageWidth + x + bx + px;
            scan0[offsetInImage] =
                new Rgba32(color.R, color.G, color.B, pixelAlpha);
          }
        }
      }
    }
  }

  private static void DecodeETC1Block_(Span<Rgb24> colors, ulong block) {
    byte r1, g1, b1, r2, g2, b2;

    byte tableIndex1 = (byte) ((block >> 37) & 0x07);
    byte tableIndex2 = (byte) ((block >> 34) & 0x07);
    byte diffBit = (byte) ((block >> 33) & 0x01);
    byte flipBit = (byte) ((block >> 32) & 0x01);

    if (diffBit == 0x00) {
      /* Individual mode */
      r1 = (byte) (((block >> 60) & 0x0F) << 4 | (block >> 60) & 0x0F);
      g1 = (byte) (((block >> 52) & 0x0F) << 4 | (block >> 52) & 0x0F);
      b1 = (byte) (((block >> 44) & 0x0F) << 4 | (block >> 44) & 0x0F);

      r2 = (byte) (((block >> 56) & 0x0F) << 4 | (block >> 56) & 0x0F);
      g2 = (byte) (((block >> 48) & 0x0F) << 4 | (block >> 48) & 0x0F);
      b2 = (byte) (((block >> 40) & 0x0F) << 4 | (block >> 40) & 0x0F);
    } else {
      /* Differential mode */

      /* 5bit base values */
      byte r1a = (byte) (((block >> 59) & 0x1F));
      byte g1a = (byte) (((block >> 51) & 0x1F));
      byte b1a = (byte) (((block >> 43) & 0x1F));

      /* Subblock 1, 8bit extended */
      r1 = (byte) ((r1a << 3) | (r1a >> 2));
      g1 = (byte) ((g1a << 3) | (g1a >> 2));
      b1 = (byte) ((b1a << 3) | (b1a >> 2));

      /* 3bit modifiers */
      sbyte dr2 = (sbyte) ((block >> 56) & 0x07);
      sbyte dg2 = (sbyte) ((block >> 48) & 0x07);
      sbyte db2 = (sbyte) ((block >> 40) & 0x07);
      if (dr2 >= 4) dr2 -= 8;
      if (dg2 >= 4) dg2 -= 8;
      if (db2 >= 4) db2 -= 8;

      /* Subblock 2, 8bit extended */
      r2 = (byte) ((r1a + dr2) << 3 | (r1a + dr2) >> 2);
      g2 = (byte) ((g1a + dg2) << 3 | (g1a + dg2) >> 2);
      b2 = (byte) ((b1a + db2) << 3 | (b1a + db2) >> 2);
    }

    for (int py = 0; py < 4; py++) {
      for (int px = 0; px < 4; px++) {
        var indexInPart = py * 4 + px;

        int index = (int) (((block >> ((px * 4) + py)) & 0x1) |
                           ((block >> (((px * 4) + py) + 16)) & 0x1) << 1);

        if ((flipBit == 0x01 && py < 2) || (flipBit == 0x00 && px < 2)) {
          int modifier =
              ETC1_MODIFIER_TABLES_[4 * tableIndex1 + index];
          colors[indexInPart] = new Rgb24(ClampByte_(r1 + modifier),
                                          ClampByte_(g1 + modifier),
                                          ClampByte_(b1 + modifier));
        } else {
          int modifier =
              ETC1_MODIFIER_TABLES_[4 * tableIndex2 + index];
          colors[indexInPart] = new Rgb24(ClampByte_(r2 + modifier),
                                          ClampByte_(g2 + modifier),
                                          ClampByte_(b2 + modifier));
        }
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte ClampByte_(int value)
    => (byte) Math.Clamp(value, byte.MinValue, byte.MaxValue);
}