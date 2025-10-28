﻿using System.Collections.Generic;
using System.Linq;

using schema.binary;

namespace grezzo.schema.csab;

public sealed class AnimationNode(Csab parent) : IBinaryDeserializable {
  public ushort BoneIndex { get; set; }

  public IReadOnlyList<CsabTrack> TranslationAxes { get; } = Enumerable.Range(0, 3)
      .Select(_ => new CsabTrack(parent, TrackType.POSITION))
      .ToArray();

  public IReadOnlyList<CsabTrack> RotationAxes { get; } = Enumerable.Range(0, 3)
      .Select(_ => new CsabTrack(parent, TrackType.ROTATION))
      .ToArray();

  public IReadOnlyList<CsabTrack> ScaleAxes { get; } = Enumerable.Range(0, 3)
      .Select(_ => new CsabTrack(parent, TrackType.SCALE))
      .ToArray();

  public bool IsPastVersion4 => parent.IsPastVersion4;

  public void Read(IBinaryReader br) {
      var basePosition = br.Position;

      br.AssertString("anod");

      this.BoneIndex = br.ReadUInt16();

      var isRotationShort = br.ReadUInt16() != 0;

      foreach (var translationAxis in this.TranslationAxes) {
        var offset = br.ReadUInt16();
        if (offset != 0) {
          br.SubreadAt(basePosition + offset, () => translationAxis.Read(br));
        }
      }

      foreach (var rotationAxis in this.RotationAxes) {
        rotationAxis.AreRotationsShort = isRotationShort;

        var offset = br.ReadUInt16();
        if (offset != 0) {
          br.SubreadAt(basePosition + offset, () => rotationAxis.Read(br));
        }
      }

      foreach (var scaleAxis in this.ScaleAxes) {
        var offset = br.ReadUInt16();
        if (offset != 0) {
          br.SubreadAt(basePosition + offset, () => scaleAxis.Read(br));
        }
      }

      br.AssertUInt16(0x00);
    }
}