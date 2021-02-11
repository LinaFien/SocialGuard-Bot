using Discord;
using Natsecure.SocialGuard.Plugin.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Plugin.Services
{
	public class ApiService
	{
		public static HttpClient Client { get; set; }

		public async Task<TrustlistUser> LookupUser(ulong userId)
		{
			HttpRequestMessage request = new(HttpMethod.Get, $"/api/user/{userId}");
			HttpResponseMessage response = await Client.SendAsync(request);

			return response.StatusCode is HttpStatusCode.NotFound 
				? null
				: await Utilities.ParseResponseFullAsync<TrustlistUser>(response);
		}

		public async Task<TrustlistUser> ListKnownUsers()
		{
			HttpRequestMessage request = new(HttpMethod.Get, $"/api/user/list");
			HttpResponseMessage response = await Client.SendAsync(request);

			return await Utilities.ParseResponseFullAsync<TrustlistUser>(response);
		}
	}
}
