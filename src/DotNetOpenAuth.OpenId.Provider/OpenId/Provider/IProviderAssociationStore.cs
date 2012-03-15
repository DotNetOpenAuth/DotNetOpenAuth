//-----------------------------------------------------------------------
// <copyright file="IProviderAssociationStore.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Provides association serialization and deserialization.
	/// </summary>
	/// <remarks>
	/// Implementations may choose to store the association details in memory or a database table and simply return a
	/// short, randomly generated string that is the key to that data.  Alternatively, an implementation may
	/// sign and encrypt the association details and then encode the results as a base64 string and return that value
	/// as the association handle, thereby avoiding any association persistence at the OpenID Provider.
	/// When taking the latter approach however, it is of course imperative that the association be encrypted
	/// to avoid disclosing the secret to anyone who sees the association handle, which itself isn't considered to
	/// be confidential.
	/// </remarks>
	[ContractClass(typeof(IProviderAssociationStoreContract))]
	internal interface IProviderAssociationStore {
		/// <summary>
		/// Stores an association and returns a handle for it.
		/// </summary>
		/// <param name="secret">The association secret.</param>
		/// <param name="expiresUtc">The UTC time that the association should expire.</param>
		/// <param name="privateAssociation">A value indicating whether this is a private association.</param>
		/// <returns>
		/// The association handle that represents this association.
		/// </returns>
		string Serialize(byte[] secret, DateTime expiresUtc, bool privateAssociation);

		/// <summary>
		/// Retrieves an association given an association handle.
		/// </summary>
		/// <param name="containingMessage">The OpenID message that referenced this association handle.</param>
		/// <param name="privateAssociation">A value indicating whether a private association is expected.</param>
		/// <param name="handle">The association handle.</param>
		/// <returns>
		/// An association instance, or <c>null</c> if the association has expired or the signature is incorrect (which may be because the OP's symmetric key has changed).
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if the association is not of the expected type.</exception>
		Association Deserialize(IProtocolMessage containingMessage, bool privateAssociation, string handle);
	}

	/// <summary>
	/// Code contract for the <see cref="IProviderAssociationStore"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IProviderAssociationStore))]
	internal abstract class IProviderAssociationStoreContract : IProviderAssociationStore {
		/// <summary>
		/// Stores an association and returns a handle for it.
		/// </summary>
		/// <param name="secret">The association secret.</param>
		/// <param name="expiresUtc">The expires UTC.</param>
		/// <param name="privateAssociation">A value indicating whether this is a private association.</param>
		/// <returns>
		/// The association handle that represents this association.
		/// </returns>
		string IProviderAssociationStore.Serialize(byte[] secret, DateTime expiresUtc, bool privateAssociation) {
			Requires.NotNull(secret, "secret");
			Requires.True(expiresUtc.Kind == DateTimeKind.Utc, "expiresUtc");
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
			throw new NotImplementedException();
		}

		/// <summary>
		/// Retrieves an association given an association handle.
		/// </summary>
		/// <param name="containingMessage">The OpenID message that referenced this association handle.</param>
		/// <param name="privateAssociation">A value indicating whether a private association is expected.</param>
		/// <param name="handle">The association handle.</param>
		/// <returns>
		/// An association instance, or <c>null</c> if the association has expired or the signature is incorrect (which may be because the OP's symmetric key has changed).
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if the association is not of the expected type.</exception>
		Association IProviderAssociationStore.Deserialize(IProtocolMessage containingMessage, bool privateAssociation, string handle) {
			Requires.NotNull(containingMessage, "containingMessage");
			Requires.NotNullOrEmpty(handle, "handle");
			throw new NotImplementedException();
		}
	}
}
