//-----------------------------------------------------------------------
// <copyright file="ProviderAssociationStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The OpenID Provider association serializer, used to encode/decode association handles.
	/// </summary>
	internal class ProviderAssociationStore {
		/// <summary>
		/// Initializes a new instance of the <see cref="ProviderAssociationStore"/> class.
		/// </summary>
		internal ProviderAssociationStore() {
			this.Secret = MessagingUtilities.GetCryptoRandomData(16);
		}

		/// <summary>
		/// Gets or sets the symmetric secret this Provider uses for protecting messages to itself.
		/// </summary>
		internal byte[] Secret { get; set; }

		/// <summary>
		/// Encodes the specified association data bag.
		/// </summary>
		/// <param name="associationDataBag">The association data bag.</param>
		/// <returns>The association handle that represents this association.</returns>
		internal string Encode(AssociationDataBag associationDataBag) {
			var formatter = AssociationDataBag.CreateFormatter(this.Secret);
			return formatter.Serialize(associationDataBag);
		}

		/// <summary>
		/// Retrieves an association given an association handle.
		/// </summary>
		/// <param name="containingMessage">The OpenID message that referenced this association handle.</param>
		/// <param name="associationType">The expected type of the retrieved association.</param>
		/// <param name="handle">The association handle.</param>
		/// <returns>An association instance, or <c>null</c> if the association has expired or the signature is incorrect (which may be because the OP's symmetric key has changed).</returns>
		/// <exception cref="ProtocolException">Thrown if the association is not of the expected type.</exception>
		internal Association Decode(IProtocolMessage containingMessage, AssociationRelyingPartyType associationType, string handle) {
			var formatter = AssociationDataBag.CreateFormatter(this.Secret);
			AssociationDataBag bag;
			try {
				bag = formatter.Deserialize(containingMessage, handle);
			} catch (ProtocolException) {
				return null;
			}

			ErrorUtilities.VerifyProtocol(bag.AssociationType == associationType, "Unexpected association type.");
			Association assoc = Association.Deserialize(handle, bag.ExpiresUtc, bag.Secret);
			return assoc.IsExpired ? null : assoc;
		}

		/// <summary>
		/// Determines whether the association with the specified handle is (still) valid.
		/// </summary>
		/// <param name="containingMessage">The OpenID message that referenced this association handle.</param>
		/// <param name="associationType">The expected type of the retrieved association.</param>
		/// <param name="handle">The association handle.</param>
		/// <returns>
		///   <c>true</c> if the specified containing message is valid; otherwise, <c>false</c>.
		/// </returns>
		internal bool IsValid(IProtocolMessage containingMessage, AssociationRelyingPartyType associationType, string handle) {
			try {
				return this.Decode(containingMessage, associationType, handle) != null;
			} catch (ProtocolException) {
				return false;
			}
		}
	}
}
