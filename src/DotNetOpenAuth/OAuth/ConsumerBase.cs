//-----------------------------------------------------------------------
// <copyright file="ConsumerBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// Base class for <see cref="WebConsumer"/> and <see cref="DesktopConsumer"/> types.
	/// </summary>
	public class ConsumerBase : IDisposable {
		/// <summary>
		/// Initializes a new instance of the <see cref="ConsumerBase"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior of the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		protected ConsumerBase(ServiceProviderDescription serviceDescription, ITokenManager tokenManager) {
			if (serviceDescription == null) {
				throw new ArgumentNullException("serviceDescription");
			}
			if (tokenManager == null) {
				throw new ArgumentNullException("tokenManager");
			}

			ITamperProtectionChannelBindingElement signingElement = serviceDescription.CreateTamperProtectionElement();
			INonceStore store = new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge);
			this.OAuthChannel = new OAuthChannel(signingElement, store, tokenManager, new OAuthConsumerMessageFactory());
			this.ServiceProvider = serviceDescription;
		}

		/// <summary>
		/// Gets or sets the Consumer Key used to communicate with the Service Provider.
		/// </summary>
		public string ConsumerKey { get; set; }

		/// <summary>
		/// Gets the Service Provider that will be accessed.
		/// </summary>
		public ServiceProviderDescription ServiceProvider { get; private set; }

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
		internal OAuthChannel OAuthChannel { get; set; }

		/// <summary>
		/// Creates a web request prepared with OAuth authorization 
		/// that may be further tailored by adding parameters by the caller.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <returns>The initialized WebRequest object.</returns>
		public WebRequest PrepareAuthorizedRequest(MessageReceivingEndpoint endpoint, string accessToken) {
			IDirectedProtocolMessage message = this.CreateAuthorizingMessage(endpoint, accessToken);
			HttpWebRequest wr = this.OAuthChannel.InitializeRequest(message);
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
		public DirectWebResponse PrepareAuthorizedRequestAndSend(MessageReceivingEndpoint endpoint, string accessToken) {
			IDirectedProtocolMessage message = this.CreateAuthorizingMessage(endpoint, accessToken);
			HttpWebRequest wr = this.OAuthChannel.InitializeRequest(message);
			return this.Channel.WebRequestHandler.GetResponse(wr);
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		/// <summary>
		/// Prepares an OAuth message that begins an authorization request that will 
		/// redirect the user to the Service Provider to provide that authorization.
		/// </summary>
		/// <param name="callback">
		/// An optional Consumer URL that the Service Provider should redirect the 
		/// User Agent to upon successful authorization.
		/// </param>
		/// <param name="requestParameters">Extra parameters to add to the request token message.  Optional.</param>
		/// <param name="redirectParameters">Extra parameters to add to the redirect to Service Provider message.  Optional.</param>
		/// <param name="requestToken">The request token that must be exchanged for an access token after the user has provided authorization.</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#", Justification = "Two results")]
		protected internal UserAuthorizationRequest PrepareRequestUserAuthorization(Uri callback, IDictionary<string, string> requestParameters, IDictionary<string, string> redirectParameters, out string requestToken) {
			// Obtain an unauthorized request token.
			var token = new UnauthorizedTokenRequest(this.ServiceProvider.RequestTokenEndpoint) {
				ConsumerKey = this.ConsumerKey,
			};
			token.AddExtraParameters(requestParameters);
			var requestTokenResponse = this.Channel.Request<UnauthorizedTokenResponse>(token);
			this.TokenManager.StoreNewRequestToken(token, requestTokenResponse);

			// Request user authorization.
			ITokenContainingMessage assignedRequestToken = requestTokenResponse;
			var requestAuthorization = new UserAuthorizationRequest(this.ServiceProvider.UserAuthorizationEndpoint, assignedRequestToken.Token) {
				Callback = callback,
			};
			requestAuthorization.AddExtraParameters(redirectParameters);
			requestToken = requestAuthorization.RequestToken;
			return requestAuthorization;
		}

		/// <summary>
		/// Creates a web request prepared with OAuth authorization 
		/// that may be further tailored by adding parameters by the caller.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <returns>The initialized WebRequest object.</returns>
		protected internal AccessProtectedResourceRequest CreateAuthorizingMessage(MessageReceivingEndpoint endpoint, string accessToken) {
			if (endpoint == null) {
				throw new ArgumentNullException("endpoint");
			}
			if (String.IsNullOrEmpty(accessToken)) {
				throw new ArgumentNullException("accessToken");
			}

			AccessProtectedResourceRequest message = new AccessProtectedResourceRequest(endpoint) {
				AccessToken = accessToken,
				ConsumerKey = this.ConsumerKey,
			};

			return message;
		}

		/// <summary>
		/// Exchanges a given request token for access token.
		/// </summary>
		/// <param name="requestToken">The request token that the user has authorized.</param>
		/// <returns>The access token assigned by the Service Provider.</returns>
		protected AuthorizedTokenResponse ProcessUserAuthorization(string requestToken) {
			var requestAccess = new AuthorizedTokenRequest(this.ServiceProvider.AccessTokenEndpoint) {
				RequestToken = requestToken,
				ConsumerKey = this.ConsumerKey,
			};
			var grantAccess = this.Channel.Request<AuthorizedTokenResponse>(requestAccess);
			this.TokenManager.ExpireRequestTokenAndStoreNewAccessToken(this.ConsumerKey, requestToken, grantAccess.AccessToken, grantAccess.TokenSecret);
			return grantAccess;
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
	}
}
