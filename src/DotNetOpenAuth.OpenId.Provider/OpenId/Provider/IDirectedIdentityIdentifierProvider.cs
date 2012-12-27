//-----------------------------------------------------------------------
// <copyright file="IDirectedIdentityIdentifierProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Diagnostics.Contracts;
	using Validation;

	/// <summary>
	/// An interface to provide custom identifiers for users logging into specific relying parties.
	/// </summary>
	/// <remarks>
	/// This interface would allow, for example, the Provider to offer PPIDs to their users,
	/// allowing the users to log into RPs without leaving any clue as to their true identity,
	/// and preventing multiple RPs from colluding to track user activity across realms.
	/// </remarks>
	public interface IDirectedIdentityIdentifierProvider {
		/// <summary>
		/// Gets the Identifier to use for the Claimed Identifier and Local Identifier of
		/// an outgoing positive assertion.
		/// </summary>
		/// <param name="localIdentifier">The OP local identifier for the authenticating user.</param>
		/// <param name="relyingPartyRealm">The realm of the relying party receiving the assertion.</param>
		/// <returns>
		/// A valid, discoverable OpenID Identifier that should be used as the value for the
		/// openid.claimed_id and openid.local_id parameters.  Must not be null.
		/// </returns>
		Uri GetIdentifier(Identifier localIdentifier, Realm relyingPartyRealm);

		/// <summary>
		/// Determines whether a given identifier is the primary (non-PPID) local identifier for some user.
		/// </summary>
		/// <param name="identifier">The identifier in question.</param>
		/// <returns>
		/// 	<c>true</c> if the given identifier is the valid, unique identifier for some uesr (and NOT a PPID); otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		bool IsUserLocalIdentifier(Identifier identifier);
	}
}
