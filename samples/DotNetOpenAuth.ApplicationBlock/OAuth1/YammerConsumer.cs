//-----------------------------------------------------------------------
// <copyright file="YammerConsumer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	public static class YammerConsumer {
		/// <summary>
		/// The Consumer to use for accessing Google data APIs.
		/// </summary>
		public static readonly ServiceProviderDescription ServiceDescription = new ServiceProviderDescription {
			RequestTokenEndpoint = new MessageReceivingEndpoint("https://www.yammer.com/oauth/request_token", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest),
			UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://www.yammer.com/oauth/authorize", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
			AccessTokenEndpoint = new MessageReceivingEndpoint("https://www.yammer.com/oauth/access_token", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest),
			TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new PlaintextSigningBindingElement() },
			ProtocolVersion = ProtocolVersion.V10,
		};

		public static DesktopConsumer CreateConsumer(IConsumerTokenManager tokenManager) {
			return new DesktopConsumer(ServiceDescription, tokenManager);
		}

		public static Uri PrepareRequestAuthorization(DesktopConsumer consumer, out string requestToken) {
			if (consumer == null) {
				throw new ArgumentNullException("consumer");
			}

			Uri authorizationUrl = consumer.RequestUserAuthorization(null, null, out requestToken);
			return authorizationUrl;
		}

		public static AuthorizedTokenResponse CompleteAuthorization(DesktopConsumer consumer, string requestToken, string userCode) {
			// Because Yammer has a proprietary callback_token parameter, and it's passed
			// with the message that specifically bans extra arguments being passed, we have
			// to cheat by adding the data to the URL itself here.
			var customServiceDescription = new ServiceProviderDescription {
				RequestTokenEndpoint = ServiceDescription.RequestTokenEndpoint,
				UserAuthorizationEndpoint = ServiceDescription.UserAuthorizationEndpoint,
				AccessTokenEndpoint = new MessageReceivingEndpoint(ServiceDescription.AccessTokenEndpoint.Location.AbsoluteUri + "?oauth_verifier=" + Uri.EscapeDataString(userCode), HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest),
				TamperProtectionElements = ServiceDescription.TamperProtectionElements,
				ProtocolVersion = ProtocolVersion.V10,
			};

			// To use a custom service description we also must create a new WebConsumer.
			var customConsumer = new DesktopConsumer(customServiceDescription, consumer.TokenManager);
			var response = customConsumer.ProcessUserAuthorization(requestToken, userCode);
			return response;
		}
	}
}
