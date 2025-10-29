using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using fin.common;
using fin.config;
using fin.io;
using fin.io.archive;
using fin.util.strings;

using gx.archives.rarc;
using gx.compression.yay0;
using gx.compression.yaz0;

using uni.games;
using uni.platforms.gcn.tools;

namespace uni.platforms.gcn;

public sealed class GcnFileHierarchyExtractor {
  private readonly RarcDump rarcDump_ = new();
  private readonly RelDump relDump_ = new();
  private readonly Yay0Dec yay0Dec_ = new();
  private readonly Yaz0Dec yaz0Dec_ = new();

  public bool TryToExtractFromGame(
      string gameName,
      out IFileHierarchy fileHierarchy)
    => this.TryToExtractFromGame(gameName,
                                 Options.Standard(),
                                 out fileHierarchy);

  public bool TryToExtractFromGame(
      string gameName,
      Options options,
      out IFileHierarchy fileHierarchy) {
    if (!this.TryToFindRom_(gameName, options, out var romFile)) {
      fileHierarchy = null;
      return false;
    }

    fileHierarchy = this.ExtractFromRom_(romFile, options);
    return true;
  }

  private bool TryToFindRom_(
      string gameName,
      Options options,
      out ISystemFile romFile) {
    var romBasenameCandidates = new List<string> { gameName };
    romBasenameCandidates.AddRange(options.RomBasenameAliases);

    foreach (var candidate in romBasenameCandidates) {
      if (DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFileWithFileType(
              candidate,
              out romFile,
              ".ciso",
              ".nkit.iso",
              ".iso",
              ".gcm")) {
        return true;
      }
    }

    var sanitizedTargets = romBasenameCandidates
        .Select(SanitizeRomBasename_)
        .Where(s => s.Length > 0)
        .ToHashSet();

    foreach (var file in DirectoryConstants.ROMS_DIRECTORY.GetExistingFiles()) {
      if (file is not ISystemFile systemFile) {
        continue;
      }

      var fileType = systemFile.FileType;
      if (fileType is not ".ciso" and not ".nkit.iso" and not ".iso" and not ".gcm") {
        continue;
      }

      var sanitizedName = SanitizeRomBasename_(systemFile.NameWithoutExtension);
      if (sanitizedTargets.Any(target => sanitizedName.Contains(target))) {
        romFile = systemFile;
        return true;
      }
    }

    romFile = null;
    return false;
  }

  private static string SanitizeRomBasename_(ReadOnlySpan<char> value) {
    var builder = new StringBuilder(value.Length);
    foreach (var ch in value) {
      if (char.IsLetterOrDigit(ch)) {
        builder.Append(char.ToLowerInvariant(ch));
      }
    }

    return builder.ToString();
  }

