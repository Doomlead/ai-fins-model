using modl.api;

namespace uni.games.def_jam_fight_for_ny;

public sealed class DefJamFightForNyMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new DefJamFightForNyFileBundleGatherer(),
                                    new BattalionWarsModelImporter());
}
