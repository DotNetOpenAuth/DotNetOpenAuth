//-----------------------------------------------------------------------
// <copyright file="ServiceProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
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
		/// <param name="serviceDescription">The endpoints and behavior on the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		public ServiceProvider(ServiceProviderDescription serviceDescription, ITokenManager tokenManager) {
			if (serviceDescription == null) {
				throw new ArgumentNullException("serviceDescription");
			}
			if (tokenManager == null) {
				throw new ArgumentNullException("tokenManager");
			}

			var signingElement = serviceDescription.CreateTamperProtectionElement();
			signingElement.SignatureVerificationCallback = this.TokenSignatureVerificationCallback;
			INonceStore store = new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge);
			this.ServiceDescription = serviceDescription;
			this.Channel = new OAuthChannel(signingElement, store, tokenManager, false);
			this.TokenGenerator = new StandardTokenGenerator();
			this.TokenManager = tokenManager;
		}

		/// <summary>
		/// Gets the description of this Service Provider.
		/// </summary>
		public ServiceProviderDescription ServiceDescription { get; private set; }

		/// <summary>
		/// Gets or sets the generator responsible for generating new tokens and secrets.
		/// </summary>
		public ITokenGenerator TokenGenerator { get; set; }

		/// <summary>
		/// Gets the persistence store for tokens and secrets.
		/// </summary>
		public ITokenManager TokenManager { get; private set; }

		/// <summary>
		/// Gets or sets the channel to use for sending/receiving messages.
		/// </summary>
		internal OAuthChannel Channel { get; set; }

		/// <summary>
		/// Gets the incoming request for an unauthorized token, if any.
		/// </summary>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public RequestTokenMessage ReadTokenRequest() {
			return this.Channel.ReadFromRequest<RequestTokenMessage>();
		}

		/// <summary>
		/// Gets the incoming request for an unauthorized token, if any.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public RequestTokenMessage ReadTokenRequest(HttpRequest request) {
			return this.ReadTokenRequest(new HttpRequestInfo(request));
		}

		/// <summary>
		/// Sends an unauthorized token back to the Consumer for use in a user agent redirect
		/// for subsequent authorization.
		/// </summary>
		/// <param name="request">The token request message the Consumer sent that the Service Provider is now responding to.</param>
		/// <param name="extraParameters">Any extra parameters the Consumer should receive with the OAuth message.</param>
		/// <returns>The actual response the Service Provider will need to forward as the HTTP response.</returns>
		public Response SendUnauthorizedTokenResponse(RequestTokenMessage request, IDictionary<string, string> extraParameters) {
			string token = this.TokenGenerator.GenerateRequestToken(request.ConsumerKey);
			string secret = this.TokenGenerator.GenerateSecret();
			this.TokenManager.StoreNewRequestToken(request.ConsumerKey, token, secret, null/*add params*/);
			UnauthorizedRequestTokenMessage response = new UnauthorizedRequestTokenMessage {
				RequestToken = token,
				TokenSecret = secret,
			};
			response.AddNonOAuthParameters(extraParameters);

			return this.Channel.Send(response);
		}

		/// <summary>
		/// Gets the incoming request for the Service Provider to authorize a Consumer's
		/// access to some protected resources.
		/// </summary>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public DirectUserToServiceProviderMessage ReadAuthorizationRequest() {
			return this.ReadAuthorizationRequest(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Gets the incoming request for the Service Provider to authorize a Consumer's
		/// access to some protected resources.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public DirectUserToServiceProviderMessage ReadAuthorizationRequest(HttpRequest request) {
			return this.ReadAuthorizationRequest(new HttpRequestInfo(request));
		}

		/// <summary>
		/// Completes user authorization of a token by redirecting the user agent back to the Consumer.
		/// </summary>
		/// <param name="request">The Consumer's original authorization request.</param>
		/// <returns>
		/// The pending user agent redirect based message to be sent as an HttpResponse,
		/// or null if the Consumer requested no callback.
		/// </returns>
		public Response SendAuthorizationResponse(DirectUserToServiceProviderMessage request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			if (request.Callback != null) {
				var authorization = new DirectUserToConsumerMessage(request.Callback) {
					RequestToken = request.RequestToken,
				};
				return this.Channel.Send(authorization);
			} else {
				return null;
			}
		}

		/// <summary>
		/// Gets the incoming request to exchange an authorized token for an access token.
		/// </summary>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public RequestAccessTokenMessage ReadAccessTokenRequest() {
			return this.ReadAccessTokenRequest(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Gets the incoming request to exchange an authorized token for an access token.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public RequestAccessTokenMessage ReadAccessTokenRequest(HttpRequest request) {
			return this.ReadAccessTokenRequest(new HttpRequestInfo(request));
		}

		/// <summary>
		/// Prepares and sends an access token to a Consumer, and invalidates the request token.
		/// </summary>
		/// <param name="request">The Consumer's message requesting an access token.</param>
		/// <param name="extraParameters">Any extra parameters the Service Provider wishes to send to the Consumer.</param>
		/// <returns>The HTTP response to actually send to the Consumer.</returns>
		public Response SendAccessToken(RequestAccessTokenMessage request, IDictionary<string, string> extraParameters) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

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
			grantAccess.AddNonOAuthParameters(extraParameters);

			return this.Channel.Send(grantAccess);
		}

		/// <summary>
		/// Gets the authorization (access token) for accessing some protected resource.
		/// </summary>
		/// <returns>The authorization message sent by the Consumer, or null if no authorization message is attached.</returns>
		/// <remarks>
		/// This method verifies that the access token and token secret are valid.
		/// It falls on the caller to verify that the access token is actually authorized
		/// to access the resources being requested.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if an unexpected message is attached to the request.</exception>
		public AccessProtectedResourcesMessage GetProtectedResourceAuthorization() {
			AccessProtectedResourcesMessage accessMessage;
			if (this.Channel.TryReadFromRequest<AccessProtectedResourcesMessage>(out accessMessage)) {
				if (this.TokenManager.GetTokenType(accessMessage.AccessToken) != TokenType.AccessToken) {
					throw new ProtocolException(
						string.Format(
							CultureInfo.CurrentCulture,
							Strings.BadAccessTokenInProtectedResourceRequest,
							accessMessage.AccessToken));
				}
			}

			return accessMessage;
		}

		/// <summary>
		/// Reads a request for an unauthorized token from the incoming HTTP request.
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		internal RequestTokenMessage ReadTokenRequest(HttpRequestInfo request) {
			RequestTokenMessage message;
			this.Channel.TryReadFromRequest(request, out message);
			return message;
		}

		/// <summary>
		/// Reads in a Consumer's request for the Service Provider to obtain permission from
		/// the user to authorize the Consumer's access of some protected resource(s).
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		internal DirectUserToServiceProviderMessage ReadAuthorizationRequest(HttpRequestInfo request) {
			DirectUserToServiceProviderMessage message;
			this.Channel.TryReadFromRequest(request, out message);
			return message;
		}

		/// <summary>
		/// Reads in a Consumer's request to exchange an authorized request token for an access token.
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		internal RequestAccessTokenMessage ReadAccessTokenRequest(HttpRequestInfo request) {
			RequestAccessTokenMessage message;
			this.Channel.TryReadFromRequest(request, out message);
			return message;
		}

		/// <summary>
		/// Fills out the secrets in an incoming message so that signature verification can be performed.
		/// </summary>
		/// <param name="message">The incoming message.</param>
		private void TokenSignatureVerificationCallback(ITamperResistantOAuthMessage message) {
			message.ConsumerSecret = this.TokenManager.GetConsumerSecret(message.ConsumerKey);

			var tokenMessage = message as ITokenContainingMessage;
			if (tokenMessage != null) {
				message.TokenSecret = this.TokenManager.GetTokenSecret(tokenMessage.Token);
			}
		}
	}
}
