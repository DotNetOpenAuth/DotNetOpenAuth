//-----------------------------------------------------------------------
// <copyright file="FacebookClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Net;
	using System.Web;
	using DotNetOpenAuth.AspNet.Resources;
	using DotNetOpenAuth.Messaging;

	public sealed class FacebookClient : OAuth2Client {
		private const string AuthorizationEndpoint = "https://www.facebook.com/dialog/oauth";
		private const string TokenEndpoint = "https://graph.facebook.com/oauth/access_token";

		private readonly string _appId;
		private readonly string _appSecret;

		public FacebookClient(string appId, string appSecret)
			: base("facebook") {
			if (String.IsNullOrEmpty(appId)) {
				throw new ArgumentException(
					String.Format(CultureInfo.CurrentCulture, WebResources.Argument_Cannot_Be_Null_Or_Empty, "appId"),
					"appId");
			}

			if (String.IsNullOrEmpty("appSecret")) {
				throw new ArgumentException(
					String.Format(CultureInfo.CurrentCulture, WebResources.Argument_Cannot_Be_Null_Or_Empty, "appSecret"),
					"appSecret");
			}

			_appId = appId;
			_appSecret = appSecret;
		}

		protected override Uri GetServiceLoginUrl(Uri returnUrl) {
			// Note: Facebook doesn't like us to url-encode the redirect_uri value
			var builder = new UriBuilder(AuthorizationEndpoint);
			builder.AppendQueryArgs(new Dictionary<string, string> {
					{ "client_id", _appId },
					{ "redirect_uri", returnUrl.AbsoluteUri },
				});
			return builder.Uri;
		}

		protected override string QueryAccessToken(Uri returnUrl, string authorizationCode) {
			// Note: Facebook doesn't like us to url-encode the redirect_uri value
			var builder = new UriBuilder(TokenEndpoint);
			builder.AppendQueryArgs(new Dictionary<string, string> {
				{ "client_id", _appId },
				{ "redirect_uri", returnUrl.AbsoluteUri },
				{ "client_secret", _appSecret },
				{ "code", authorizationCode },
			});

			using (WebClient client = new WebClient()) {
				string data = client.DownloadString(builder.Uri);
				if (String.IsNullOrEmpty(data)) {
					return null;
				}

				var parsedQueryString = HttpUtility.ParseQueryString(data);
				return parsedQueryString["access_token"];
			}
		}

		protected override IDictionary<string, string> GetUserData(string accessToken) {
			FacebookGraphData graphData;
			var request = WebRequest.Create("https://graph.facebook.com/me?access_token=" + MessagingUtilities.EscapeUriDataStringRfc3986(accessToken));
			using (var response = request.GetResponse()) {
				using (var responseStream = response.GetResponseStream()) {
					graphData = JsonHelper.Deserialize<FacebookGraphData>(responseStream);
				}
			}

			// this dictionary must contains 
			var userData = new Dictionary<string, string>();
			userData.AddItemIfNotEmpty("id", graphData.Id);
			userData.AddItemIfNotEmpty("username", graphData.Email);
			userData.AddItemIfNotEmpty("name", graphData.Name);
			userData.AddItemIfNotEmpty("link", graphData.Link == null ? null : graphData.Link.AbsoluteUri);
			userData.AddItemIfNotEmpty("gender", graphData.Gender);
			userData.AddItemIfNotEmpty("birthday", graphData.Birthday);
			return userData;
		}
	}
}
