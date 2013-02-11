//-----------------------------------------------------------------------
// <copyright file="TwitterClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// Represents a Twitter client
	/// </summary>
	public class TwitterClient : OAuthClient {
		#region Constants and Fields

		/// <summary>
		/// The description of Twitter's OAuth protocol URIs for use with their "Sign in with Twitter" feature.
		/// </summary>
		public static readonly ServiceProviderDescription TwitterServiceDescription = new ServiceProviderDescription {
			RequestTokenEndpoint =
				new MessageReceivingEndpoint(
					"https://api.twitter.com/oauth/request_token",
					HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
			UserAuthorizationEndpoint =
				new MessageReceivingEndpoint(
					"https://api.twitter.com/oauth/authenticate",
					HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
			AccessTokenEndpoint =
				new MessageReceivingEndpoint(
					"https://api.twitter.com/oauth/access_token",
					HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
			TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
		};

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="TwitterClient"/> class with the specified consumer key and consumer secret.
		/// </summary>
		/// <remarks>
		/// Tokens exchanged during the OAuth handshake are stored in cookies.
		/// </remarks>
		/// <param name="consumerKey">
		/// The consumer key. 
		/// </param>
		/// <param name="consumerSecret">
		/// The consumer secret. 
		/// </param>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
			Justification = "We can't dispose the object because we still need it through the app lifetime.")]
		public TwitterClient(string consumerKey, string consumerSecret)
			: this(consumerKey, consumerSecret, new AuthenticationOnlyCookieOAuthTokenManager()) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="TwitterClient"/> class.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		/// <param name="tokenManager">The token manager.</param>
		public TwitterClient(string consumerKey, string consumerSecret, IOAuthTokenManager tokenManager)
			: base("twitter", TwitterServiceDescription, new SimpleConsumerTokenManager(consumerKey, consumerSecret, tokenManager)) {
		}

		#endregion

		#region Methods

		/// <summary>
		/// Check if authentication succeeded after user is redirected back from the service provider.
		/// </summary>
		/// <param name="response">The response token returned from service provider</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// Authentication result
		/// </returns>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "We don't care if the request for additional data fails.")]
		protected override async Task<AuthenticationResult> VerifyAuthenticationCoreAsync(AuthorizedTokenResponse response, CancellationToken cancellationToken) {
			string accessToken = response.AccessToken;
			string userId = response.ExtraData["user_id"];
			string userName = response.ExtraData["screen_name"];

			var profileRequestUrl = new Uri("https://api.twitter.com/1/users/show.xml?user_id="
									   + MessagingUtilities.EscapeUriDataStringRfc3986(userId));
			var profileEndpoint = new MessageReceivingEndpoint(profileRequestUrl, HttpDeliveryMethods.GetRequest);
			HttpRequestMessage request = await this.WebWorker.PrepareAuthorizedRequestAsync(profileEndpoint, accessToken, cancellationToken);

			var extraData = new Dictionary<string, string>();
			extraData.Add("accesstoken", accessToken);
			try {
				using (var httpClient = new HttpClient()) {
					using (HttpResponseMessage profileResponse = await httpClient.SendAsync(request)) {
						using (Stream responseStream = await profileResponse.Content.ReadAsStreamAsync()) {
							XDocument document = LoadXDocumentFromStream(responseStream);
							extraData.AddDataIfNotEmpty(document, "name");
							extraData.AddDataIfNotEmpty(document, "location");
							extraData.AddDataIfNotEmpty(document, "description");
							extraData.AddDataIfNotEmpty(document, "url");
						}
					}
				}
			}
			catch (Exception) {
				// At this point, the authentication is already successful.
				// Here we are just trying to get additional data if we can.
				// If it fails, no problem.
			}

			return new AuthenticationResult(
				isSuccessful: true, provider: this.ProviderName, providerUserId: userId, userName: userName, extraData: extraData);
		}

		#endregion
	}
}