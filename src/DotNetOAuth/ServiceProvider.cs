//-----------------------------------------------------------------------
// <copyright file="ServiceProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.Globalization;
	using System.Web;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messages;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;

	/// <summary>
	/// A web application that allows access via OAuth.
	/// </summary>
	/// <remarks>
	/// <para>The Service Provider’s documentation should include:</para>
	/// <list>
	/// <item>The URLs (Request URLs) the Consumer will use when making OAuth requests, and the HTTP methods (i.e. GET, POST, etc.) used in the Request Token URL and Access Token URL.</item>
	/// <item>Signature methods supported by the Service Provider.</item>
	/// <item>Any additional request parameters that the Service Provider requires in order to obtain a Token. Service Provider specific parameters MUST NOT begin with oauth_.</item>
	/// </list>
	/// </remarks>
	public class ServiceProvider {
		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProvider"/> class.
		/// </summary>
		/// <param name="endpoints">The endpoints on the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		public ServiceProvider(ServiceProviderEndpoints endpoints, ITokenManager tokenManager) {
			if (endpoints == null) {
				throw new ArgumentNullException("endpoints");
			}
			if (tokenManager == null) {
				throw new ArgumentNullException("tokenManager");
			}

			SigningBindingElementBase signingElement = new PlainTextSigningBindingElement(this.TokenSignatureVerificationCallback);
			INonceStore store = new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge);
			this.Endpoints = endpoints;
			this.Channel = new OAuthChannel(signingElement, store, tokenManager);
			this.TokenGenerator = new StandardTokenGenerator();
			this.TokenManager = tokenManager;
		}

		/// <summary>
		/// Gets the endpoints exposed by this Service Provider.
		/// </summary>
		public ServiceProviderEndpoints Endpoints { get; private set; }

		/// <summary>
		/// Gets the pending user agent redirect based message to be sent as an HttpResponse.
		/// </summary>
		public Response PendingRequest { get; private set; }

		/// <summary>
		/// Gets or sets the channel to use for sending/receiving messages.
		/// </summary>
		internal OAuthChannel Channel { get; set; }

		/// <summary>
		/// Gets or sets the generator responsible for generating new tokens and secrets.
		/// </summary>
		internal ITokenGenerator TokenGenerator { get; set; }

		/// <summary>
		/// Gets the persistence store for tokens and secrets.
		/// </summary>
		internal ITokenManager TokenManager { get; private set; }

		internal RequestTokenMessage ReadTokenRequest() {
			return this.Channel.ReadFromRequest<RequestTokenMessage>();
		}

		internal RequestTokenMessage ReadTokenRequest(HttpRequest request) {
			return this.ReadTokenRequest(new HttpRequestInfo(request));
		}

		internal RequestTokenMessage ReadTokenRequest(HttpRequestInfo request) {
			return this.Channel.ReadFromRequest<RequestTokenMessage>(request);
		}

		internal void SendUnauthorizedTokenResponse(RequestTokenMessage request) {
			string token = this.TokenGenerator.GenerateRequestToken(request.ConsumerKey);
			string secret = this.TokenGenerator.GenerateSecret();
			this.TokenManager.StoreNewRequestToken(request.ConsumerKey, token, secret, null/*add params*/);
			UnauthorizedRequestTokenMessage response = new UnauthorizedRequestTokenMessage {
				RequestToken = token,
				TokenSecret = secret,
			};

			this.Channel.Send(response);
		}

		internal DirectUserToServiceProviderMessage ReadAuthorizationRequest() {
			return this.Channel.ReadFromRequest<DirectUserToServiceProviderMessage>();
		}

		internal DirectUserToServiceProviderMessage ReadAuthorizationRequest(HttpRequest request) {
			return this.ReadAuthorizationRequest(new HttpRequestInfo(request));
		}

		internal DirectUserToServiceProviderMessage ReadAuthorizationRequest(HttpRequestInfo request) {
			return this.Channel.ReadFromRequest<DirectUserToServiceProviderMessage>(request);
		}

		internal void SendAuthorizationResponse(DirectUserToServiceProviderMessage request) {
			var authorization = new DirectUserToConsumerMessage(request.Callback) {
				RequestToken = request.RequestToken,
			};
			this.Channel.Send(authorization);
			this.PendingRequest = this.Channel.DequeueIndirectOrResponseMessage();
		}

		internal RequestAccessTokenMessage ReadAccessTokenRequest() {
			return this.Channel.ReadFromRequest<RequestAccessTokenMessage>();
		}

		internal RequestAccessTokenMessage ReadAccessTokenRequest(HttpRequest request) {
			return this.ReadAccessTokenRequest(new HttpRequestInfo(request));
		}

		internal RequestAccessTokenMessage ReadAccessTokenRequest(HttpRequestInfo request) {
			return this.Channel.ReadFromRequest<RequestAccessTokenMessage>(request);
		}

		internal void SendAccessToken(RequestAccessTokenMessage request) {
			if (!this.TokenManager.IsRequestTokenAuthorized(request.RequestToken)) {
				throw new ProtocolException(
					string.Format(
						CultureInfo.CurrentCulture,
						Strings.AccessTokenNotAuthorized,
						request.RequestToken));
			}

			string accessToken = this.TokenGenerator.GenerateAccessToken(request.ConsumerKey);
			string tokenSecret = this.TokenGenerator.GenerateSecret();
			this.TokenManager.ExpireRequestTokenAndStoreNewAccessToken(request.ConsumerKey, request.RequestToken, accessToken, tokenSecret);
			var grantAccess = new GrantAccessTokenMessage {
				AccessToken = accessToken,
				TokenSecret = tokenSecret,
			};

			this.Channel.Send(grantAccess);
		}

		internal string GetAccessTokenInRequest() {
			var accessMessage = this.Channel.ReadFromRequest<AccessProtectedResourcesMessage>();
			if (this.TokenManager.GetTokenType(accessMessage.AccessToken) != TokenType.AccessToken) {
				throw new ProtocolException(
					string.Format(
						CultureInfo.CurrentCulture,
						Strings.BadAccessTokenInProtectedResourceRequest,
						accessMessage.AccessToken));
			}

			return accessMessage.AccessToken;
		}

		private void TokenSignatureVerificationCallback(ITamperResistantOAuthMessage message) {
			message.ConsumerSecret = this.TokenManager.GetConsumerSecret(message.ConsumerKey);

			var tokenMessage = message as ITokenContainingMessage;
			if (tokenMessage != null) {
				message.TokenSecret = this.TokenManager.GetTokenSecret(tokenMessage.Token);
			}

			// TODO: more complete filling of message properties.
			////message.Recipient = 
			////message.AdditionalParametersInHttpRequest = 
			////message.HttpMethod = 
		}
	}
}
