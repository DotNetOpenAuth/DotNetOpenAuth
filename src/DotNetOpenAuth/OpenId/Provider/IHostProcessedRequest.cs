//-----------------------------------------------------------------------
// <copyright file="IHostProcessedRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Interface exposing incoming messages to the OpenID Provider that
	/// require interaction with the host site.
	/// </summary>
	public interface IHostProcessedRequest : IRequest {
		/// <summary>
		/// Gets the version of OpenID being used by the relying party that sent the request.
		/// </summary>
		ProtocolVersion RelyingPartyVersion { get; }

		/// <summary>
		/// Gets the URL the consumer site claims to use as its 'base' address.
		/// </summary>
		Realm Realm { get; }

		/// <summary>
		/// Gets a value indicating whether the consumer demands an immediate response.
		/// If false, the consumer is willing to wait for the identity provider
		/// to authenticate the user.
		/// </summary>
		bool Immediate { get; }

		/// <summary>
		/// Attempts to perform relying party discovery of the return URL claimed by the Relying Party.
		/// </summary>
		/// <param name="requestHandler">The request handler to use to perform relying party discovery.</param>
		/// <returns>
		/// 	The details of how successful the relying party discovery was.
		/// </returns>
		/// <remarks>
		/// 	<para>Return URL verification is only attempted if this method is called.</para>
		/// 	<para>See OpenID Authentication 2.0 spec section 9.2.1.</para>
		/// </remarks>
		RelyingPartyDiscoveryResult IsReturnUrlDiscoverable(IDirectWebRequestHandler requestHandler);
	}
}