  public IFileHierarchy ExtractFromRom_(
      ISystemFile romFile,
      Options options) {
    var directory = ExtractorUtil.GetOrCreateExtractedDirectory(romFile);
    if (new SubArchiveExtractor().TryToExtractIntoNewDirectory<GcmReader>(
            romFile,
            directory) ==
        ArchiveExtractionResult.FAILED) {
      throw new Exception();
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy(romFile.NameWithoutExtension,
                                         directory);
    var hasChanged = false;

    // Decompresses all of the archives,
    foreach (var subdir in fileHierarchy) {
      var didDecompress = false;

      // Decompresses files
      foreach (var file in subdir.FilesWithExtensions(
                   options.Yay0DecExtensions)) {
        didDecompress |=
            this.yay0Dec_.Run(file,
                              file.Impl.CloneWithFileType(".rarc"),
                              options.ContainerCleanupEnabled);
      }

      foreach (var file in subdir.FilesWithExtensions(
                   options.Yaz0DecExtensions)) {
        didDecompress |=
            this.yaz0Dec_.Run(file, options.ContainerCleanupEnabled);
      }

      // Updates to see any new decompressed files.
      if (didDecompress) {
        hasChanged = true;
        subdir.Refresh();
      }


      // Dumps any REL files
      var didDump = false;
      var relFiles =
          subdir.GetExistingFiles()
                .Where(file => file.Name.IndexOf(".rel") != -1 &&
                               file.FileType == ".rarc")
                .ToArray();
      foreach (var relFile in relFiles) {
        var prefix = relFile.Name.SubstringUpTo(".rel").ToString();
        var mapFile =
            subdir.GetExistingFiles()
                  .Single(file => file.Name.StartsWith(prefix) &&
                                  file.FileType == ".map");
        didDump |=
            this.relDump_.Run(relFile,
                              mapFile,
                              options.ContainerCleanupEnabled);
      }

      // Dumps any ARC/RARC files.
      var arcFiles =
          subdir.GetExistingFiles()
                .Where(file => !relFiles.Contains(file))
                .Where(file => options.RarcDumpExtensions.Contains(
                           file.FileType))
                .ToArray();
      foreach (var arcFile in arcFiles) {
        didDump |=
            this.rarcDump_.Run(arcFile,
                               options.ContainerCleanupEnabled,
                               options.RarcDumpPruneNames);
      }

      // Updates to see any new dumped directories.
      if (didDump) {
        hasChanged = true;
        subdir.Refresh();
      }
    }

    if (hasChanged) {
      fileHierarchy.RefreshRootAndUpdateCache();
    }

    return fileHierarchy;
  }

  public sealed class Options {
    private readonly HashSet<string> rarcDumpExtensions_ = [];
    private readonly HashSet<string> rarcDumpPruneNames_ = [];
    private readonly HashSet<string> yay0DecExtensions_ = [];
    private readonly HashSet<string> yaz0DecExtensions_ = [];
    private readonly List<string> romBasenameAliases_ = [];

    private Options() {
      this.RarcDumpExtensions = this.rarcDumpExtensions_;
      this.RarcDumpPruneNames = this.rarcDumpPruneNames_;
      this.Yay0DecExtensions = this.yay0DecExtensions_;
      this.Yaz0DecExtensions = this.yaz0DecExtensions_;
      this.RomBasenameAliases = this.romBasenameAliases_;
    }

    public static Options Empty() => new();

    public static Options Standard()
      => new Options().UseYaz0DecForExtensions(".szs")
                      .UseRarcDumpForExtensions(".rarc")
                      .EnableContainerCleanup(FinConfig.CleanUpArchives);

    public IReadOnlySet<string> RarcDumpExtensions { get; }
    public IReadOnlySet<string> RarcDumpPruneNames { get; }
    public IReadOnlySet<string> Yaz0DecExtensions { get; }
    public IReadOnlySet<string> Yay0DecExtensions { get; }
    public IReadOnlyList<string> RomBasenameAliases { get; }
    public bool ContainerCleanupEnabled { get; private set; }

    public Options UseRarcDumpForExtensions(
        string first,
        params string[] rest) {
      this.rarcDumpExtensions_.Add(first);
      foreach (var o in rest) {
        this.rarcDumpExtensions_.Add(o);
      }

      return this;
    }

    public Options PruneRarcDumpNames(
        string first,
        params string[] rest) {
      this.rarcDumpPruneNames_.Add(first);
      foreach (var o in rest) {
        this.rarcDumpPruneNames_.Add(o);
      }

      return this;
    }

    public Options UseYay0DecForExtensions(
        string first,
        params string[] rest) {
      this.yay0DecExtensions_.Add(first);
      foreach (var o in rest) {
        this.yay0DecExtensions_.Add(o);
      }

      return this;
    }

    public Options UseYaz0DecForExtensions(
        string first,
        params string[] rest) {
      this.yaz0DecExtensions_.Add(first);
      foreach (var o in rest) {
        this.yaz0DecExtensions_.Add(o);
      }

      return this;
    }

    public Options AddRomBasenameAliases(
        string first,
        params string[] rest) {
      this.romBasenameAliases_.Add(first);
      foreach (var alias in rest) {
        this.romBasenameAliases_.Add(alias);
      }

      return this;
    }

    public Options EnableContainerCleanup(bool enabled) {
      this.ContainerCleanupEnabled = enabled;
      return this;
    }
  }
}