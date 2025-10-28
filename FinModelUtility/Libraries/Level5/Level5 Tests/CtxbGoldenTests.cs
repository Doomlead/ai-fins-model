﻿using System.Reflection;

using fin.image;
using fin.io;
using fin.testing;

using level5.schema;

namespace level5;

public sealed class XiGoldenTests {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IReadOnlySystemDirectory goldenDirectory) {
    var inputFile = goldenDirectory.AssertGetExistingSubdir("input")
                                   .GetFilesWithFileType(".xi")
                                   .Single();

    var xi = new Xi();
    xi.Open(inputFile);
    var inputImage = xi.ToBitmap();

    var outputFileName = $"{inputFile.NameWithoutExtension}.png";
    var outputDirectory = goldenDirectory.AssertGetExistingSubdir("output");

    var outputFile
        = new FinFile(Path.Join(outputDirectory.FullPath, outputFileName));
    if (outputFile.Exists) {
      var outputImage = await FinImage.FromFileAsync(outputFile);
      Assert.That(inputImage, Is.EqualTo(outputImage));
    } else {
      using var s = outputFile.OpenWrite();
      inputImage.ExportToStream(s, LocalImageFormat.PNG);
    }
  }

  private static IReadOnlySystemDirectory[] GetGoldenDirectories_() {
    var rootGoldenDirectory
        = GoldenAssert
          .GetRootGoldensDirectory(Assembly.GetExecutingAssembly())
          .AssertGetExistingSubdir("xi");
    return rootGoldenDirectory.GetExistingSubdirs().ToArray();
  }
}