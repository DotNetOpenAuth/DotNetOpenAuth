//-----------------------------------------------------------------------
// <copyright file="ProviderAssociationHandleEncoder.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Threading;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Provides association storage in the association handle itself, but embedding signed and encrypted association
	/// details in the handle.
	/// </summary>
	public class ProviderAssociationHandleEncoder : IProviderAssociationStore {
		/// <summary>
		/// The thread synchronization object.
		/// </summary>
		private readonly object syncObject = new object();

		/// <summary>
		/// Backing field for the <see cref="Secret"/> property.
		/// </summary>
		private byte[] secret;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProviderAssociationHandleEncoder"/> class.
		/// </summary>
		public ProviderAssociationHandleEncoder() {
		}

		/// <summary>
		/// Gets or sets the symmetric secret this Provider uses for protecting messages to itself.
		/// </summary>
		/// <remarks>
		/// If the value is not set by the time this property is requested, a random key will be generated.
		/// </remarks>
		public byte[] Secret {
			get {
				if (this.secret == null) {
					lock (this.syncObject) {
						if (this.secret == null) {
							Logger.OpenId.Info("Generating a symmetric secret for signing and encrypting association handles.");
							this.secret = MessagingUtilities.GetCryptoRandomData(32); // 256-bit symmetric key protects association secrets.
						}
					}
				}

				return this.secret;
			}

			set {
				ErrorUtilities.VerifyOperation(this.secret == null, "The symmetric secret has already been set.");
				this.secret = value;
			}
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
			var formatter = AssociationDataBag.CreateFormatter(this.Secret);
			return formatter.Serialize(associationDataBag);
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
			var formatter = AssociationDataBag.CreateFormatter(this.Secret);
			AssociationDataBag bag;
			try {
				bag = formatter.Deserialize(containingMessage, handle);
			} catch (ProtocolException) {
				return null;
			}

			ErrorUtilities.VerifyProtocol(bag.IsPrivateAssociation == isPrivateAssociation, "Unexpected association type.");
			Association assoc = Association.Deserialize(handle, bag.ExpiresUtc, bag.Secret);
			return assoc.IsExpired ? null : assoc;
		}
	}
}
