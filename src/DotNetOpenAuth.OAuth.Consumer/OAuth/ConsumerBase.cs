//-----------------------------------------------------------------------
// <copyright file="ConsumerBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net;
	using DotNetOpenAuth.Configuration;
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
		protected ConsumerBase(ServiceProviderDescription serviceDescription, IConsumerTokenManager tokenManager) {
			Requires.NotNull(serviceDescription, "serviceDescription");
			Requires.NotNull(tokenManager, "tokenManager");

			ITamperProtectionChannelBindingElement signingElement = serviceDescription.CreateTamperProtectionElement();
			INonceStore store = new NonceMemoryStore(StandardExpirationBindingElement.MaximumMessageAge);
			this.SecuritySettings = OAuthElement.Configuration.Consumer.SecuritySettings.CreateSecuritySettings();
			this.OAuthChannel = new OAuthConsumerChannel(signingElement, store, tokenManager, this.SecuritySettings);
			this.ServiceProvider = serviceDescription;

			OAuthReporting.RecordFeatureAndDependencyUse(this, serviceDescription, tokenManager, null);
		}

		/// <summary>
		/// Gets the Consumer Key used to communicate with the Service Provider.
		/// </summary>
		public string ConsumerKey {
			get { return this.TokenManager.ConsumerKey; }
		}

		/// <summary>
		/// Gets the Service Provider that will be accessed.
		/// </summary>
		public ServiceProviderDescription ServiceProvider { get; private set; }

		/// <summary>
		/// Gets the persistence store for tokens and secrets.
		/// </summary>
		public IConsumerTokenManager TokenManager {
			get { return (IConsumerTokenManager)this.OAuthChannel.TokenManager; }
		}

		/// <summary>
		/// Gets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel {
			get { return this.OAuthChannel; }
		}

		/// <summary>
		/// Gets the security settings for this consumer.
		/// </summary>
		internal ConsumerSecuritySettings SecuritySettings { get; private set; }

		/// <summary>
		/// Gets or sets the channel to use for sending/receiving messages.
		/// </summary>
		internal OAuthChannel OAuthChannel { get; set; }

		/// <summary>
		/// Obtains an access token for a new account at the Service Provider via 2-legged OAuth.
		/// </summary>
		/// <param name="requestParameters">Any applicable parameters to include in the query string of the token request.</param>
		/// <returns>The access token.</returns>
		/// <remarks>
		/// The token secret is stored in the <see cref="TokenManager"/>.
		/// </remarks>
		public string RequestNewClientAccount(IDictionary<string, string> requestParameters = null) {
			// Obtain an unauthorized request token.  Force use of OAuth 1.0 (not 1.0a) so that 
			// we are not expected to provide an oauth_verifier which doesn't apply in 2-legged OAuth.
			var token = new UnauthorizedTokenRequest(this.ServiceProvider.RequestTokenEndpoint, Protocol.V10.Version) {
				ConsumerKey = this.ConsumerKey,
			};
			var tokenAccessor = this.Channel.MessageDescriptions.GetAccessor(token);
			tokenAccessor.AddExtraParameters(requestParameters);
			var requestTokenResponse = this.Channel.Request<UnauthorizedTokenResponse>(token);
			this.TokenManager.StoreNewRequestToken(token, requestTokenResponse);

			var requestAccess = new AuthorizedTokenRequest(this.ServiceProvider.AccessTokenEndpoint, Protocol.V10.Version) {
				RequestToken = requestTokenResponse.RequestToken,
				ConsumerKey = this.ConsumerKey,
			};
			var grantAccess = this.Channel.Request<AuthorizedTokenResponse>(requestAccess);
			this.TokenManager.ExpireRequestTokenAndStoreNewAccessToken(this.ConsumerKey, requestTokenResponse.RequestToken, grantAccess.AccessToken, grantAccess.TokenSecret);
			return grantAccess.AccessToken;
		}

		/// <summary>
		/// Creates a web request prepared with OAuth authorization 
		/// that may be further tailored by adding parameters by the caller.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <returns>The initialized WebRequest object.</returns>
		public HttpWebRequest PrepareAuthorizedRequest(MessageReceivingEndpoint endpoint, string accessToken) {
			Requires.NotNull(endpoint, "endpoint");
			Requires.NotNullOrEmpty(accessToken, "accessToken");

			return this.PrepareAuthorizedRequest(endpoint, accessToken, EmptyDictionary<string, string>.Instance);
		}

		/// <summary>
		/// Creates a web request prepared with OAuth authorization
		/// that may be further tailored by adding parameters by the caller.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <param name="extraData">Extra parameters to include in the message.  Must not be null, but may be empty.</param>
		/// <returns>The initialized WebRequest object.</returns>
		public HttpWebRequest PrepareAuthorizedRequest(MessageReceivingEndpoint endpoint, string accessToken, IDictionary<string, string> extraData) {
			Requires.NotNull(endpoint, "endpoint");
			Requires.NotNullOrEmpty(accessToken, "accessToken");
			Requires.NotNull(extraData, "extraData");

			IDirectedProtocolMessage message = this.CreateAuthorizingMessage(endpoint, accessToken);
			foreach (var pair in extraData) {
				message.ExtraData.Add(pair);
			}

			HttpWebRequest wr = this.OAuthChannel.InitializeRequest(message);
			return wr;
		}

		/// <summary>
		/// Prepares an authorized request that carries an HTTP multi-part POST, allowing for binary data.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <param name="binaryData">Extra parameters to include in the message.  Must not be null, but may be empty.</param>
		/// <returns>The initialized WebRequest object.</returns>
		public HttpWebRequest PrepareAuthorizedRequest(MessageReceivingEndpoint endpoint, string accessToken, IEnumerable<MultipartPostPart> binaryData) {
			Requires.NotNull(endpoint, "endpoint");
			Requires.NotNullOrEmpty(accessToken, "accessToken");
			Requires.NotNull(binaryData, "binaryData");

			AccessProtectedResourceRequest message = this.CreateAuthorizingMessage(endpoint, accessToken);
			foreach (MultipartPostPart part in binaryData) {
				message.BinaryData.Add(part);
			}

			HttpWebRequest wr = this.OAuthChannel.InitializeRequest(message);
			return wr;
		}

		/// <summary>
		/// Prepares an HTTP request that has OAuth authorization already attached to it.
		/// </summary>
		/// <param name="message">The OAuth authorization message to attach to the HTTP request.</param>
		/// <returns>
		/// The HttpWebRequest that can be used to send the HTTP request to the remote service provider.
		/// </returns>
		/// <remarks>
		/// If <see cref="IDirectedProtocolMessage.HttpMethods"/> property on the
		/// <paramref name="message"/> has the
		/// <see cref="HttpDeliveryMethods.AuthorizationHeaderRequest"/> flag set and
		/// <see cref="ITamperResistantOAuthMessage.HttpMethod"/> is set to an HTTP method
		/// that includes an entity body, the request stream is automatically sent
		/// if and only if the <see cref="IMessage.ExtraData"/> dictionary is non-empty.
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Type of parameter forces the method to apply only to specific scenario.")]
		public HttpWebRequest PrepareAuthorizedRequest(AccessProtectedResourceRequest message) {
			Requires.NotNull(message, "message");
			return this.OAuthChannel.InitializeRequest(message);
		}

		/// <summary>
		/// Creates a web request prepared with OAuth authorization 
		/// that may be further tailored by adding parameters by the caller.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <returns>The initialized WebRequest object.</returns>
		/// <exception cref="WebException">Thrown if the request fails for any reason after it is sent to the Service Provider.</exception>
		public IncomingWebResponse PrepareAuthorizedRequestAndSend(MessageReceivingEndpoint endpoint, string accessToken) {
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
		/// Creates a web request prepared with OAuth authorization 
		/// that may be further tailored by adding parameters by the caller.
		/// </summary>
		/// <param name="endpoint">The URL and method on the Service Provider to send the request to.</param>
		/// <param name="accessToken">The access token that permits access to the protected resource.</param>
		/// <returns>The initialized WebRequest object.</returns>
		protected internal AccessProtectedResourceRequest CreateAuthorizingMessage(MessageReceivingEndpoint endpoint, string accessToken) {
			Requires.NotNull(endpoint, "endpoint");
			Requires.NotNullOrEmpty(accessToken, "accessToken");

			AccessProtectedResourceRequest message = new AccessProtectedResourceRequest(endpoint, this.ServiceProvider.Version) {
				AccessToken = accessToken,
				ConsumerKey = this.ConsumerKey,
			};

			return message;
		}

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
			// Obtain an unauthorized request token.  Assume the OAuth version given in the service description.
			var token = new UnauthorizedTokenRequest(this.ServiceProvider.RequestTokenEndpoint, this.ServiceProvider.Version) {
				ConsumerKey = this.ConsumerKey,
				Callback = callback,
			};
			var tokenAccessor = this.Channel.MessageDescriptions.GetAccessor(token);
			tokenAccessor.AddExtraParameters(requestParameters);
			var requestTokenResponse = this.Channel.Request<UnauthorizedTokenResponse>(token);
			this.TokenManager.StoreNewRequestToken(token, requestTokenResponse);

			// Fine-tune our understanding of the SP's supported OAuth version if it's wrong.
			if (this.ServiceProvider.Version != requestTokenResponse.Version) {
				Logger.OAuth.WarnFormat("Expected OAuth service provider at endpoint {0} to use OAuth {1} but {2} was detected.  Adjusting service description to new version.", this.ServiceProvider.RequestTokenEndpoint.Location, this.ServiceProvider.Version, requestTokenResponse.Version);
				this.ServiceProvider.ProtocolVersion = Protocol.Lookup(requestTokenResponse.Version).ProtocolVersion;
			}

			// Request user authorization.  The OAuth version will automatically include 
			// or drop the callback that we're setting here.
			ITokenContainingMessage assignedRequestToken = requestTokenResponse;
			var requestAuthorization = new UserAuthorizationRequest(this.ServiceProvider.UserAuthorizationEndpoint, assignedRequestToken.Token, requestTokenResponse.Version) {
				Callback = callback,
			};
			var requestAuthorizationAccessor = this.Channel.MessageDescriptions.GetAccessor(requestAuthorization);
			requestAuthorizationAccessor.AddExtraParameters(redirectParameters);
			requestToken = requestAuthorization.RequestToken;
			return requestAuthorization;
		}

		/// <summary>
		/// Exchanges a given request token for access token.
		/// </summary>
		/// <param name="requestToken">The request token that the user has authorized.</param>
		/// <param name="verifier">The verifier code.</param>
		/// <returns>
		/// The access token assigned by the Service Provider.
		/// </returns>
		protected AuthorizedTokenResponse ProcessUserAuthorization(string requestToken, string verifier) {
			Requires.NotNullOrEmpty(requestToken, "requestToken");
			Contract.Ensures(Contract.Result<AuthorizedTokenResponse>() != null);

			var requestAccess = new AuthorizedTokenRequest(this.ServiceProvider.AccessTokenEndpoint, this.ServiceProvider.Version) {
				RequestToken = requestToken,
				VerificationCode = verifier,
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
