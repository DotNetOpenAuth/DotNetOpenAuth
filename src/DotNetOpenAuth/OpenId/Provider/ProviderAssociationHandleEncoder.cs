//-----------------------------------------------------------------------
// <copyright file="ProviderAssociationHandleEncoder.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Diagnostics.Contracts;
	using System.Threading;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Provides association storage in the association handle itself, but embedding signed and encrypted association
	/// details in the handle.
	/// </summary>
	public class ProviderAssociationHandleEncoder : IProviderAssociationStore {
		internal const string AssociationHandleEncodingSecretBucket = "https://localhost/dnoa/association_handles";

		private readonly ICryptoKeyStore cryptoKeyStore;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProviderAssociationHandleEncoder"/> class.
		/// </summary>
		public ProviderAssociationHandleEncoder(ICryptoKeyStore cryptoKeyStore) {
			Contract.Requires<ArgumentNullException>(cryptoKeyStore != null, "cryptoKeyStore");
			this.cryptoKeyStore = cryptoKeyStore;
		}

		/// <summary>
		/// Encodes the specified association data bag.
		/// </summary>
		/// <param name="secret">The symmetric secret.</param>
		/// <param name="expiresUtc">The UTC time that the association should expire.</param>
		/// <param name="isPrivateAssociation">A value indicating whether this is a private association.</param>
		/// <returns>
		/// The association handle that represents this association.
		/// </returns>
		public string Serialize(byte[] secret, DateTime expiresUtc, bool isPrivateAssociation) {
			var associationDataBag = new AssociationDataBag {
				Secret = secret,
				IsPrivateAssociation = isPrivateAssociation,
				ExpiresUtc = expiresUtc,
			};

			var encodingSecret = this.cryptoKeyStore.GetCurrentKey(AssociationHandleEncodingSecretBucket, DotNetOpenAuthSection.Configuration.OpenId.MaxAuthenticationTime);
			var formatter = AssociationDataBag.CreateFormatter(encodingSecret.Value.Key);
			return encodingSecret.Key + "!" + formatter.Serialize(associationDataBag);
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
			int privateHandleIndex = handle.IndexOf('!');
			ErrorUtilities.VerifyProtocol(privateHandleIndex > 0, MessagingStrings.UnexpectedMessagePartValue, containingMessage.GetProtocol().openid.assoc_handle, handle);
			string privateHandle = handle.Substring(0, privateHandleIndex);
			string encodedHandle = handle.Substring(privateHandleIndex + 1);
			var encodingSecret = this.cryptoKeyStore.GetKey(AssociationHandleEncodingSecretBucket, privateHandle);
			if (encodingSecret == null) {
				Logger.OpenId.Error("Rejecting an association because the symmetric secret it was encoded with is missing or has expired.");
				return null;
			}

			var formatter = AssociationDataBag.CreateFormatter(encodingSecret.Key);
			AssociationDataBag bag;
			try {
				bag = formatter.Deserialize(containingMessage, encodedHandle);
			} catch (ProtocolException) {
				return null;
			}

			ErrorUtilities.VerifyProtocol(bag.IsPrivateAssociation == isPrivateAssociation, "Unexpected association type.");
			Association assoc = Association.Deserialize(handle, bag.ExpiresUtc, bag.Secret);
			return assoc.IsExpired ? null : assoc;
		}
	}
}
