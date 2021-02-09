using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Nodsoft.YumeChan.PluginBase.Tools.Data;
using System;

namespace Natsecure.SocialGuard.Plugin.Data.Config
{
	public record GuildConfig : IDocument<ulong>
	{
		[BsonId, BsonRepresentation(BsonType.Int64)]
		public ulong Id { get; set; }


		public ulong GuildId { get; init; }

		public ulong JoinLogChannel { get; set; }

		public ulong BanLogChannel { get; set; }
	}
}
