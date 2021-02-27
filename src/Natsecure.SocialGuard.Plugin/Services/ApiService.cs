﻿using Discord;
using Microsoft.Extensions.Http;
using Natsecure.SocialGuard.Plugin.Data.Config;
using Natsecure.SocialGuard.Plugin.Data.Models;
using Nodsoft.YumeChan.PluginBase.Tools;
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
		private const string AccessKeyName = "Access-Key";

		private readonly HttpClient client;

		public ApiService(IHttpClientFactory factory, IConfigProvider<IApiConfig> config)
		{
			client = factory.CreateClient(nameof(PluginManifest));
			client.BaseAddress = new(config.Configuration.ApiHost);
			client.DefaultRequestHeaders.Add(AccessKeyName, config.Configuration.AccessKey);
		}

		public async Task<TrustlistUser> LookupUser(ulong userId)
		{
			HttpRequestMessage request = new(HttpMethod.Get, $"/api/user/{userId}");
			HttpResponseMessage response = await client.SendAsync(request);

			return response.StatusCode is HttpStatusCode.NotFound 
				? null
				: await Utilities.ParseResponseFullAsync<TrustlistUser>(response);
		}

		public async Task<TrustlistUser> ListKnownUsers()
		{
			HttpRequestMessage request = new(HttpMethod.Get, $"/api/user/list");
			HttpResponseMessage response = await client.SendAsync(request);

			return await Utilities.ParseResponseFullAsync<TrustlistUser>(response);
		}
	}
}