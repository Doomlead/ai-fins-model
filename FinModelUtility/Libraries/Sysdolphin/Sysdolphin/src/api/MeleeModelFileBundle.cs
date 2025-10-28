﻿using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace sysdolphin.api;

public sealed class MeleeModelFileBundle : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.PrimaryDatFile;
  public required IReadOnlyTreeFile PrimaryDatFile { get; init; }
  public IReadOnlyTreeFile? AnimationDatFile { get; init; }
  public IReadOnlyTreeFile? FighterDatFile { get; init; }

  public IEnumerable<IReadOnlyGenericFile> Files
    => this.MainFile.Yield()
           .ConcatIfNonnull(this.AnimationDatFile)
           .ConcatIfNonnull(this.FighterDatFile);
}