using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Plugin
{
	public class GuildTrafficHandler
	{

		public static GuildTrafficHandler Instance { get; internal set; }

		public Task OnGuildUserJoined(SocketGuildUser arg)
		{
			throw new NotImplementedException();
		}
	}
}
