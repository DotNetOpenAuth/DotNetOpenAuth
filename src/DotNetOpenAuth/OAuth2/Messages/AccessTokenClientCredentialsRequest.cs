//-----------------------------------------------------------------------
// <copyright file="AccessTokenClientCredentialsRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
	/// A request for an access token for a client application that has its
	/// own (non-user affiliated) client name and password.
	/// </summary>
	/// <remarks>
	/// This is somewhat analogous to 2-legged OAuth.
	/// </remarks>
	internal class AccessTokenClientCredentialsRequest : AccessTokenRequestBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenClientCredentialsRequest"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal AccessTokenClientCredentialsRequest(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		/// <summary>
		/// Gets the type of the grant.
		/// </summary>
		/// <value>The type of the grant.</value>
		internal override GrantType GrantType {
			get { return Messages.GrantType.None; }
		}
	}
}
