using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;



namespace Natsecure.SocialGuard.Plugin
{
	public class PluginManifest : Nodsoft.YumeChan.PluginBase.Plugin
	{
		public override string PluginDisplayName => "Natsecure SocialGuard (YC)";
		public override bool PluginStealth => false;


		private readonly DiscordSocketClient coreClient;

		public PluginManifest(DiscordSocketClient client)
		{
			coreClient = client;
		}

		public override async Task LoadPlugin() 
		{
			coreClient.UserJoined += GuildTrafficHandler.Instance.OnGuildUserJoined;

			await base.LoadPlugin();
		}

		public override async Task UnloadPlugin()
		{
			await base.UnloadPlugin();
		}
	}
}
