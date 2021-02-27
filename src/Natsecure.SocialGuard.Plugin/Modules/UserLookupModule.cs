using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB;
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

		public UserLookupModule(ApiService service)
		{
			this.service = service;
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
					await service.InsertOrEscalateUserAsync(new()
					{
						Id = user.Id,
						EscalationLevel = level,
						EscalationNote = reason
					});

					await ReplyAsync($"{Context.User.Mention} User '{user.Mention}' successfully inserted into Trustlist.");
					await LookupAsync(user, user.Id);
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

		private async Task LookupAsync(IUser user, ulong userId)
		{
			TrustlistUser entry = await service.LookupUserAsync(userId);
			(Color color, string name, string desc) escalation = entry?.EscalationLevel switch
			{
				null or 0 => (Color.Green, "Clear", "This user has no record, and is cleared safe."),
				1 => (Color.Blue, "Suspicious", "This user is marked as suspicious. Their behaviour should be monitored."),
				2 => (Color.DarkOrange, "Untrusted", "This user is marked as untrusted. Please exerce caution when interacting with them."),
				>= 3 => (Color.Red, "Blacklisted", "This user is dangerous and has been blacklisted. Banning this user is greatly advised.")
			};

			EmbedBuilder builder = new();
			builder.WithTitle($"Trustlist User : {user?.Username ?? userId.ToString()}");

			if (user is not null)
			{
				builder.AddField("ID", $"``{user?.Id}``", true);
			}

			builder.Color = escalation.color;
			builder.Description = escalation.desc;
			builder.Footer = new() { Text = SignatureFooter };

			if (entry is not null)
			{
				builder.AddField("Escalation Level", $"{entry.EscalationLevel} - {escalation.name}");
				builder.AddField("First Entered", entry.EntryAt.ToString(), true);
				builder.AddField("Last Escalation", entry.LastEscalated.ToString(), true);
				builder.AddField("Escalation Reason", entry.EscalationNote);
			}

			await ReplyAsync(Context.User.Mention, embed: builder.Build());
		}
	}
}
