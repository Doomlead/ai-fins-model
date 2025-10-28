﻿using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace visceral.api;

public sealed class GeoModelFileBundle : IModelFileBundle {
  public IReadOnlyTreeFile? MainFile
    => this.RcbFile ?? this.GeoFiles.First();

  public IEnumerable<IReadOnlyGenericFile> Files
    => this.GeoFiles
           .ConcatIfNonnull(this.RcbFile);

  public required IReadOnlyList<IReadOnlyTreeFile> GeoFiles { get; init; }
  public required IReadOnlyTreeFile? RcbFile { get; init; }

  public required BnkFileIdsDictionary BnkFileIdsDictionary { get; init; }
  public required MtlbFileIdsDictionary MtlbFileIdsDictionary { get; init; }
  public required Tg4hFileIdDictionary Tg4hFileIdDictionary { get; init; }
}