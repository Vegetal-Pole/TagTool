﻿using System;
using System.Collections.Generic;
using TagTool.Cache;
using TagTool.Commands.Common;

namespace TagTool.Commands.Tags
{
    class DuplicateTagCommand : Command
    {
        private GameCache Cache { get; }

        public DuplicateTagCommand(GameCache cache)
            : base(false,
                  
                  "DuplicateTag",
                  "Copies a tags data into a new tag.",
                  
                  "DuplicateTag <tag>",
                  "Copies a tags data into a new tag.")
        {
            Cache = cache;
        }

        public override object Execute(List<string> args)
        {
            if (args.Count < 1)
                return new TagToolError(CommandError.ArgCount);
            if (!Cache.TagCache.TryGetCachedTag(args[0], out var originalTag))
                return new TagToolError(CommandError.TagInvalid);

            var newTag = Cache.TagCache.AllocateTag(originalTag.Group);

            using (var stream = Cache.OpenCacheReadWrite())
            {
                var originalDefinition = Cache.Deserialize(stream, originalTag);
                Cache.Serialize(stream, newTag, originalDefinition);
            }

            Console.Write($"Tag duplicated to 0x{newTag.Index.ToString("X")}\n");
            return true;
        }
    }
}
