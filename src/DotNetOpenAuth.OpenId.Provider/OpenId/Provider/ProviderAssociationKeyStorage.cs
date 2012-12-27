//-----------------------------------------------------------------------
// <copyright file="ProviderAssociationKeyStorage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using Validation;

	/// <summary>
	/// An association storage mechanism that stores the association secrets in a private store,
	/// and returns randomly generated association handles to refer to these secrets.
	/// </summary>
	internal class ProviderAssociationKeyStorage : IProviderAssociationStore {
		/// <summary>
		/// The bucket to use when recording shared associations.
		/// </summary>
		internal const string SharedAssociationBucket = "https://localhost/dnoa/shared_associations";

		/// <summary>
		/// The bucket to use when recording private associations.
		/// </summary>
		internal const string PrivateAssociationBucket = "https://localhost/dnoa/private_associations";

		/// <summary>
		/// The backing crypto key store.
		/// </summary>
		private readonly ICryptoKeyStore cryptoKeyStore;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProviderAssociationKeyStorage"/> class.
		/// </summary>
		/// <param name="cryptoKeyStore">The store where association secrets will be recorded.</param>
		internal ProviderAssociationKeyStorage(ICryptoKeyStore cryptoKeyStore) {
			Requires.NotNull(cryptoKeyStore, "cryptoKeyStore");
			this.cryptoKeyStore = cryptoKeyStore;
		}

		/// <summary>
		/// Stores an association and returns a handle for it.
		/// </summary>
		/// <param name="secret">The association secret.</param>
		/// <param name="expiresUtc">The UTC time that the association should expire.</param>
		/// <param name="privateAssociation">A value indicating whether this is a private association.</param>
		/// <returns>
		/// The association handle that represents this association.
		/// </returns>
		public string Serialize(byte[] secret, DateTime expiresUtc, bool privateAssociation) {
			string handle;
			this.cryptoKeyStore.StoreKey(
				privateAssociation ? PrivateAssociationBucket : SharedAssociationBucket,
				handle = OpenIdUtilities.GenerateRandomAssociationHandle(),
				new CryptoKey(secret, expiresUtc));
			return handle;
		}

		/// <summary>
		/// Retrieves an association given an association handle.
		/// </summary>
		/// <param name="containingMessage">The OpenID message that referenced this association handle.</param>
		/// <param name="isPrivateAssociation">A value indicating whether a private association is expected.</param>
		/// <param name="handle">The association handle.</param>
		/// <returns>
		/// An association instance, or <c>null</c> if the association has expired or the signature is incorrect (which may be because the OP's symmetric key has changed).
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if the association is not of the expected type.</exception>
		public Association Deserialize(IProtocolMessage containingMessage, bool isPrivateAssociation, string handle) {
			var key = this.cryptoKeyStore.GetKey(isPrivateAssociation ? PrivateAssociationBucket : SharedAssociationBucket, handle);
			if (key != null) {
				return Association.Deserialize(handle, key.ExpiresUtc, key.Key);
			}

			return null;
		}
	}
}
