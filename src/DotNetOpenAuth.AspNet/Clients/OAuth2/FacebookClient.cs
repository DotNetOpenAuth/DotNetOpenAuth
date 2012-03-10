//-----------------------------------------------------------------------
// <copyright file="FacebookClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Net;
	using System.Web;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The facebook client.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Facebook", Justification = "Brand name")]
	public sealed class FacebookClient : OAuth2Client {
		#region Constants and Fields

		/// <summary>
		/// The authorization endpoint.
		/// </summary>
		private const string AuthorizationEndpoint = "https://www.facebook.com/dialog/oauth";

		/// <summary>
		/// The token endpoint.
		/// </summary>
		private const string TokenEndpoint = "https://graph.facebook.com/oauth/access_token";

		/// <summary>
		/// The _app id.
		/// </summary>
		private readonly string _appId;

		/// <summary>
		/// The _app secret.
		/// </summary>
		private readonly string _appSecret;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="FacebookClient"/> class.
		/// </summary>
		/// <param name="appId">
		/// The app id.
		/// </param>
		/// <param name="appSecret">
		/// The app secret.
		/// </param>
		/// <exception cref="ArgumentException">
		/// </exception>
		/// <exception cref="ArgumentException">
		/// </exception>
		public FacebookClient(string appId, string appSecret)
			: base("facebook") {
			Requires.NotNullOrEmpty(appId, "appId");
			Requires.NotNullOrEmpty(appSecret, "appSecret");

			this._appId = appId;
			this._appSecret = appSecret;
		}

		#endregion

		#region Methods

		/// <summary>
		/// The get service login url.
		/// </summary>
		/// <param name="returnUrl">
		/// The return url.
		/// </param>
		/// <returns>
		/// </returns>
		protected override Uri GetServiceLoginUrl(Uri returnUrl) {
			// Note: Facebook doesn't like us to url-encode the redirect_uri value
			var builder = new UriBuilder(AuthorizationEndpoint);
			builder.AppendQueryArgs(
				new Dictionary<string, string> { { "client_id", this._appId }, { "redirect_uri", returnUrl.AbsoluteUri }, });
			return builder.Uri;
		}

		/// <summary>
		/// The get user data.
		/// </summary>
		/// <param name="accessToken">
		/// The access token.
		/// </param>
		/// <returns>
		/// </returns>
		protected override IDictionary<string, string> GetUserData(string accessToken) {
			FacebookGraphData graphData;
			var request =
				WebRequest.Create(
					"https://graph.facebook.com/me?access_token=" + MessagingUtilities.EscapeUriDataStringRfc3986(accessToken));
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

		/// <summary>
		/// The query access token.
		/// </summary>
		/// <param name="returnUrl">
		/// The return url.
		/// </param>
		/// <param name="authorizationCode">
		/// The authorization code.
		/// </param>
		/// <returns>
		/// The query access token.
		/// </returns>
		protected override string QueryAccessToken(Uri returnUrl, string authorizationCode) {
			// Note: Facebook doesn't like us to url-encode the redirect_uri value
			var builder = new UriBuilder(TokenEndpoint);
			builder.AppendQueryArgs(
				new Dictionary<string, string> {
					{ "client_id", this._appId }, 
					{ "redirect_uri", returnUrl.AbsoluteUri }, 
					{ "client_secret", this._appSecret }, 
					{ "code", authorizationCode }, 
				});

			using (WebClient client = new WebClient()) {
				string data = client.DownloadString(builder.Uri);
				if (string.IsNullOrEmpty(data)) {
					return null;
				}

				var parsedQueryString = HttpUtility.ParseQueryString(data);
				return parsedQueryString["access_token"];
			}
		}

		#endregion
	}
}
