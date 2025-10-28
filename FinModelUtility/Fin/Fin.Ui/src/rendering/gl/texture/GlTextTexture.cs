﻿using System.Drawing;
using System.Runtime.CompilerServices;

using fin.image;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using QuickFont;

namespace fin.ui.rendering.gl.texture;

public sealed class GlTextTexture : IGlTexture {
  private readonly GlFbo impl_;

  public GlTextTexture(string text,
                       QFont font,
                       Color color,
                       QFontAlignment alignment = QFontAlignment.Left) {
    var roughSize = font.Measure(text);
    var roughWidth = (int) MathF.Ceiling(roughSize.Width);
    var roughHeight = (int) MathF.Ceiling(roughSize.Height);

    var x = alignment switch {
        QFontAlignment.Left   => 0,
        QFontAlignment.Centre => roughWidth / 2,
        QFontAlignment.Right  => roughWidth,
        _ => throw new ArgumentOutOfRangeException(
            nameof(alignment),
            alignment,
            null)
    };

    using var drawing = new QFontDrawing();
    var size = drawing.Print(font,
                             text,
                             new Vector3(x, roughHeight, 0),
                             alignment,
                             new QFontRenderOptions { Colour = color });

    drawing.ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(
        0,
        roughWidth,
        0,
        roughHeight,
        -1.0f,
        1.0f);
    drawing.RefreshBuffers();

    var width = (int) Math.Ceiling(size.Width);
    var height = (int) Math.Ceiling(size.Height);
    this.impl_ = new GlFbo(width, height);

    this.impl_.TargetFbo();
    GlUtil.SetViewport(new Rectangle(0, 0, width, height));
    GlUtil.SetClearColor(Color.Transparent);
    GlUtil.ClearColorAndDepth();
    drawing.Draw();
    GL.Finish();
    this.impl_.UntargetFbo();
  }

  ~GlTextTexture() => this.ReleaseUnmanagedResources_();

  public bool IsDisposed { get; private set; }

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.IsDisposed = true;
    this.impl_.Dispose();
  }

  public int Id => this.impl_.ColorTextureId;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Bind(int textureIndex = 0) => this.impl_.Bind();

  public IImage ConvertToImage(bool flipVertical = false)
    => this.impl_.ConvertToImage(flipVertical);
}