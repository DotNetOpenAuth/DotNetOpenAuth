//-----------------------------------------------------------------------
// <copyright file="ServiceProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Net.Http;
	using System.Security.Principal;
	using System.ServiceModel.Channels;
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
		/// The name of the key to use in the HttpApplication cache to store the
		/// instance of <see cref="MemoryNonceStore"/> to use.
		/// </summary>
		private const string ApplicationStoreKey = "DotNetOpenAuth.OAuth.ServiceProvider.HttpApplicationStore";

		/// <summary>
		/// The length of the verifier code (in raw bytes before base64 encoding) to generate.
		/// </summary>
		private const int VerifierCodeLength = 5;

		/// <summary>
		/// The field behind the <see cref="OAuthChannel"/> property.
		/// </summary>
		private OAuthChannel channel;

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProvider"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior on the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		public ServiceProvider(ServiceProviderHostDescription serviceDescription, IServiceProviderTokenManager tokenManager)
			: this(serviceDescription, tokenManager, new OAuthServiceProviderMessageFactory(tokenManager)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProvider"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior on the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		/// <param name="messageTypeProvider">An object that can figure out what type of message is being received for deserialization.</param>
		public ServiceProvider(ServiceProviderHostDescription serviceDescription, IServiceProviderTokenManager tokenManager, OAuthServiceProviderMessageFactory messageTypeProvider)
			: this(serviceDescription, tokenManager, OAuthElement.Configuration.ServiceProvider.ApplicationStore.CreateInstance(GetHttpApplicationStore(), null), messageTypeProvider) {
			Requires.NotNull(serviceDescription, "serviceDescription");
			Requires.NotNull(tokenManager, "tokenManager");
			Requires.NotNull(messageTypeProvider, "messageTypeProvider");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProvider"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior on the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		/// <param name="nonceStore">The nonce store.</param>
		public ServiceProvider(ServiceProviderHostDescription serviceDescription, IServiceProviderTokenManager tokenManager, INonceStore nonceStore)
			: this(serviceDescription, tokenManager, nonceStore, new OAuthServiceProviderMessageFactory(tokenManager)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProvider"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior on the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		/// <param name="nonceStore">The nonce store.</param>
		/// <param name="messageTypeProvider">An object that can figure out what type of message is being received for deserialization.</param>
		public ServiceProvider(ServiceProviderHostDescription serviceDescription, IServiceProviderTokenManager tokenManager, INonceStore nonceStore, OAuthServiceProviderMessageFactory messageTypeProvider) {
			Requires.NotNull(serviceDescription, "serviceDescription");
			Requires.NotNull(tokenManager, "tokenManager");
			Requires.NotNull(nonceStore, "nonceStore");
			Requires.NotNull(messageTypeProvider, "messageTypeProvider");

			var signingElement = serviceDescription.CreateTamperProtectionElement();
			this.ServiceDescription = serviceDescription;
			this.SecuritySettings = OAuthElement.Configuration.ServiceProvider.SecuritySettings.CreateSecuritySettings();
			this.OAuthChannel = new OAuthServiceProviderChannel(signingElement, nonceStore, tokenManager, this.SecuritySettings, messageTypeProvider);
			this.TokenGenerator = new StandardTokenGenerator();

			OAuthReporting.RecordFeatureAndDependencyUse(this, serviceDescription, tokenManager, nonceStore);
		}

		/// <summary>
		/// Gets the description of this Service Provider.
		/// </summary>
		public ServiceProviderHostDescription ServiceDescription { get; private set; }

		/// <summary>
		/// Gets or sets the generator responsible for generating new tokens and secrets.
		/// </summary>
		public ITokenGenerator TokenGenerator { get; set; }

		/// <summary>
		/// Gets the persistence store for tokens and secrets.
		/// </summary>
		public IServiceProviderTokenManager TokenManager {
			get { return (IServiceProviderTokenManager)this.OAuthChannel.TokenManager; }
		}

		/// <summary>
		/// Gets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel {
			get { return this.OAuthChannel; }
		}

		/// <summary>
		/// Gets the security settings for this service provider.
		/// </summary>
		public ServiceProviderSecuritySettings SecuritySettings { get; private set; }

		/// <summary>
		/// Gets or sets the channel to use for sending/receiving messages.
		/// </summary>
		internal OAuthChannel OAuthChannel {
			get {
				return this.channel;
			}

			set {
				Requires.NotNull(value, "value");
				this.channel = value;
			}
		}

		/// <summary>
		/// Gets the standard state storage mechanism that uses ASP.NET's
		/// HttpApplication state dictionary to store associations and nonces.
		/// </summary>
		/// <param name="context">The HTTP context. If <c>null</c>, this method must be called while <see cref="HttpContext.Current"/> is non-null.</param>
		/// <returns>The nonce store.</returns>
		public static INonceStore GetHttpApplicationStore(HttpContextBase context = null) {
			if (context == null) {
				ErrorUtilities.VerifyOperation(HttpContext.Current != null, Strings.StoreRequiredWhenNoHttpContextAvailable, typeof(INonceStore).Name);
				context = new HttpContextWrapper(HttpContext.Current);
			}

			var store = (INonceStore)context.Application[ApplicationStoreKey];
			if (store == null) {
				context.Application.Lock();
				try {
					if ((store = (INonceStore)context.Application[ApplicationStoreKey]) == null) {
						context.Application[ApplicationStoreKey] = store = new MemoryNonceStore(StandardExpirationBindingElement.MaximumMessageAge);
					}
				} finally {
					context.Application.UnLock();
				}
			}

			return store;
		}

		/// <summary>
		/// Creates a cryptographically strong random verification code.
		/// </summary>
		/// <param name="format">The desired format of the verification code.</param>
		/// <param name="length">The length of the code.
		/// When <paramref name="format"/> is <see cref="VerificationCodeFormat.IncludedInCallback"/>,
		/// this is the length of the original byte array before base64 encoding rather than the actual
		/// length of the final string.</param>
		/// <returns>The verification code.</returns>
		public static string CreateVerificationCode(VerificationCodeFormat format, int length) {
			Requires.Range(length >= 0, "length");

			switch (format) {
				case VerificationCodeFormat.IncludedInCallback:
					return MessagingUtilities.GetCryptoRandomDataAsBase64(length);
				case VerificationCodeFormat.AlphaNumericNoLookAlikes:
					return MessagingUtilities.GetRandomString(length, MessagingUtilities.AlphaNumericNoLookAlikes);
				case VerificationCodeFormat.AlphaUpper:
					return MessagingUtilities.GetRandomString(length, MessagingUtilities.UppercaseLetters);
				case VerificationCodeFormat.AlphaLower:
					return MessagingUtilities.GetRandomString(length, MessagingUtilities.LowercaseLetters);
				case VerificationCodeFormat.Numeric:
					return MessagingUtilities.GetRandomString(length, MessagingUtilities.Digits);
				default:
					throw new ArgumentOutOfRangeException("format");
			}
		}

		/// <summary>
		/// Reads any incoming OAuth message.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The deserialized message.
		/// </returns>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public Task<IDirectedProtocolMessage> ReadRequestAsync(HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken)) {
			return this.ReadRequestAsync((request ?? this.Channel.GetRequestFromContext()).AsHttpRequestMessage(), cancellationToken);
		}

		/// <summary>
		/// Reads any incoming OAuth message.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The deserialized message.
		/// </returns>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public Task<IDirectedProtocolMessage> ReadRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(request, "request");
			return this.Channel.ReadFromRequestAsync(request, cancellationToken);
		}

		/// <summary>
		/// Reads a request for an unauthorized token from the incoming HTTP request.
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The incoming request, or null if no OAuth message was attached.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public Task<UnauthorizedTokenRequest> ReadTokenRequestAsync(
			HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken)) {
			return this.ReadTokenRequestAsync((request ?? this.channel.GetRequestFromContext()).AsHttpRequestMessage(), cancellationToken);
		}

		/// <summary>
		/// Reads a request for an unauthorized token from the incoming HTTP request.
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The incoming request, or null if no OAuth message was attached.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public async Task<UnauthorizedTokenRequest> ReadTokenRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(request, "request");

			var message = await this.Channel.TryReadFromRequestAsync<UnauthorizedTokenRequest>(request, cancellationToken);
			if (message != null) {
				ErrorUtilities.VerifyProtocol(message.Version >= Protocol.Lookup(this.SecuritySettings.MinimumRequiredOAuthVersion).Version, OAuthStrings.MinimumConsumerVersionRequirementNotMet, this.SecuritySettings.MinimumRequiredOAuthVersion, message.Version);
			}

			return message;
		}

		/// <summary>
		/// Prepares a message containing an unauthorized token for the Consumer to use in a 
		/// user agent redirect for subsequent authorization.
		/// </summary>
		/// <param name="request">The token request message the Consumer sent that the Service Provider is now responding to.</param>
		/// <returns>The response message to send using the <see cref="Channel"/>, after optionally adding extra data to it.</returns>
		public UnauthorizedTokenResponse PrepareUnauthorizedTokenMessage(UnauthorizedTokenRequest request) {
			Requires.NotNull(request, "request");

			string token = this.TokenGenerator.GenerateRequestToken(request.ConsumerKey);
			string secret = this.TokenGenerator.GenerateSecret();
			UnauthorizedTokenResponse response = new UnauthorizedTokenResponse(request, token, secret);

			return response;
		}

		/// <summary>
		/// Reads in a Consumer's request for the Service Provider to obtain permission from
		/// the user to authorize the Consumer's access of some protected resource(s).
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The incoming request, or null if no OAuth message was attached.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public Task<UserAuthorizationRequest> ReadAuthorizationRequestAsync(HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken)) {
			request = request ?? this.channel.GetRequestFromContext();
			return this.ReadAuthorizationRequestAsync(request.AsHttpRequestMessage(), cancellationToken);
		}

		/// <summary>
		/// Reads in a Consumer's request for the Service Provider to obtain permission from
		/// the user to authorize the Consumer's access of some protected resource(s).
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The incoming request, or null if no OAuth message was attached.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public Task<UserAuthorizationRequest> ReadAuthorizationRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(request, "request");
			return this.Channel.TryReadFromRequestAsync<UserAuthorizationRequest>(request, cancellationToken);
		}

		/// <summary>
		/// Prepares the message to send back to the consumer following proper authorization of
		/// a token by an interactive user at the Service Provider's web site.
		/// </summary>
		/// <param name="request">The Consumer's original authorization request.</param>
		/// <returns>
		/// The message to send to the Consumer using <see cref="Channel"/> if one is necessary.
		/// Null if the Consumer did not request a callback as part of the authorization request.
		/// </returns>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Consistent user experience with instance.")]
		public UserAuthorizationResponse PrepareAuthorizationResponse(UserAuthorizationRequest request) {
			Requires.NotNull(request, "request");

			// It is very important for us to ignore the oauth_callback argument in the
			// UserAuthorizationRequest if the Consumer is a 1.0a consumer or else we
			// open up a security exploit.
			IServiceProviderRequestToken token = this.TokenManager.GetRequestToken(request.RequestToken);
			Uri callback;
			if (request.Version >= Protocol.V10a.Version) {
				// In OAuth 1.0a, we'll prefer the token-specific callback to the pre-registered one.
				if (token.Callback != null) {
					callback = token.Callback;
				} else {
					IConsumerDescription consumer = this.TokenManager.GetConsumer(token.ConsumerKey);
					callback = consumer.Callback;
				}
			} else {
				// In OAuth 1.0, we'll prefer the pre-registered callback over the token-specific one
				// since 1.0 has a security weakness for user-modified callback URIs.
				IConsumerDescription consumer = this.TokenManager.GetConsumer(token.ConsumerKey);
				callback = consumer.Callback ?? request.Callback;
			}

			return callback != null ? this.PrepareAuthorizationResponse(request, callback) : null;
		}

		/// <summary>
		/// Prepares the message to send back to the consumer following proper authorization of
		/// a token by an interactive user at the Service Provider's web site.
		/// </summary>
		/// <param name="request">The Consumer's original authorization request.</param>
		/// <param name="callback">The callback URI the consumer has previously registered
		/// with this service provider or that came in the <see cref="UnauthorizedTokenRequest"/>.</param>
		/// <returns>
		/// The message to send to the Consumer using <see cref="Channel"/>.
		/// </returns>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Consistent user experience with instance.")]
		public UserAuthorizationResponse PrepareAuthorizationResponse(UserAuthorizationRequest request, Uri callback) {
			Requires.NotNull(request, "request");
			Requires.NotNull(callback, "callback");

			var authorization = new UserAuthorizationResponse(callback, request.Version) {
				RequestToken = request.RequestToken,
			};

			if (authorization.Version >= Protocol.V10a.Version) {
				authorization.VerificationCode = CreateVerificationCode(VerificationCodeFormat.IncludedInCallback, VerifierCodeLength);
			}

			return authorization;
		}

		/// <summary>
		/// Reads in a Consumer's request to exchange an authorized request token for an access token.
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The incoming request, or null if no OAuth message was attached.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public Task<AuthorizedTokenRequest> ReadAccessTokenRequestAsync(HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken)) {
			request = request ?? this.Channel.GetRequestFromContext();
			return this.ReadAccessTokenRequestAsync(request.AsHttpRequestMessage(), cancellationToken);
		}

		/// <summary>
		/// Reads in a Consumer's request to exchange an authorized request token for an access token.
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The incoming request, or null if no OAuth message was attached.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public Task<AuthorizedTokenRequest> ReadAccessTokenRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(request, "request");
			return this.Channel.TryReadFromRequestAsync<AuthorizedTokenRequest>(request, cancellationToken);
		}

		/// <summary>
		/// Prepares and sends an access token to a Consumer, and invalidates the request token.
		/// </summary>
		/// <param name="request">The Consumer's message requesting an access token.</param>
		/// <returns>The HTTP response to actually send to the Consumer.</returns>
		public AuthorizedTokenResponse PrepareAccessTokenMessage(AuthorizedTokenRequest request) {
			Requires.NotNull(request, "request");

			ErrorUtilities.VerifyProtocol(this.TokenManager.IsRequestTokenAuthorized(request.RequestToken), OAuthStrings.AccessTokenNotAuthorized, request.RequestToken);

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
		/// <param name="request">HTTP details from an incoming WCF message.</param>
		/// <param name="requestUri">The URI of the WCF service endpoint.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The authorization message sent by the Consumer, or null if no authorization message is attached.</returns>
		/// <remarks>
		/// This method verifies that the access token and token secret are valid.
		/// It falls on the caller to verify that the access token is actually authorized
		/// to access the resources being requested.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if an unexpected message is attached to the request.</exception>
		public Task<AccessProtectedResourceRequest> ReadProtectedResourceAuthorizationAsync(HttpRequestMessageProperty request, Uri requestUri, CancellationToken cancellationToken = default(CancellationToken)) {
			return this.ReadProtectedResourceAuthorizationAsync(new HttpRequestInfo(request, requestUri), cancellationToken);
		}

		/// <summary>
		/// Gets the authorization (access token) for accessing some protected resource.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The authorization message sent by the Consumer, or null if no authorization message is attached.</returns>
		/// <remarks>
		/// This method verifies that the access token and token secret are valid.
		/// It falls on the caller to verify that the access token is actually authorized
		/// to access the resources being requested.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if an unexpected message is attached to the request.</exception>
		public Task<AccessProtectedResourceRequest> ReadProtectedResourceAuthorizationAsync(HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken)) {
			request = request ?? this.channel.GetRequestFromContext();
			return this.ReadProtectedResourceAuthorizationAsync(request.AsHttpRequestMessage(), cancellationToken);
		}

		/// <summary>
		/// Gets the authorization (access token) for accessing some protected resource.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The authorization message sent by the Consumer, or null if no authorization message is attached.</returns>
		/// <remarks>
		/// This method verifies that the access token and token secret are valid.
		/// It falls on the caller to verify that the access token is actually authorized
		/// to access the resources being requested.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if an unexpected message is attached to the request.</exception>
		public async Task<AccessProtectedResourceRequest> ReadProtectedResourceAuthorizationAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(request, "request");
			var accessMessage = await this.Channel.TryReadFromRequestAsync<AccessProtectedResourceRequest>(request, cancellationToken);
			if (accessMessage != null) {
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

		/// <summary>
		/// Creates a security principal that may be used.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>The <see cref="IPrincipal"/> instance that can be used for access control of resources.</returns>
		public IPrincipal CreatePrincipal(AccessProtectedResourceRequest request) {
			Requires.NotNull(request, "request");

			IServiceProviderAccessToken accessToken = this.TokenManager.GetAccessToken(request.AccessToken);
			return OAuthPrincipal.CreatePrincipal(accessToken.Username, accessToken.Roles);
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
	}
}
