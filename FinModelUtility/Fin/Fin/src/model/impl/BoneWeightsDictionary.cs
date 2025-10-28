﻿using System;
using System.Collections.Generic;
using System.Linq;

using fin.util.asserts;

namespace fin.model.impl;

public sealed class BoneWeightsDictionary {
  private readonly List<IBoneWeights> boneWeights_ = [];

  private readonly Dictionary<int, BoneWeightsSet> boneWeightsByCount_ =
      new();

  private const float MIN_WEIGHT = .0001f;
  private const float WEIGHT_ERROR = .01f;

  public IReadOnlyList<IBoneWeights> List => this.boneWeights_;

  public IBoneWeights GetOrCreate(
      VertexSpace vertexSpace,
      out bool newlyCreated,
      params IReadOnlyBoneWeight[] weights
  ) {
    if (weights.Length > 1) {
      weights = weights.Where(boneWeight => boneWeight.Weight > MIN_WEIGHT)
                       .ToArray();
    }

    var totalWeight = 0f;
    foreach (var weight in weights) {
      totalWeight += weight.Weight;
    }

    Asserts.True(Math.Abs(totalWeight - 1) < WEIGHT_ERROR);

    if (!this.boneWeightsByCount_.TryGetValue(
            weights.Length,
            out var allBoneWeightsWithCount)) {
      allBoneWeightsWithCount = this.boneWeightsByCount_[weights.Length] =
          new BoneWeightsSet();
    }

    newlyCreated = false;
    if (!allBoneWeightsWithCount.TryGetExisting(vertexSpace,
                                                weights,
                                                out var boneWeights)) {
      newlyCreated = true;
      allBoneWeightsWithCount.Add(
          boneWeights = this.CreateInstance_(vertexSpace, weights));
    }

    return boneWeights;
  }

  public IBoneWeights Create(
      VertexSpace vertexSpace,
      params IReadOnlyBoneWeight[] weights
  ) {
    if (weights.Length > 1) {
      weights = weights.Where(boneWeight => boneWeight.Weight > MIN_WEIGHT)
                       .ToArray();
    }

    var totalWeight = 0f;
    foreach (var weight in weights) {
      totalWeight += weight.Weight;
    }

    Asserts.True(Math.Abs(totalWeight - 1) < WEIGHT_ERROR);

    if (!this.boneWeightsByCount_.TryGetValue(
            weights.Length,
            out var allBoneWeightsWithCount)) {
      allBoneWeightsWithCount = this.boneWeightsByCount_[weights.Length] =
          new BoneWeightsSet();
    }

    var boneWeights = this.CreateInstance_(vertexSpace, weights);
    allBoneWeightsWithCount.Add(boneWeights);

    return boneWeights;
  }

  private IBoneWeights CreateInstance_(
      VertexSpace vertexSpace,
      params IReadOnlyBoneWeight[] weights) {
    if (weights.Length > 1) {
      weights = weights.Where(boneWeight => boneWeight.Weight > MIN_WEIGHT)
                       .ToArray();
    }

    var totalWeight = weights.Select(weight => weight.Weight).Sum();
    Asserts.True(Math.Abs(totalWeight - 1) < WEIGHT_ERROR);

    var boneWeights = new BoneWeightsImpl {
        Index = this.boneWeights_.Count,
        VertexSpace = vertexSpace,
        Weights = weights,
    };

    this.boneWeights_.Add(boneWeights);

    return boneWeights;
  }

  public static int GetHashCode(VertexSpace vertexSpace,
                                IReadOnlyList<IReadOnlyBoneWeight> weights) {
    int hash = 216613626;
    var sub = 16780669;
    hash = hash * sub ^ vertexSpace.GetHashCode();
    foreach (var weight in weights) {
      hash = hash * sub ^ weight.GetHashCode();
    }

    return hash;
  }

  private class BoneWeightsImpl : IBoneWeights {
    public int Index { get; init; }
    public VertexSpace VertexSpace { get; init; }
    public IReadOnlyList<IReadOnlyBoneWeight> Weights { get; init; }

    public override int GetHashCode()
      => BoneWeightsSet.GetHashCode(this.VertexSpace, this.Weights);

    public override bool Equals(object? obj) {
      if (obj is not BoneWeightsImpl other) {
        return false;
      }

      return this.Equals(other);
    }

    public bool Equals(IReadOnlyBoneWeights? weights)
      => weights != null && this.Equals(weights.VertexSpace, weights.Weights);

    public bool Equals(VertexSpace vertexSpace,
                       IReadOnlyList<IReadOnlyBoneWeight> weights) {
      if (vertexSpace != this.VertexSpace) {
        return false;
      }

      var otherWeights = weights;
      if (this.Weights.Count != otherWeights.Count) {
        return false;
      }

      for (var w = 0; w < this.Weights.Count; ++w) {
        var weight = this.Weights[w];
        var existingWeight = otherWeights[w];

        if (weight.Bone != existingWeight.Bone) {
          return false;
        }

        if (Math.Abs(weight.Weight - existingWeight.Weight) > .0001) {
          return false;
        }

        if (!(weight.InverseBindMatrix == null &&
              existingWeight.InverseBindMatrix == null) ||
            !(weight.InverseBindMatrix?.Equals(
                  existingWeight.InverseBindMatrix) ??
              false)) {
          return false;
        }
      }

      return true;
    }
  }
}