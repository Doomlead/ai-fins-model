﻿using System;
using System.Collections.Generic;
using System.IO;

using hw.schema.xtt;

namespace HaloWarsTools;
// TODO put value cache in HWContext

public enum HWResourceType {
  None,
  Xtt, // Terrain Mesh/Albedo
  Xtd, // Terrain Opacity/AO
  Scn, // Scenario
  Sc2, // Scenario
  Sc3, // Scenario
  Gls, // Scenario Lighting
  Ugx, // Mesh
  Vis  // Visual Representation
}

public sealed class HWResourceTypeDefinition(
    HWResourceType type,
    Type resourceClass) {
  public HWResourceType Type = type;
  public Type Class = resourceClass;
}

public abstract class HWResource {
  private static LazyValueCache StaticValuesCache = new LazyValueCache();

  private static Dictionary<string, HWResourceTypeDefinition>
      TypeDefinitions =
          new Dictionary<string, HWResourceTypeDefinition>() {
              {
                  ".xtt",
                  new HWResourceTypeDefinition(
                      HWResourceType.Xtt, typeof(Xtt))
              }, {
                  ".xtd",
                  new HWResourceTypeDefinition(
                      HWResourceType.Xtd, typeof(HWXtdResource))
              },
              /*{".scn", new HWResourceTypeDefinition(HWResourceType.Scn, typeof(HWScnResource))},
              {".sc2", new HWResourceTypeDefinition(HWResourceType.Sc2, typeof(HWSc2Resource))},
              {".sc3", new HWResourceTypeDefinition(HWResourceType.Sc3, typeof(HWSc3Resource))},
              {".gls", new HWResourceTypeDefinition(HWResourceType.Gls, typeof(HWGlsResource))},*/
              {".ugx", new HWResourceTypeDefinition(HWResourceType.Ugx, typeof(HWUgxResource))},
              {".vis", new HWResourceTypeDefinition(HWResourceType.Vis, typeof(HWVisResource))},
          };

  protected static Dictionary<HWResourceType, string> TypeExtensions =>
      StaticValuesCache.Get(() => {
        var dictionary = new Dictionary<HWResourceType, string>();
        foreach (var kvp in TypeDefinitions) {
          dictionary.Add(kvp.Value.Type, kvp.Key);
        }
        return dictionary;
      });

  protected abstract void Load(byte[] bytes);

  public HWContext Context { get; protected set; }
  public string RelativePath { get; protected set; }
  public string AbsolutePath => this.Context.GetAbsoluteScratchPath(this.RelativePath);
  public HWResourceType Type;

  protected static HWResource GetOrCreateFromFile(HWContext? context,
                                                  string filename,
                                                  HWResourceType
                                                      expectedType =
                                                      HWResourceType.None) {
    // Set the extension based on the resource type if the filename doesn't have one
    if (string.IsNullOrEmpty(Path.GetExtension(filename)) &&
        TypeExtensions.TryGetValue(expectedType,
                                   out string defaultExtension)) {
      filename = Path.ChangeExtension(filename, defaultExtension);
    }

    var resource = CreateResource(context, filename);
    resource?.Load(File.ReadAllBytes(filename));

    return resource;
  }

  protected static HWResource
      CreateResource(HWContext? context, string filename) {
    string extension = Path.GetExtension(filename).ToLowerInvariant();

    if (TypeDefinitions.TryGetValue(extension,
                                    out HWResourceTypeDefinition
                                        definition)) {
      if (Activator.CreateInstance(definition.Class) is HWResource resource) {
        resource.Type = definition.Type;
        resource.Context = context;
        resource.RelativePath = filename;
        return resource;
      }
    }

    return null;
  }
}