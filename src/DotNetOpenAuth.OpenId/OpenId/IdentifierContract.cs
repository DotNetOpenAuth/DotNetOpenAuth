//-----------------------------------------------------------------------
// <copyright file="IdentifierContract.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Code Contract for the <see cref="Identifier"/> class.
	/// </summary>
	[ContractClassFor(typeof(Identifier))]
	internal abstract class IdentifierContract : Identifier {
		/// <summary>
		/// Prevents a default instance of the IdentifierContract class from being created.
		/// </summary>
		private IdentifierContract()
			: base(null, false) {
		}

		/// <summary>
		/// Returns an <see cref="Identifier"/> that has no URI fragment.
		/// Quietly returns the original <see cref="Identifier"/> if it is not
		/// a <see cref="UriIdentifier"/> or no fragment exists.
		/// </summary>
		/// <returns>
		/// A new <see cref="Identifier"/> instance if there was a
		/// fragment to remove, otherwise this same instance..
		/// </returns>
		internal override Identifier TrimFragment() {
			Contract.Ensures(Contract.Result<Identifier>() != null);
			throw new NotImplementedException();
		}

		/// <summary>
		/// Converts a given identifier to its secure equivalent.
		/// UriIdentifiers originally created with an implied HTTP scheme change to HTTPS.
		/// Discovery is made to require SSL for the entire resolution process.
		/// </summary>
		/// <param name="secureIdentifier">The newly created secure identifier.
		/// If the conversion fails, <paramref name="secureIdentifier"/> retains
		/// <i>this</i> identifiers identity, but will never discover any endpoints.</param>
		/// <returns>
		/// True if the secure conversion was successful.
		/// False if the Identifier was originally created with an explicit HTTP scheme.
		/// </returns>
		internal override bool TryRequireSsl(out Identifier secureIdentifier) {
			Contract.Ensures(Contract.ValueAtReturn(out secureIdentifier) != null);
			throw new NotImplementedException();
		}
	}
}
