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

	internal class AccessTokenAuthorizationCodeRequest : AccessTokenRequestBase, ITokenCarryingRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenAuthorizationCodeRequest"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The Authorization Server's access token endpoint URL.</param>
		/// <param name="version">The version.</param>
		internal AccessTokenAuthorizationCodeRequest(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
		}


		internal AccessTokenAuthorizationCodeRequest(AuthorizationServerDescription authorizationServer)
			: this(authorizationServer.TokenEndpoint, authorizationServer.Version)
		{
			Contract.Requires<ArgumentNullException>(authorizationServer != null, "authorizationServer");
		}

		internal override GrantType GrantType {
			get { return Messages.GrantType.AuthorizationCode; }
		}

		/// <summary>
		/// Gets the type of the code or token.
		/// </summary>
		/// <value>The type of the code or token.</value>
		CodeOrTokenType ITokenCarryingRequest.CodeOrTokenType {
			get { return CodeOrTokenType.AuthorizationCode; }
		}

		/// <summary>
		/// Gets or sets the verification code or refresh/access token.
		/// </summary>
		/// <value>The code or token.</value>
		string ITokenCarryingRequest.CodeOrToken {
			get { return this.AuthorizationCode; }
			set { this.AuthorizationCode = value; }
		}

		/// <summary>
		/// Gets or sets the authorization that the token describes.
		/// </summary>
		IAuthorizationDescription ITokenCarryingRequest.AuthorizationDescription { get; set; }

		/// <summary>
		/// Gets or sets the verification code previously communicated to the Client
		/// in <see cref="WebServerSuccessResponse.VerificationCode"/>.
		/// </summary>
		/// <value>The verification code received from the authorization server.</value>
		[MessagePart(Protocol.code, IsRequired = true, AllowEmpty = false)]
		internal string AuthorizationCode { get; set; }

		/// <summary>
		/// Gets or sets the callback URL used in <see cref="EndUserAuthorizationRequest.Callback"/>
		/// </summary>
		/// <value>
		/// The Callback URL used to obtain the Verification Code.
		/// </value>
		[MessagePart(Protocol.redirect_uri, IsRequired = true, AllowEmpty = false)]
		internal Uri Callback { get; set; }

		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		/// <value>The client identifier.</value>
		[MessagePart(Protocol.client_id, IsRequired = true, AllowEmpty = false)]
		public string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the client secret.
		/// </summary>
		/// <value>The client secret.</value>
		/// <remarks>
		/// REQUIRED if the client identifier has a matching secret. The client secret as described in Section 3.4  (Client Credentials). 
		/// </remarks>
		[MessagePart(Protocol.client_secret, IsRequired = false, AllowEmpty = true)]
		public string ClientSecret { get; set; }
	}
}
