//-----------------------------------------------------------------------
// <copyright file="WebAppClient.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.Messages;

	/// <summary>
	/// An OAuth WRAP consumer designed for web applications.
	/// </summary>
	public class WebAppClient : ClientBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppClient"/> class.
		/// </summary>
		/// <param name="authorizationServer">The token issuer.</param>
		public WebAppClient(AuthorizationServerDescription authorizationServer)
			: base(authorizationServer) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppClient"/> class.
		/// </summary>
		/// <param name="tokenIssuerEndpoint">The token issuer endpoint.</param>
		public WebAppClient(Uri tokenIssuerEndpoint)
			: base(tokenIssuerEndpoint) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppClient"/> class.
		/// </summary>
		/// <param name="tokenIssuerEndpoint">The token issuer endpoint.</param>
		public WebAppClient(string tokenIssuerEndpoint)
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

		public WebAppRequest PrepareRequestUserAuthorization() {
			Contract.Requires<InvalidOperationException>(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));
			Contract.Ensures(Contract.Result<WebAppRequest>() != null);
			Contract.Ensures(Contract.Result<WebAppRequest>().ClientIdentifier == this.ClientIdentifier);

			return this.PrepareRequestUserAuthorization(this.Channel.GetRequestFromContext().UrlBeforeRewriting);
		}

		public WebAppRequest PrepareRequestUserAuthorization(Uri callback) {
			Contract.Requires<ArgumentNullException>(callback != null);
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));
			Contract.Ensures(Contract.Result<WebAppRequest>() != null);
			Contract.Ensures(Contract.Result<WebAppRequest>().Callback == callback);
			Contract.Ensures(Contract.Result<WebAppRequest>().ClientIdentifier == this.ClientIdentifier);

			var request = new WebAppRequest(this.AuthorizationServer.EndpointUrl, this.AuthorizationServer.Version) {
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
					message is WebAppSuccessResponse || message is WebAppFailedResponse,
					MessagingStrings.UnexpectedMessageReceivedOfMany);
			}
			return message;
		}
	}
}
