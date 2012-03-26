//-----------------------------------------------------------------------
// <copyright file="AccessTokenAuthorizationCodeRequestAS.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// A request from a Client to an Authorization Server to exchange an authorization code for an access token,
	/// and (at the authorization server's option) a refresh token.
	/// </summary>
	internal class AccessTokenAuthorizationCodeRequestAS : AccessTokenAuthorizationCodeRequest, IAuthorizationCodeCarryingRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenAuthorizationCodeRequestAS"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The Authorization Server's access token endpoint URL.</param>
		/// <param name="version">The version.</param>
		internal AccessTokenAuthorizationCodeRequestAS(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
		}

		#region IAuthorizationCodeCarryingRequest Members

		/// <summary>
		/// Gets or sets the verification code or refresh/access token.
		/// </summary>
		/// <value>The code or token.</value>
		string IAuthorizationCodeCarryingRequest.Code {
			get { return this.AuthorizationCode; }
			set { this.AuthorizationCode = value; }
		}

		/// <summary>
		/// Gets or sets the authorization that the token describes.
		/// </summary>
		AuthorizationCode IAuthorizationCodeCarryingRequest.AuthorizationDescription { get; set; }

		/// <summary>
		/// Gets the authorization that the code describes.
		/// </summary>
		IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription {
			get { return ((IAuthorizationCodeCarryingRequest)this).AuthorizationDescription; }
		}

		#endregion
	}
}
