//-----------------------------------------------------------------------
// <copyright file="ProviderAssociationKeyStorage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;

	internal class ProviderAssociationKeyStorage : IProviderAssociationStore {
		private const string SharedAssociationBucket = "https://localhost/dnoa/shared_associations";

		private const string PrivateAssociationBucket = "https://localhost/dnoa/private_associations";

		private readonly ICryptoKeyStore cryptoKeyStore;

		internal ProviderAssociationKeyStorage(ICryptoKeyStore cryptoKeyStore) {
			Contract.Requires<ArgumentNullException>(cryptoKeyStore != null, "cryptoKeyStore");
			this.cryptoKeyStore = cryptoKeyStore;
		}

		public string Serialize(byte[] secret, DateTime expiresUtc, bool privateAssociation) {
			string handle;
			this.cryptoKeyStore.StoreKey(
				privateAssociation ? PrivateAssociationBucket : SharedAssociationBucket,
				handle = OpenIdUtilities.GenerateRandomAssociationHandle(),
				new CryptoKey(secret, expiresUtc));
			return handle;
		}

		public Association Deserialize(IProtocolMessage containingMessage, bool isPrivateAssociation, string handle) {
			var key = this.cryptoKeyStore.GetKey(isPrivateAssociation ? PrivateAssociationBucket : SharedAssociationBucket, handle);
			if (key != null) {
				return Association.Deserialize(handle, key.ExpiresUtc, key.Key);
			}

			return null;
		}
	}
}
