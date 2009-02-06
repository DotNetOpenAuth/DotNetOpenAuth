//-----------------------------------------------------------------------
// <copyright file="ServiceProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.ServiceModel.Channels;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

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
	public class ServiceProvider : IDisposable {
		/// <summary>
		/// The field behind the <see cref="OAuthChannel"/> property.
		/// </summary>
		private OAuthChannel channel;

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProvider"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior on the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		public ServiceProvider(ServiceProviderDescription serviceDescription, ITokenManager tokenManager)
			: this(serviceDescription, tokenManager, new OAuthServiceProviderMessageFactory(tokenManager)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProvider"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior on the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		/// <param name="messageTypeProvider">An object that can figure out what type of message is being received for deserialization.</param>
		public ServiceProvider(ServiceProviderDescription serviceDescription, ITokenManager tokenManager, OAuthServiceProviderMessageFactory messageTypeProvider) {
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
			this.OAuthChannel = new OAuthChannel(signingElement, store, tokenManager, messageTypeProvider);
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
			get { return this.OAuthChannel.TokenManager; }
		}

		/// <summary>
		/// Gets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel {
			get { return this.OAuthChannel; }
		}

		/// <summary>
		/// Gets or sets the channel to use for sending/receiving messages.
		/// </summary>
		internal OAuthChannel OAuthChannel {
			get {
				return this.channel;
			}

			set {
				if (this.channel != null) {
					this.channel.Sending -= this.OAuthChannel_Sending;
				}

				this.channel = value;

				if (this.channel != null) {
					this.channel.Sending += this.OAuthChannel_Sending;
				}
			}
		}

		/// <summary>
		/// Reads any incoming OAuth message.
		/// </summary>
		/// <returns>The deserialized message.</returns>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public IDirectedProtocolMessage ReadRequest() {
			return (IDirectedProtocolMessage)this.Channel.ReadFromRequest();
		}

		/// <summary>
		/// Reads any incoming OAuth message.
		/// </summary>
		/// <param name="request">The HTTP request to read the message from.</param>
		/// <returns>The deserialized message.</returns>
		public IDirectedProtocolMessage ReadRequest(HttpRequest request) {
			return this.Channel.ReadFromRequest(new HttpRequestInfo(request));
		}

		/// <summary>
		/// Gets the incoming request for an unauthorized token, if any.
		/// </summary>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public UnauthorizedTokenRequest ReadTokenRequest() {
			return this.ReadTokenRequest(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Gets the incoming request for an unauthorized token, if any.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public UnauthorizedTokenRequest ReadTokenRequest(HttpRequest request) {
			return this.ReadTokenRequest(new HttpRequestInfo(request));
		}

		/// <summary>
		/// Reads a request for an unauthorized token from the incoming HTTP request.
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public UnauthorizedTokenRequest ReadTokenRequest(HttpRequestInfo request) {
			UnauthorizedTokenRequest message;
			this.Channel.TryReadFromRequest(request, out message);
			return message;
		}

		/// <summary>
		/// Prepares a message containing an unauthorized token for the Consumer to use in a 
		/// user agent redirect for subsequent authorization.
		/// </summary>
		/// <param name="request">The token request message the Consumer sent that the Service Provider is now responding to.</param>
		/// <returns>The response message to send using the <see cref="Channel"/>, after optionally adding extra data to it.</returns>
		public UnauthorizedTokenResponse PrepareUnauthorizedTokenMessage(UnauthorizedTokenRequest request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			string token = this.TokenGenerator.GenerateRequestToken(request.ConsumerKey);
			string secret = this.TokenGenerator.GenerateSecret();
			UnauthorizedTokenResponse response = new UnauthorizedTokenResponse(request, token, secret);

			return response;
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
		public UserAuthorizationRequest ReadAuthorizationRequest() {
			return this.ReadAuthorizationRequest(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Gets the incoming request for the Service Provider to authorize a Consumer's
		/// access to some protected resources.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public UserAuthorizationRequest ReadAuthorizationRequest(HttpRequest request) {
			return this.ReadAuthorizationRequest(new HttpRequestInfo(request));
		}

		/// <summary>
		/// Reads in a Consumer's request for the Service Provider to obtain permission from
		/// the user to authorize the Consumer's access of some protected resource(s).
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public UserAuthorizationRequest ReadAuthorizationRequest(HttpRequestInfo request) {
			UserAuthorizationRequest message;
			this.Channel.TryReadFromRequest(request, out message);
			return message;
		}

		/// <summary>
		/// Prepares the message to send back to the consumer following proper authorization of
		/// a token by an interactive user at the Service Provider's web site.
		/// </summary>
		/// <param name="request">The Consumer's original authorization request.</param>
		/// <returns>
		/// The message to send to the Consumer using <see cref="Channel"/> if one is necessary.
		/// Null if the Consumer did not request a callback.
		/// </returns>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Consistent user experience with instance.")]
		public UserAuthorizationResponse PrepareAuthorizationResponse(UserAuthorizationRequest request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			if (request.Callback != null) {
				var authorization = new UserAuthorizationResponse(request.Callback) {
					RequestToken = request.RequestToken,
				};
				return authorization;
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
		public AuthorizedTokenRequest ReadAccessTokenRequest() {
			return this.ReadAccessTokenRequest(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Gets the incoming request to exchange an authorized token for an access token.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public AuthorizedTokenRequest ReadAccessTokenRequest(HttpRequest request) {
			return this.ReadAccessTokenRequest(new HttpRequestInfo(request));
		}

		/// <summary>
		/// Reads in a Consumer's request to exchange an authorized request token for an access token.
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public AuthorizedTokenRequest ReadAccessTokenRequest(HttpRequestInfo request) {
			AuthorizedTokenRequest message;
			this.Channel.TryReadFromRequest(request, out message);
			return message;
		}

		/// <summary>
		/// Prepares and sends an access token to a Consumer, and invalidates the request token.
		/// </summary>
		/// <param name="request">The Consumer's message requesting an access token.</param>
		/// <returns>The HTTP response to actually send to the Consumer.</returns>
		public AuthorizedTokenResponse PrepareAccessTokenMessage(AuthorizedTokenRequest request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			if (!this.TokenManager.IsRequestTokenAuthorized(request.RequestToken)) {
				throw new ProtocolException(
					string.Format(
						CultureInfo.CurrentCulture,
						OAuthStrings.AccessTokenNotAuthorized,
						request.RequestToken));
			}

			string accessToken = this.TokenGenerator.GenerateAccessToken(request.ConsumerKey);
			string tokenSecret = this.TokenGenerator.GenerateSecret();
			this.TokenManager.ExpireRequestTokenAndStoreNewAccessToken(request.ConsumerKey, request.RequestToken, accessToken, tokenSecret);
			var grantAccess = new AuthorizedTokenResponse(request) {
				AccessToken = accessToken,
				TokenSecret = tokenSecret,
			};

			return grantAccess;
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
		public AccessProtectedResourceRequest ReadProtectedResourceAuthorization() {
			return this.ReadProtectedResourceAuthorization(this.Channel.GetRequestFromContext());
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
		public AccessProtectedResourceRequest ReadProtectedResourceAuthorization(HttpRequest request) {
			return this.ReadProtectedResourceAuthorization(new HttpRequestInfo(request));
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
		public AccessProtectedResourceRequest ReadProtectedResourceAuthorization(HttpRequestMessageProperty request, Uri requestUri) {
			return this.ReadProtectedResourceAuthorization(new HttpRequestInfo(request, requestUri));
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
		public AccessProtectedResourceRequest ReadProtectedResourceAuthorization(HttpRequestInfo request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			AccessProtectedResourceRequest accessMessage;
			if (this.Channel.TryReadFromRequest<AccessProtectedResourceRequest>(request, out accessMessage)) {
				if (this.TokenManager.GetTokenType(accessMessage.AccessToken) != TokenType.AccessToken) {
					throw new ProtocolException(
						string.Format(
							CultureInfo.CurrentCulture,
							OAuthStrings.BadAccessTokenInProtectedResourceRequest,
							accessMessage.AccessToken));
				}
			}

			return accessMessage;
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				this.Channel.Dispose();
			}
		}

		#endregion

		/// <summary>
		/// Hooks the channel in order to perform some operations on some outgoing messages.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DotNetOpenAuth.Messaging.ChannelEventArgs"/> instance containing the event data.</param>
		private void OAuthChannel_Sending(object sender, ChannelEventArgs e) {
			// Hook to store the token and secret on its way down to the Consumer.
			var grantRequestTokenResponse = e.Message as UnauthorizedTokenResponse;
			if (grantRequestTokenResponse != null) {
				this.TokenManager.StoreNewRequestToken(grantRequestTokenResponse.RequestMessage, grantRequestTokenResponse);
			}
		}
	}
}
