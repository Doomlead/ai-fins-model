﻿using fin.audio;
using fin.audio.stubbed;

using OpenTK.Audio.OpenAL;


namespace fin.ui.playback.al;

public partial class AlAudioManager : IAudioManager<short> {
  private readonly ALDevice device_;
  private readonly ALContext context_;

  public bool IsDisposed { get; private set; }
  public IAudioPlayer<short> AudioPlayer { get; }

  public static IAudioManager<short> TryToCreateOrStub() {
    try {
      return new AlAudioManager();
    } catch (Exception ex) {
      return new StubbedAudioManager();
    }
  }

  private AlAudioManager() {
    this.device_ = ALC.OpenDevice(null);
    this.context_
        = ALC.CreateContext(this.device_, new ALContextAttributes());
    ALC.ProcessContext(this.context_);
    ALC.MakeContextCurrent(this.context_);

    this.AudioPlayer = new AlAudioPlayer();
  }

  ~AlAudioManager() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.IsDisposed = true;
    this.AudioPlayer.Dispose();
    ALC.DestroyContext(this.context_);
  }
}