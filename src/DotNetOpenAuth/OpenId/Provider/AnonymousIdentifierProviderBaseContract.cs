//-----------------------------------------------------------------------
// <copyright file="AnonymousIdentifierProviderBaseContract.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Diagnostics.Contracts;

	[ContractClassFor(typeof(AnonymousIdentifierProviderBase))]
	internal abstract class AnonymousIdentifierProviderBaseContract : AnonymousIdentifierProviderBase {
		private AnonymousIdentifierProviderBaseContract()
			: base(null) {
		}

		protected override byte[] GetHashSaltForLocalIdentifier(Identifier localIdentifier) {
			Contract.Requires(localIdentifier != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);
			throw new NotImplementedException();
		}
	}
}
