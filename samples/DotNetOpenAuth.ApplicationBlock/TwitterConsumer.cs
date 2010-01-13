//-----------------------------------------------------------------------
// <copyright file="TwitterConsumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
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
		public static readonly ServiceProviderDescription ServiceDescription = new ServiceProviderDescription {
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

		private static readonly MessageReceivingEndpoint UpdateProfileBackgroundImageEndpoint = new MessageReceivingEndpoint("http://twitter.com/account/update_profile_background_image.xml", HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

		private static readonly MessageReceivingEndpoint UpdateProfileImageEndpoint = new MessageReceivingEndpoint("http://twitter.com/account/update_profile_image.xml", HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

		/// <summary>
		/// Initializes static members of the <see cref="TwitterConsumer"/> class.
		/// </summary>
		static TwitterConsumer() {
			// Twitter can't handle the Expect 100 Continue HTTP header. 
			ServicePointManager.FindServicePoint(GetFavoritesEndpoint.Location).Expect100Continue = false;
		}

		public static XDocument GetUpdates(ConsumerBase twitter, string accessToken) {
			IncomingWebResponse response = twitter.PrepareAuthorizedRequestAndSend(GetFriendTimelineStatusEndpoint, accessToken);
			return XDocument.Load(XmlReader.Create(response.GetResponseReader()));
		}

		public static XDocument GetFavorites(ConsumerBase twitter, string accessToken) {
			IncomingWebResponse response = twitter.PrepareAuthorizedRequestAndSend(GetFavoritesEndpoint, accessToken);
			return XDocument.Load(XmlReader.Create(response.GetResponseReader()));
		}

		public static XDocument UpdateProfileBackgroundImage(ConsumerBase twitter, string accessToken, string image, bool tile) {
			var parts = new[] {
				MultipartPostPart.CreateFormFilePart("image", image, "image/" + Path.GetExtension(image).Substring(1).ToLowerInvariant()),
				MultipartPostPart.CreateFormPart("tile", tile.ToString().ToLowerInvariant()),
			};
			HttpWebRequest request = twitter.PrepareAuthorizedRequest(UpdateProfileBackgroundImageEndpoint, accessToken, parts);
			request.ServicePoint.Expect100Continue = false;
			IncomingWebResponse response = twitter.Channel.WebRequestHandler.GetResponse(request);
			string responseString = response.GetResponseReader().ReadToEnd();
			return XDocument.Parse(responseString);
		}

		public static XDocument UpdateProfileImage(ConsumerBase twitter, string accessToken, string pathToImage) {
			string contentType = "image/" + Path.GetExtension(pathToImage).Substring(1).ToLowerInvariant();
			return UpdateProfileImage(twitter, accessToken, File.OpenRead(pathToImage), contentType);
		}

		public static XDocument UpdateProfileImage(ConsumerBase twitter, string accessToken, Stream image, string contentType) {
			var parts = new[] {
				MultipartPostPart.CreateFormFilePart("image", "twitterPhoto", contentType, image),
			};
			HttpWebRequest request = twitter.PrepareAuthorizedRequest(UpdateProfileImageEndpoint, accessToken, parts);
			IncomingWebResponse response = twitter.Channel.WebRequestHandler.GetResponse(request);
			string responseString = response.GetResponseReader().ReadToEnd();
			return XDocument.Parse(responseString);
		}
	}
}
