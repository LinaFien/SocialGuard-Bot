using Discord;
using Discord.WebSocket;
using Natsecure.SocialGuard.Plugin.Data.Config;
using Natsecure.SocialGuard.Plugin.Data.Models;
using Natsecure.SocialGuard.Plugin.Services;
using Nodsoft.YumeChan.PluginBase.Tools.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Plugin
{
	public class GuildTrafficHandler
	{
		private readonly ApiService apiService;
		private readonly IEntityRepository<GuildConfig, ulong> configRepository;

		public GuildTrafficHandler(ApiService api, IDatabaseProvider<PluginManifest> database)
		{
			apiService = api;
			configRepository = database.GetEntityRepository<GuildConfig, ulong>();
		}


		public async Task OnGuildUserJoinedAsync(SocketGuildUser user)
		{
			GuildConfig config = await configRepository.FindByIdAsync(user.Guild.Id);

			if (config.JoinLogChannel is not 0)
			{
				TrustlistUser entry = await apiService.LookupUserAsync(user.Id);
				ITextChannel joinLog = user.Guild.GetTextChannel(config.JoinLogChannel);
				Embed entryEmbed = Utilities.BuildUserRecordEmbed(entry, user, user.Id);


				await joinLog.SendMessageAsync($"User **{user}** ({user.Mention}) has joined the server.", embed: entryEmbed);

				if (entry?.EscalationLevel >= 3 && config.AutoBanBlacklisted)
				{
					await user.BanAsync(0, $"[SocialGuard] \n{entry.EscalationNote}");

					await user.Guild.GetTextChannel(config.BanLogChannel is not 0 ? config.BanLogChannel : config.JoinLogChannel)
						.SendMessageAsync($"User **{user}** ({user.Mention}) banned on server join.", embed: entryEmbed);
				}
			}
		}
	}
}
