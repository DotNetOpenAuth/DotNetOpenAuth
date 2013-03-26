//-----------------------------------------------------------------------
// <copyright file="Consumer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Security.Cryptography.X509Certificates;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using Validation;

	/// <summary>
	/// Provides OAuth 1.0 consumer services to a client or web application.
	/// </summary>
	public class Consumer {
		/// <summary>
		/// The host factories.
		/// </summary>
		private IHostFactories hostFactories;

		/// <summary>
		/// Initializes a new instance of the <see cref="Consumer"/> class.
		/// </summary>
		public Consumer() {
			this.HostFactories = new DefaultOAuthHostFactories();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Consumer" /> class.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		/// <param name="serviceProvider">The service provider.</param>
		/// <param name="temporaryCredentialStorage">The temporary credential storage.</param>
		/// <param name="hostFactories">The host factories.</param>
		public Consumer(
			string consumerKey,
			string consumerSecret,
			ServiceProviderDescription serviceProvider,
			ITemporaryCredentialStorage temporaryCredentialStorage,
			IHostFactories hostFactories = null) {
			this.ConsumerKey = consumerKey;
			this.ConsumerSecret = consumerSecret;
			this.ServiceProvider = serviceProvider;
			this.TemporaryCredentialStorage = temporaryCredentialStorage;
			this.HostFactories = hostFactories ?? new DefaultOAuthHostFactories();
		}

		/// <summary>
		/// Gets or sets the object with factories for host-customizable services.
		/// </summary>
		public IHostFactories HostFactories {
			get {
				return this.hostFactories;
			}

			set {
				Requires.NotNull(value, "value");
				this.hostFactories = value;
			}
		}

		/// <summary>
		/// Gets or sets the Consumer Key used to communicate with the Service Provider.
		/// </summary>
		public string ConsumerKey { get; set; }

		/// <summary>
		/// Gets or sets the consumer secret.
		/// </summary>
		/// <value>
		/// The consumer secret.
		/// </value>
		public string ConsumerSecret { get; set; }

		/// <summary>
		/// Gets or sets the consumer certificate.
		/// </summary>
		/// <value>
		/// The consumer certificate.
		/// </value>
		/// <remarks>
		/// If set, this causes all outgoing messages to be signed with the certificate instead of the consumer secret.
		/// </remarks> 
		public X509Certificate2 ConsumerCertificate { get; set; }

		/// <summary>
		/// Gets or sets the Service Provider that will be accessed.
		/// </summary>
		public ServiceProviderDescription ServiceProvider { get; set; }

		/// <summary>
		/// Gets or sets the persistence store for tokens and secrets.
		/// </summary>
		public ITemporaryCredentialStorage TemporaryCredentialStorage { get; set; }

		/// <summary>
		/// Obtains an access token for a new account at the Service Provider via 2-legged OAuth.
		/// </summary>
		/// <param name="requestParameters">Any applicable parameters to include in the query string of the token request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The access token.</returns>
		public async Task<AccessTokenResponse> RequestNewClientAccountAsync(IEnumerable<KeyValuePair<string, string>> requestParameters = null, CancellationToken cancellationToken = default(CancellationToken)) {
			Verify.Operation(this.ConsumerKey != null, Strings.RequiredPropertyNotYetPreset, "ConsumerKey");
			Verify.Operation(this.ServiceProvider != null, Strings.RequiredPropertyNotYetPreset, "ServiceProvider");

			using (var handler = this.CreateMessageHandler()) {
				using (var client = this.CreateHttpClient(handler)) {
					string identifier, secret;

					var requestUri = new UriBuilder(this.ServiceProvider.TemporaryCredentialsRequestEndpoint);
					requestUri.AppendQueryArgument(Protocol.CallbackParameter, "oob");
					requestUri.AppendQueryArgs(requestParameters);
					var request = new HttpRequestMessage(this.ServiceProvider.TemporaryCredentialsRequestEndpointMethod, requestUri.Uri);
					using (var response = await client.SendAsync(request, cancellationToken)) {
						response.EnsureSuccessStatusCode();
						cancellationToken.ThrowIfCancellationRequested();

						// Parse the response and ensure that it meets the requirements of the OAuth 1.0 spec.
						string content = await response.Content.ReadAsStringAsync();
						var responseData = HttpUtility.ParseQueryString(content);
						identifier = responseData[Protocol.TokenParameter];
						secret = responseData[Protocol.TokenSecretParameter];
						ErrorUtilities.VerifyProtocol(!string.IsNullOrEmpty(identifier), MessagingStrings.RequiredParametersMissing, typeof(UnauthorizedTokenResponse).Name, Protocol.TokenParameter);
						ErrorUtilities.VerifyProtocol(secret != null, MessagingStrings.RequiredParametersMissing, typeof(UnauthorizedTokenResponse).Name, Protocol.TokenSecretParameter);
					}

					// Immediately exchange the temporary credential for an access token.
					handler.AccessToken = identifier;
					handler.AccessTokenSecret = secret;
					request = new HttpRequestMessage(this.ServiceProvider.TokenRequestEndpointMethod, this.ServiceProvider.TokenRequestEndpoint);
					using (var response = await client.SendAsync(request, cancellationToken)) {
						response.EnsureSuccessStatusCode();

						// Parse the response and ensure that it meets the requirements of the OAuth 1.0 spec.
						string content = await response.Content.ReadAsStringAsync();
						var responseData = HttpUtility.ParseQueryString(content);
						string accessToken = responseData[Protocol.TokenParameter];
						string tokenSecret = responseData[Protocol.TokenSecretParameter];
						ErrorUtilities.VerifyProtocol(!string.IsNullOrEmpty(accessToken), MessagingStrings.RequiredParametersMissing, typeof(AuthorizedTokenResponse).Name, Protocol.TokenParameter);
						ErrorUtilities.VerifyProtocol(tokenSecret != null, MessagingStrings.RequiredParametersMissing, typeof(AuthorizedTokenResponse).Name, Protocol.TokenSecretParameter);

						responseData.Remove(Protocol.TokenParameter);
						responseData.Remove(Protocol.TokenSecretParameter);
						return new AccessTokenResponse(accessToken, tokenSecret, responseData);
					}
				}
			}
		}

		/// <summary>
		/// Prepares an OAuth message that begins an authorization request that will
		/// redirect the user to the Service Provider to provide that authorization.
		/// </summary>
		/// <param name="callback">The absolute URI that the Service Provider should redirect the
		/// User Agent to upon successful authorization, or <c>null</c> to signify an out of band return.</param>
		/// <param name="requestParameters">Extra parameters to add to the request token message.  Optional.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The URL to direct the user agent to for user authorization.
		/// </returns>
		public async Task<Uri> RequestUserAuthorizationAsync(Uri callback = null, IEnumerable<KeyValuePair<string, string>> requestParameters = null, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(callback, "callback");
			Verify.Operation(this.ConsumerKey != null, Strings.RequiredPropertyNotYetPreset, "ConsumerKey");
			Verify.Operation(this.TemporaryCredentialStorage != null, Strings.RequiredPropertyNotYetPreset, "TemporaryCredentialStorage");
			Verify.Operation(this.ServiceProvider != null, Strings.RequiredPropertyNotYetPreset, "ServiceProvider");

			// Obtain temporary credentials before the redirect.
			using (var client = this.CreateHttpClient(new AccessToken())) {
				var requestUri = new UriBuilder(this.ServiceProvider.TemporaryCredentialsRequestEndpoint);
				requestUri.AppendQueryArgument(Protocol.CallbackParameter, callback != null ? callback.AbsoluteUri : "oob");
				requestUri.AppendQueryArgs(requestParameters);
				var request = new HttpRequestMessage(this.ServiceProvider.TemporaryCredentialsRequestEndpointMethod, requestUri.Uri);
				using (var response = await client.SendAsync(request, cancellationToken)) {
					response.EnsureSuccessStatusCode();
					cancellationToken.ThrowIfCancellationRequested();

					// Parse the response and ensure that it meets the requirements of the OAuth 1.0 spec.
					string content = await response.Content.ReadAsStringAsync();
					var responseData = HttpUtility.ParseQueryString(content);
					ErrorUtilities.VerifyProtocol(string.Equals(responseData[Protocol.CallbackConfirmedParameter], "true", StringComparison.Ordinal), MessagingStrings.RequiredParametersMissing, typeof(UnauthorizedTokenResponse).Name, Protocol.CallbackConfirmedParameter);
					string identifier = responseData[Protocol.TokenParameter];
					string secret = responseData[Protocol.TokenSecretParameter];
					ErrorUtilities.VerifyProtocol(!string.IsNullOrEmpty(identifier), MessagingStrings.RequiredParametersMissing, typeof(UnauthorizedTokenResponse).Name, Protocol.TokenParameter);
					ErrorUtilities.VerifyProtocol(secret != null, MessagingStrings.RequiredParametersMissing, typeof(UnauthorizedTokenResponse).Name, Protocol.TokenSecretParameter);

					// Save the temporary credential we received so that after user authorization
					// we can use it to obtain the access token.
					cancellationToken.ThrowIfCancellationRequested();
					this.TemporaryCredentialStorage.SaveTemporaryCredential(identifier, secret);

					// Construct the user authorization URL so our caller can direct a browser to it.
					var authorizationEndpoint = new UriBuilder(this.ServiceProvider.ResourceOwnerAuthorizationEndpoint);
					authorizationEndpoint.AppendQueryArgument(Protocol.TokenParameter, identifier);
					return authorizationEndpoint.Uri;
				}
			}
		}

		/// <summary>
		/// Obtains an access token after a successful user authorization.
		/// </summary>
		/// <param name="authorizationCompleteUri">The URI used to redirect back to the consumer that contains a message from the service provider.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The access token assigned by the Service Provider, or <c>null</c> if no response was detected in the specified URL.
		/// </returns>
		public async Task<AccessTokenResponse> ProcessUserAuthorizationAsync(Uri authorizationCompleteUri, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(authorizationCompleteUri, "authorizationCompleteUri");
			Verify.Operation(this.TemporaryCredentialStorage != null, Strings.RequiredPropertyNotYetPreset, "TemporaryCredentialStorage");

			// Parse the response and verify that it meets spec requirements.
			var queryString = HttpUtility.ParseQueryString(authorizationCompleteUri.Query);
			string identifier = queryString[Protocol.TokenParameter];
			string verifier = queryString[Protocol.VerifierParameter];

			if (identifier == null) {
				// We assume there is no response message here at all, and return null to indicate that.
				return null;
			}

			ErrorUtilities.VerifyProtocol(!string.IsNullOrEmpty(identifier), MessagingStrings.RequiredNonEmptyParameterWasEmpty, typeof(UserAuthorizationResponse).Name, Protocol.TokenParameter);
			ErrorUtilities.VerifyProtocol(!string.IsNullOrEmpty(verifier), MessagingStrings.RequiredNonEmptyParameterWasEmpty, typeof(UserAuthorizationResponse).Name, Protocol.VerifierParameter);

			var temporaryCredential = this.TemporaryCredentialStorage.RetrieveTemporaryCredential();
			Verify.Operation(string.Equals(temporaryCredential.Key, identifier, StringComparison.Ordinal), "Temporary credential identifiers do not match.");

			return await this.ProcessUserAuthorizationAsync(verifier, cancellationToken);
		}

		/// <summary>
		/// Obtains an access token after a successful user authorization.
		/// </summary>
		/// <param name="verifier">The verifier.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The access token assigned by the Service Provider.
		/// </returns>
		public async Task<AccessTokenResponse> ProcessUserAuthorizationAsync(string verifier, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(verifier, "verifier");
			Verify.Operation(this.ConsumerKey != null, Strings.RequiredPropertyNotYetPreset, "ConsumerKey");
			Verify.Operation(this.TemporaryCredentialStorage != null, Strings.RequiredPropertyNotYetPreset, "TemporaryCredentialStorage");
			Verify.Operation(this.ServiceProvider != null, Strings.RequiredPropertyNotYetPreset, "ServiceProvider");

			var temporaryCredential = this.TemporaryCredentialStorage.RetrieveTemporaryCredential();

			using (var client = this.CreateHttpClient(new AccessToken(temporaryCredential.Key, temporaryCredential.Value))) {
				var requestUri = new UriBuilder(this.ServiceProvider.TokenRequestEndpoint);
				requestUri.AppendQueryArgument(Protocol.VerifierParameter, verifier);
				var request = new HttpRequestMessage(this.ServiceProvider.TokenRequestEndpointMethod, requestUri.Uri);
				using (var response = await client.SendAsync(request, cancellationToken)) {
					response.EnsureSuccessStatusCode();

					// Parse the response and ensure that it meets the requirements of the OAuth 1.0 spec.
					string content = await response.Content.ReadAsStringAsync();
					var responseData = HttpUtility.ParseQueryString(content);
					string accessToken = responseData[Protocol.TokenParameter];
					string tokenSecret = responseData[Protocol.TokenSecretParameter];
					ErrorUtilities.VerifyProtocol(!string.IsNullOrEmpty(accessToken), MessagingStrings.RequiredParametersMissing, typeof(AuthorizedTokenResponse).Name, Protocol.TokenParameter);
					ErrorUtilities.VerifyProtocol(tokenSecret != null, MessagingStrings.RequiredParametersMissing, typeof(AuthorizedTokenResponse).Name, Protocol.TokenSecretParameter);

					responseData.Remove(Protocol.TokenParameter);
					responseData.Remove(Protocol.TokenSecretParameter);
					return new AccessTokenResponse(accessToken, tokenSecret, responseData);
				}
			}
		}

		/// <summary>
		/// Creates a message handler that signs outbound requests with a previously obtained authorization.
		/// </summary>
		/// <param name="accessToken">The access token to authorize outbound HTTP requests with.</param>
		/// <param name="innerHandler">The inner handler that actually sends the HTTP message on the network.</param>
		/// <returns>
		/// A message handler.
		/// </returns>
		/// <remarks>
		/// Overrides of this method may allow various derived types of handlers to be returned,
		/// enabling consumers that use RSA or other signing methods.
		/// </remarks>
		public virtual OAuth1HttpMessageHandlerBase CreateMessageHandler(AccessToken accessToken = default(AccessToken), HttpMessageHandler innerHandler = null) {
			Verify.Operation(this.ConsumerKey != null, Strings.RequiredPropertyNotYetPreset, "ConsumerKey");

			innerHandler = innerHandler ?? this.HostFactories.CreateHttpMessageHandler();
			OAuth1HttpMessageHandlerBase handler;
			if (this.ConsumerCertificate != null) {
				handler = new OAuth1RsaSha1HttpMessageHandler(innerHandler) {
					SigningCertificate = this.ConsumerCertificate,
				};
			} else {
				handler = new OAuth1HmacSha1HttpMessageHandler(innerHandler);
			}

			handler.ConsumerKey = this.ConsumerKey;
			handler.ConsumerSecret = this.ConsumerSecret;
			handler.AccessToken = accessToken.Token;
			handler.AccessTokenSecret = accessToken.Secret;

			return handler;
		}

		/// <summary>
		/// Creates the HTTP client.
		/// </summary>
		/// <param name="accessToken">The access token to authorize outbound HTTP requests with.</param>
		/// <param name="innerHandler">The inner handler that actually sends the HTTP message on the network.</param>
		/// <returns>The HttpClient to use.</returns>
		public HttpClient CreateHttpClient(AccessToken accessToken, HttpMessageHandler innerHandler = null) {
			var handler = this.CreateMessageHandler(accessToken, innerHandler);
			var client = this.HostFactories.CreateHttpClient(handler);
			return client;
		}

		/// <summary>
		/// Creates the HTTP client.
		/// </summary>
		/// <param name="innerHandler">The inner handler that actually sends the HTTP message on the network.</param>
		/// <returns>The HttpClient to use.</returns>
		public HttpClient CreateHttpClient(OAuth1HttpMessageHandlerBase innerHandler) {
			Requires.NotNull(innerHandler, "innerHandler");

			var client = this.HostFactories.CreateHttpClient(innerHandler);
			return client;
		}
	}
}
