﻿using CommandLine;

namespace uni.cli;

[Verb("ui", HelpText = "Open UI")]
public sealed class UiOptions;

[Verb("list_plugins",
      HelpText = "Lists all of the supported plugins for conversion.")]
public sealed class ListPluginOptions;

[Verb("convert", HelpText = "Converts model(s) from one format to another. The best plugin will automatically be detected based on the list of input files.")]
public sealed class ConvertOptions {
  [Option('i',
          "inputs",
          Required = true,
          HelpText = "Input files to convert.",
          Separator = ',',
          Min = 1)]
  public required IEnumerable<string> Inputs { get; set; }

  [Option('o',
          "output",
          Required = true,
          HelpText = "Output file to write.")]
  public required string Output { get; set; }

  [Option('f',
          "framerate",
          Required = false,
          HelpText = "Framerate of animations (only used in some plugins).",
          Default = 30)]
  public float FrameRate { get; set; }
}


[Verb("debug",
      HelpText =
          "Extract models with hardcoded input files. Not meant for general use.")]
public sealed class DebugOptions;