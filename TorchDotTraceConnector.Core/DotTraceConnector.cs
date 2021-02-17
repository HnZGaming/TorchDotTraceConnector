using System;
using System.Threading;
using System.Threading.Tasks;
using Utils.General;

namespace TorchDotTraceConnector.Core
{
    public sealed class DotTraceConnector
    {
        public interface IConfig
        {
            string ExePath { get; }
            string SaveFilePathFormat { get; }
            int TraceSecs { get; }
            ProfilingType ProfilingType { get; }
        }

        readonly IConfig _config;

        public DotTraceConnector(IConfig config)
        {
            _config = config;
        }

        public bool IsTracing { get; private set; }

        public async Task<string> StartTracing(CancellationToken cancellationToken, TimeSpan? secsOrDefault = null, ProfilingType? profilingTypeOrDefault = null)
        {
            if (IsTracing)
            {
                throw new Exception("Tracing running");
            }

            IsTracing = true;
            using (new ActionDisposable(() => IsTracing = false))
            {
                var interval = secsOrDefault ?? _config.TraceSecs.Seconds();
                var profilingType = profilingTypeOrDefault ?? _config.ProfilingType;
                var timestampStr = $"{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}";
                var outputPath = _config.SaveFilePathFormat.Replace("{timestamp}", timestampStr);
                using (var process = new DotTraceProcess(_config.ExePath, outputPath, interval, profilingType))
                {
                    await process.RunAsync(cancellationToken);
                }

                return outputPath;
            }
        }
    }
}