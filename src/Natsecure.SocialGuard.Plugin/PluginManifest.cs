using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Natsecure.SocialGuard.Plugin.Data.Config;
using Natsecure.SocialGuard.Plugin.Services;
using Nodsoft.YumeChan.PluginBase.Tools;
using System.Net.Http;
using System.Threading.Tasks;



namespace Natsecure.SocialGuard.Plugin
{
	public class PluginManifest : Nodsoft.YumeChan.PluginBase.Plugin
	{
		public override string PluginDisplayName => "Natsecure SocialGuard (YC)";
		public override bool PluginStealth => false;

		public IApiConfig ApiConfig { get; set; }
		public IHttpClientFactory HttpClientFactory { get; set; }
		public ILogger<PluginManifest> Logger { get; set; }


		private readonly DiscordSocketClient coreClient;

		public PluginManifest(DiscordSocketClient client, IConfigProvider<IApiConfig> apiConfig, ILogger<PluginManifest> logger)
		{
			coreClient = client;
			ApiConfig = apiConfig.InitConfig("api").PopulateApiConfig();
			Logger = logger;
		}

		public override async Task LoadPlugin() 
		{
//			coreClient.UserJoined += GuildTrafficHandler.Instance.OnGuildUserJoined;

			await base.LoadPlugin();

			Logger.LogInformation("Loaded Plugin.");
		}

		public override async Task UnloadPlugin()
		{
			coreClient.UserJoined -= GuildTrafficHandler.Instance.OnGuildUserJoined;

			await base.UnloadPlugin();
		}

		public override IServiceCollection ConfigureServices(IServiceCollection services) => services
			.AddSingleton<ApiService>();
	}
}
