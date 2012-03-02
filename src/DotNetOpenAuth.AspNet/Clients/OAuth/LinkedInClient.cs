//-----------------------------------------------------------------------
// <copyright file="LinkedInClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Xml.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// Represents LinkedIn authentication client.
	/// </summary>
	public sealed class LinkedInClient : OAuthClient {
		public static readonly ServiceProviderDescription LinkedInServiceDescription = new ServiceProviderDescription {
			RequestTokenEndpoint = new MessageReceivingEndpoint("https://api.linkedin.com/uas/oauth/requestToken", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
			UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://www.linkedin.com/uas/oauth/authenticate", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
			AccessTokenEndpoint = new MessageReceivingEndpoint("https://api.linkedin.com/uas/oauth/accessToken", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
			TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="LinkedInClient"/> class.
		/// </summary>
		/// <param name="consumerKey">The LinkedIn app's consumer key.</param>
		/// <param name="consumerSecret">The LinkedIn app's consumer secret.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Reliability",
			"CA2000:Dispose objects before losing scope",
			Justification = "We can't dispose the object because we still need it through the app lifetime.")]
		public LinkedInClient(string consumerKey, string consumerSecret) :
			base("linkedIn", LinkedInServiceDescription, consumerKey, consumerSecret) {
		}

		/// <summary>
		/// Check if authentication succeeded after user is redirected back from the service provider.
		/// </summary>
		/// <param name="response">The response token returned from service provider</param>
		/// <returns>
		/// Authentication result.
		/// </returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Design",
			"CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "We don't care if the request fails.")]
		protected override AuthenticationResult VerifyAuthenticationCore(AuthorizedTokenResponse response) {
			// See here for Field Selectors API http://developer.linkedin.com/docs/DOC-1014
			const string profileRequestUrl = "http://api.linkedin.com/v1/people/~:(id,first-name,last-name,headline,industry,summary)";

			string accessToken = response.AccessToken;

			var profileEndpoint = new MessageReceivingEndpoint(profileRequestUrl, HttpDeliveryMethods.GetRequest);
			HttpWebRequest request = WebWorker.PrepareAuthorizedRequest(profileEndpoint, accessToken);

			try {
				using (WebResponse profileResponse = request.GetResponse()) {
					using (Stream responseStream = profileResponse.GetResponseStream()) {
						XDocument document = XDocument.Load(responseStream);
						string userId = document.Root.Element("id").Value;

						string firstName = document.Root.Element("first-name").Value;
						string lastName = document.Root.Element("last-name").Value;
						string userName = firstName + " " + lastName;

						var extraData = new Dictionary<string, string>();
						extraData.Add("name", userName);
						extraData.AddDataIfNotEmpty(document, "headline");
						extraData.AddDataIfNotEmpty(document, "summary");
						extraData.AddDataIfNotEmpty(document, "industry");

						return new AuthenticationResult(
							isSuccessful: true,
							provider: ProviderName,
							providerUserId: userId,
							userName: userName,
							extraData: extraData);
					}
				}
			} catch (Exception exception) {
				return new AuthenticationResult(exception);
			}
		}
	}
}