//-----------------------------------------------------------------------
// <copyright file="ResourceServer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Security.Claims;
	using System.Security.Principal;
	using System.ServiceModel.Channels;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using ChannelElements;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using Messages;
	using Messaging;
	using Validation;

	/// <summary>
	/// Provides services for validating OAuth access tokens.
	/// </summary>
	public class ResourceServer {
		/// <summary>
		/// A reusable instance of the scope satisfied checker.
		/// </summary>
		private static readonly IScopeSatisfiedCheck DefaultScopeSatisfiedCheck = new StandardScopeSatisfiedCheck();

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceServer"/> class.
		/// </summary>
		/// <param name="accessTokenAnalyzer">The access token analyzer.</param>
		public ResourceServer(IAccessTokenAnalyzer accessTokenAnalyzer) {
			Requires.NotNull(accessTokenAnalyzer, "accessTokenAnalyzer");

			this.AccessTokenAnalyzer = accessTokenAnalyzer;
			this.Channel = new OAuth2ResourceServerChannel();
			this.ResourceOwnerPrincipalPrefix = string.Empty;
			this.ClientPrincipalPrefix = "client:";
			this.ScopeSatisfiedCheck = DefaultScopeSatisfiedCheck;
		}

		/// <summary>
		/// Gets the access token analyzer.
		/// </summary>
		/// <value>The access token analyzer.</value>
		public IAccessTokenAnalyzer AccessTokenAnalyzer { get; private set; }

		/// <summary>
		/// Gets or sets the service that checks whether a granted set of scopes satisfies a required set of scopes.
		/// </summary>
		public IScopeSatisfiedCheck ScopeSatisfiedCheck { get; set; }

		/// <summary>
		/// Gets or sets the prefix to apply to a resource owner's username when used as the username in an <see cref="IPrincipal"/>.
		/// </summary>
		/// <value>The default value is the empty string.</value>
		public string ResourceOwnerPrincipalPrefix { get; set; }

		/// <summary>
		/// Gets or sets the prefix to apply to a client identifier when used as the username in an <see cref="IPrincipal"/>.
		/// </summary>
		/// <value>The default value is "client:"</value>
		public string ClientPrincipalPrefix { get; set; }

		/// <summary>
		/// Gets the channel.
		/// </summary>
		/// <value>The channel.</value>
		internal OAuth2ResourceServerChannel Channel { get; private set; }

		/// <summary>
		/// Discovers what access the client should have considering the access token in the current request.
		/// </summary>
		/// <param name="httpRequestInfo">The HTTP request info.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="requiredScopes">The set of scopes required to approve this request.</param>
		/// <returns>
		/// The access token describing the authorization the client has.  Never <c>null</c>.
		/// </returns>
		/// <exception cref="ProtocolFaultResponseException">Thrown when the client is not authorized.  This exception should be caught and the
		/// <see cref="ProtocolFaultResponseException.ErrorResponseMessage" /> message should be returned to the client.</exception>
		public virtual Task<AccessToken> GetAccessTokenAsync(HttpRequestBase httpRequestInfo = null, CancellationToken cancellationToken = default(CancellationToken), params string[] requiredScopes) {
			Requires.NotNull(requiredScopes, "requiredScopes");
			RequiresEx.ValidState(this.ScopeSatisfiedCheck != null, Strings.RequiredPropertyNotYetPreset);

			httpRequestInfo = httpRequestInfo ?? this.Channel.GetRequestFromContext();
			return this.GetAccessTokenAsync(httpRequestInfo.AsHttpRequestMessage(), cancellationToken, requiredScopes);
		}

		/// <summary>
		/// Discovers what access the client should have considering the access token in the current request.
		/// </summary>
		/// <param name="requestMessage">The request message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="requiredScopes">The set of scopes required to approve this request.</param>
		/// <returns>
		/// The access token describing the authorization the client has.  Never <c>null</c>.
		/// </returns>
		/// <exception cref="ProtocolFaultResponseException">Thrown when the client is not authorized.  This exception should be caught and the
		/// <see cref="ProtocolFaultResponseException.ErrorResponseMessage" /> message should be returned to the client.</exception>
		public virtual async Task<AccessToken> GetAccessTokenAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken = default(CancellationToken), params string[] requiredScopes) {
			Requires.NotNull(requestMessage, "requestMessage");
			Requires.NotNull(requiredScopes, "requiredScopes");
			RequiresEx.ValidState(this.ScopeSatisfiedCheck != null, Strings.RequiredPropertyNotYetPreset);

			AccessToken accessToken;
			AccessProtectedResourceRequest request = null;
			try {
				request = await this.Channel.TryReadFromRequestAsync<AccessProtectedResourceRequest>(requestMessage, cancellationToken);
				if (request != null) {
					accessToken = this.AccessTokenAnalyzer.DeserializeAccessToken(request, request.AccessToken);
					ErrorUtilities.VerifyHost(accessToken != null, "IAccessTokenAnalyzer.DeserializeAccessToken returned a null result.");
					if (string.IsNullOrEmpty(accessToken.User) && string.IsNullOrEmpty(accessToken.ClientIdentifier)) {
						Logger.OAuth.Error("Access token rejected because both the username and client id properties were null or empty.");
						ErrorUtilities.ThrowProtocol(ResourceServerStrings.InvalidAccessToken);
					}

					var requiredScopesSet = OAuthUtilities.ParseScopeSet(requiredScopes);
					if (!this.ScopeSatisfiedCheck.IsScopeSatisfied(requiredScope: requiredScopesSet, grantedScope: accessToken.Scope)) {
						var response = UnauthorizedResponse.InsufficientScope(request, requiredScopesSet);
						throw new ProtocolFaultResponseException(this.Channel, response);
					}

					return accessToken;
				} else {
					var ex = new ProtocolException(ResourceServerStrings.MissingAccessToken);
					var response = UnauthorizedResponse.InvalidRequest(ex);
					throw new ProtocolFaultResponseException(this.Channel, response, innerException: ex);
				}
			} catch (ProtocolException ex) {
				if (ex is ProtocolFaultResponseException) {
					// This doesn't need to be wrapped again.
					throw;
				}

				var response = request != null ? UnauthorizedResponse.InvalidToken(request, ex) : UnauthorizedResponse.InvalidRequest(ex);
				throw new ProtocolFaultResponseException(this.Channel, response, innerException: ex);
			}
		}

		/// <summary>
		/// Discovers what access the client should have considering the access token in the current request.
		/// </summary>
		/// <param name="httpRequestInfo">The HTTP request info.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="requiredScopes">The set of scopes required to approve this request.</param>
		/// <returns>
		/// The principal that contains the user and roles that the access token is authorized for.  Never <c>null</c>.
		/// </returns>
		/// <exception cref="ProtocolFaultResponseException">Thrown when the client is not authorized.  This exception should be caught and the
		/// <see cref="ProtocolFaultResponseException.ErrorResponseMessage" /> message should be returned to the client.</exception>
		public virtual async Task<IPrincipal> GetPrincipalAsync(HttpRequestBase httpRequestInfo = null, CancellationToken cancellationToken = default(CancellationToken), params string[] requiredScopes) {
			AccessToken accessToken = await this.GetAccessTokenAsync(httpRequestInfo, cancellationToken, requiredScopes);

			// Mitigates attacks on this approach of differentiating clients from resource owners
			// by checking that a username doesn't look suspiciously engineered to appear like the other type.
			ErrorUtilities.VerifyProtocol(accessToken.User == null || string.IsNullOrEmpty(this.ClientPrincipalPrefix) || !accessToken.User.StartsWith(this.ClientPrincipalPrefix, StringComparison.OrdinalIgnoreCase), ResourceServerStrings.ResourceOwnerNameLooksLikeClientIdentifier);
			ErrorUtilities.VerifyProtocol(accessToken.ClientIdentifier == null || string.IsNullOrEmpty(this.ResourceOwnerPrincipalPrefix) || !accessToken.ClientIdentifier.StartsWith(this.ResourceOwnerPrincipalPrefix, StringComparison.OrdinalIgnoreCase), ResourceServerStrings.ClientIdentifierLooksLikeResourceOwnerName);

			string principalUserName = !string.IsNullOrEmpty(accessToken.User)
				? this.ResourceOwnerPrincipalPrefix + accessToken.User
				: this.ClientPrincipalPrefix + accessToken.ClientIdentifier;

			return OAuthPrincipal.CreatePrincipal(principalUserName, accessToken.Scope);
		}

		/// <summary>
		/// Discovers what access the client should have considering the access token in the current request.
		/// </summary>
		/// <param name="request">HTTP details from an incoming WCF message.</param>
		/// <param name="requestUri">The URI of the WCF service endpoint.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="requiredScopes">The set of scopes required to approve this request.</param>
		/// <returns>
		/// The principal that contains the user and roles that the access token is authorized for.  Never <c>null</c>.
		/// </returns>
		/// <exception cref="ProtocolFaultResponseException">Thrown when the client is not authorized.  This exception should be caught and the
		/// <see cref="ProtocolFaultResponseException.ErrorResponseMessage" /> message should be returned to the client.</exception>
		public virtual Task<IPrincipal> GetPrincipalAsync(HttpRequestMessageProperty request, Uri requestUri, CancellationToken cancellationToken = default(CancellationToken), params string[] requiredScopes) {
			Requires.NotNull(request, "request");
			Requires.NotNull(requestUri, "requestUri");

			return this.GetPrincipalAsync(new HttpRequestInfo(request, requestUri), cancellationToken, requiredScopes);
		}

		/// <summary>
		/// Discovers what access the client should have considering the access token in the current request.
		/// </summary>
		/// <param name="request">HTTP details from an incoming HTTP request message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="requiredScopes">The set of scopes required to approve this request.</param>
		/// <returns>
		/// The principal that contains the user and roles that the access token is authorized for.  Never <c>null</c>.
		/// </returns>
		/// <exception cref="ProtocolFaultResponseException">Thrown when the client is not authorized.  This exception should be caught and the
		/// <see cref="ProtocolFaultResponseException.ErrorResponseMessage" /> message should be returned to the client.</exception>
		public Task<IPrincipal> GetPrincipalAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken), params string[] requiredScopes) {
			Requires.NotNull(request, "request");
			return this.GetPrincipalAsync(new HttpRequestInfo(request), cancellationToken, requiredScopes);
		}
	}
}
