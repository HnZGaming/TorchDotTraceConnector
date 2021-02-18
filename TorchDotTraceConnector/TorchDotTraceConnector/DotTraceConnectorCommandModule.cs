using System;
using Torch.Commands;
using Torch.Commands.Permissions;
using TorchDotTraceConnector.Core;
using Utils.General;
using Utils.Torch;
using VRage.Game.ModAPI;
using VRageMath;

namespace TorchDotTraceConnector
{
    [Category("dottrace")]
    public sealed class DotTraceConnectorCommandModule : CommandModule
    {
        DotTraceConnectorPlugin Plugin => (DotTraceConnectorPlugin) Context.Plugin;

        [Command("configs", "Show the list of configurable commands")]
        [Permission(MyPromoteLevel.Admin)]
        public void Configs() => this.CatchAndReport(() =>
        {
            this.GetOrSetProperty(Plugin.Config);
        });

        [Command("commands", "Show the list of available commands")]
        [Permission(MyPromoteLevel.Admin)]
        public void Commands() => this.CatchAndReport(() =>
        {
            this.ShowCommands();
        });

        [Command("trace", "Run dotTrace. --interval=30 --profiling-type=sampling|tracing")]
        [Permission(MyPromoteLevel.Admin)]
        public void Trace() => this.CatchAndReport(async () =>
        {
            TimeSpan? interval = null;
            ProfilingType profilingType = Plugin.Config.ProfilingType;
            foreach (var arg in Context.Args)
            {
                if (!CommandOption.TryGetOption(arg, out var option)) continue;

                if (option.TryParseInt("interval", out var intervalSecs))
                {
                    interval = intervalSecs.Seconds();
                    continue;
                }

                if (option.TryParse("profiling-type", out var profilingTypeStr))
                {
                    profilingType = profilingTypeStr.ToLower() switch
                    {
                        "sampling" => ProfilingType.Sampling,
                        "tracing" => ProfilingType.Tracing,
                        _ => throw new ArgumentException($"invalid profiling type: {profilingTypeStr}")
                    };

                    continue;
                }

                Context.Respond($"Invalid argument: {arg}", Color.Red);
                return;
            }

            Context.Respond("Started tracing...");

            await Plugin.StartTracing(interval, profilingType);

            Context.Respond("Finished tracing");
        });
    }
}