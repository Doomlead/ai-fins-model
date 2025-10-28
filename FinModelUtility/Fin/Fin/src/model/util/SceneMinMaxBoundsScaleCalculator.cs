﻿using System;
using System.Collections.Generic;
using System.Numerics;

using fin.model.skeleton;
using fin.scene;
using fin.scene.components;
using fin.util.linq;

namespace fin.model.util;

public sealed class SceneMinMaxBoundsScaleCalculator
    : BMinMaxBoundsScaleCalculator<ISceneInstance> {
  public override Bounds CalculateBounds(ISceneInstance scene) {
    var minX = float.MaxValue;
    var minY = float.MaxValue;
    var minZ = float.MaxValue;
    var maxX = float.MinValue;
    var maxY = float.MinValue;
    var maxZ = float.MinValue;

    foreach (var animatableModel in scene.EnumerateAllAnimatableModels()) {
      FactorInModel_(animatableModel.Model,
                     animatableModel.BoneTransformManager,
                     ref minX,
                     ref minY,
                     ref minZ,
                     ref maxX,
                     ref maxY,
                     ref maxZ);
    }

    return new Bounds(minX, minY, minZ, maxX, maxY, maxZ);
  }

  private static void FactorInModel_(
      IReadOnlyModel model,
      IReadOnlyBoneTransformManager2 boneTransformManager,
      ref float minX,
      ref float minY,
      ref float minZ,
      ref float maxX,
      ref float maxY,
      ref float maxZ) {
    var anyVertices = false;
    foreach (var vertex in model.Skin.Vertices) {
      anyVertices = true;

      boneTransformManager.ProjectVertexPosition(
          vertex,
          out var position);

      var x = position.X;
      var y = position.Y;
      var z = position.Z;

      minX = MathF.Min(minX, x);
      maxX = MathF.Max(maxX, x);

      minY = MathF.Min(minY, y);
      maxY = MathF.Max(maxY, y);

      minZ = MathF.Min(minZ, z);
      maxZ = MathF.Max(maxZ, z);
    }

    if (!anyVertices) {
      var boneQueue = new Queue<IReadOnlyBone>();
      boneQueue.Enqueue(model.Skeleton.Root);

      while (boneQueue.Count > 0) {
        var bone = boneQueue.Dequeue();

        var xyz = new Vector3();
        boneTransformManager.ProjectPosition(bone, ref xyz);

        minX = MathF.Min(minX, xyz.X);
        maxX = MathF.Max(maxX, xyz.X);

        minY = MathF.Min(minY, xyz.Y);
        maxY = MathF.Max(maxY, xyz.Y);

        minZ = MathF.Min(minZ, xyz.Z);
        maxZ = MathF.Max(maxZ, xyz.Z);

        foreach (var child in bone.Children) {
          boneQueue.Enqueue(child);
        }
      }
    }
  }
}