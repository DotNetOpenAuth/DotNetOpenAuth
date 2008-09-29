//-----------------------------------------------------------------------
// <copyright file="Consumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messages;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;

	/// <summary>
	/// A website or application that uses OAuth to access the Service Provider on behalf of the User.
	/// </summary>
	public class Consumer {
		/// <summary>
		/// Initializes a new instance of the <see cref="Consumer"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior of the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		internal Consumer(ServiceProviderDescription serviceDescription, ITokenManager tokenManager) {
			if (serviceDescription == null) {
				throw new ArgumentNullException("serviceDescription");
			}
			if (tokenManager == null) {
				throw new ArgumentNullException("tokenManager");
			}

			this.WebRequestHandler = new StandardWebRequestHandler();
			ITamperProtectionChannelBindingElement signingElement = serviceDescription.CreateTamperProtectionElement();
			INonceStore store = new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge);
			this.Channel = new OAuthChannel(signingElement, store, new OAuthMessageTypeProvider(tokenManager), this.WebRequestHandler);
			this.ServiceProvider = serviceDescription;
			this.TokenManager = tokenManager;
		}

		/// <summary>
		/// Gets or sets the Consumer Key used to communicate with the Service Provider.
		/// </summary>
		public string ConsumerKey { get; set; }

		/// <summary>
		/// Gets or sets the Consumer Secret used to communicate with the Service Provider.
		/// </summary>
		public string ConsumerSecret { get; set; }

		/// <summary>
		/// Gets the Service Provider that will be accessed.
		/// </summary>
		public ServiceProviderDescription ServiceProvider { get; private set; }

		/// <summary>
		/// Gets the persistence store for tokens and secrets.
		/// </summary>
		public ITokenManager TokenManager { get; private set; }

		/// <summary>
		/// Gets the pending user agent redirect based message to be sent as an HttpResponse.
		/// </summary>
		public Response PendingRequest { get; private set; }

		/// <summary>
		/// Gets or sets the object that processes <see cref="HttpWebRequest"/>s.
		/// </summary>
		/// <remarks>
		/// This defaults to a straightforward implementation, but can be set
		/// to a mock object for testing purposes.
		/// </remarks>
		internal IWebRequestHandler WebRequestHandler { get; set; }

		/// <summary>
		/// Gets or sets the channel to use for sending/receiving messages.
		/// </summary>
		internal OAuthChannel Channel { get; set; }

		/// <summary>
		/// Begins an OAuth authorization request and redirects the user to the Service Provider
		/// to provide that authorization.
		/// </summary>
		/// <param name="callback">
		/// An optional Consumer URL that the Service Provider should redirect the 
		/// User Agent to upon successful authorization.
		/// </param>
		/// <param name="extraParameters">Extra parameters to add to the request token message.  Optional.</param>
		public void RequestUserAuthorization(Uri callback, IDictionary<string, string> extraParameters) {
			// Obtain an unauthorized request token.
			var requestToken = new RequestTokenMessage(this.ServiceProvider.RequestTokenEndpoint) {
				ConsumerKey = this.ConsumerKey,
				ConsumerSecret = this.ConsumerSecret,
			};
			requestToken.AddExtraFields(extraParameters);
			var requestTokenResponse = this.Channel.Request<UnauthorizedRequestTokenMessage>(requestToken);
			this.TokenManager.StoreNewRequestToken(this.ConsumerKey, requestTokenResponse.RequestToken, requestTokenResponse.TokenSecret, null/*TODO*/);

			// Request user authorization.
			var requestAuthorization = new DirectUserToServiceProviderMessage(this.ServiceProvider.UserAuthorizationEndpoint) {
				Callback = callback,
				RequestToken = requestTokenResponse.RequestToken,
			};
			this.Channel.Send(requestAuthorization);
			this.PendingRequest = this.Channel.DequeueIndirectOrResponseMessage();
		}

		/// <summary>
		/// Processes an incoming authorization-granted message from an SP and obtains an access token.
		/// </summary>
		/// <returns>The access token.</returns>
		public string ProcessUserAuthorization() {
			var authorizationMessage = this.Channel.ReadFromRequest<DirectUserToConsumerMessage>();

			// Exchange request token for access token.
			string requestTokenSecret = this.TokenManager.GetTokenSecret(authorizationMessage.RequestToken);
			var requestAccess = new RequestAccessTokenMessage(this.ServiceProvider.AccessTokenEndpoint) {
				RequestToken = authorizationMessage.RequestToken,
				TokenSecret = requestTokenSecret,
				ConsumerKey = this.ConsumerKey,
				ConsumerSecret = this.ConsumerSecret,
			};
			var grantAccess = this.Channel.Request<GrantAccessTokenMessage>(requestAccess);
			this.TokenManager.ExpireRequestTokenAndStoreNewAccessToken(this.ConsumerKey, authorizationMessage.RequestToken, grantAccess.AccessToken, grantAccess.TokenSecret);
			return grantAccess.AccessToken;
		}

		/// <summary>
		/// Creates a web request prepared with OAuth authorization 
		/// that may be further tailored by adding parameters by the caller.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <returns>The initialized WebRequest object.</returns>
		public WebRequest CreateAuthorizedRequest(MessageReceivingEndpoint endpoint, string accessToken) {
			IDirectedProtocolMessage message = this.CreateAuthorizedRequestInternal(endpoint, accessToken);
			WebRequest wr = this.Channel.InitializeRequest(message);
			return wr;
		}

		/// <summary>
		/// Creates a web request prepared with OAuth authorization 
		/// that may be further tailored by adding parameters by the caller.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <returns>The initialized WebRequest object.</returns>
		/// <exception cref="WebException">Thrown if the request fails for any reason after it is sent to the Service Provider.</exception>
		public Response SendAuthorizedRequest(MessageReceivingEndpoint endpoint, string accessToken) {
			IDirectedProtocolMessage message = this.CreateAuthorizedRequestInternal(endpoint, accessToken);
			HttpWebRequest wr = this.Channel.InitializeRequest(message);
			return this.WebRequestHandler.GetResponse(wr);
		}

		/// <summary>
		/// Creates a web request prepared with OAuth authorization 
		/// that may be further tailored by adding parameters by the caller.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <returns>The initialized WebRequest object.</returns>
		internal AccessProtectedResourcesMessage CreateAuthorizedRequestInternal(MessageReceivingEndpoint endpoint, string accessToken) {
			if (endpoint == null) {
				throw new ArgumentNullException("endpoint");
			}
			if (String.IsNullOrEmpty(accessToken)) {
				throw new ArgumentNullException("accessToken");
			}

			AccessProtectedResourcesMessage message = new AccessProtectedResourcesMessage(endpoint) {
				AccessToken = accessToken,
				TokenSecret = this.TokenManager.GetTokenSecret(accessToken),
				ConsumerKey = this.ConsumerKey,
				ConsumerSecret = this.ConsumerSecret,
			};

			return message;
		}
	}
}
