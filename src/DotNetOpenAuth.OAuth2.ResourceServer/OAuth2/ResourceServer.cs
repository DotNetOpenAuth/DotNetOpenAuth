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
		}

		/// <summary>
		/// Gets the access token analyzer.
		/// </summary>
		/// <value>The access token analyzer.</value>
		public IAccessTokenAnalyzer AccessTokenAnalyzer { get; private set; }

		/// <summary>
		/// Gets the channel.
		/// </summary>
		/// <value>The channel.</value>
		internal OAuth2ResourceServerChannel Channel { get; private set; }

		/// <summary>
		/// Discovers what access the client should have considering the access token in the current request.
		/// </summary>
		/// <param name="userName">The name on the account the client has access to.</param>
		/// <param name="scope">The set of operations the client is authorized for.</param>
		/// <returns>An error to return to the client if access is not authorized; <c>null</c> if access is granted.</returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#", Justification = "Try pattern")]
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "Try pattern")]
		public OutgoingWebResponse VerifyAccess(out string userName, out HashSet<string> scope) {
			return this.VerifyAccess(this.Channel.GetRequestFromContext(), out userName, out scope);
		}

		/// <summary>
		/// Discovers what access the client should have considering the access token in the current request.
		/// </summary>
		/// <param name="httpRequestInfo">The HTTP request info.</param>
		/// <param name="userName">The name on the account the client has access to.</param>
		/// <param name="scope">The set of operations the client is authorized for.</param>
		/// <returns>
		/// An error to return to the client if access is not authorized; <c>null</c> if access is granted.
		/// </returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "Try pattern")]
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "Try pattern")]
		public virtual OutgoingWebResponse VerifyAccess(HttpRequestInfo httpRequestInfo, out string userName, out HashSet<string> scope) {
			Requires.NotNull(httpRequestInfo, "httpRequestInfo");

			AccessProtectedResourceRequest request = null;
			try {
				if (this.Channel.TryReadFromRequest<AccessProtectedResourceRequest>(httpRequestInfo, out request)) {
					if (this.AccessTokenAnalyzer.TryValidateAccessToken(request, request.AccessToken, out userName, out scope)) {
						// No errors to return.
						return null;
					}

					throw ErrorUtilities.ThrowProtocol(OAuth2Strings.InvalidAccessToken);
				} else {
					var response = new UnauthorizedResponse(new ProtocolException(OAuth2Strings.MissingAccessToken));

					userName = null;
					scope = null;
					return this.Channel.PrepareResponse(response);
				}
			} catch (ProtocolException ex) {
				var response = request != null ? new UnauthorizedResponse(request, ex) : new UnauthorizedResponse(ex);

				userName = null;
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
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "Try pattern")]
		public virtual OutgoingWebResponse VerifyAccess(HttpRequestInfo httpRequestInfo, out IPrincipal principal) {
			string username;
			HashSet<string> scope;
			var result = this.VerifyAccess(httpRequestInfo, out username, out scope);
			principal = result == null ? new OAuthPrincipal(username, scope != null ? scope.ToArray() : new string[0]) : null;
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
