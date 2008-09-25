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
		public Consumer() {
			this.WebRequestHandler = new StandardWebRequestHandler();
			SigningBindingElementBase signingElement = new PlainTextSigningBindingElement();
			INonceStore store = new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge);
			this.Channel = new OAuthChannel(signingElement, store, new OAuthMessageTypeProvider(), this.WebRequestHandler);
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
		/// Gets or sets the Service Provider that will be accessed.
		/// </summary>
		public ServiceProvider ServiceProvider { get; set; }

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
		/// <returns>The Request Token Secret.</returns>
		public string RequestUserAuthorization(Uri callback) {
			// Obtain an unauthorized request token.
			var requestToken = new RequestTokenMessage(ServiceProvider.RequestTokenEndpoint) {
				ConsumerKey = this.ConsumerKey,
				ConsumerSecret = this.ConsumerSecret,
			};
			var requestTokenResponse = this.Channel.Request<UnauthorizedRequestTokenMessage>(requestToken);

			// Request user authorization.
			var requestAuthorization = new DirectUserToServiceProviderMessage(ServiceProvider.UserAuthorizationEndpoint) {
				Callback = callback,
				RequestToken = requestTokenResponse.RequestToken,
			};
			this.Channel.Send(requestAuthorization);
			this.PendingRequest = this.Channel.DequeueIndirectOrResponseMessage();
			return requestTokenResponse.TokenSecret;
		}

		internal GrantAccessTokenMessage ProcessUserAuthorization(string requestTokenSecret) {
			var authorizationMessage = this.Channel.ReadFromRequest<DirectUserToConsumerMessage>();
			
			// Exchange request token for access token.
			var requestAccess = new RequestAccessTokenMessage(ServiceProvider.AccessTokenEndpoint) {
				RequestToken = authorizationMessage.RequestToken,
				TokenSecret = requestTokenSecret,
				ConsumerKey = this.ConsumerKey,
				ConsumerSecret = this.ConsumerSecret,
			};
			var grantAccess = this.Channel.Request<GrantAccessTokenMessage>(requestAccess);
			return grantAccess;
		}
	}
}
