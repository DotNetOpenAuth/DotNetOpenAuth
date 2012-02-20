//-----------------------------------------------------------------------
// <copyright file="ScopedAccessTokenRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// An access token request that includes a scope parameter.
	/// </summary>
	internal abstract class ScopedAccessTokenRequest : AccessTokenRequestBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="ScopedAccessTokenRequest"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The Authorization Server's access token endpoint URL.</param>
		/// <param name="version">The version.</param>
		internal ScopedAccessTokenRequest(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
			this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
		}

		/// <summary>
		/// Gets the set of scopes the Client would like the access token to provide access to.
		/// </summary>
		/// <value>A set of scopes.  Never null.</value>
		[MessagePart(Protocol.scope, IsRequired = false, Encoder = typeof(ScopeEncoder))]
		internal HashSet<string> Scope { get; private set; }

		/// <summary>
		/// Gets the scope of operations the client is allowed to invoke.
		/// </summary>
		protected override HashSet<string> RequestedScope {
			get { return this.Scope; }
		}
	}
}
