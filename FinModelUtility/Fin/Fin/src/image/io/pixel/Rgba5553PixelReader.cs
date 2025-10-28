﻿using System;

using CommunityToolkit.HighPerformance;

using fin.image.formats;
using fin.math;
using fin.color;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel;

public sealed class Rgba5553PixelReader : IPixelReader<Rgba32> {
  public IImage<Rgba32> CreateImage(int width, int height)
    => new Rgba32Image(PixelFormat.RGBA5553, width, height);

  public void Decode(ReadOnlySpan<byte> data, Span<Rgba32> scan0, int offset) {
    var pix = data.Cast<byte, ushort>()[0];

    // Alpha flag
    if (BitLogic.ExtractFromRight(pix, 15, 1) == 1) {
      scan0[offset] = new Rgba32(
          ColorUtil.ExtractScaled(pix, 10, 5),
          ColorUtil.ExtractScaled(pix, 5, 5),
          ColorUtil.ExtractScaled(pix, 0, 5),
          255);
    } else {
      scan0[offset] = new Rgba32(
          ColorUtil.ExtractScaled(pix, 8, 4, 17),
          ColorUtil.ExtractScaled(pix, 4, 4, 17),
          ColorUtil.ExtractScaled(pix, 0, 4, 17),
          ColorUtil.ExtractScaled(pix, 12, 3));
    }
  }

  public int BitsPerPixel => 16;
}