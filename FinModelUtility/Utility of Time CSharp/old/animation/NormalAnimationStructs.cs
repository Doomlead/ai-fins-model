﻿using System.Collections.Generic;

namespace UoT {
  // Based on the structs at:
  // https://wiki.cloudmodding.com/oot/Animation_Format#Normal_Animations

  public sealed class NormalAnimation : IAnimation {
    public ushort[] Angles = [];
    public IList<Vec3s> Positions;
    public NormalAnimationTrack[] Tracks = [];
    public uint TrackOffset;
    public uint AngleCount;

    public ushort FrameCount { get; set; }

    public int PositionCount => this.Positions.Count;
    public Vec3s GetPosition(int i) => this.Positions[i];

    public int TrackCount => this.Tracks.Length;
    public IAnimationTrack GetTrack(int i) => this.Tracks[i];

    public FacialState GetFacialState(int _) => FacialState.DEFAULT;
  }

  public sealed class NormalAnimationTrack : IAnimationTrack {
    public int Type { get; set; } // 0 = constant, 1 = keyframe
    public IList<ushort> Frames { get; set; } = [];
  }
}
