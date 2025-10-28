﻿using fin.animation.keyframes;
using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.nodes;
using fin.data.queues;
using fin.io;
using fin.math.rotations;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;

using level5.schema;

using OpenTK.Mathematics;

using schema.binary;

using Quaternion = System.Numerics.Quaternion;


namespace level5.api;

public sealed class XcModelImporter : IModelImporter<XcModelFileBundle> {
  public IModel Import(XcModelFileBundle modelFileBundle) {
    var endianness = Endianness.LittleEndian;

    var modelDirectory = modelFileBundle.ModelDirectory;
    var modelResourceFile =
        new Resource(modelDirectory.GetFilesWithFileType(".bin").Single());

    var model = new ModelImpl {
        FileBundle = modelFileBundle,
        Files = modelFileBundle.Files.ToHashSet()
    };

    var finBoneByIndex = new Dictionary<uint, IBone>();
    var finBoneByName = new Dictionary<string, IBone>();
    {
      var mbnFiles = modelDirectory.GetFilesWithFileType(".mbn").ToArray();
      if (mbnFiles.Any()) {
        var mbns = mbnFiles
                   .Select(mbnFile => mbnFile.ReadNew<Mbn>(endianness))
                   .ToArray();
        var mbnNodeList =
            mbns.Where(mbn => mbn.Id != mbn.ParentId)
                .DistinctBy(mbn => mbn.Id)
                .Select(mbn => new TreeNode<Mbn> {Value = mbn})
                .ToArray();
        var mbnByIndex =
            mbnNodeList.ToDictionary(node => node.Value.Id);

        foreach (var mbnNode in mbnNodeList) {
          var mbn = mbnNode.Value;
          if (mbn.ParentId == 0) {
            continue;
          }

          mbnNode.Parent = mbnByIndex[mbn.ParentId];
        }

        var rootMbnNodes =
            mbnNodeList.Where(node => node.Value.ParentId == 0);

        var mbnQueue =
            new FinTuple2Queue<ITreeNode<Mbn>, IBone>(
                rootMbnNodes.Select(
                    node => ((ITreeNode<Mbn>) node, model.Skeleton.Root)));
        while (mbnQueue.TryDequeue(out var mbnNode, out var parentBone)) {
          var mbn = mbnNode.Value;

          var bone = parentBone.AddChild(mbn.Position);
          bone.Name = modelResourceFile.GetResourceName(mbn.Id);

          var mat3 = mbn.RotationMatrix3;
          var matrix = new Matrix3(mat3[0],
                                   mat3[1],
                                   mat3[2],
                                   mat3[3],
                                   mat3[4],
                                   mat3[5],
                                   mat3[6],
                                   mat3[7],
                                   mat3[8]);
          var openTkQuaternion = matrix.ExtractRotation();
          var quaternion = new Quaternion(openTkQuaternion.X,
                                          openTkQuaternion.Y,
                                          openTkQuaternion.Z,
                                          openTkQuaternion.W);
          var eulerRadians = QuaternionUtil.ToEulerRadians(quaternion);
          bone.LocalTransform.SetRotationRadians(eulerRadians);

          var scale = mbn.Scale;
          bone.LocalTransform.SetScale(scale);

          finBoneByIndex[mbn.Id] = bone;
          finBoneByName[bone.Name] = bone;

          mbnQueue.Enqueue(
              mbnNode.ChildNodes.Select(childNode => (childNode, bone)));
        }
      }
    }

    var xiFiles = modelDirectory.GetFilesWithFileType(".xi").ToArray();
    var lazyTextures = new LazyCaseInvariantStringDictionary<ITexture>(
        textureName => {
          var textureIndex =
              modelResourceFile.TextureNames.IndexOf(textureName);
          var xiFile = xiFiles[textureIndex];

          var xi = new Xi();
          xi.Open(xiFile);

          var image = xi.ToBitmap();
          var texture = model.MaterialManager.CreateTexture(image);
          texture.Name = textureName;

          return texture;
        });

    var lazyMaterials = new LazyCaseInvariantStringDictionary<IMaterial>(
        materialName => {
          var binMaterial =
              modelResourceFile.Materials.Single(
                  mat => mat.Name == materialName);
          var finTexture = lazyTextures[binMaterial.TexName];

          var finMaterial =
              model.MaterialManager.AddTextureMaterial(finTexture);
          finMaterial.Name = binMaterial.Name;

          // TODO: Figure out how to fix culling issues
          finMaterial.CullingMode = CullingMode.SHOW_BOTH;

          return finMaterial;
        });

    // Adds vertices
    Dictionary<uint, (Prm prm, IMesh mesh)> prmsByAnimationReferenceHash =
        new();
    {
      var prmFiles = modelDirectory.GetFilesWithFileType(".prm").ToArray();
      if (prmFiles.Any()) {
        foreach (var prmFile in prmFiles) {
          var prm = new Prm(prmFile);

          var finMaterial = lazyMaterials[prm.MaterialName];

          var mesh = model.Skin.AddMesh();
          mesh.Name = prm.Name;

          var prmAnimationReferenceHashes = prm.AnimationReferenceHashes;
          foreach (var hash in prmAnimationReferenceHashes) {
            prmsByAnimationReferenceHash[hash] = (prm, mesh);
          }

          var finVertices = new List<IVertex>();
          foreach (var prmVertex in prm.Vertices) {
            var position = prmVertex.Pos;
            var finVertex =
                model.Skin.AddVertex(position.X, position.Y, position.Z);

            var uv = prmVertex.Uv0;
            finVertex.SetUv(uv.X, uv.Y);

            var normal = prmVertex.Nrm;
            finVertex.SetLocalNormal(normal.X, normal.Y, normal.Z);

            var prmBones = prmVertex.Bones;
            if (prmBones != null) {
              var boneWeightList = new List<BoneWeight>();
              for (var b = 0; b < 4; ++b) {
                var boneId = prmVertex.Bones[b];
                var weight = prmVertex.Weights[b];

                var finBone = finBoneByIndex[boneId];
                boneWeightList.Add(new BoneWeight(finBone, null, weight));
              }

              var boneWeights =
                  model.Skin.GetOrCreateBoneWeights(
                      VertexSpace.RELATIVE_TO_BONE,
                      boneWeightList.ToArray());
              finVertex.SetBoneWeights(boneWeights);
            }

            finVertices.Add(finVertex);
          }

          var finTriangleVertices = prm.Triangles
                                       .Select(vertexIndex =>
                                                   finVertices[
                                                       (int) vertexIndex])
                                       .ToArray();
          var triangles = mesh.AddTriangles(finTriangleVertices);
          triangles.SetMaterial(finMaterial);
        }
      }
    }

    // Adds animations
    {
      foreach (var animationDirectory in modelFileBundle.AnimationDirectories) {
        var binFile = animationDirectory.GetFilesWithFileType(".bin").Single();
        var animationResourceFile = new Resource(binFile);

        var mtn2Files = animationDirectory.GetFilesWithFileType(".mtn2").ToArray();
        if (mtn2Files.Any()) {
          foreach (var mtn2File in mtn2Files) {
            var mtn2 = new Mtn2();
            mtn2.Open(mtn2File);

            var anim = mtn2.Anim;

            var finAnimation = model.AnimationManager.AddAnimation();
            finAnimation.Name = anim.Name;
            finAnimation.FrameRate = 30;
            finAnimation.FrameCount = anim.FrameCount;

            foreach (var (animationReferenceHash, framesAndValues) in mtn2
                         .Somethings.GetPairs()) {
              if (!prmsByAnimationReferenceHash.TryGetValue(
                      animationReferenceHash,
                      out var prmAndMesh)) {
                continue;
              }

              var (prm, mesh) = prmAndMesh;
              var meshTracks = finAnimation.AddMeshTracks(mesh);
              var displayStates = meshTracks.DisplayStates;

              foreach (var frameAndValue in framesAndValues) {
                // TODO: This value seems to encode mesh visibility, but
                // probably also encodes which frame/texture offset to use
                // for textures on faces. My guess is that there's some
                // array of data in the PRM or material, and this value is
                // used to index into that array.
                var (frame, value) = frameAndValue;

                // TODO: Various values may be used to encode texture info?
                var bits = Convert.ToString(value, 2).PadLeft(16, '0');

                // TODO: This is just a guess, but still doesn't look right.
                var displayState = (MeshDisplayState?) null;
                if (value == 257) {
                  displayState = MeshDisplayState.VISIBLE;
                } else if (value == 0) {
                  displayState = MeshDisplayState.HIDDEN;
                }

                if (displayState != null) {
                  displayStates.SetKeyframe(frame, displayState.Value);
                }
              }
            }

            foreach (var transformNode in anim.TransformNodes) {
              if (!finBoneByIndex.TryGetValue(transformNode.Hash,
                                              out var finBone)) {
                continue;
              }

              var finBoneTracks = finAnimation.GetOrCreateBoneTracks(finBone);
              var positions = finBoneTracks
                  .UseSeparateTranslationKeyframesWithTangents();
              var rotations = finBoneTracks
                  .UseSeparateEulerRadiansKeyframesWithTangents();
              var scales =
                  finBoneTracks.UseSeparateScaleKeyframesWithTangents();

              foreach (var mtnTrack in transformNode.Tracks.Values) {
                foreach (var mtnKey in mtnTrack.Keys.Keys) {
                  var frame = (int) mtnKey.Frame;
                  var value = mtnKey.Value;

                  var inTan = mtnKey.InTan;
                  var outTan = mtnKey.OutTan;

                  if (mtnTrack.Type.IsTranslation(out var translationAxis)) {
                    positions.Axes[translationAxis]
                             .Add(new KeyframeWithTangents<float>(
                                      frame,
                                      value,
                                      inTan,
                                      outTan));
                  } else if (mtnTrack.Type.IsRotation(out var rotationAxis)) {
                    rotations.Axes[rotationAxis]
                             .SetKeyframe(
                                 frame,
                                 value,
                                 inTan,
                                 outTan);
                  } else if (mtnTrack.Type.IsScale(out var scaleAxis)) {
                    scales.Axes[scaleAxis]
                          .Add(new KeyframeWithTangents<float>(frame,
                                 value,
                                 inTan,
                                 outTan));
                  } else {
                    throw new NotSupportedException();
                  }
                }
              }
            }
          }
        }
      }
    }

    return model;
  }
}