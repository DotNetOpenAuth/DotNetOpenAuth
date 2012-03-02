//-----------------------------------------------------------------------
// <copyright file="DotNetOpenAuthWebConsumer.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	public class DotNetOpenAuthWebConsumer : IOAuthWebWorker {
		private readonly WebConsumer _webConsumer;

		public DotNetOpenAuthWebConsumer(ServiceProviderDescription serviceDescription, IConsumerTokenManager tokenManager) {
			if (serviceDescription == null) {
				throw new ArgumentNullException("consumer");
			}

			if (tokenManager == null) {
				throw new ArgumentNullException("tokenManager");
			}

			_webConsumer = new WebConsumer(serviceDescription, tokenManager);
		}

		public void RequestAuthentication(Uri callback) {
			var redirectParameters = new Dictionary<string, string>() { { "force_login", "false" } };
			UserAuthorizationRequest request = _webConsumer.PrepareRequestUserAuthorization(callback, null, redirectParameters);
			_webConsumer.Channel.PrepareResponse(request).Send();
		}

		public AuthorizedTokenResponse ProcessUserAuthorization() {
			return _webConsumer.ProcessUserAuthorization();
		}

		public HttpWebRequest PrepareAuthorizedRequest(MessageReceivingEndpoint profileEndpoint, string accessToken) {
			return _webConsumer.PrepareAuthorizedRequest(profileEndpoint, accessToken);
		}
	}
}