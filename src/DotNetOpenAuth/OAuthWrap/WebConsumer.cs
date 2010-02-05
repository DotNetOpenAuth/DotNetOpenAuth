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
	using System.Diagnostics.Contracts;
	using System.Web;

	/// <summary>
	/// An OAuth WRAP consumer designed for web applications.
	/// </summary>
	public class WebConsumer : ConsumerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebConsumer"/> class.
		/// </summary>
		/// <param name="authorizationServer">The token issuer.</param>
		public WebConsumer(AuthorizationServerDescription authorizationServer)
			: base(authorizationServer) {
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

		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		public string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the client secret shared with the Authorization Server.
		/// </summary>
		public string ClientSecret { get; set; }

		public UserAuthorizationInUserAgentRequest PrepareRequestUserAuthorization() {
			Contract.Requires<InvalidOperationException>(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));
			Contract.Ensures(Contract.Result<UserAuthorizationInUserAgentRequest>() != null);
			Contract.Ensures(Contract.Result<UserAuthorizationInUserAgentRequest>().ClientIdentifier == this.ClientIdentifier);

			return this.PrepareRequestUserAuthorization(this.Channel.GetRequestFromContext().UrlBeforeRewriting);
		}

		public UserAuthorizationInUserAgentRequest PrepareRequestUserAuthorization(Uri callback) {
			Contract.Requires<ArgumentNullException>(callback != null);
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));
			Contract.Ensures(Contract.Result<UserAuthorizationInUserAgentRequest>() != null);
			Contract.Ensures(Contract.Result<UserAuthorizationInUserAgentRequest>().Callback == callback);
			Contract.Ensures(Contract.Result<UserAuthorizationInUserAgentRequest>().ClientIdentifier == this.ClientIdentifier);

			var request = new UserAuthorizationInUserAgentRequest(this.TokenIssuer.EndpointUrl, this.TokenIssuer.Version) {
				ClientIdentifier = this.ClientIdentifier,
				Callback = callback,
			};
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
