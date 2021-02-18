using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using TorchDotTraceConnector.Core;
using Utils.General;
using Utils.Torch;

namespace TorchDotTraceConnector
{
    public sealed class DotTraceConnectorPlugin : TorchPluginBase, IWpfPlugin
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        Persistent<DotTraceConnectorConfig> _config;
        UserControl _userControl;
        CancellationTokenSource _cancellationTokenSource;
        FileLoggingConfigurator _fileLoggingConfigurator;
        DotTraceConnector _connector;
        SimDropObserver _simDropObserver;

        public DotTraceConnectorConfig Config => _config.Data ?? throw new Exception("Config not loaded");

        UserControl IWpfPlugin.GetControl()
        {
            return _config.GetOrCreateUserControl(ref _userControl);
        }

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            this.ListenOnGameLoaded(OnGameLoaded);
            this.ListenOnGameUnloading(OnGameUnloading);

            GameLoopObserverManager.Add(torch);

            _cancellationTokenSource = new CancellationTokenSource();

            var configPath = this.MakeConfigFilePath();
            _config = Persistent<DotTraceConnectorConfig>.Load(configPath);

            _fileLoggingConfigurator = new FileLoggingConfigurator(
                "TorchDotTraceConnector",
                new[] {"TorchDotTraceConnector.*"},
                Config.LogFilePath);

            _fileLoggingConfigurator.Initialize();
            _fileLoggingConfigurator.Configure(Config);

            _connector = new DotTraceConnector(Config);
            _simDropObserver = new SimDropObserver(10);

            Config.PropertyChanged += (sender, args) =>
            {
                _fileLoggingConfigurator.Configure(Config);
            };
        }

        void OnGameLoaded()
        {
            TaskUtils.RunUntilCancelledAsync(Main, _cancellationTokenSource.Token).Forget(Log);
            TaskUtils.RunUntilCancelledAsync(_simDropObserver.Main, _cancellationTokenSource.Token).Forget(Log);
        }

        async Task Main(CancellationToken cancellationToken)
        {
            if (!Config.AutoTrace) return;

            await Task.Delay(Config.InitialWaitSecs.Seconds(), cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                while (!_simDropObserver.IsLaggierThan(Config.SimThreshold))
                {
                    await Task.Delay(1.Seconds(), cancellationToken);
                }

                if (_connector.IsTracing)
                {
                    await Task.Delay(1.Seconds(), cancellationToken);
                    continue;
                }

                try
                {
                    Log.Info("Auto-tracing...");
                    await _connector.StartTracing(cancellationToken);
                    Log.Info("Auto-tracing done");
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
                finally
                {
                    _simDropObserver.Reset();
                }

                await Task.Delay(1.Seconds(), cancellationToken);
            }
        }

        void OnGameUnloading()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        public async Task StartTracing(TimeSpan? interval, ProfilingType profilingType)
        {
            await _connector.StartTracing(_cancellationTokenSource.Token, interval, profilingType);
        }
    }
}