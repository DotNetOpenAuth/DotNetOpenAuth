//-----------------------------------------------------------------------
// <copyright file="IAccessTokenRequest.cs" company="Outercurve Foundation">
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
	/// A request from a client that should be responded to directly with an access token.
	/// </summary>
	public interface IAccessTokenRequest : IMessage {
		/// <summary>
		/// Gets a value indicating whether the client requesting the access token has authenticated itself.
		/// </summary>
		/// <value>
		///   <c>false</c> for implicit grant requests; otherwise, <c>true</c>.
		/// </value>
		bool ClientAuthenticated { get; }

		/// <summary>
		/// Gets the identifier of the client authorized to access protected data.
		/// </summary>
		string ClientIdentifier { get; }

		/// <summary>
		/// Gets the username of the authorizing user, when applicable.
		/// </summary>
		/// <value>A non-empty string; or <c>null</c> when no user has authorized this access token.</value>
		string UserName { get; }

		/// <summary>
		/// Gets the scope of operations the client is allowed to invoke.
		/// </summary>
		HashSet<string> Scope { get; }
	}
}
