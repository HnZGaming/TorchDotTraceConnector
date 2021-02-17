using Torch.Commands;
using Torch.Commands.Permissions;
using Utils.Torch;
using VRage.Game.ModAPI;

namespace TorchDotTraceConnector
{
    [Category("dottrace")]
    public sealed class DotTraceConnectorCommandModule : CommandModule
    {
        DotTraceConnectorPlugin Plugin => (DotTraceConnectorPlugin) Context.Plugin;

        [Command("configs")]
        [Permission(MyPromoteLevel.Admin)]
        public void Configs() => this.CatchAndReport(() =>
        {
            this.GetOrSetProperty(Plugin.Config);
        });

        [Command("commands")]
        [Permission(MyPromoteLevel.Admin)]
        public void Commands() => this.CatchAndReport(() =>
        {
            this.ShowCommands();
        });
    }
}