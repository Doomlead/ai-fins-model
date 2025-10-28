﻿using System.Collections.Generic;

using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace pikmin1.api {
  public sealed class ModModelFileBundle : IModelFileBundle {
    public IReadOnlyTreeFile MainFile => this.ModFile;
    public IEnumerable<IReadOnlyGenericFile> Files
      => this.ModFile.Yield().ConcatIfNonnull(this.AnmFile);

    public required IReadOnlyTreeFile ModFile { get; init; }
    public required IReadOnlyTreeFile? AnmFile { get; init; }
  }
}