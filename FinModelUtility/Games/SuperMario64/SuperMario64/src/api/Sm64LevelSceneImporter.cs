﻿using System.Numerics;

using fin.data.lazy;
using fin.io;
using fin.model;
using fin.scene;
using fin.util.sets;

using sm64.LevelInfo;
using sm64.Scripts;

namespace sm64.api;

public sealed class Sm64LevelSceneFileBundle(
    IReadOnlyTreeDirectory directory,
    IReadOnlyTreeFile sm64Rom,
    LevelId levelId)
    : ISceneFileBundle {
  public IReadOnlyTreeFile? MainFile => null;
  public IReadOnlyTreeDirectory Directory { get; } = directory;

  public IReadOnlyTreeFile Sm64Rom { get; } = sm64Rom;
  public LevelId LevelId { get; } = levelId;
  string IUiFile.HumanReadableName => $"{this.LevelId}".ToLower();
  public string TrueFullPath => this.Sm64Rom.FullPath;
}

public sealed class Sm64LevelSceneImporter : ISceneImporter<Sm64LevelSceneFileBundle> {
  public IScene Import(Sm64LevelSceneFileBundle levelModelFileBundle) {
    var sm64Level = Sm64LevelImporter.LoadLevel(levelModelFileBundle);

    var finScene = new SceneImpl {
        FileBundle = levelModelFileBundle,
        Files = levelModelFileBundle.Sm64Rom.AsFileSet()
    };

    var lazyModelDictionary = new LazyDictionary<ushort, IModel?>(
        sm64ModelId => {
          if (sm64Level.ModelIDs.TryGetValue(sm64ModelId,
                                             out var sm64Model)) {
            return sm64Model.HighestLod2.Model;
          }

          return null;
        });

    foreach (var sm64Area in sm64Level.Areas) {
      AddAreaToScene_(
          finScene,
          lazyModelDictionary,
          sm64Area);
    }

    return finScene;
  }

  private static void AddAreaToScene_(
      IScene finScene,
      LazyDictionary<ushort, IModel?> lazyModelDictionary,
      Area sm64Area) {
    var finArea = finScene.AddArea();
    AddAreaModelToScene_(finArea, sm64Area);

    var objects =
        sm64Area.Objects.Concat(sm64Area.MacroObjects)
                .Concat(sm64Area.SpecialObjects)
                .ToArray();

    foreach (var obj in objects) {
      AddAreaObjectToScene_(finArea, lazyModelDictionary, obj);
    }
  }

  private static void AddAreaModelToScene_(ISceneArea finArea, Area sm64Area)
    => finArea.AddRootNode()
              .AddSceneModel(sm64Area.AreaModel.HighestLod2.Model);

  private static void AddAreaObjectToScene_(
      ISceneArea finArea,
      LazyDictionary<ushort, IModel?> lazyModelDictionary,
      Object3D sm64Object) {
    var finModel = lazyModelDictionary[sm64Object.ModelID];
    if (finModel == null) {
      return;
    }

    var finObject = finArea.AddRootNode();
    finObject.AddSceneModel(finModel);
    finObject.SetPosition(sm64Object.xPos, sm64Object.yPos, sm64Object.zPos);
    finObject.SetRotationDegrees(sm64Object.xRot,
                                 sm64Object.yRot,
                                 sm64Object.zRot);

    var scale = 1f;
    var billboard = false;

    var scripts = sm64Object.ParseBehavior();
    foreach (var script in scripts) {
      if (script.Command == BehaviorCommand.SCALE) {
        var rawScale = BitLogic.BytesToInt(script.data, 2, 2);
        scale = rawScale / 100f;
      }

      if (script.Command == BehaviorCommand.billboard) {
        billboard = true;
      }
    }

    finObject.SetScale(scale, scale, scale);
    if (billboard) {
      var rotateYaw =
          Quaternion.CreateFromYawPitchRoll(-MathF.PI / 2, 0, 0);
      finModel.Skeleton.Root.AlwaysFaceTowardsCamera(
          FaceTowardsCameraType.YAW_ONLY,
          rotateYaw);
    }
  }
}