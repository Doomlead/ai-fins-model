﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace fin.io;

public static class FileExtensions {
  public static IEnumerable<TFile> WithFileType<TFile>(
      this IEnumerable<TFile> files,
      string fileType) where TFile : IReadOnlyTreeFile
    => files.Where(
        file => file.FullPath.EndsWith(fileType,
                                       StringComparison.OrdinalIgnoreCase));

  public static IEnumerable<TFile> WithFileTypes<TFile>(
      this IEnumerable<TFile> files,
      params string[] fileTypes) where TFile : IReadOnlyTreeFile
    => files.Where(file
                       => fileTypes.Any(fileType
                                            => file.FullPath.EndsWith(
                                                fileType,
                                                StringComparison
                                                    .OrdinalIgnoreCase)));
}