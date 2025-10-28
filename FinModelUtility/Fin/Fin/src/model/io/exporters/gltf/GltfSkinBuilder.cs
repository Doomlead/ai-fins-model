﻿using System;
using System.Collections.Generic;
using System.Linq;

using fin.data.indexable;
using fin.model.accessor;
using fin.model.util;
using fin.util.enumerables;

using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;

using IGltfMeshBuilder
    = SharpGLTF.Geometry.IMeshBuilder<SharpGLTF.Materials.MaterialBuilder>;

namespace fin.model.io.exporters.gltf;

public sealed class GltfSkinBuilder {
  public bool UvIndices { get; set; }

  public IList<(Mesh gltfMesh, bool hasJoints)> AddSkin(
      ModelRoot gltfModel,
      IReadOnlyModel model,
      float scale,
      IDictionary<IReadOnlyMaterial, MaterialBuilder>
          finToTexCoordAndGltfMaterial) {
    var skin = model.Skin;

    var boneTransformManager = new BoneTransformManager();
    boneTransformManager.CalculateStaticMatricesForManualProjection(model);

    var boneToIndex
        = model.Skeleton.Skip(1)
               .ToIndexByValueIndexableDictionary();

    var nullMaterialBuilder =
        new MaterialBuilder("null").WithDoubleSide(false)
                                   .WithSpecularGlossiness();

    var vertexAccessor = MaximalVertexAccessor.GetAccessorForModel(model);
    var vertexToBuilder
        = new IndexableDictionary<IReadOnlyVertex, IVertexBuilder>(
            skin.Vertices.Count);

    var gltfVertexBuilder = new GltfVertexBuilder(model, boneToIndex) {
        UvIndices = this.UvIndices
    };

    var gltfMeshes = new List<(Mesh, bool)>();
    foreach (var finMesh in skin.Meshes) {
      bool hasNormals = false;
      bool hasTangents = false;
      int colorCount = 0;
      int uvCount = 0;
      int weightCount = 0;

      var verticesInMesh = finMesh.Primitives.SelectMany(p => p.Vertices)
                                  .Distinct()
                                  .ToArray();
      foreach (var finVertex in verticesInMesh) {
        vertexAccessor.Target(finVertex);

        hasNormals |= vertexAccessor.LocalNormal != null;
        hasTangents |= vertexAccessor.LocalTangent != null;
        colorCount = Math.Max(colorCount, vertexAccessor.ColorCount);
        uvCount = Math.Max(uvCount, vertexAccessor.UvCount);
        weightCount = Math.Max(weightCount,
                               vertexAccessor.BoneWeights?.Weights.Count ??
                               0);
      }

      foreach (var finVertex in verticesInMesh) {
        vertexAccessor.Target(finVertex);

        var vertexBuilder = gltfVertexBuilder.CreateVertexBuilder(
            boneTransformManager,
            vertexAccessor,
            scale,
            hasNormals,
            hasTangents,
            colorCount,
            uvCount,
            weightCount);

        vertexToBuilder[finVertex] = vertexBuilder;
      }

      IGltfMeshBuilder gltfMeshBuilder
          = GltfMeshBuilderUtil.CreateMeshBuilder(hasNormals,
                                                  hasTangents,
                                                  colorCount,
                                                  uvCount,
                                                  weightCount);
      gltfMeshBuilder.Name = finMesh.Name;

      foreach (var primitive in finMesh.Primitives) {
        MaterialBuilder materialBuilder;
        if (primitive.Material != null) {
          materialBuilder =
              finToTexCoordAndGltfMaterial[primitive.Material];
        } else {
          materialBuilder = nullMaterialBuilder;
        }

        switch (primitive.Type) {
          case PrimitiveType.TRIANGLES:
          case PrimitiveType.TRIANGLE_STRIP:
          case PrimitiveType.TRIANGLE_FAN: {
            var triangles = gltfMeshBuilder.UsePrimitive(materialBuilder);

            foreach (var (v1, v2, v3) in primitive
                                         .GetOrderedTriangleVertices()
                                         .Select(v => vertexToBuilder[v])
                                         .SeparateTriplets()) {
              triangles.AddTriangle(v1, v2, v3);
            }

            break;
          }
          case PrimitiveType.QUADS: {
            var quads = gltfMeshBuilder.UsePrimitive(materialBuilder);
            var verticesInPrimitive = primitive.Vertices;
            for (var v = 0; v < verticesInPrimitive.Count; v += 4) {
              quads.AddQuadrangle(vertexToBuilder[verticesInPrimitive[v + 0]],
                                  vertexToBuilder[verticesInPrimitive[v + 1]],
                                  vertexToBuilder[verticesInPrimitive[v + 2]],
                                  vertexToBuilder[verticesInPrimitive[v + 3]]);
            }

            break;
          }
          case PrimitiveType.QUAD_STRIP: {
            var quads = gltfMeshBuilder.UsePrimitive(materialBuilder);
            var verticesInPrimitive = primitive.Vertices;

            // https://edeleastar.github.io/opengl-programming/topic06/pdf/1.Polygons.pdf
            var firstVertex = 0;
            var secondVertex = 1;
            for (var v = 3; v < verticesInPrimitive.Count; v += 2) {
              var a = firstVertex;
              var b = secondVertex;
              var c = v - 1;
              var d = v;

              var v0 = a;
              var v1 = b;
              var v2 = d;
              var v3 = c;

              quads.AddQuadrangle(vertexToBuilder[verticesInPrimitive[v0]],
                                  vertexToBuilder[verticesInPrimitive[v1]],
                                  vertexToBuilder[verticesInPrimitive[v2]],
                                  vertexToBuilder[verticesInPrimitive[v3]]);

              firstVertex = c;
              secondVertex = d;
            }

            break;
          }
          case PrimitiveType.POINTS: {
            var pointPrimitive
                = gltfMeshBuilder.UsePrimitive(materialBuilder, 1);
            var verticesInPrimitive = primitive.Vertices;
            for (var v = 0; v < verticesInPrimitive.Count; v += 4) {
              pointPrimitive.AddPoint(vertexToBuilder[verticesInPrimitive[v]]);
            }

            break;
          }
          default: throw new NotSupportedException();
        }
      }

      gltfMeshes.Add((gltfModel.CreateMesh(gltfMeshBuilder),
                      weightCount > 0));
    }

    return gltfMeshes;
  }
}