using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Plugin
{
	public class PluginManifest : Nodsoft.YumeChan.PluginBase.Plugin
	{
		public override string PluginDisplayName => "Natsecure SocialGuard (YC)";
		public override bool PluginStealth => false;

		public override async Task LoadPlugin() 
		{
			await base.LoadPlugin();
		}

		public override async Task UnloadPlugin()
		{
			await base.UnloadPlugin();
		}
	}
}
