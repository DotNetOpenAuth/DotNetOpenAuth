//-----------------------------------------------------------------------
// <copyright file="WebServerClient.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
			: base(authorizationServer, clientIdentifier, clientSecret) {
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
		/// <param name="state">The state of the client that should be sent back with the authorization response.</param>
		/// <param name="returnTo">The URL the authorization server should redirect the browser (typically on this site) to when the authorization is completed.  If null, the current request's URL will be used.</param>
		public void RequestUserAuthorization(IEnumerable<string> scope = null, string state = null, Uri returnTo = null) {
			var authorizationState = new AuthorizationState(scope) {
				Callback = returnTo,
			};
			this.PrepareRequestUserAuthorization(authorizationState, state).Send();
		}

		/// <summary>
		/// Prepares a request for user authorization from an authorization server.
		/// </summary>
		/// <param name="scopes">The scope of authorized access requested.</param>
		/// <param name="state">The state of the client that should be sent back with the authorization response.</param>
		/// <returns>The authorization request.</returns>
		public OutgoingWebResponse PrepareRequestUserAuthorization(IEnumerable<string> scopes = null, string state = null) {
			var authorizationState = new AuthorizationState(scopes);
			return this.PrepareRequestUserAuthorization(authorizationState, state);
		}

		/// <summary>
		/// Prepares a request for user authorization from an authorization server.
		/// </summary>
		/// <param name="authorization">The authorization state to associate with this particular request.</param>
		/// <param name="state">The state of the client that should be sent back with the authorization response.</param>
		/// <returns>The authorization request.</returns>
		public OutgoingWebResponse PrepareRequestUserAuthorization(IAuthorizationState authorization, string state = null) {
			Contract.Requires<ArgumentNullException>(authorization != null);
			Contract.Requires<InvalidOperationException>(authorization.Callback != null || (HttpContext.Current != null && HttpContext.Current.Request != null), MessagingStrings.HttpContextRequired);
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			if (authorization.Callback == null) {
				authorization.Callback = this.Channel.GetRequestFromContext().UrlBeforeRewriting
					.StripMessagePartsFromQueryString(this.Channel.MessageDescriptions.Get(typeof(EndUserAuthorizationSuccessResponseBase), Protocol.Default.Version))
					.StripMessagePartsFromQueryString(this.Channel.MessageDescriptions.Get(typeof(EndUserAuthorizationFailedResponse), Protocol.Default.Version));
				authorization.SaveChanges();
			}

			var request = new EndUserAuthorizationRequest(this.AuthorizationServer) {
				ClientIdentifier = this.ClientIdentifier,
				Callback = authorization.Callback,
				ClientState = state,
			};
			request.Scope.ResetContents(authorization.Scope);

			return this.Channel.PrepareResponse(request);
		}

		/// <summary>
		/// Processes the authorization response from an authorization server, if available.
		/// </summary>
		/// <param name="request">The incoming HTTP request that may carry an authorization response.</param>
		/// <returns>The authorization state that contains the details of the authorization.</returns>
		public IAuthorizationState ProcessUserAuthorization(HttpRequestInfo request = null) {
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientIdentifier));
			Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(this.ClientSecret));

			if (request == null) {
				request = this.Channel.GetRequestFromContext();
			}

			IMessageWithClientState response;
			if (this.Channel.TryReadFromRequest<IMessageWithClientState>(request, out response)) {
				Uri callback = MessagingUtilities.StripMessagePartsFromQueryString(request.UrlBeforeRewriting, this.Channel.MessageDescriptions.Get(response));
				IAuthorizationState authorizationState;
				if (this.AuthorizationTracker != null) {
					authorizationState = this.AuthorizationTracker.GetAuthorizationState(callback, response.ClientState);
					ErrorUtilities.VerifyProtocol(authorizationState != null, "Unexpected OAuth authorization response received with callback and client state that does not match an expected value.");
				} else {
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
