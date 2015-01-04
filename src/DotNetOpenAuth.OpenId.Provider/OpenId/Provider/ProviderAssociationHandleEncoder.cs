//-----------------------------------------------------------------------
// <copyright file="ProviderAssociationHandleEncoder.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Threading;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using Validation;

	/// <summary>
	/// Provides association storage in the association handle itself, by embedding signed and encrypted association
	/// details in the handle.
	/// </summary>
	public class ProviderAssociationHandleEncoder : IProviderAssociationStore {
		/// <summary>
		/// The name of the bucket in which to store keys that encrypt association data into association handles.
		/// </summary>
		internal const string AssociationHandleEncodingSecretBucket = "https://localhost/dnoa/association_handles";

		/// <summary>
		/// The crypto key store used to persist encryption keys.
		/// </summary>
		private readonly ICryptoKeyStore cryptoKeyStore;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProviderAssociationHandleEncoder"/> class.
		/// </summary>
		/// <param name="cryptoKeyStore">The crypto key store.</param>
		public ProviderAssociationHandleEncoder(ICryptoKeyStore cryptoKeyStore) {
			Requires.NotNull(cryptoKeyStore, "cryptoKeyStore");
			this.cryptoKeyStore = cryptoKeyStore;
		}

		/// <summary>
		/// Encodes the specified association data bag.
		/// </summary>
		/// <param name="secret">The symmetric secret.</param>
		/// <param name="expiresUtc">The UTC time that the association should expire.</param>
		/// <param name="privateAssociation">A value indicating whether this is a private association.</param>
		/// <returns>
		/// The association handle that represents this association.
		/// </returns>
		public string Serialize(byte[] secret, DateTime expiresUtc, bool privateAssociation) {
			var associationDataBag = new AssociationDataBag {
				Secret = secret,
				IsPrivateAssociation = privateAssociation,
				ExpiresUtc = expiresUtc,
			};

			var formatter = AssociationDataBag.CreateFormatter(this.cryptoKeyStore, AssociationHandleEncodingSecretBucket, expiresUtc - DateTime.UtcNow);
			return formatter.Serialize(associationDataBag);
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
		public Association Deserialize(IProtocolMessage containingMessage, bool privateAssociation, string handle) {
			var formatter = AssociationDataBag.CreateFormatter(this.cryptoKeyStore, AssociationHandleEncodingSecretBucket);
			AssociationDataBag bag = new AssociationDataBag();
			try {
				formatter.Deserialize(bag, handle, containingMessage, Protocol.Default.openid.assoc_handle);
			} catch (ProtocolException ex) {
				Logger.OpenId.ErrorException("Rejecting an association because deserialization of the encoded handle failed.", ex);
				return null;
			}

			ErrorUtilities.VerifyProtocol(bag.IsPrivateAssociation == privateAssociation, "Unexpected association type.");
			Association assoc = Association.Deserialize(handle, bag.ExpiresUtc, bag.Secret);
			return assoc.IsExpired ? null : assoc;
		}
	}
}
