//-----------------------------------------------------------------------
// <copyright file="ResourceServer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net;
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
			this.Channel = new OAuthWrapResourceServerChannel();
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
		/// Gets or sets the channel.
		/// </summary>
		/// <value>The channel.</value>
		internal OAuthWrapResourceServerChannel Channel { get; private set; }

		public OutgoingWebResponse VerifyAccess(out string username, out string scope) {
			return this.VerifyAccess(this.Channel.GetRequestFromContext(), out username, out scope);
		}

		public virtual OutgoingWebResponse VerifyAccess(HttpRequestInfo httpRequestInfo, out string username, out string scope) {
			Contract.Requires<ArgumentNullException>(httpRequestInfo != null, "httpRequestInfo");

			try {
				AccessProtectedResourceRequest request;
				if (this.Channel.TryReadFromRequest<AccessProtectedResourceRequest>(httpRequestInfo, out request)) {
					if (this.AccessTokenAnalyzer.TryValidateAccessToken(request.AccessToken, out username, out scope)) {
						// No errors to return.
						return null;
					}

					throw ErrorUtilities.ThrowProtocol("Bad access token");
				} else {
					throw ErrorUtilities.ThrowProtocol("Missing access token.");
				}
			} catch (ProtocolException ex) {
				var unauthorizedError = new OutgoingWebResponse {
					Status = HttpStatusCode.Unauthorized,
				};

				var authenticate = new StringBuilder();
				authenticate.Append("Token ");
				authenticate.AppendFormat("realm='{0}'", "Service");
				authenticate.Append(",");
				authenticate.AppendFormat("error=\"{0}\"", ex.Message);
				unauthorizedError.Headers.Add(HttpResponseHeader.WwwAuthenticate, authenticate.ToString());

				username = null;
				scope = null;
				return unauthorizedError;
			}
		}
	}
}
