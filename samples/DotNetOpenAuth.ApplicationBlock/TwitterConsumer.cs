//-----------------------------------------------------------------------
// <copyright file="TwitterConsumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using System.Xml.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// A consumer capable of communicating with Twitter.
	/// </summary>
	public static class TwitterConsumer {
		/// <summary>
		/// The description of Twitter's OAuth protocol URIs.
		/// </summary>
		private static readonly ServiceProviderDescription TwitterDescription = new ServiceProviderDescription {
			RequestTokenEndpoint = new MessageReceivingEndpoint("http://twitter.com/oauth/request_token", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
			UserAuthorizationEndpoint = new MessageReceivingEndpoint("http://twitter.com/oauth/authorize", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
			AccessTokenEndpoint = new MessageReceivingEndpoint("http://twitter.com/oauth/access_token", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
			TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
		};

		/// <summary>
		/// The URI to get a user's favorites.
		/// </summary>
		private static readonly MessageReceivingEndpoint GetFavoritesEndpoint = new MessageReceivingEndpoint("http://twitter.com/favorites.xml", HttpDeliveryMethods.GetRequest);

		/// <summary>
		/// The URI to get the data on the user's home page.
		/// </summary>
		private static readonly MessageReceivingEndpoint GetFriendTimelineStatusEndpoint = new MessageReceivingEndpoint("http://twitter.com/statuses/friends_timeline.xml", HttpDeliveryMethods.GetRequest);

		/// <summary>
		/// Initializes a new instance of the <see cref="WebConsumer"/> class that is 
		/// prepared to communicate with Twitter.
		/// </summary>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="consumerKey">The consumer key.</param>
		/// <returns>The newly instantiated <see cref="WebConsumer"/>.</returns>
		public static WebConsumer CreateWebConsumer(ITokenManager tokenManager, string consumerKey) {
			return new WebConsumer(TwitterDescription, tokenManager) {
				ConsumerKey = consumerKey,
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DesktopConsumer"/> class that is 
		/// prepared to communicate with Twitter.
		/// </summary>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="consumerKey">The consumer key.</param>
		/// <returns>The newly instantiated <see cref="DesktopConsumer"/>.</returns>
		public static DesktopConsumer CreateDesktopConsumer(ITokenManager tokenManager, string consumerKey) {
			return new DesktopConsumer(TwitterDescription, tokenManager) {
				ConsumerKey = consumerKey,
			};
		}

		public static XDocument GetUpdates(ConsumerBase twitter, string accessToken) {
			IncomingWebResponse response = twitter.PrepareAuthorizedRequestAndSend(GetFriendTimelineStatusEndpoint, accessToken);
			return XDocument.Load(XmlReader.Create(response.GetResponseReader()));
		}

		public static XDocument GetFavorites(ConsumerBase twitter, string accessToken) {
			IncomingWebResponse response = twitter.PrepareAuthorizedRequestAndSend(GetFavoritesEndpoint, accessToken);
			return XDocument.Load(XmlReader.Create(response.GetResponseReader()));
		}
	}
}
