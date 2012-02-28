//-----------------------------------------------------------------------
// <copyright file="AccessTokenRefreshRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// A request from the client to the token endpoint for a new access token
	/// in exchange for a refresh token that the client has previously obtained.
	/// </summary>
	internal class AccessTokenRefreshRequest : ScopedAccessTokenRequest, IRefreshTokenCarryingRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenRefreshRequest"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The token endpoint.</param>
		/// <param name="version">The version.</param>
		internal AccessTokenRefreshRequest(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenRefreshRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal AccessTokenRefreshRequest(AuthorizationServerDescription authorizationServer)
			: this(authorizationServer.TokenEndpoint, authorizationServer.Version) {
		}

		#region IRefreshTokenCarryingRequest members

		/// <summary>
		/// Gets or sets the verification code or refresh/access token.
		/// </summary>
		/// <value>The code or token.</value>
		string IRefreshTokenCarryingRequest.RefreshToken {
			get { return this.RefreshToken; }
			set { this.RefreshToken = value; }
		}

		/// <summary>
		/// Gets or sets the authorization that the token describes.
		/// </summary>
		RefreshToken IRefreshTokenCarryingRequest.AuthorizationDescription { get; set; }

		/// <summary>
		/// Gets the authorization that the token describes.
		/// </summary>
		IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription {
			get { return ((IRefreshTokenCarryingRequest)this).AuthorizationDescription; }
		}

		#endregion

		/// <summary>
		/// Gets or sets the refresh token.
		/// </summary>
		/// <value>The refresh token.</value>
		/// <remarks>
		/// REQUIRED. The refresh token associated with the access token to be refreshed. 
		/// </remarks>
		[MessagePart(Protocol.refresh_token, IsRequired = true)]
		internal string RefreshToken { get; set; }

		/// <summary>
		/// Gets the type of the grant.
		/// </summary>
		/// <value>The type of the grant.</value>
		internal override GrantType GrantType {
			get { return Messages.GrantType.RefreshToken; }
		}
	}
}
