﻿using ast.api;

using fin.audio;
using fin.audio.io;
using fin.audio.io.importers;
using fin.audio.io.importers.midi;
using fin.audio.io.importers.ogg;

using fmod.api;

using ssm.api;

namespace uni.api;

public sealed class GlobalAudioReader : IAudioImporter<IAudioFileBundle> {
  public ILoadedAudioBuffer<short>[] ImportAudio(
      IAudioManager<short> audioManager,
      IAudioFileBundle audioFileBundle)
    => audioFileBundle switch {
        AstAudioFileBundle astAudioFileBundle
            => new AstAudioReader().ImportAudio(
                audioManager,
                astAudioFileBundle),
        BankAudioFileBundle bankAudioFileBundle
            => new BankAudioImporter().ImportAudio(
                audioManager,
                bankAudioFileBundle),
        MidiAudioFileBundle midiAudioFileBundle
            => new MidiAudioImporter().ImportAudio(
                audioManager,
                midiAudioFileBundle),
        OggAudioFileBundle oggAudioFileBundle
            => new OggAudioImporter().ImportAudio(
                audioManager,
                oggAudioFileBundle),
        SsmAudioFileBundle ssmAudioFileBundle
            => new SsmAudioImporter().ImportAudio(
                audioManager,
                ssmAudioFileBundle),
        _ => throw new ArgumentOutOfRangeException(nameof(audioFileBundle))
    };
}