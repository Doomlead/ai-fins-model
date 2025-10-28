﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace fin.scene.instance;

public partial class SceneInstanceImpl {
  private class SceneAreaInstanceImpl(IReadOnlySceneArea sceneArea)
      : ISceneAreaInstance {
    ~SceneAreaInstanceImpl() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      this.CustomSkyboxObject?.Dispose();
      foreach (var obj in this.RootNodes) {
        obj.Dispose();
      }
    }

    public IReadOnlySceneArea Definition => sceneArea;

    public IReadOnlyList<ISceneNodeInstance> RootNodes { get; } = sceneArea
        .RootNodes
        .Select(o => new SceneNodeInstanceImpl(o))
        .ToArray();

    public void Tick() {
      foreach (var node in this.RootNodes) {
        node.Tick();
      }
    }

    private float viewerScale_ = 1;

    public float ViewerScale {
      get => this.viewerScale_;
      set {
        this.viewerScale_ = value;
        foreach (var obj in this.RootNodes) {
          obj.ViewerScale = this.viewerScale_;
        }
      }
    }

    public Color? BackgroundColor => sceneArea.BackgroundColor;

    public ISceneNodeInstance? CustomSkyboxObject { get; }
      = sceneArea.CustomSkyboxNode != null
          ? new SceneNodeInstanceImpl(sceneArea.CustomSkyboxNode)
          : null;
  }
}