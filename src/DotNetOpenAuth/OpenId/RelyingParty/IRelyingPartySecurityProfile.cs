//-----------------------------------------------------------------------
// <copyright file="IRelyingPartySecurityProfile.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	/// <summary>
	/// Applies a custom security policy to certain OpenID security settings and behaviors.
	/// </summary>
	/// <remarks>
	/// BEFORE MARKING THIS INTERFACE PUBLIC: it's very important that we shift the methods to be channel-level
	/// rather than facade class level and for the OpenIdChannel to be the one to invoke these methods.
	/// </remarks>
	internal interface IRelyingPartySecurityProfile : ISecurityProfile {
		/// <summary>
		/// Called when an authentication request is about to be sent.
		/// </summary>
		/// <param name="relyingParty">The relying party.</param>
		/// <param name="request">The request.</param>
		/// <remarks>
		/// Implementations should be prepared to be called multiple times on the same outgoing message
		/// without malfunctioning.
		/// </remarks>
		void OnOutgoingAuthenticationRequest(OpenIdRelyingParty relyingParty, IAuthenticationRequest request);

		/// <summary>
		/// Called when an incoming positive assertion is received.
		/// </summary>
		/// <param name="relyingParty">The relying party.</param>
		/// <param name="assertion">The positive assertion.</param>
		void OnIncomingPositiveAssertion(OpenIdRelyingParty relyingParty, IAuthenticationResponse assertion);
	}
}
