//-----------------------------------------------------------------------
// <copyright file="OpenIdUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A set of utilities especially useful to OpenID.
	/// </summary>
	internal static class OpenIdUtilities {
		/// <summary>
		/// Gets the OpenID protocol instance for the version in a message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>The OpenID protocol instance.</returns>
		internal static Protocol GetProtocol(this IProtocolMessage message) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");
			return Protocol.Lookup(message.Version);
		}
	}
}
