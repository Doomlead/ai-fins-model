﻿using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;

using sysdolphin.api;

namespace sysdolphin;

public sealed class DatModelGoldenTests
    : BModelGoldenTests<DatModelFileBundle, DatModelImporter> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override DatModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory) {
    var gameName = directory.Parent.Parent.Name;
    var datFile = directory.FilesWithExtension(".dat").Single();

    return new DatModelFileBundle {
        DatFile = datFile
    };
  }

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .Where(dir => !(dir.Name is "super_smash_bros_melee"))
       .SelectMany(dir => dir.GetExistingSubdirs())
       .ToArray();
}