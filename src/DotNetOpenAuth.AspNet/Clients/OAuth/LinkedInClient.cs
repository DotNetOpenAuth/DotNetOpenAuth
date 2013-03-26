//-----------------------------------------------------------------------
// <copyright file="LinkedInClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
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
	/// Represents LinkedIn authentication client.
	/// </summary>
	public sealed class LinkedInClient : OAuthClient {
		#region Constants and Fields

		/// <summary>
		/// Describes the OAuth service provider endpoints for LinkedIn.
		/// </summary>
		public static readonly ServiceProviderDescription LinkedInServiceDescription = new ServiceProviderDescription(
			"https://api.linkedin.com/uas/oauth/requestToken",
			"https://www.linkedin.com/uas/oauth/authenticate",
			"https://api.linkedin.com/uas/oauth/accessToken");

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="LinkedInClient"/> class.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		public LinkedInClient(string consumerKey, string consumerSecret)
			: base("linkedIn", LinkedInServiceDescription, consumerKey, consumerSecret) {
		}

		#endregion

		#region Methods

		/// <summary>
		/// Check if authentication succeeded after user is redirected back from the service provider.
		/// </summary>
		/// <param name="response">The response token returned from service provider</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// Authentication result.
		/// </returns>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "We don't care if the request fails.")]
		protected override async Task<AuthenticationResult> VerifyAuthenticationCoreAsync(AccessTokenResponse response, CancellationToken cancellationToken = default(CancellationToken)) {
			// See here for Field Selectors API http://developer.linkedin.com/docs/DOC-1014
			const string ProfileRequestUrl = "https://api.linkedin.com/v1/people/~:(id,first-name,last-name,headline,industry,summary)";

			var accessToken = response.AccessToken;
			var authorizingHandler = this.WebWorker.CreateMessageHandler(accessToken);
			try {
				using (var httpClient = new HttpClient(authorizingHandler)) {
					using (HttpResponseMessage profileResponse = await httpClient.GetAsync(ProfileRequestUrl, cancellationToken)) {
						using (Stream responseStream = await profileResponse.Content.ReadAsStreamAsync()) {
							XDocument document = LoadXDocumentFromStream(responseStream);
							string userId = document.Root.Element("id").Value;

							string firstName = document.Root.Element("first-name").Value;
							string lastName = document.Root.Element("last-name").Value;
							string userName = firstName + " " + lastName;

							var extraData = new NameValueCollection();
							extraData.Add("accesstoken", accessToken.Token);
							extraData.Add("accesstokensecret", accessToken.Secret);
							extraData.Add("name", userName);
							extraData.AddDataIfNotEmpty(document, "headline");
							extraData.AddDataIfNotEmpty(document, "summary");
							extraData.AddDataIfNotEmpty(document, "industry");

							return new AuthenticationResult(
								isSuccessful: true,
								provider: this.ProviderName,
								providerUserId: userId,
								userName: userName,
								extraData: extraData);
						}
					}
				}
			} catch (Exception exception) {
				return new AuthenticationResult(exception);
			}
		}

		#endregion
	}
}