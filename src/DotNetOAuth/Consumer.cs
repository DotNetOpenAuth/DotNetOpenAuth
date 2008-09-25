//-----------------------------------------------------------------------
// <copyright file="Consumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
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
		/// <param name="endpoints">The endpoints on the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		public Consumer(ServiceProviderEndpoints endpoints, ITokenManager tokenManager) {
			if (endpoints == null) {
				throw new ArgumentNullException("endpoints");
			}
			if (tokenManager == null) {
				throw new ArgumentNullException("tokenManager");
			}

			this.WebRequestHandler = new StandardWebRequestHandler();
			SigningBindingElementBase signingElement = new PlainTextSigningBindingElement();
			INonceStore store = new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge);
			this.Channel = new OAuthChannel(signingElement, store, new OAuthMessageTypeProvider(), this.WebRequestHandler);
			this.ServiceProvider = endpoints;
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
		public ServiceProviderEndpoints ServiceProvider { get; private set; }

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
		public void RequestUserAuthorization(Uri callback) {
			// Obtain an unauthorized request token.
			var requestToken = new RequestTokenMessage(this.ServiceProvider.RequestTokenEndpoint) {
				ConsumerKey = this.ConsumerKey,
				ConsumerSecret = this.ConsumerSecret,
			};
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
		internal string ProcessUserAuthorization() {
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
	}
}
