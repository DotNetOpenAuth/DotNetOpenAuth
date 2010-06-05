//-----------------------------------------------------------------------
// <copyright file="UserAgentClient.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuthWrap.Messages;
	using DotNetOpenAuth.Messaging;
	using System.Diagnostics.Contracts;

	public class UserAgentClient : ClientBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentClient"/> class.
		/// </summary>
		public UserAgentClient(AuthorizationServerDescription authorizationServer)
			: base(authorizationServer) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentClient"/> class.
		/// </summary>
		/// <param name="authorizationEndpoint">The authorization endpoint.</param>
		public UserAgentClient(Uri authorizationEndpoint)
			: base(new AuthorizationServerDescription { AuthorizationEndpoint = authorizationEndpoint }) {
			Contract.Requires<ArgumentNullException>(authorizationEndpoint != null, "authorizationEndpoint");
		}

		public Uri RequestUserAuthorization(IAuthorizationState authorization = null, bool immediate = false) {
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));

			if (authorization == null) {
				authorization = new AuthorizationState();
			}

			if (authorization.Callback == null) {
				authorization.Callback = new Uri("http://localhost/");
			}

			var request = new UserAgentRequest(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				Scope = authorization.Scope,
				SecretType = authorization.AccessTokenSecretType,
				Callback = authorization.Callback,
				Immediate = immediate,
			};

			return this.Channel.PrepareResponse(request).GetDirectUriRequest(this.Channel);
		}

		public IAuthorizationState ProcessUserAuthorization(Uri actualRedirectUrl, IAuthorizationState authorization = null) {
			Contract.Requires<ArgumentNullException>(actualRedirectUrl != null, "actualRedirectUrl");

			if (authorization == null) {
				authorization = new AuthorizationState();
			}

			var carrier = new HttpRequestInfo("GET", actualRedirectUrl, actualRedirectUrl.PathAndQuery, new System.Net.WebHeaderCollection(), null);
			IDirectedProtocolMessage response = this.Channel.ReadFromRequest(carrier);
			if (response == null) {
				return null;
			}

			UserAgentSuccessResponse success;
			UserAgentFailedResponse failure;
			if ((success = response as UserAgentSuccessResponse) != null) {
				this.UpdateAuthorizationWithResponse(authorization, success);
			} else if ((failure = response as UserAgentFailedResponse) != null) {
				authorization.Delete();
				return null;
			} else {
				ErrorUtilities.ThrowProtocol(MessagingStrings.UnexpectedMessageReceivedOfMany);
			}

			return authorization;
		}
	}
}
