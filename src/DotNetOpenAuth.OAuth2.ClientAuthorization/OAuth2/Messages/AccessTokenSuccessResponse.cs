//-----------------------------------------------------------------------
// <copyright file="AccessTokenSuccessResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// A response from the Authorization Server to the Client containing a delegation code
	/// that the Client should use to obtain an access token.
	/// </summary>
	/// <remarks>
	/// This message type is shared by the Web App, Rich App, and Username/Password profiles.
	/// </remarks>
	internal class AccessTokenSuccessResponse : MessageBase, IHttpDirectResponse, IAccessTokenIssuingResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenSuccessResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal AccessTokenSuccessResponse(AccessTokenRequestBase request)
			: base(request) {
			this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
			this.TokenType = Protocol.AccessTokenTypes.Bearer;
		}

		/// <summary>
		/// Gets the HTTP status code that the direct response should be sent with.
		/// </summary>
		/// <value>Always HttpStatusCode.OK</value>
		HttpStatusCode IHttpDirectResponse.HttpStatusCode {
			get { return HttpStatusCode.OK; }
		}

		/// <summary>
		/// Gets the HTTP headers to add to the response.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		WebHeaderCollection IHttpDirectResponse.Headers {
			get {
				return new WebHeaderCollection
				{
					{ HttpResponseHeader.CacheControl, "no-store" },
					{ HttpResponseHeader.Pragma, "no-cache" },
				};
			}
		}

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>The access token.</value>
		[MessagePart(Protocol.access_token, IsRequired = true)]
		public string AccessToken { get; internal set; }

		/// <summary>
		/// Gets or sets the token type.
		/// </summary>
		/// <value>Usually "bearer".</value>
		/// <remarks>
		/// Described in OAuth 2.0 section 7.1.
		/// </remarks>
		[MessagePart(Protocol.token_type, IsRequired = false)] // HACKHACK: This is actually required, but wasn't in older drafts of OAuth 2
		public string TokenType { get; internal set; }

		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart(Protocol.expires_in, IsRequired = false, Encoder = typeof(TimespanSecondsEncoder))]
		public TimeSpan? Lifetime { get; internal set; }

		/// <summary>
		/// Gets or sets the refresh token.
		/// </summary>
		/// <value>The refresh token.</value>
		/// <remarks>
		/// OPTIONAL. The refresh token used to obtain new access tokens using the same end-user access grant as described in Section 6  (Refreshing an Access Token). 
		/// </remarks>
		[MessagePart(Protocol.refresh_token, IsRequired = false)]
		public string RefreshToken { get; internal set; }

		/// <summary>
		/// Gets the scope of access being requested.
		/// </summary>
		/// <value>The scope of the access request expressed as a list of space-delimited strings. The value of the scope parameter is defined by the authorization server. If the value contains multiple space-delimited strings, their order does not matter, and each string adds an additional access range to the requested scope.</value>
		[MessagePart(Protocol.scope, IsRequired = false, Encoder = typeof(ScopeEncoder))]
		public HashSet<string> Scope { get; private set; }

		#region IAccessTokenIssuingResponse Members

		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>
		/// The lifetime.
		/// </value>
		TimeSpan? IAccessTokenIssuingResponse.Lifetime {
			get { return this.Lifetime; }
			set { this.Lifetime = value; }
		}

		#endregion

		#region IAuthorizationCarryingRequest

		/// <summary>
		/// Gets the authorization that the token describes.
		/// </summary>
		IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription {
			get { return ((IAccessTokenCarryingRequest)this).AuthorizationDescription; }
		}

		#endregion

		#region IAccessTokenCarryingRequest Members

		/// <summary>
		/// Gets or sets the authorization that the token describes.
		/// </summary>
		/// <value></value>
		AccessToken IAccessTokenCarryingRequest.AuthorizationDescription { get; set; }

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		string IAccessTokenCarryingRequest.AccessToken {
			get { return this.AccessToken; }
			set { this.AccessToken = value; }
		}

		#endregion

		/// <summary>
		/// Gets or sets a value indicating whether a refresh token is or should be included in the response.
		/// </summary>
		internal bool HasRefreshToken { get; set; }

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();

			// Per OAuth 2.0 section 4.4.3 (draft 23), refresh tokens should never be included
			// in a response to an access token request that used the client credential grant type.
			ErrorUtilities.VerifyProtocol(!this.HasRefreshToken || !(this.OriginatingRequest is AccessTokenClientCredentialsRequest), ClientAuthorizationStrings.RefreshTokenInappropriateForRequestType, this.OriginatingRequest.GetType().Name);
		}
	}
}
