//-----------------------------------------------------------------------
// <copyright file="ServiceProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Security.Principal;
	using System.ServiceModel.Channels;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.OAuth;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;

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
		public ServiceProvider(ServiceProviderDescription serviceDescription, IServiceProviderTokenManager tokenManager)
			: this(serviceDescription, tokenManager, new OAuthServiceProviderMessageFactory(tokenManager)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProvider"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior on the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		/// <param name="messageTypeProvider">An object that can figure out what type of message is being received for deserialization.</param>
		public ServiceProvider(ServiceProviderDescription serviceDescription, IServiceProviderTokenManager tokenManager, OAuthServiceProviderMessageFactory messageTypeProvider) {
			ErrorUtilities.VerifyArgumentNotNull(serviceDescription, "serviceDescription");
			ErrorUtilities.VerifyArgumentNotNull(tokenManager, "tokenManager");
			ErrorUtilities.VerifyArgumentNotNull(messageTypeProvider, "messageTypeProvider");

			var signingElement = serviceDescription.CreateTamperProtectionElement();
			INonceStore store = new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge);
			this.ServiceDescription = serviceDescription;
			this.OAuthChannel = new OAuthChannel(signingElement, store, tokenManager, messageTypeProvider);
			this.TokenGenerator = new StandardTokenGenerator();
			this.SecuritySettings = DotNetOpenAuthSection.Configuration.OAuth.ServiceProvider.SecuritySettings.CreateSecuritySettings();
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
				Contract.Requires(value != null);
				ErrorUtilities.VerifyArgumentNotNull(value, "value");
				this.channel = value;
			}
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
			Contract.Requires(length >= 0);
			ErrorUtilities.VerifyArgumentInRange(length >= 0, "length");

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
		/// <returns>The deserialized message.</returns>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public IDirectedProtocolMessage ReadRequest() {
			return this.Channel.ReadFromRequest();
		}

		/// <summary>
		/// Reads any incoming OAuth message.
		/// </summary>
		/// <param name="request">The HTTP request to read the message from.</param>
		/// <returns>The deserialized message.</returns>
		public IDirectedProtocolMessage ReadRequest(HttpRequestInfo request) {
			return this.Channel.ReadFromRequest(request);
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
		/// Reads a request for an unauthorized token from the incoming HTTP request.
		/// </summary>
		/// <param name="request">The HTTP request to read from.</param>
		/// <returns>The incoming request, or null if no OAuth message was attached.</returns>
		/// <exception cref="ProtocolException">Thrown if an unexpected OAuth message is attached to the incoming request.</exception>
		public UnauthorizedTokenRequest ReadTokenRequest(HttpRequestInfo request) {
			UnauthorizedTokenRequest message;
			if (this.Channel.TryReadFromRequest(request, out message)) {
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
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

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
		/// Gets the OAuth authorization request included with an OpenID authentication
		/// request.
		/// </summary>
		/// <param name="openIdRequest">The OpenID authentication request.</param>
		/// <returns>
		/// The scope of access the relying party is requesting.
		/// </returns>
		/// <remarks>
		/// <para>Call this method rather than simply extracting the OAuth extension
		/// out from the authentication request directly to ensure that the additional
		/// security measures that are required are taken.</para>
		/// </remarks>
		public AuthorizationRequest ReadAuthorizationRequest(IHostProcessedRequest openIdRequest) {
			Contract.Requires(openIdRequest != null);
			Contract.Requires(this.TokenManager is ICombinedOpenIdProviderTokenManager);
			ErrorUtilities.VerifyArgumentNotNull(openIdRequest, "openIdAuthenticationRequest");
			var openidTokenManager = this.TokenManager as ICombinedOpenIdProviderTokenManager;
			ErrorUtilities.VerifyOperation(openidTokenManager != null, OAuthStrings.OpenIdOAuthExtensionRequiresSpecialTokenManagerInterface, typeof(IOpenIdOAuthTokenManager).FullName);

			var authzRequest = openIdRequest.GetExtension<AuthorizationRequest>();
			if (authzRequest == null) {
				return null;
			}

			// OpenID+OAuth spec section 9:
			// The Combined Provider SHOULD verify that the consumer key passed in the
			// request is authorized to be used for the realm passed in the request.
			string expectedConsumerKey = openidTokenManager.GetConsumerKey(openIdRequest.Realm);
			ErrorUtilities.VerifyProtocol(
				string.Equals(expectedConsumerKey, authzRequest.Consumer, StringComparison.Ordinal),
				OAuthStrings.OpenIdOAuthRealmConsumerKeyDoNotMatch);

			return authzRequest;
		}

		/// <summary>
		/// Attaches the authorization response to an OpenID authentication response.
		/// </summary>
		/// <param name="openIdAuthenticationRequest">The OpenID authentication request.</param>
		/// <param name="consumerKey">The consumer key.  May and should be <c>null</c> if and only if <paramref name="scope"/> is null.</param>
		/// <param name="scope">The approved access scope.  Use <c>null</c> to indicate no access was granted.  The empty string will be interpreted as some default level of access is granted.</param>
		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We want to take IAuthenticationRequest because that's the only supported use case.")]
		public void AttachAuthorizationResponse(IAuthenticationRequest openIdAuthenticationRequest, string consumerKey, string scope) {
			Contract.Requires(openIdAuthenticationRequest != null);
			Contract.Requires((consumerKey == null) == (scope == null));
			Contract.Requires(this.TokenManager is IOpenIdOAuthTokenManager);
			ErrorUtilities.VerifyArgumentNotNull(openIdAuthenticationRequest, "openIdAuthenticationRequest");
			var openidTokenManager = this.TokenManager as IOpenIdOAuthTokenManager;
			ErrorUtilities.VerifyOperation(openidTokenManager != null, OAuthStrings.OpenIdOAuthExtensionRequiresSpecialTokenManagerInterface, typeof(IOpenIdOAuthTokenManager).FullName);

			IOpenIdMessageExtension response;
			if (scope != null) {
				// Generate an authorized request token to return to the relying party.
				var approvedResponse = new AuthorizationApprovedResponse {
					RequestToken = this.TokenGenerator.GenerateRequestToken(consumerKey),
					Scope = scope,
				};
				openidTokenManager.StoreOpenIdAuthorizedRequestToken(consumerKey, approvedResponse);
				response = approvedResponse;
			} else {
				response = new AuthorizationDeclinedResponse();
			}

			openIdAuthenticationRequest.AddResponseExtension(response);
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
			Contract.Requires(request != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

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
			Contract.Requires(request != null);
			Contract.Requires(callback != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			ErrorUtilities.VerifyArgumentNotNull(callback, "callback");

			var authorization = new UserAuthorizationResponse(callback, request.Version) {
				RequestToken = request.RequestToken,
			};

			if (authorization.Version >= Protocol.V10a.Version) {
				authorization.VerificationCode = CreateVerificationCode(VerificationCodeFormat.IncludedInCallback, VerifierCodeLength);
			}

			return authorization;
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
			Contract.Requires(request != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

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
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

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

		/// <summary>
		/// Creates a security principal that may be used.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>The <see cref="IPrincipal"/> instance that can be used for access control of resources.</returns>
		public OAuthPrincipal CreatePrincipal(AccessProtectedResourceRequest request) {
			Contract.Requires(request != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			IServiceProviderAccessToken accessToken = this.TokenManager.GetAccessToken(request.AccessToken);
			return new OAuthPrincipal(accessToken);
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
