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

	internal class AccessTokenResourceOwnerPasswordCredentialsRequest : AccessTokenRequestBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenResourceOwnerPasswordCredentialsRequest"/> class.
		/// </summary>
		internal AccessTokenResourceOwnerPasswordCredentialsRequest(Uri accessTokenEndpoint, Version version)
			: base(accessTokenEndpoint, version) {
		}

		internal override GrantType GrantType {
			get { return Messages.GrantType.BasicCredentials; }
		}

		/// <summary>
		/// Gets or sets the user's account username.
		/// </summary>
		/// <value>The username on the user's account.</value>
		[MessagePart(Protocol.username, IsRequired = true, AllowEmpty = false)]
		internal string UserName { get; set; }

		/// <summary>
		/// Gets or sets the user's password.
		/// </summary>
		/// <value>The password.</value>
		[MessagePart(Protocol.password, IsRequired = true, AllowEmpty = true)]
		internal string Password { get; set; }

	}
}
