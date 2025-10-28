﻿using fin.io;
using fin.model;
using fin.model.io.importers;

using HaloWarsTools;

using hw.schema.xtt;

namespace hw.api;

public sealed class XtdModelImporter : IModelImporter<XtdModelFileBundle> {
  public IModel Import(XtdModelFileBundle modelFileBundle) {
    var xtdFile = modelFileBundle.XtdFile;
    var xttFile = modelFileBundle.XttFile;

    var mapName = xtdFile.AssertGetParent().Name;

    var xtd = HWXtdResource.FromFile(null, xtdFile.FullPath);
    var xtt = xttFile.ReadNew<Xtt>();

    var finModel = xtd.Mesh;
    var xttMaterial = finModel.MaterialManager.AddStandardMaterial();

    var diffuseTexture = finModel.MaterialManager.CreateTexture(
        xtt.AlbedoTexture);
    diffuseTexture.Name = $"{mapName}_albedo";
    xttMaterial.DiffuseTexture = diffuseTexture;

    var ambientOcclusionTexture = finModel.MaterialManager.CreateTexture(
        xtd.AmbientOcclusionTexture);
    ambientOcclusionTexture.Name = $"{mapName}_ao";
    xttMaterial.AmbientOcclusionTexture = ambientOcclusionTexture;

    foreach (var primitive in finModel.Skin.Meshes[0].Primitives) {
      primitive.SetMaterial(xttMaterial);
    }

    return xtd.Mesh;
  }
}