using Discord;
using Discord.Commands;
using MongoDB;
using Natsecure.SocialGuard.Plugin.Data.Models;
using Natsecure.SocialGuard.Plugin.Services;
using Nodsoft.YumeChan.PluginBase.Tools.Data;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Plugin.Modules
{
	[Group("socialguard"), Alias("sg")]
	public class UserLookupModule : ModuleBase<ICommandContext>
	{
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


		[Command("ban")]
		public async Task BanUserAsync(IUser user)
		{
			
		}


		private async Task LookupAsync(IUser user, ulong userId)
		{
			TrustlistUser entry = await service.LookupUser(userId);
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
			builder.Footer = new() { Text = $"Natsecure SocialGuard (YC) - Powered by Nodsoft Systems" };

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
