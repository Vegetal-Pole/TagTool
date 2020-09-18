﻿using TagTool.Cache;
using System.Collections.Generic;
using System;
using TagTool.Commands.Common;
using TagTool.Commands.Tags;
using System.IO;

namespace TagTool.Commands.Modding
{
    class OpenModPackageCommand : Command
    {
        private readonly GameCacheHaloOnlineBase Cache;
        private CommandContextStack ContextStack { get; }
        private GameCacheModPackage ModCache;
        private CommandContext Context;

        public OpenModPackageCommand(CommandContextStack contextStack, GameCacheHaloOnlineBase cache) :
            base(true,

                "OpenModPackage",
                "Create context for an existing mod package.",

                "OpenModPackage <.pak path>",

                "Create context for an existing mod package.")
        {
            ContextStack = contextStack;
            Cache = cache;
        }

        public override object Execute(List<string> args)
        {
            if (args.Count != 1)
                return new TagToolError(CommandError.ArgCount);

            var file = new FileInfo(args[0]);

            if (!file.Exists)
                return new TagToolError(CommandError.FileNotFound, $"\"{args[0]}\"");

            Console.WriteLine("Initializing cache...");

            ModCache = new GameCacheModPackage(Cache, file);
            Context = TagCacheContextFactory.Create(ContextStack, ModCache, $"{ModCache.BaseModPackage.Metadata.Name}.pak");
            ContextStack.Push(Context);
            ContextStack.ContextPopped += ContextStack_ContextPopped;

            Console.WriteLine("Done!");

            return true;
        }

        private void ContextStack_ContextPopped(CommandContext context)
        {
            if (context != Context)
                return;

            ContextStack.ContextPopped -= ContextStack_ContextPopped;
            ModCache.BaseCacheStream?.Dispose();
        }
    }
}