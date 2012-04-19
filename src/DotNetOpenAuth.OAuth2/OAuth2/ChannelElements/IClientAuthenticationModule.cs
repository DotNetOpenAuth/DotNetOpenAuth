//-----------------------------------------------------------------------
// <copyright file="IClientAuthenticationModule.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An interface implemented by extension that can read incoming messages and extract the client identifier and
	/// possibly authentication information (like a shared secret, signed nonce, etc.)
	/// </summary>
	public interface IClientAuthenticationModule {
		/// <summary>
		/// Attempts to extract client identification/authentication information from a message.
		/// </summary>
		/// <param name="requestMessage">The incoming message.  Always an instance of <see cref="AuthenticatedClientRequestBase"/></param>
		/// <param name="clientIdentifier">Receives the client identifier, if one was found.</param>
		/// <returns>The level of the extracted client information.</returns>
		ClientAuthenticationResult TryAuthenticateClient(IDirectedProtocolMessage requestMessage, out string clientIdentifier);
	}
}
