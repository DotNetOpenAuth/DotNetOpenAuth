//-----------------------------------------------------------------------
// <copyright file="WebServerClient.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// An OAuth 2.0 consumer designed for web applications.
	/// </summary>
	public class WebServerClient : ClientBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebServerClient"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <param name="clientSecret">The client secret.</param>
		public WebServerClient(AuthorizationServerDescription authorizationServer, string clientIdentifier = null, string clientSecret = null)
			: this(authorizationServer, clientIdentifier, DefaultSecretApplicator(clientSecret)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebServerClient"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <param name="clientCredentialApplicator">
		/// The tool to use to apply client credentials to authenticated requests to the Authorization Server.
		/// May be <c>null</c> for clients with no secret or other means of authentication.
		/// </param>
		public WebServerClient(AuthorizationServerDescription authorizationServer, string clientIdentifier, ClientCredentialApplicator clientCredentialApplicator)
			: base(authorizationServer, clientIdentifier, clientCredentialApplicator) {
		}

		/// <summary>
		/// Gets or sets an optional component that gives you greater control to record and influence the authorization process.
		/// </summary>
		/// <value>The authorization tracker.</value>
		public IClientAuthorizationTracker AuthorizationTracker { get; set; }

		/// <summary>
		/// Prepares a request for user authorization from an authorization server.
		/// </summary>
		/// <param name="scope">The scope of authorized access requested.</param>
		/// <param name="returnTo">The URL the authorization server should redirect the browser (typically on this site) to when the authorization is completed.  If null, the current request's URL will be used.</param>
		public void RequestUserAuthorization(IEnumerable<string> scope = null, Uri returnTo = null) {
			var authorizationState = new AuthorizationState(scope) {
				Callback = returnTo,
			};
			this.PrepareRequestUserAuthorization(authorizationState).Send();
		}

		/// <summary>
		/// Prepares a request for user authorization from an authorization server.
		/// </summary>
		/// <param name="scopes">The scope of authorized access requested.</param>
		/// <param name="returnTo">The URL the authorization server should redirect the browser (typically on this site) to when the authorization is completed.  If null, the current request's URL will be used.</param>
		/// <returns>The authorization request.</returns>
		public OutgoingWebResponse PrepareRequestUserAuthorization(IEnumerable<string> scopes = null, Uri returnTo = null) {
			var authorizationState = new AuthorizationState(scopes) {
				Callback = returnTo,
			};
			return this.PrepareRequestUserAuthorization(authorizationState);
		}

		/// <summary>
		/// Prepares a request for user authorization from an authorization server.
		/// </summary>
		/// <param name="authorization">The authorization state to associate with this particular request.</param>
		/// <returns>The authorization request.</returns>
		public OutgoingWebResponse PrepareRequestUserAuthorization(IAuthorizationState authorization) {
			Requires.NotNull(authorization, "authorization");
			Requires.ValidState(authorization.Callback != null || (HttpContext.Current != null && HttpContext.Current.Request != null), MessagingStrings.HttpContextRequired);
			Requires.ValidState(!string.IsNullOrEmpty(this.ClientIdentifier), Strings.RequiredPropertyNotYetPreset, "ClientIdentifier");
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			if (authorization.Callback == null) {
				authorization.Callback = this.Channel.GetRequestFromContext().GetPublicFacingUrl()
					.StripMessagePartsFromQueryString(this.Channel.MessageDescriptions.Get(typeof(EndUserAuthorizationSuccessResponseBase), Protocol.Default.Version))
					.StripMessagePartsFromQueryString(this.Channel.MessageDescriptions.Get(typeof(EndUserAuthorizationFailedResponse), Protocol.Default.Version));
				authorization.SaveChanges();
			}

			var request = new EndUserAuthorizationRequestC(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				Callback = authorization.Callback,
			};
			request.Scope.ResetContents(authorization.Scope);

			// Mitigate XSRF attacks by including a state value that would be unpredictable between users, but
			// verifiable for the same user/session.
			// If the host is implementing the authorization tracker though, they're handling this protection themselves.
			if (this.AuthorizationTracker == null) {
				var context = this.Channel.GetHttpContext();
				if (context.Session != null) {
					request.ClientState = context.Session.SessionID;
				} else {
					Logger.OAuth.WarnFormat("No request context discovered, so no client state parameter could be set to mitigate XSRF attacks.");
				}
			}

			return this.Channel.PrepareResponse(request);
		}

		/// <summary>
		/// Processes the authorization response from an authorization server, if available.
		/// </summary>
		/// <param name="request">The incoming HTTP request that may carry an authorization response.</param>
		/// <returns>The authorization state that contains the details of the authorization.</returns>
		public IAuthorizationState ProcessUserAuthorization(HttpRequestBase request = null) {
			Requires.ValidState(!string.IsNullOrEmpty(this.ClientIdentifier), Strings.RequiredPropertyNotYetPreset, "ClientIdentifier");
			Requires.ValidState(this.ClientCredentialApplicator != null, Strings.RequiredPropertyNotYetPreset, "ClientCredentialApplicator");

			if (request == null) {
				request = this.Channel.GetRequestFromContext();
			}

			IMessageWithClientState response;
			if (this.Channel.TryReadFromRequest<IMessageWithClientState>(request, out response)) {
				Uri callback = MessagingUtilities.StripMessagePartsFromQueryString(request.GetPublicFacingUrl(), this.Channel.MessageDescriptions.Get(response));
				IAuthorizationState authorizationState;
				if (this.AuthorizationTracker != null) {
					authorizationState = this.AuthorizationTracker.GetAuthorizationState(callback, response.ClientState);
					ErrorUtilities.VerifyProtocol(authorizationState != null, ClientStrings.AuthorizationResponseUnexpectedMismatch);
				} else {
					var context = this.Channel.GetHttpContext();
					if (context.Session != null) {
						ErrorUtilities.VerifyProtocol(string.Equals(response.ClientState, context.Session.SessionID, StringComparison.Ordinal), ClientStrings.AuthorizationResponseUnexpectedMismatch);
					} else {
						Logger.OAuth.WarnFormat("No request context discovered, so no client state parameter could be checked to mitigate XSRF attacks.");
					}

					authorizationState = new AuthorizationState { Callback = callback };
				}
				var success = response as EndUserAuthorizationSuccessAuthCodeResponse;
				var failure = response as EndUserAuthorizationFailedResponse;
				ErrorUtilities.VerifyProtocol(success != null || failure != null, MessagingStrings.UnexpectedMessageReceivedOfMany);
				if (success != null) {
					this.UpdateAuthorizationWithResponse(authorizationState, success);
				} else { // failure
					Logger.OAuth.Info("User refused to grant the requested authorization at the Authorization Server.");
					authorizationState.Delete();
				}

				return authorizationState;
			}

			return null;
		}
	}
}
