﻿using grezzo.api;

namespace uni.games.luigis_mansion_3d;

public sealed class LuigisMansion3dMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new LuigisMansion3dFileBundleGatherer(),
                                    new CmbModelImporter());
}