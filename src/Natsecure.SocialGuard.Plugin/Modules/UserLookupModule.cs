using Discord;
using Discord.Commands;
using Natsecure.SocialGuard.Plugin.Data.Config;
using Natsecure.SocialGuard.Plugin.Data.Models;
using Natsecure.SocialGuard.Plugin.Services;
using Nodsoft.YumeChan.PluginBase.Tools.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Plugin.Modules
{
	[Group("socialguard"), Alias("sg")]
	public class UserLookupModule : ModuleBase<ICommandContext>
	{
		const string SignatureFooter = "Natsecure SocialGuard (YC) - Powered by Nodsoft Systems";

		private readonly ApiService service;
		private readonly IEntityRepository<GuildConfig, ulong> repository;

		public UserLookupModule(ApiService service, IDatabaseProvider<PluginManifest> databaseProvider)
		{
			this.service = service;
			repository = databaseProvider.GetEntityRepository<GuildConfig, ulong>();
		}


		[Command("lookup"), Priority(10)]
		public async Task LookupAsync(ulong userId)
		{
			IUser user = await Context.Client.GetUserAsync(userId);
			await LookupAsync(user, userId);
		}

		[Command("lookup")]
		public async Task LookupAsync(IUser user) => await LookupAsync(user, user.Id);

		[Command("insert"), Alias("add")]
		[RequireContext(ContextType.Guild), RequireUserPermission(GuildPermission.BanMembers), RequireBotPermission(GuildPermission.BanMembers)]
		public async Task InsertUserAsync(IGuildUser user, [Range(0, 3)] byte level, [Remainder] string reason)
		{
			if (user.Id == Context.User.Id)
			{
				await ReplyAsync($"{Context.User.Mention} You cannot insert yourself in the Trustlist.");
			}
			else if (user.IsBot)
			{
				await ReplyAsync($"{Context.User.Mention} You cannot insert a Bot in the Trustlist.");
			}
			else if (user.GuildPermissions.ManageGuild)
			{
				await ReplyAsync($"{Context.User.Mention} You cannot insert a server operator in the Trustlist. Demote them first.");
			}
			else if (reason.Length < 5)
			{
				await ReplyAsync($"{Context.User.Mention} Reason is too short");
			}

			else
			{
				try
				{
					if ((await FindOrCreateConfigAsync()).WriteAccessKey is string key and not null)
					{
						await service.InsertOrEscalateUserAsync(new()
						{
							Id = user.Id,
							EscalationLevel = level,
							EscalationNote = reason
						}, key);

						await ReplyAsync($"{Context.User.Mention} User '{user.Mention}' successfully inserted into Trustlist.");
						await LookupAsync(user, user.Id);
					}
					else
					{
						await ReplyAsync($"{Context.User.Mention} No Access Key set. Use ``sg config accesskey <key>`` to set an Access Key.");
					}
				}
				catch (ApplicationException e)
				{
					await ReplyAsync($"{Context.User.Mention} {e.Message}");
#if DEBUG
					throw;
#endif
				}
			}
		}

		public async Task LookupAsync(IUser user, ulong userId, bool silenceOnClear = false)
		{
			TrustlistUser entry = await service.LookupUserAsync(userId);

			if (!silenceOnClear || entry.EscalationLevel is not 0)
			{
				await ReplyAsync(Context.User.Mention, embed: BuildUserRecordEmbed(entry, user, userId));
			}
		}

		internal static Embed BuildUserRecordEmbed(TrustlistUser trustlistUser, IUser discordUser, ulong userId)
		{
			(Color color, string name, string desc) = trustlistUser?.EscalationLevel switch
			{
				null or 0 => (Color.Green, "Clear", "This user has no record, and is cleared safe."),
				1 => (Color.Blue, "Suspicious", "This user is marked as suspicious. Their behaviour should be monitored."),
				2 => (Color.Orange, "Untrusted", "This user is marked as untrusted. Please exerce caution when interacting with them."),
				>= 3 => (Color.Red, "Blacklisted", "This user is dangerous and has been blacklisted. Banning this user is greatly advised.")
			};

			EmbedBuilder builder = new();
			builder.WithTitle($"Trustlist User : {discordUser?.Username ?? userId.ToString()}");

			if (discordUser is not null)
			{
				builder.AddField("ID", $"``{discordUser?.Id}``", true);
			}

			builder.Color = color;
			builder.Description = desc;
			builder.Footer = new() { Text = SignatureFooter };

			if (trustlistUser is not null)
			{
				builder.AddField("Escalation Level", $"{trustlistUser.EscalationLevel} - {name}");
				builder.AddField("First Entered", trustlistUser.EntryAt.ToString(), true);
				builder.AddField("Last Escalation", trustlistUser.LastEscalated.ToString(), true);
				builder.AddField("Escalation Reason", trustlistUser.EscalationNote);
			}

			return builder.Build();
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
