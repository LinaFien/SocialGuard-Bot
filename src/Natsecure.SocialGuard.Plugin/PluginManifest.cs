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

		internal const string ApiConfigFileName = "api";
		public IHttpClientFactory HttpClientFactory { get; set; }
		public ILogger<PluginManifest> Logger { get; set; }

		public readonly IApiConfig apiConfig;
		
		private readonly ILogger<PluginManifest> logger;
		private readonly DiscordSocketClient coreClient;
		private readonly ServiceRegistry services;

		public PluginManifest(DiscordSocketClient client, IConfigProvider<IApiConfig> apiConfig, ILogger<PluginManifest> logger)


		public PluginManifest(DiscordSocketClient client, ILogger<PluginManifest> logger, ServiceRegistry services)
		{
			coreClient = client;
			this.logger = logger;
			this.services = services;
		}

		public override async Task LoadPlugin() 
		{
//			coreClient.UserJoined += GuildTrafficHandler.Instance.OnGuildUserJoined;
			coreClient.UserJoined += TrafficHandler.OnGuildUserJoinedAsync;

			await base.LoadPlugin();

			logger.LogInformation("Loaded Plugin.");
		}

		public override async Task UnloadPlugin()
		{
			coreClient.UserJoined -= TrafficHandler.OnGuildUserJoinedAsync;

			await base.UnloadPlugin();
		}

		public override IServiceCollection ConfigureServices(IServiceCollection services) => services
			.AddSingleton(c => c.GetService<IConfigProvider<IApiConfig>>().InitConfig(ApiConfigFileName).PopulateApiConfig())
			.AddSingleton<ApiService>()
			.AddSingleton<GuildTrafficHandler>();
	}
}
