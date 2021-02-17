using System;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Utils.Torch;

namespace TorchDotTraceConnector
{
    public sealed class DotTraceConnectorPlugin : TorchPluginBase, IWpfPlugin
    {
        Persistent<DotTraceConnectorConfig> _config;
        UserControl _userControl;

        public DotTraceConnectorConfig Config => _config.Data ?? throw new Exception("Config not loaded");

        UserControl IWpfPlugin.GetControl()
        {
            return _config.GetOrCreateUserControl(ref _userControl);
        }

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            var configPath = this.MakeConfigFilePath();
            _config = Persistent<DotTraceConnectorConfig>.Load(configPath);
        }
    }
}