﻿using fin.io;
using fin.scene;

namespace games.pikmin2.api;

public sealed class Pikmin2SceneFileBundle : ISceneFileBundle {
  public string? GameName => "pikmin_2";
  public IReadOnlyTreeFile? MainFile => this.LevelBmd;

  public required IReadOnlyTreeFile LevelBmd { get; init; }
  public required IReadOnlyTreeFile RouteTxt { get; init; }
}