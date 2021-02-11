using Natsecure.SocialGuard.Plugin.Data.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Plugin
{
	public static class Utilities
	{
		private static JsonSerializerOptions SerializerOptions => new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		public static async Task<TData> ParseResponseFullAsync<TData>(HttpResponseMessage response) => JsonSerializer.Deserialize<TData>(await response.Content.ReadAsStringAsync(), SerializerOptions);
	
		public static IApiConfig PopulateApiConfig(this IApiConfig config)
		{
			config.ApiHost ??= "https://socialguard.natsecure.fr";
			config.AccessKey ??= string.Empty;

			return config;
		}
	}
}
