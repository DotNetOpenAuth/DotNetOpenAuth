//-----------------------------------------------------------------------
// <copyright file="BearerTokenHttpMessageHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// An <see cref="HttpMessageHandler"/> that applies a bearer token to each outbound HTTP request.
	/// </summary>
	internal class BearerTokenHttpMessageHandler : DelegatingHandler {
		/// <summary>
		/// Initializes a new instance of the <see cref="BearerTokenHttpMessageHandler" /> class.
		/// </summary>
		/// <param name="bearerToken">The bearer token.</param>
		/// <param name="innerHandler">The inner handler.</param>
		public BearerTokenHttpMessageHandler(string bearerToken, HttpMessageHandler innerHandler)
			: base(innerHandler) {
			Requires.NotNullOrEmpty(bearerToken, "bearerToken");
			this.BearerToken = bearerToken;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BearerTokenHttpMessageHandler" /> class.
		/// </summary>
		/// <param name="client">The client associated with the authorization.</param>
		/// <param name="authorization">The authorization.</param>
		/// <param name="innerHandler">The inner handler.</param>
		public BearerTokenHttpMessageHandler(ClientBase client, IAuthorizationState authorization, HttpMessageHandler innerHandler)
			: base(innerHandler) {
			Requires.NotNull(client, "client");
			Requires.NotNull(authorization, "authorization");
			Requires.That(!string.IsNullOrEmpty(authorization.AccessToken), "authorization.AccessToken", "AccessToken must be non-empty");
			this.Client = client;
			this.Authorization = authorization;
		}

		/// <summary>
		/// Gets the bearer token.
		/// </summary>
		/// <value>
		/// The bearer token.
		/// </value>
		internal string BearerToken { get; private set; }

		/// <summary>
		/// Gets the authorization.
		/// </summary>
		internal IAuthorizationState Authorization { get; private set; }

		/// <summary>
		/// Gets the OAuth 2 client associated with the <see cref="Authorization"/>.
		/// </summary>
		internal ClientBase Client { get; private set; }

		/// <summary>
		/// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
		/// </summary>
		/// <param name="request">The HTTP request message to send to the server.</param>
		/// <param name="cancellationToken">A cancellation token to cancel operation.</param>
		/// <returns>
		/// Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.
		/// </returns>
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			string bearerToken = this.BearerToken;
			if (bearerToken == null) {
				ErrorUtilities.VerifyProtocol(!this.Authorization.AccessTokenExpirationUtc.HasValue || this.Authorization.AccessTokenExpirationUtc >= DateTime.UtcNow || this.Authorization.RefreshToken != null, ClientStrings.AuthorizationExpired);

				if (this.Authorization.AccessTokenExpirationUtc.HasValue && this.Authorization.AccessTokenExpirationUtc.Value < DateTime.UtcNow) {
					ErrorUtilities.VerifyProtocol(this.Authorization.RefreshToken != null, ClientStrings.AccessTokenRefreshFailed);
					await this.Client.RefreshAuthorizationAsync(this.Authorization, cancellationToken: cancellationToken);
				}

				bearerToken = this.Authorization.AccessToken;
			}

			request.Headers.Authorization = new AuthenticationHeaderValue(Protocol.BearerHttpAuthorizationScheme, bearerToken);
			return await base.SendAsync(request, cancellationToken);
		}
	}
}
