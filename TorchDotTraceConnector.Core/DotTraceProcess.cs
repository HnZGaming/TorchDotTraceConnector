using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Utils.General;

namespace TorchDotTraceConnector.Core
{
    public sealed class DotTraceProcess : IDisposable
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        readonly Process _process;
        readonly string _savePath;
        readonly TimeSpan _interval;

        public DotTraceProcess(string exePath, string savePath, TimeSpan interval, ProfilingType profilingType)
        {
            _process = new Process();
            _savePath = savePath;
            _interval = interval;

            var pid = Process.GetCurrentProcess().Id;

            _process.StartInfo.FileName = exePath;
            _process.StartInfo.Arguments = $"attach {pid} --profiling-type={profilingType} --save-to=\"{savePath}\" --timeout={interval.TotalSeconds}s";

            Log.Info($"{_process.StartInfo.FileName} {_process.StartInfo.Arguments}");
        }

        // can run once per instance
        public async Task RunAsync(CancellationToken cancellationToken) // use this token on .NET 5.0 when Process.WaitForExitAsync() is available
        {
            // make sure the output dir exists
            var saveDirPath = Path.GetDirectoryName(_savePath);
            if (!Directory.Exists(saveDirPath))
            {
                Directory.CreateDirectory(saveDirPath);
            }

            _process.Start();
            Log.Info("Tracing started");

            // make sure cancellation works
            //var timeout = Task.Delay(_interval + 1.Seconds(), cancellationToken);
            //await await Task.WhenAny(_process.WaitForExitAsync(), timeout);

            var exitCode = await _process.WaitForExitAsync();
            Log.Info($"Tracing finished: {exitCode}");
        }

        public void Dispose()
        {
            _process.Dispose();
        }
    }
}