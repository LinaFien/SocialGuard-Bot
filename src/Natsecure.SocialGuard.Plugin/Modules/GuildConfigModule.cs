using Discord;
using Discord.Commands;
using Natsecure.SocialGuard.Plugin.Data.Config;
using Nodsoft.YumeChan.PluginBase.Tools.Data;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Plugin.Modules
{
	[Group("socialguard config"), Alias("sg config")]
	public class GuildConfigModule : ModuleBase<SocketCommandContext>
	{
		private readonly IEntityRepository<GuildConfig, ulong> repository;

		public GuildConfigModule(IDatabaseProvider<PluginManifest> database)
		{
			repository = database.GetEntityRepository<GuildConfig, ulong>();
		}


		[Command("joinlog"), RequireUserPermission(GuildPermission.ManageGuild)]
		public async Task GetJoinLogAsync()
		{
			GuildConfig config = await FindOrCreateConfigAsync();
			ITextChannel current = Context.Guild.GetTextChannel(config.JoinLogChannel);
			await ReplyAsync($"Current Join Log channel : {current?.Mention ?? "None"}.");
		}

		[Command("joinlog"), Priority(10), RequireUserPermission(GuildPermission.ManageGuild)]
		public async Task SetJoinLogAsync(ITextChannel channel)
		{
			GuildConfig config = await FindOrCreateConfigAsync();
			config.JoinLogChannel = channel.Id;
			await repository.ReplaceOneAsync(config);
			await ReplyAsync($"Join Log channel set to : {Context.Guild.GetTextChannel(config.JoinLogChannel).Mention}.");
		}


		[Command("banlog"), RequireUserPermission(GuildPermission.ManageGuild)]
		public async Task GetBanLogAsync()
		{
			GuildConfig config = await FindOrCreateConfigAsync();
			ITextChannel current = Context.Guild.GetTextChannel(config.BanLogChannel);
			await ReplyAsync($"Current Ban Log channel : {current?.Mention ?? "None"}.");
		}

		[Command("banlog"), Priority(10), RequireUserPermission(GuildPermission.ManageGuild)]
		public async Task SetBanLogAsync(ITextChannel channel)
		{
			GuildConfig config = await FindOrCreateConfigAsync();
			config.BanLogChannel = channel.Id;
			await repository.ReplaceOneAsync(config);
			await ReplyAsync($"Join Ban channel set to : {Context.Guild.GetTextChannel(config.BanLogChannel).Mention}.");
		}

		[Command("accesskey"), RequireUserPermission(GuildPermission.ManageGuild)]
		public async Task ConfigureAccessKeyAsync()
		{
			GuildConfig config = await FindOrCreateConfigAsync();
			await ReplyAsync(config.WriteAccessKey is null ? "No Access key has been set for this guild." : "Access Key has already been set.");
		}


		[Command("accesskey"), Priority(10), RequireUserPermission(GuildPermission.ManageGuild)]
		public async Task ConfigureAccessKeyAsync(string key)
		{
			await Context.Message.DeleteAsync();

			GuildConfig config = await FindOrCreateConfigAsync();
			config.WriteAccessKey = key;
			await repository.ReplaceOneAsync(config);
			await ReplyAsync($"Access key has been set.");
		}

		[Command("autobanblacklisted"), Alias("autoban"), RequireUserPermission(GuildPermission.ManageGuild), RequireBotPermission(GuildPermission.ManageGuild)]
		public async Task ConfigureBanLogAsync()
		{
			GuildConfig config = await FindOrCreateConfigAsync();
			await ReplyAsync($"Auto-ban Blacklist is {(config.AutoBanBlacklisted ? "on" : "off")}.");
		}

		[Command("autobanblacklisted"), Alias("autoban"), Priority(10), RequireUserPermission(GuildPermission.ManageGuild), RequireBotPermission(GuildPermission.ManageGuild)]
		public async Task ConfigureBanLogAsync(string key)
		{
			GuildConfig config = await FindOrCreateConfigAsync();

			config.AutoBanBlacklisted = key.ToLowerInvariant() switch
			{
				"true" or "yes" or "on" or "1" => true,
				"false" or "no" or "off" or "0" => false,
				_ => config.AutoBanBlacklisted
			};

			await repository.ReplaceOneAsync(config);
			await ReplyAsync($"Auto-ban Blacklist has been turned {(config.AutoBanBlacklisted ? "on" : "off")}.");
		}


		private async Task<GuildConfig> FindOrCreateConfigAsync()
		{
			GuildConfig config = await repository.FindByIdAsync(Context.Guild.Id);

			if (config is null)
			{
				await repository.InsertOneAsync(config = new() { Id = Context.Guild.Id });
			}

			return config;
		}
	}
}
