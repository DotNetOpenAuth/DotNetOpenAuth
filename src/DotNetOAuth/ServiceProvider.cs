//-----------------------------------------------------------------------
// <copyright file="ServiceProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.ServiceModel.Channels;
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
		public ServiceProvider(ServiceProviderDescription serviceDescription, ITokenManager tokenManager)
			: this(serviceDescription, tokenManager, new OAuthServiceProviderMessageTypeProvider(tokenManager)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProvider"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior on the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		/// <param name="messageTypeProvider">An object that can figure out what type of message is being received for deserialization.</param>
		public ServiceProvider(ServiceProviderDescription serviceDescription, ITokenManager tokenManager, OAuthServiceProviderMessageTypeProvider messageTypeProvider) {
			if (serviceDescription == null) {
				throw new ArgumentNullException("serviceDescription");
			}
			if (tokenManager == null) {
				throw new ArgumentNullException("tokenManager");
			}
			if (messageTypeProvider == null) {
				throw new ArgumentNullException("messageTypeProvider");
			}

			var signingElement = serviceDescription.CreateTamperProtectionElement();
			INonceStore store = new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge);
			this.ServiceDescription = serviceDescription;
			this.Channel = new OAuthChannel(signingElement, store, tokenManager, messageTypeProvider, new StandardWebRequestHandler());
			this.TokenGenerator = new StandardTokenGenerator();
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
		public ITokenManager TokenManager {
			get { return this.Channel.TokenManager; }
		}

		/// <summary>
		/// Gets or sets the channel to use for sending/receiving messages.
		/// </summary>
		internal OAuthChannel Channel { get; set; }

		/// <summary>
		/// Reads any incoming OAuth message.
		/// </summary>
		/// <returns>The deserialized message.</returns>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public IOAuthDirectedMessage ReadRequest() {
			return (IOAuthDirectedMessage)this.Channel.ReadFromRequest();
		}

		/// <summary>
		/// Reads any incoming OAuth message.
		/// </summary>
		/// <param name="request">The HTTP request to read the message from.</param>
		/// <returns>The deserialized message.</returns>
		public IOAuthDirectedMessage ReadRequest(HttpRequest request) {
			return (IOAuthDirectedMessage)this.Channel.ReadFromRequest(new HttpRequestInfo(request));
		}

		/// <summary>
		/// Gets the incoming request for an unauthorized token, if any.
		/// </summary>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public GetRequestTokenMessage ReadTokenRequest() {
			return this.ReadTokenRequest(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Gets the incoming request for an unauthorized token, if any.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public GetRequestTokenMessage ReadTokenRequest(HttpRequest request) {
			return this.ReadTokenRequest(new HttpRequestInfo(request));
		}

		/// <summary>
		/// Sends an unauthorized token back to the Consumer for use in a user agent redirect
		/// for subsequent authorization.
		/// </summary>
		/// <param name="request">The token request message the Consumer sent that the Service Provider is now responding to.</param>
		/// <param name="extraParameters">Any extra parameters the Consumer should receive with the OAuth message.  May be null.</param>
		/// <returns>The actual response the Service Provider will need to forward as the HTTP response.</returns>
		public Response SendUnauthorizedTokenResponse(GetRequestTokenMessage request, IDictionary<string, string> extraParameters) {
			string token = this.TokenGenerator.GenerateRequestToken(request.ConsumerKey);
			string secret = this.TokenGenerator.GenerateSecret();
			GrantRequestTokenMessage response = new GrantRequestTokenMessage {
				RequestToken = token,
				TokenSecret = secret,
			};
			response.AddNonOAuthParameters(extraParameters);
			this.TokenManager.StoreNewRequestToken(request, response);

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
		public GetAccessTokenMessage ReadAccessTokenRequest() {
			return this.ReadAccessTokenRequest(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Gets the incoming request to exchange an authorized token for an access token.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public GetAccessTokenMessage ReadAccessTokenRequest(HttpRequest request) {
			return this.ReadAccessTokenRequest(new HttpRequestInfo(request));
		}

		/// <summary>
		/// Prepares and sends an access token to a Consumer, and invalidates the request token.
		/// </summary>
		/// <param name="request">The Consumer's message requesting an access token.</param>
		/// <param name="extraParameters">Any extra parameters the Consumer should receive with the OAuth message.  May be null.</param>
		/// <returns>The HTTP response to actually send to the Consumer.</returns>
		public Response SendAccessToken(GetAccessTokenMessage request, IDictionary<string, string> extraParameters) {
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
		public AccessProtectedResourceMessage GetProtectedResourceAuthorization() {
			return this.GetProtectedResourceAuthorization(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Gets the authorization (access token) for accessing some protected resource.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The authorization message sent by the Consumer, or null if no authorization message is attached.</returns>
		/// <remarks>
		/// This method verifies that the access token and token secret are valid.
		/// It falls on the caller to verify that the access token is actually authorized
		/// to access the resources being requested.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if an unexpected message is attached to the request.</exception>
		public AccessProtectedResourceMessage GetProtectedResourceAuthorization(HttpRequest request) {
			return this.GetProtectedResourceAuthorization(new HttpRequestInfo(request));
		}

		/// <summary>
		/// Gets the authorization (access token) for accessing some protected resource.
		/// </summary>
		/// <param name="request">HTTP details from an incoming WCF message.</param>
		/// <param name="requestUri">The URI of the WCF service endpoint.</param>
		/// <returns>The authorization message sent by the Consumer, or null if no authorization message is attached.</returns>
		/// <remarks>
		/// This method verifies that the access token and token secret are valid.
		/// It falls on the caller to verify that the access token is actually authorized
		/// to access the resources being requested.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if an unexpected message is attached to the request.</exception>
		public AccessProtectedResourceMessage GetProtectedResourceAuthorization(HttpRequestMessageProperty request, Uri requestUri) {
			return this.GetProtectedResourceAuthorization(new HttpRequestInfo(request, requestUri));
		}

		/// <summary>
		/// Reads a request for an unauthorized token from the incoming HTTP request.
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		internal GetRequestTokenMessage ReadTokenRequest(HttpRequestInfo request) {
			GetRequestTokenMessage message;
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
		internal GetAccessTokenMessage ReadAccessTokenRequest(HttpRequestInfo request) {
			GetAccessTokenMessage message;
			this.Channel.TryReadFromRequest(request, out message);
			return message;
		}

		/// <summary>
		/// Gets the authorization (access token) for accessing some protected resource.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The authorization message sent by the Consumer, or null if no authorization message is attached.</returns>
		/// <remarks>
		/// This method verifies that the access token and token secret are valid.
		/// It falls on the caller to verify that the access token is actually authorized
		/// to access the resources being requested.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if an unexpected message is attached to the request.</exception>
		internal AccessProtectedResourceMessage GetProtectedResourceAuthorization(HttpRequestInfo request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			AccessProtectedResourceMessage accessMessage;
			if (this.Channel.TryReadFromRequest<AccessProtectedResourceMessage>(request, out accessMessage)) {
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
	}
}
