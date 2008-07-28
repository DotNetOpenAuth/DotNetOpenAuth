using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// An interface to expose useful properties and functionality for handling
	/// authentication responses that are returned from Immediate authentication
	/// requests that require a subsequent request to be made in non-immediate mode.
	/// </summary>
	public interface ISetupRequiredAuthenticationResponse {
		/// <summary>
		/// The <see cref="Identifier"/> to pass to <see cref="OpenIdRelyingParty.CreateRequest(Identifier)"/>
		/// in a subsequent authentication attempt.
		/// </summary>
		/// <remarks>
		/// When directed identity is used, this will be the Provider Identifier given by the user.
		/// Otherwise it will be the Claimed Identifier derived from the user-supplied identifier.
		/// </remarks>
		Identifier ClaimedOrProviderIdentifier { get; }
		/// <summary>
		/// Generates an <see cref="IAuthenticationRequest"/> representing an authentication
		/// request similar to the original, except that instead of using immediate mode it
		/// uses setup mode.
		/// </summary>
		/// <returns>
		/// The generated <see cref="IAuthenticationRequest"/> that can
		/// be used to begin a new setup authentication request.
		/// </returns>
		IAuthenticationRequest CreateSetupRequest();
	}
}
