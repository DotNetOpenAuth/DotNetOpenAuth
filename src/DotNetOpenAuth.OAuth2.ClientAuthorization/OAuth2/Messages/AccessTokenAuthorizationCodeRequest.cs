//-----------------------------------------------------------------------
// <copyright file="AccessTokenAuthorizationCodeRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// A request from a Client to an Authorization Server to exchange an authorization code for an access token,
	/// and (at the authorization server's option) a refresh token.
	/// </summary>
	internal class AccessTokenAuthorizationCodeRequest : AccessTokenRequestBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenAuthorizationCodeRequest"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The Authorization Server's access token endpoint URL.</param>
		/// <param name="version">The version.</param>
		protected AccessTokenAuthorizationCodeRequest(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
		}

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
		/// <remarks>
		/// REQUIRED, if the redirect_uri parameter was included in the authorization request as described in Section 4.1.1, and their values MUST be identical.
		/// </remarks>
		[MessagePart(Protocol.redirect_uri, IsRequired = false)]
		internal Uri Callback { get; set; }

		/// <summary>
		/// Gets the scope of operations the client is allowed to invoke.
		/// </summary>
		protected override HashSet<string> RequestedScope {
			get { return ((IAuthorizationCarryingRequest)this).AuthorizationDescription.Scope; }
		}
	}
}
