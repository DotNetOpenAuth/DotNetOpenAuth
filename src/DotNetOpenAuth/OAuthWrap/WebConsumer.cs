//-----------------------------------------------------------------------
// <copyright file="WebConsumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.Messages;

	/// <summary>
	/// An OAuth WRAP consumer designed for web applications.
	/// </summary>
	public class WebConsumer : ConsumerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebConsumer"/> class.
		/// </summary>
		/// <param name="tokenIssuer">The token issuer.</param>
		public WebConsumer(AuthorizationServerDescription tokenIssuer)
			: base(tokenIssuer) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebConsumer"/> class.
		/// </summary>
		/// <param name="tokenIssuerEndpoint">The token issuer endpoint.</param>
		public WebConsumer(Uri tokenIssuerEndpoint)
			: base(tokenIssuerEndpoint) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebConsumer"/> class.
		/// </summary>
		/// <param name="tokenIssuerEndpoint">The token issuer endpoint.</param>
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
