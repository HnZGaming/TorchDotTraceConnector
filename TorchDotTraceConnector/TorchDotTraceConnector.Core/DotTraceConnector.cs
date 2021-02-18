using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Profiler.SelfApi;
using NLog;
using Utils.General;

namespace TorchDotTraceConnector.Core
{
    public sealed class DotTraceConnector
    {
        public interface IConfig
        {
            string SaveDirPath { get; }
            int TraceSecs { get; }
            ProfilingType ProfilingType { get; }
        }

        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        readonly IConfig _config;

        public DotTraceConnector(IConfig config)
        {
            _config = config;
        }

        public bool IsTracing { get; private set; }

        public async Task StartTracing(
            CancellationToken cancellationToken,
            TimeSpan? secsOrDefault = null,
            ProfilingType? profilingTypeOrDefault = null)
        {
            if (IsTracing)
            {
                throw new Exception("already running");
            }

            await TaskUtils.MoveToThreadPool(cancellationToken);

            try
            {
                IsTracing = true;

                await DotTrace.EnsurePrerequisiteAsync(cancellationToken);

                var interval = secsOrDefault ?? _config.TraceSecs.Seconds();
                var profilingType = profilingTypeOrDefault ?? _config.ProfilingType;

                var dirPath = _config.SaveDirPath;
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                    Log.Info($"Created directory: \"{dirPath}\"");
                }

                var config = new DotTrace.Config().SaveToDir(dirPath);
                if (profilingType == ProfilingType.Sampling)
                {
                    config = config.UseTimelineProfilingType();
                    Log.Info("Making a timeline; this can be laggy for the server!");
                }

                DotTrace.Attach(config);

                try
                {
                    Log.Info($"Tracing (type: {profilingType}, {interval.TotalSeconds:0} seconds)...");

                    DotTrace.StartCollectingData();

                    await Task.Delay(interval, cancellationToken);

                    DotTrace.SaveData();

                    Log.Info("Finished tracing!");
                }
                finally
                {
                    DotTrace.Detach();
                }
            }
            finally
            {
                IsTracing = false;
            }
        }
    }
}