//-----------------------------------------------------------------------
// <copyright file="CryptoKeyStoreAsRelyingPartyAssociationStore.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Linq;
	using DotNetOpenAuth.Messaging.Bindings;
	using Validation;

	/// <summary>
	/// Wraps a standard <see cref="ICryptoKeyStore"/> so that it behaves as an association store.
	/// </summary>
	internal class CryptoKeyStoreAsRelyingPartyAssociationStore : IRelyingPartyAssociationStore {
		/// <summary>
		/// The underlying key store.
		/// </summary>
		private readonly ICryptoKeyStore keyStore;

		/// <summary>
		/// Initializes a new instance of the <see cref="CryptoKeyStoreAsRelyingPartyAssociationStore"/> class.
		/// </summary>
		/// <param name="keyStore">The key store.</param>
		internal CryptoKeyStoreAsRelyingPartyAssociationStore(ICryptoKeyStore keyStore) {
			Requires.NotNull(keyStore, "keyStore");
			this.keyStore = keyStore;
		}

		/// <summary>
		/// Saves an <see cref="Association"/> for later recall.
		/// </summary>
		/// <param name="providerEndpoint">The OP Endpoint with which the association is established.</param>
		/// <param name="association">The association to store.</param>
		public void StoreAssociation(Uri providerEndpoint, Association association) {
			var cryptoKey = new CryptoKey(association.SerializePrivateData(), association.Expires);
			this.keyStore.StoreKey(providerEndpoint.AbsoluteUri, association.Handle, cryptoKey);
		}

		/// <summary>
		/// Gets the best association (the one with the longest remaining life) for a given key.
		/// </summary>
		/// <param name="providerEndpoint">The OP Endpoint with which the association is established.</param>
		/// <param name="securityRequirements">The security requirements that the returned association must meet.</param>
		/// <returns>
		/// The requested association, or null if no unexpired <see cref="Association"/>s exist for the given key.
		/// </returns>
		public Association GetAssociation(Uri providerEndpoint, SecuritySettings securityRequirements) {
			var matches = from cryptoKey in this.keyStore.GetKeys(providerEndpoint.AbsoluteUri)
						  where cryptoKey.Value.ExpiresUtc > DateTime.UtcNow
						  orderby cryptoKey.Value.ExpiresUtc descending
						  let assoc = Association.Deserialize(cryptoKey.Key, cryptoKey.Value.ExpiresUtc, cryptoKey.Value.Key)
						  where assoc.HashBitLength >= securityRequirements.MinimumHashBitLength
						  where assoc.HashBitLength <= securityRequirements.MaximumHashBitLength
						  select assoc;
			return matches.FirstOrDefault();
		}

		/// <summary>
		/// Gets the association for a given key and handle.
		/// </summary>
		/// <param name="providerEndpoint">The OP Endpoint with which the association is established.</param>
		/// <param name="handle">The handle of the specific association that must be recalled.</param>
		/// <returns>
		/// The requested association, or null if no unexpired <see cref="Association"/>s exist for the given key and handle.
		/// </returns>
		public Association GetAssociation(Uri providerEndpoint, string handle) {
			var cryptoKey = this.keyStore.GetKey(providerEndpoint.AbsoluteUri, handle);
			return cryptoKey != null ? Association.Deserialize(handle, cryptoKey.ExpiresUtc, cryptoKey.Key) : null;
		}

		/// <summary>
		/// Removes a specified handle that may exist in the store.
		/// </summary>
		/// <param name="providerEndpoint">The OP Endpoint with which the association is established.</param>
		/// <param name="handle">The handle of the specific association that must be deleted.</param>
		/// <returns>
		/// True if the association existed in this store previous to this call.
		/// </returns>
		public bool RemoveAssociation(Uri providerEndpoint, string handle) {
			this.keyStore.RemoveKey(providerEndpoint.AbsoluteUri, handle);
			return true; // return value isn't used by DNOA.
		}
	}
}
