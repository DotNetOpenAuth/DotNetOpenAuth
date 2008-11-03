//-----------------------------------------------------------------------
// <copyright file="GoogleConsumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.CommonConsumers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A consumer capable of communicating with Google Data APIs.
	/// </summary>
	public class GoogleConsumer : CommonConsumerBase {
		/// <summary>
		/// The Consumer to use for accessing Google data APIs.
		/// </summary>
		private static readonly ServiceProviderDescription GoogleDescription = new ServiceProviderDescription {
			RequestTokenEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthGetRequestToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
			UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthAuthorizeToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
			AccessTokenEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthGetAccessToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
			TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
		};

		/// <summary>
		/// A mapping between Google's applications and their URI scope values.
		/// </summary>
		private static readonly Dictionary<Applications, string> DataScopeUris = new Dictionary<Applications, string> {
			{ Applications.Contacts, "http://www.google.com/m8/feeds/" },
			{ Applications.Calendar, "http://www.google.com/calendar/feeds/" },
		};

		/// <summary>
		/// The URI to get contacts once authorization is granted.
		/// </summary>
		private static readonly MessageReceivingEndpoint GetContactsEndpoint = new MessageReceivingEndpoint("http://www.google.com/m8/feeds/contacts/default/full/", HttpDeliveryMethods.GetRequest);

		/// <summary>
		/// Initializes a new instance of the <see cref="GoogleConsumer"/> class.
		/// </summary>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="consumerKey">The consumer key.</param>
		public GoogleConsumer(ITokenManager tokenManager, string consumerKey)
			: base(GoogleDescription, tokenManager, consumerKey) {
		}

		/// <summary>
		/// The many specific authorization scopes Google offers.
		/// </summary>
		[Flags]
		public enum Applications : long {
			/// <summary>
			/// The Gmail address book.
			/// </summary>
			Contacts = 0x1,

			/// <summary>
			/// Appointments in Google Calendar.
			/// </summary>
			Calendar = 0x2,
		}

		/// <summary>
		/// Requests authorization from Google to access data from a set of Google applications.
		/// </summary>
		/// <param name="requestedAccessScope">The requested access scope.</param>
		public void RequestAuthorization(Applications requestedAccessScope) {
			Uri callback = MessagingUtilities.GetRequestUrlFromContext().StripQueryArgumentsWithPrefix(Protocol.Default.ParameterPrefix);
			var extraParameters = new Dictionary<string, string> {
				{ "scope", this.GetScopeUri(requestedAccessScope) },
			};
			var request = this.Consumer.PrepareRequestUserAuthorization(callback, extraParameters, null);
			this.Consumer.Channel.Send(request).Send();
		}

		/// <summary>
		/// Gets the access token on the next page request after a call to <see cref="RequestAuthorization"/>.
		/// </summary>
		/// <returns>The access token that should be stored for later use.</returns>
		public string GetAccessToken() {
			var response = this.Consumer.ProcessUserAuthorization();
			return response != null ? response.AccessToken : null;
		}

		/// <summary>
		/// Gets the Gmail address book's contents.
		/// </summary>
		/// <param name="accessToken">The access token previously retrieved from the <see cref="GetAccessToken"/> method.</param>
		/// <returns>An XML document returned by Google.</returns>
		public XDocument GetContacts(string accessToken) {
			var response = this.PrepareAuthorizedRequestAndSend(GetContactsEndpoint, accessToken);
			XDocument result = XDocument.Parse(response.Body);
			return result;
		}

		/// <summary>
		/// A general method for sending OAuth-authorized requests for user data from Google.
		/// </summary>
		/// <param name="endpoint">The Google URL to retrieve the data from.</param>
		/// <param name="accessToken">The access token previously retrieved from the <see cref="GetAccessToken"/> method.</param>
		/// <returns>Whatever the response Google sends.</returns>
		public Response PrepareAuthorizedRequestAndSend(MessageReceivingEndpoint endpoint, string accessToken) {
			return this.Consumer.PrepareAuthorizedRequestAndSend(endpoint, accessToken);
		}

		/// <summary>
		/// Gets the scope URI in Google's format.
		/// </summary>
		/// <param name="scope">The scope, which may include one or several Google applications.</param>
		/// <returns>A space-delimited list of URIs for the requested Google applications.</returns>
		private string GetScopeUri(Applications scope) {
			return string.Join(" ", GetIndividualFlags(scope).Select(app => DataScopeUris[(Applications)app]).ToArray());
		}
	}
}
