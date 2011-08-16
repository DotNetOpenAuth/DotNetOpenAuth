//-----------------------------------------------------------------------
// <copyright file="AccessTokenResourceOwnerPasswordCredentialsRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A request from a Client to an Authorization Server to exchange the user's username and password for an access token.
	/// </summary>
	internal class AccessTokenResourceOwnerPasswordCredentialsRequest : ScopedAccessTokenRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenResourceOwnerPasswordCredentialsRequest"/> class.
		/// </summary>
		/// <param name="accessTokenEndpoint">The access token endpoint.</param>
		/// <param name="version">The protocol version.</param>
		internal AccessTokenResourceOwnerPasswordCredentialsRequest(Uri accessTokenEndpoint, Version version)
			: base(accessTokenEndpoint, version) {
		}

		/// <summary>
		/// Gets the type of the grant.
		/// </summary>
		/// <value>The type of the grant.</value>
		internal override GrantType GrantType {
			get { return Messages.GrantType.Password; }
		}

		/// <summary>
		/// Gets or sets the user's account username.
		/// </summary>
		/// <value>The username on the user's account.</value>
		[MessagePart(Protocol.username, IsRequired = true)]
		internal string UserName { get; set; }

		/// <summary>
		/// Gets or sets the user's password.
		/// </summary>
		/// <value>The password.</value>
		[MessagePart(Protocol.password, IsRequired = true)]
		internal string Password { get; set; }
	}
}
