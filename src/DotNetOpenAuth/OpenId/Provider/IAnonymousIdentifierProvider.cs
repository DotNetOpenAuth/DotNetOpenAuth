//-----------------------------------------------------------------------
// <copyright file="IAnonymousIdentifierProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
using System.Diagnostics.Contracts;

	/// <summary>
	/// Services for generating and consuming anonymous OpenID identifiers.
	/// </summary>
	[ContractClass(typeof(IAnonymousIdentifierProviderContract))]
	public interface IAnonymousIdentifierProvider {
		/// <summary>
		/// Gets the anonymous identifier for some user.
		/// </summary>
		/// <param name="localIdentifier">The OP local identifier for the authenticating user.</param>
		/// <param name="relyingPartyRealm">The realm of the relying party requesting authentication.  May be null if a pairwise-unique identifier based on the realm is not desired.</param>
		/// <returns>
		/// A discoverable OpenID Claimed Identifier that gives no hint regarding the real identity of the controlling user.
		/// </returns>
		Uri GetAnonymousIdentifier(Identifier localIdentifier, Realm relyingPartyRealm);
	}

	[ContractClassFor(typeof(IAnonymousIdentifierProvider))]
	internal abstract class IAnonymousIdentifierProviderContract : IAnonymousIdentifierProvider {
		private IAnonymousIdentifierProviderContract() {
		}

		#region IAnonymousIdentifierProvider Members

		Uri IAnonymousIdentifierProvider.GetAnonymousIdentifier(Identifier localIdentifier, Realm relyingPartyRealm) {
			Contract.Requires(localIdentifier != null);
			Contract.Ensures(Contract.Result<Uri>() != null);
			throw new NotImplementedException();
		}
		#endregion
	}
}
