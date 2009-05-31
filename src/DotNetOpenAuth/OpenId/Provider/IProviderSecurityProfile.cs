//-----------------------------------------------------------------------
// <copyright file="IProviderSecurityProfile.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using DotNetOpenAuth.OpenId.ChannelElements;

	/// <summary>
	/// Applies a custom security policy to certain OpenID security settings and behaviors.
	/// </summary>
	/// <remarks>
	/// BEFORE MARKING THIS INTERFACE PUBLIC: it's very important that we shift the methods to be channel-level
	/// rather than facade class level and for the OpenIdChannel to be the one to invoke these methods.
	/// </remarks>
	internal interface IProviderSecurityProfile : ISecurityProfile {
		/// <summary>
		/// Called when a request is received by the Provider.
		/// </summary>
		/// <param name="provider">The provider.</param>
		/// <param name="request">The incoming request.</param>
		void OnIncomingRequest(OpenIdProvider provider, IRequest request);

		/// <summary>
		/// Called when the Provider is preparing to send a response to an authentication request.
		/// </summary>
		/// <param name="provider">The provider.</param>
		/// <param name="request">The request that is configured to generate the outgoing response.</param>
		void OnOutgoingResponse(OpenIdProvider provider, IAuthenticationRequest request);
	}
}
