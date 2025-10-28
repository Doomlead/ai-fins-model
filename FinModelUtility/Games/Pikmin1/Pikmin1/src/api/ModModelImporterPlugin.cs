﻿using System.Collections.Generic;
using System.Linq;

using fin.io;
using fin.model;
using fin.model.io;

namespace pikmin1.api {
  public sealed class ModModelImporterPlugin : IModelImporterPlugin {
    public string DisplayName => "Mod";

    public string Description => "Pikmin 1 model format.";

    public IReadOnlyList<string> KnownPlatforms { get; } =
      ["GameCube"];

    public IReadOnlyList<string> KnownGames { get; } = ["Pikmin 1",];


    public IReadOnlyList<string> MainFileExtensions { get; } = [".mod"];

    public IReadOnlyList<string> FileExtensions { get; } =
      [".anm", ".mod"];

    public IModel Import(IEnumerable<IReadOnlyTreeFile> files,
                         float frameRate = 30) {
      var filesArray = files.ToArray();
      var anmFile = filesArray.WithFileType(".anm").SingleOrDefault();
      var modFile = filesArray.WithFileType(".mod").Single();

      var modBundle = new ModModelFileBundle {
          AnmFile = anmFile, ModFile = modFile,
      };

      var modImporter = new ModModelImporter();
      return modImporter.Import(modBundle);
    }
  }
}