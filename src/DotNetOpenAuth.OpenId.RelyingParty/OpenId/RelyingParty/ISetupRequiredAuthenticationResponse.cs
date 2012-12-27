//-----------------------------------------------------------------------
// <copyright file="ISetupRequiredAuthenticationResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;

	/// <summary>
	/// An interface to expose useful properties and functionality for handling
	/// authentication responses that are returned from Immediate authentication
	/// requests that require a subsequent request to be made in non-immediate mode.
	/// </summary>
	public interface ISetupRequiredAuthenticationResponse {
		/// <summary>
		/// Gets the <see cref="Identifier"/> to pass to <see cref="OpenIdRelyingParty.CreateRequest(Identifier)"/>
		/// in a subsequent authentication attempt.
		/// </summary>
		Identifier UserSuppliedIdentifier { get; }
	}
}
