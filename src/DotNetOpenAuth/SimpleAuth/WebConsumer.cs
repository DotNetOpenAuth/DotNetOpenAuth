//-----------------------------------------------------------------------
// <copyright file="WebConsumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.SimpleAuth.Messages;
	using DotNetOpenAuth.Messaging;

	public class WebConsumer : ConsumerBase {
		public WebConsumer(TokenIssuerDescription tokenIssuer)
			: base(tokenIssuer) {
		}

		public WebConsumer(Uri tokenIssuerEndpoint)
			: base(tokenIssuerEndpoint) {
		}

		public WebConsumer(string tokenIssuerEndpoint)
			: base(tokenIssuerEndpoint) {
		}

		public UserAuthorizationInUserAgentRequest PrepareRequestUserAuthorization(string consumerKey) {
			var request = new UserAuthorizationInUserAgentRequest(this.TokenIssuer.EndpointUrl, this.TokenIssuer.Version);
			request.ConsumerKey = consumerKey;
			request.Callback = this.Channel.GetRequestFromContext().UrlBeforeRewriting;
			return request;
		}

		public IDirectedProtocolMessage ProcessUserAuthorization() {
			return this.ProcessUserAuthorization(this.Channel.GetRequestFromContext());
		}

		public IDirectedProtocolMessage ProcessUserAuthorization(HttpRequestInfo request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			IDirectedProtocolMessage message = this.Channel.ReadFromRequest();
			if (message != null) {
				ErrorUtilities.VerifyProtocol(
					message is UserAuthorizationInUserAgentGrantedResponse || message is UserAuthorizationInUserAgentDeniedResponse,
					MessagingStrings.UnexpectedMessageReceivedOfMany);
			}
			return message;
		}
	}
}
