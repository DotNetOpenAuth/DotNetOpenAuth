//-----------------------------------------------------------------------
// <copyright file="AccessTokenAuthorizationCodeRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// A request from a Client to an Authorization Server to exchange an authorization code for an access token.
	/// </summary>
	internal class AccessTokenAuthorizationCodeRequest : AccessTokenRequestBase, IAuthorizationCarryingRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenAuthorizationCodeRequest"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The Authorization Server's access token endpoint URL.</param>
		/// <param name="version">The version.</param>
		internal AccessTokenAuthorizationCodeRequest(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenAuthorizationCodeRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal AccessTokenAuthorizationCodeRequest(AuthorizationServerDescription authorizationServer)
			: this(authorizationServer.TokenEndpoint, authorizationServer.Version)
		{
			Contract.Requires<ArgumentNullException>(authorizationServer != null);
		}

		/// <summary>
		/// Gets the type of the code or token.
		/// </summary>
		/// <value>The type of the code or token.</value>
		CodeOrTokenType IAuthorizationCarryingRequest.CodeOrTokenType {
			get { return CodeOrTokenType.AuthorizationCode; }
		}

		/// <summary>
		/// Gets or sets the verification code or refresh/access token.
		/// </summary>
		/// <value>The code or token.</value>
		string IAuthorizationCarryingRequest.CodeOrToken {
			get { return this.AuthorizationCode; }
			set { this.AuthorizationCode = value; }
		}

		/// <summary>
		/// Gets or sets the authorization that the token describes.
		/// </summary>
		IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription { get; set; }

		/// <summary>
		/// Gets the type of the grant.
		/// </summary>
		/// <value>The type of the grant.</value>
		internal override GrantType GrantType {
			get { return Messages.GrantType.AuthorizationCode; }
		}

		/// <summary>
		/// Gets or sets the verification code previously communicated to the Client
		/// in <see cref="EndUserAuthorizationSuccessAuthCodeResponse.AuthorizationCode"/>.
		/// </summary>
		/// <value>The verification code received from the authorization server.</value>
		[MessagePart(Protocol.code, IsRequired = true)]
		internal string AuthorizationCode { get; set; }

		/// <summary>
		/// Gets or sets the callback URL used in <see cref="EndUserAuthorizationRequest.Callback"/>
		/// </summary>
		/// <value>
		/// The Callback URL used to obtain the Verification Code.
		/// </value>
		[MessagePart(Protocol.redirect_uri, IsRequired = true)]
		internal Uri Callback { get; set; }
	}
}
