﻿using System.Reflection;

using ast.api;

using fin.io;
using fin.testing;
using fin.testing.audio;

namespace ast;

public sealed class AstGoldenTests
    : BAudioGoldenTests<AstAudioFileBundle, AstAudioReader> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => this.AssertGolden(goldenDirectory);

  public override AstAudioFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new() { AstFile = directory.FilesWithExtension(".ast").Single() };

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .ToArray();
}