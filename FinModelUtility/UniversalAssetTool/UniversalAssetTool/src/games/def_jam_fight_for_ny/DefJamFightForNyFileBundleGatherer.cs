using System.Linq;

using fin.io;
using fin.io.bundles;
using fin.util.progress;

using modl.api;

using uni.games.battalion_wars_1;
using uni.platforms.gcn;
using uni.util.io;

namespace uni.games.def_jam_fight_for_ny;

public sealed class DefJamFightForNyFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "def_jam_fight_for_ny";

  public override GcnFileHierarchyExtractor.Options Options
    => GcnFileHierarchyExtractor.Options.Standard()
           .AddRomBasenameAliases(
               "Def Jam - Fight for NY (USA)",
               "Def Jam - Fight for NY (Europe)",
               "Def Jam - Fight for NY (USA) (En,Fr,Es)",
               "Def Jam - Fight for NY (Europe) (En,Fr,De,Es,It,Nl)");

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var didUpdateAny = false;
    foreach (var directory in fileHierarchy) {
      var didUpdate = false;
      foreach (var resFile in directory
                   .GetExistingFiles()
                   .Where(file => file.Name.EndsWith(".res") ||
                                  file.Name.EndsWith(".res.gz"))) {
        didUpdateAny |= didUpdate |= new ResDump().Run(resFile);
      }

      if (didUpdate) {
        directory.Refresh();
      }
    }

    if (didUpdateAny) {
      fileHierarchy.RefreshRootAndUpdateCache();
    }

    new FileHierarchyAssetBundleSeparator(
        fileHierarchy,
        (directory, organizer) => {
          var animFiles = directory.FilesWithExtension(".anim").ToArray();
          foreach (var modlFile in directory.FilesWithExtension(".modl")) {
            organizer.Add(new ModlModelFileBundle {
                ModlFile = modlFile,
                GameVersion = GameVersion.BW2,
                AnimFiles = animFiles,
            }.Annotate(modlFile));
          }

          foreach (var outFile in directory
                       .GetExistingFiles()
                       .Where(file => file.Name.EndsWith(".out") ||
                                      file.Name.EndsWith(".out.gz"))) {
            organizer.Add(new OutModelFileBundle {
                OutFile = outFile,
                GameVersion = GameVersion.BW2,
            }.Annotate(outFile));
          }
        })
        .GatherFileBundles(organizer, mutablePercentageProgress);
  }
}
