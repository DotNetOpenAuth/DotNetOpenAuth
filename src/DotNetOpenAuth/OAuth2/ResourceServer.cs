//-----------------------------------------------------------------------
// <copyright file="ResourceServer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net;
	using System.Security.Principal;
	using System.ServiceModel.Channels;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using ChannelElements;
	using Messages;
	using Messaging;

	/// <summary>
	/// Provides services for validating OAuth access tokens.
	/// </summary>
	public class ResourceServer {
		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceServer"/> class.
		/// </summary>
		/// <param name="accessTokenAnalyzer">The access token analyzer.</param>
		public ResourceServer(IAccessTokenAnalyzer accessTokenAnalyzer) {
			Contract.Requires<ArgumentNullException>(accessTokenAnalyzer != null, "accessTokenAnalyzer");

			this.AccessTokenAnalyzer = accessTokenAnalyzer;
			this.Channel = new OAuth2ResourceServerChannel();
		}

		/// <summary>
		/// Gets the access token analyzer.
		/// </summary>
		/// <value>The access token analyzer.</value>
		public IAccessTokenAnalyzer AccessTokenAnalyzer { get; private set; }

		/// <summary>
		/// Gets or sets the endpoint information for an authorization server that may be used
		/// to obtain an access token for this resource.
		/// </summary>
		/// <value>The authorization server description.</value>
		/// <remarks>
		/// This value is optional.  If set, this information will be used to generate
		/// more useful HTTP 401 Unauthorized responses for requests that lack an OAuth access token.
		/// </remarks>
		public AuthorizationServerDescription AuthorizationServerDescription { get; set; }

		/// <summary>
		/// Gets the channel.
		/// </summary>
		/// <value>The channel.</value>
		internal OAuth2ResourceServerChannel Channel { get; private set; }

		/// <summary>
		/// Discovers what access the client should have considering the access token in the current request.
		/// </summary>
		/// <param name="username">The name on the account the client has access to.</param>
		/// <param name="scope">The set of operations the client is authorized for.</param>
		/// <returns>An error to return to the client if access is not authorized; <c>null</c> if access is granted.</returns>
		public OutgoingWebResponse VerifyAccess(out string username, out string scope) {
			return this.VerifyAccess(this.Channel.GetRequestFromContext(), out username, out scope);
		}

		/// <summary>
		/// Discovers what access the client should have considering the access token in the current request.
		/// </summary>
		/// <param name="httpRequestInfo">The HTTP request info.</param>
		/// <param name="username">The name on the account the client has access to.</param>
		/// <param name="scope">The set of operations the client is authorized for.</param>
		/// <returns>
		/// An error to return to the client if access is not authorized; <c>null</c> if access is granted.
		/// </returns>
		public virtual OutgoingWebResponse VerifyAccess(HttpRequestInfo httpRequestInfo, out string username, out string scope) {
			Contract.Requires<ArgumentNullException>(httpRequestInfo != null, "httpRequestInfo");

			AccessProtectedResourceRequest request = null;
			try {
				if (this.Channel.TryReadFromRequest<AccessProtectedResourceRequest>(httpRequestInfo, out request)) {
					if (this.AccessTokenAnalyzer.TryValidateAccessToken(request, request.AccessToken, out username, out scope)) {
						// No errors to return.
						return null;
					}

					throw ErrorUtilities.ThrowProtocol("Bad access token");
				} else {
					throw ErrorUtilities.ThrowProtocol("Missing access token.");
				}
			} catch (ProtocolException ex) {
				var response = new UnauthorizedResponse(request, ex);

				username = null;
				scope = null;
				return this.Channel.PrepareResponse(response);
			}
		}

		/// <summary>
		/// Discovers what access the client should have considering the access token in the current request.
		/// </summary>
		/// <param name="httpRequestInfo">The HTTP request info.</param>
		/// <param name="principal">The principal that contains the user and roles that the access token is authorized for.</param>
		/// <returns>
		/// An error to return to the client if access is not authorized; <c>null</c> if access is granted.
		/// </returns>
		public virtual OutgoingWebResponse VerifyAccess(HttpRequestInfo httpRequestInfo, out IPrincipal principal) {
			string username, scope;
			var result = this.VerifyAccess(httpRequestInfo, out username, out scope);
			if (result == null) {
				principal = new OAuth.ChannelElements.OAuthPrincipal(username, scope != null ? scope.Split(' ') : new string[0]);
			} else {
				principal = null;
			}

			return result;
		}

		/// <summary>
		/// Discovers what access the client should have considering the access token in the current request.
		/// </summary>
		/// <param name="request">HTTP details from an incoming WCF message.</param>
		/// <param name="requestUri">The URI of the WCF service endpoint.</param>
		/// <param name="principal">The principal that contains the user and roles that the access token is authorized for.</param>
		/// <returns>
		/// An error to return to the client if access is not authorized; <c>null</c> if access is granted.
		/// </returns>
		public virtual OutgoingWebResponse VerifyAccess(HttpRequestMessageProperty request, Uri requestUri, out IPrincipal principal) {
			Contract.Requires<ArgumentNullException>(request != null, "request");
			Contract.Requires<ArgumentNullException>(requestUri != null, "requestUri");

			return this.VerifyAccess(new HttpRequestInfo(request, requestUri), out principal);
		}
	}
}
