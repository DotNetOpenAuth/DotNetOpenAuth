//-----------------------------------------------------------------------
// <copyright file="ResourceServer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net;
	using System.Security.Principal;
	using System.ServiceModel.Channels;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using ChannelElements;
	using DotNetOpenAuth.OAuth.ChannelElements;
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
			Requires.NotNull(accessTokenAnalyzer, "accessTokenAnalyzer");

			this.AccessTokenAnalyzer = accessTokenAnalyzer;
			this.Channel = new OAuth2ResourceServerChannel();
			this.ResourceOwnerPrincipalPrefix = string.Empty;
			this.ClientPrincipalPrefix = "client:";
		}

		/// <summary>
		/// Gets the access token analyzer.
		/// </summary>
		/// <value>The access token analyzer.</value>
		public IAccessTokenAnalyzer AccessTokenAnalyzer { get; private set; }

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
		/// <param name="accessToken">Receives the access token describing the authorization the client has.</param>
		/// <returns>An error to return to the client if access is not authorized; <c>null</c> if access is granted.</returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#", Justification = "Try pattern")]
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "Try pattern")]
		public OutgoingWebResponse VerifyAccess(out AccessToken accessToken) {
			return this.VerifyAccess(this.Channel.GetRequestFromContext(), out accessToken);
		}

		/// <summary>
		/// Discovers what access the client should have considering the access token in the current request.
		/// </summary>
		/// <param name="httpRequestInfo">The HTTP request info.</param>
		/// <param name="accessToken">Receives the access token describing the authorization the client has.</param>
		/// <returns>
		/// An error to return to the client if access is not authorized; <c>null</c> if access is granted.
		/// </returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "Try pattern")]
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "Try pattern")]
		public virtual OutgoingWebResponse VerifyAccess(HttpRequestBase httpRequestInfo, out AccessToken accessToken) {
			Requires.NotNull(httpRequestInfo, "httpRequestInfo");

			AccessProtectedResourceRequest request = null;
			try {
				if (this.Channel.TryReadFromRequest<AccessProtectedResourceRequest>(httpRequestInfo, out request)) {
					accessToken = this.AccessTokenAnalyzer.DeserializeAccessToken(request, request.AccessToken);
					ErrorUtilities.VerifyHost(accessToken != null, "IAccessTokenAnalyzer.DeserializeAccessToken returned a null reslut.");
					if (string.IsNullOrEmpty(accessToken.User) && string.IsNullOrEmpty(accessToken.ClientIdentifier)) {
						Logger.OAuth.Error("Access token rejected because both the username and client id properties were null or empty.");
						ErrorUtilities.ThrowProtocol(ResourceServerStrings.InvalidAccessToken);
					}

					return null;
				} else {
					var response = new UnauthorizedResponse(new ProtocolException(ResourceServerStrings.MissingAccessToken));

					accessToken = null;
					return this.Channel.PrepareResponse(response);
				}
			} catch (ProtocolException ex) {
				var response = request != null ? new UnauthorizedResponse(request, ex) : new UnauthorizedResponse(ex);

				accessToken = null;
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
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "Try pattern")]
		public virtual OutgoingWebResponse VerifyAccess(HttpRequestBase httpRequestInfo, out IPrincipal principal) {
			AccessToken accessToken;
			var result = this.VerifyAccess(httpRequestInfo, out accessToken);
			if (result == null) {
				// Mitigates attacks on this approach of differentiating clients from resource owners
				// by checking that a username doesn't look suspiciously engineered to appear like the other type.
				ErrorUtilities.VerifyProtocol(accessToken.User == null || string.IsNullOrEmpty(this.ClientPrincipalPrefix) || !accessToken.User.StartsWith(this.ClientPrincipalPrefix, StringComparison.OrdinalIgnoreCase), ResourceServerStrings.ResourceOwnerNameLooksLikeClientIdentifier);
				ErrorUtilities.VerifyProtocol(accessToken.ClientIdentifier == null || string.IsNullOrEmpty(this.ResourceOwnerPrincipalPrefix) || !accessToken.ClientIdentifier.StartsWith(this.ResourceOwnerPrincipalPrefix, StringComparison.OrdinalIgnoreCase), ResourceServerStrings.ClientIdentifierLooksLikeResourceOwnerName);

				string principalUserName = !string.IsNullOrEmpty(accessToken.User)
					? this.ResourceOwnerPrincipalPrefix + accessToken.User
					: this.ClientPrincipalPrefix + accessToken.ClientIdentifier;
				string[] principalScope = accessToken.Scope != null ? accessToken.Scope.ToArray() : new string[0];
				principal = new OAuthPrincipal(principalUserName, principalScope);
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
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "Try pattern")]
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "Try pattern")]
		public virtual OutgoingWebResponse VerifyAccess(HttpRequestMessageProperty request, Uri requestUri, out IPrincipal principal) {
			Requires.NotNull(request, "request");
			Requires.NotNull(requestUri, "requestUri");

			return this.VerifyAccess(new HttpRequestInfo(request, requestUri), out principal);
		}
	}
}
