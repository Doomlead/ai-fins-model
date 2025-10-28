﻿using fin.io;
using fin.log;
using fin.util.asserts;
using fin.util.cmd;

using uni.games;
using uni.platforms.gcn.tools;

namespace uni.platforms.threeDs.tools.ctrtool;

public static partial class Ctrtool {
  public sealed class CciExtractor {
    public bool Run(IReadOnlySystemFile romFile,
                    out IFileHierarchy hierarchy) {
      Asserts.True(
          romFile.Exists,
          $"Cannot dump ROM because it does not exist: {romFile}");

      var didChange = false;

      if (ExtractorUtil.HasNotBeenExtractedYet(romFile, out var directory)) {
        didChange = true;
        this.DumpRom_(romFile, directory);
        Asserts.False(directory.IsEmpty,
                      $"Failed to extract contents from the ROM: {romFile.FullPath}");
      }

      hierarchy = ExtractorUtil.GetFileHierarchy(romFile.NameWithoutExtension, directory);
      return didChange;
    }

    private void DumpRom_(IReadOnlySystemFile romFile,
                          ISystemDirectory dstDirectory) {
      var logger = Logging.Create<CciExtractor>();
      logger.LogInformation($"Dumping ROM {romFile}...");

      RunInCtrDirectoryAndCleanUp_(
          () => {
            ProcessUtil
                .ExecuteBlockingSilently(
                    ThreeDsToolsConstants
                        .EXTRACT_CCI_BAT,
                    $"\"{romFile.FullPath}\"",
                    $"\"{dstDirectory.FullPath}\"");
          });
    }
  }
}