using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOAuth;
using DotNetOAuth.ChannelElements;
using DotNetOAuth.Messaging;

/// <summary>
/// Service Provider definitions.
/// </summary>
public static class Constants {
	/// <summary>
	/// The Consumer to use for accessing Google data APIs.
	/// </summary>
	public static readonly ServiceProviderDescription GoogleDescription = new ServiceProviderDescription {
		RequestTokenEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthGetRequestToken", HttpDeliveryMethod.AuthorizationHeaderRequest),
		UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthAuthorizeToken", HttpDeliveryMethod.AuthorizationHeaderRequest),
		AccessTokenEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthGetAccessToken", HttpDeliveryMethod.AuthorizationHeaderRequest),
		TamperProtectionElements = new ITamperProtectionChannelBindingElement[] {
				new HmacSha1SigningBindingElement(),
			},
	};

	/// <summary>
	/// Values of the "scope" parameter that indicates what data streams the Consumer
	/// wants access to.
	/// </summary>
	public static class GoogleScopes {
		/// <summary>
		/// Access to the Gmail address book.
		/// </summary>
		public const string Contacts = "http://www.google.com/m8/feeds/";

		/// <summary>
		/// The URI to get contacts once authorization is granted.
		/// </summary>
		public static readonly MessageReceivingEndpoint GetContacts = new MessageReceivingEndpoint("http://www.google.com/m8/feeds/contacts/default/full/", HttpDeliveryMethod.GetRequest);
	}
}
