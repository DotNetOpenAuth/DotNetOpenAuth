//-----------------------------------------------------------------------
// <copyright file="ServiceProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
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
		/// The field used to store the value of the <see cref="RequestTokenEndpoint"/> property.
		/// </summary>
		private ServiceProviderEndpoint requestTokenEndpoint;

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProvider"/> class.
		/// </summary>
		public ServiceProvider() {
			SigningBindingElementBase signingElement = new PlainTextSigningBindingElement();
			INonceStore store = new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge);
			this.Channel = new OAuthChannel(signingElement, store);
		}

		/// <summary>
		/// Gets or sets the URL used to obtain an unauthorized Request Token,
		/// described in Section 6.1 (Obtaining an Unauthorized Request Token).
		/// </summary>
		/// <remarks>
		/// The request URL query MUST NOT contain any OAuth Protocol Parameters.
		/// This is the URL that <see cref="Messages.RequestTokenMessage"/> messages are directed to.
		/// </remarks>
		/// <exception cref="ArgumentException">Thrown if this property is set to a URI with OAuth protocol parameters.</exception>
		public ServiceProviderEndpoint RequestTokenEndpoint {
			get {
				return this.requestTokenEndpoint;
			}

			set {
				if (value != null && UriUtil.QueryStringContainsOAuthParameters(value.Location)) {
					throw new ArgumentException(Strings.RequestUrlMustNotHaveOAuthParameters);
				}

				this.requestTokenEndpoint = value;
			}
		}

		/// <summary>
		/// Gets or sets the URL used to obtain User authorization for Consumer access, 
		/// described in Section 6.2 (Obtaining User Authorization).
		/// </summary>
		/// <remarks>
		/// This is the URL that <see cref="Messages.DirectUserToServiceProviderMessage"/> messages are
		/// indirectly (via the user agent) sent to.
		/// </remarks>
		public ServiceProviderEndpoint UserAuthorizationEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the URL used to exchange the User-authorized Request Token 
		/// for an Access Token, described in Section 6.3 (Obtaining an Access Token).
		/// </summary>
		/// <remarks>
		/// This is the URL that <see cref="Messages.RequestAccessTokenMessage"/> messages are directed to.
		/// </remarks>
		public ServiceProviderEndpoint AccessTokenEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the channel to use for sending/receiving messages.
		/// </summary>
		internal OAuthChannel Channel { get; set; }

		/// <summary>
		/// Gets the pending user agent redirect based message to be sent as an HttpResponse.
		/// </summary>
		public Response PendingRequest { get; private set; }

		internal RequestTokenMessage ReadTokenRequest() {
			return this.Channel.ReadFromRequest<RequestTokenMessage>();
		}

		internal RequestTokenMessage ReadTokenRequest(HttpRequest request) {
			return this.ReadTokenRequest(new HttpRequestInfo(request));
		}

		internal RequestTokenMessage ReadTokenRequest(HttpRequestInfo request) {
			return this.Channel.ReadFromRequest<RequestTokenMessage>(request);
		}

		internal void SendUnauthorizedTokenResponse(string token, string secret) {
			UnauthorizedRequestTokenMessage response = new UnauthorizedRequestTokenMessage {
				RequestToken = token,
				TokenSecret = secret,
			};

			this.Channel.Send(response);
		}

		internal DirectUserToServiceProviderMessage ReadAuthorizationRequest() {
			return this.Channel.ReadFromRequest<DirectUserToServiceProviderMessage>();
		}

		internal DirectUserToServiceProviderMessage ReadAuthorizationRequest(HttpRequest request) {
			return this.ReadAuthorizationRequest(new HttpRequestInfo(request));
		}

		internal DirectUserToServiceProviderMessage ReadAuthorizationRequest(HttpRequestInfo request) {
			return this.Channel.ReadFromRequest<DirectUserToServiceProviderMessage>(request);
		}

		internal void SendAuthorizationResponse(DirectUserToServiceProviderMessage request) {
			var authorization = new DirectUserToConsumerMessage(request.Callback) {
				RequestToken = request.RequestToken,
			};
			this.Channel.Send(authorization);
			this.PendingRequest = this.Channel.DequeueIndirectOrResponseMessage();
		}

		internal RequestAccessTokenMessage ReadAccessTokenRequest() {
			return this.Channel.ReadFromRequest<RequestAccessTokenMessage>();
		}

		internal RequestAccessTokenMessage ReadAccessTokenRequest(HttpRequest request) {
			return this.ReadAccessTokenRequest(new HttpRequestInfo(request));
		}

		internal RequestAccessTokenMessage ReadAccessTokenRequest(HttpRequestInfo request) {
			return this.Channel.ReadFromRequest<RequestAccessTokenMessage>(request);
		}

		internal void SendAccessToken(string accessToken, string tokenSecret) {
			var grantAccess = new GrantAccessTokenMessage {
				AccessToken = accessToken,
				TokenSecret = tokenSecret,
			};

			this.Channel.Send(grantAccess);
		}
	}
}
