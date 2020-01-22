﻿using TagTool.Bitmaps;
using TagTool.Cache;
using TagTool.Tags.Definitions;
using System;
using System.Collections.Generic;
using System.IO;
using TagTool.IO;

namespace TagTool.Commands.RenderModels
{
    class ExtractBitmapsCommand : Command
    {
        private GameCache Cache { get; }
        private RenderModel Definition { get; }

        public ExtractBitmapsCommand(GameCache cacheContext, RenderModel definition)
            : base(true,
                  
                  "ExtractBitmaps",
                  "Extracts all bitmaps used by the render model's shaders to a specific directory.",
                  
                  "ExtractBitmaps <Output Folder>",

                  "Extracts all bitmaps used by the render model's shaders to a specific directory.")
        {
            Cache = cacheContext;
            Definition = definition;
        }

        public override object Execute(List<string> args)
        {
            if (args.Count != 1)
                return false;

            var directory = args[0];

            if (!Directory.Exists(directory))
            {
                Console.Write("Destination directory does not exist. Create it? [y/n] ");
                var answer = Console.ReadLine().ToLower();

                if (answer.Length == 0 || !(answer.StartsWith("y") || answer.StartsWith("n")))
                    return false;

                if (answer.StartsWith("y"))
                    Directory.CreateDirectory(directory);
                else
                    return false;
            }

            using (var cacheStream = Cache.OpenCacheRead())
            {
                foreach (var shader in Definition.Materials)
                {
                    var renderMethod = (RenderMethod)Cache.Deserialize(cacheStream, shader.RenderMethod);

                    foreach (var property in renderMethod.ShaderProperties)
                    {
                        var template = Cache.Deserialize<RenderMethodTemplate>(cacheStream, property.Template);

                        for (var i = 0; i < template.SamplerArguments.Count; i++)
                        {
                            var mapTemplate = template.SamplerArguments[i];

                            var bitmap = Cache.Deserialize<Bitmap>(cacheStream, property.ShaderMaps[i].Bitmap);
                            var ddsOutDir = directory;

                            if (bitmap.Images.Count > 1)
                            {
                                ddsOutDir = Path.Combine(directory, property.ShaderMaps[i].Bitmap.Index.ToString("X8"));
                                Directory.CreateDirectory(ddsOutDir);
                            }

                            for (var j = 0; j < bitmap.Images.Count; j++)
                            {
                                var outPath = Path.Combine(ddsOutDir, Cache.StringTable.GetString(mapTemplate.Name) + "_" + property.ShaderMaps[i].Bitmap.Index.ToString("X4")) + ".dds";

                                using (var outStream = File.Open(outPath, FileMode.Create, FileAccess.Write))
                                using(var writer = new EndianWriter(outStream))
                                {
                                    var ddsFile = BitmapExtractor.ExtractBitmap(Cache, bitmap, j);
                                    ddsFile.Write(writer);
                                }

                                Console.WriteLine($"Bitmap {i} ({Cache.StringTable.GetString(mapTemplate.Name)}): {property.ShaderMaps[i].Bitmap.Group.Tag} 0x{property.ShaderMaps[i].Bitmap.Index:X4} extracted to '{outPath}'");
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}