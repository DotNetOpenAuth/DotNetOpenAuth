//-----------------------------------------------------------------------
// <copyright file="ProviderAssociationStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

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

		internal string Encode(AssociationDataBag associationDataBag) {
			var formatter = AssociationDataBag.CreateFormatter(this.Secret);
			return formatter.Serialize(associationDataBag);
		}

		internal Association Decode(IProtocolMessage containingMessage, AssociationRelyingPartyType associationType, string handle) {
			var formatter = AssociationDataBag.CreateFormatter(this.Secret);
			var bag = formatter.Deserialize(containingMessage, handle);
			ErrorUtilities.VerifyProtocol(bag.AssociationType == associationType, "Unexpected association type.");
			Association assoc = Association.Deserialize(handle, bag.ExpiresUtc, bag.Secret);
			return assoc;
		}

		internal bool IsValid(IProtocolMessage containingMessage, AssociationRelyingPartyType associationType, string handle) {
			try {
				this.Decode(containingMessage, associationType, handle);
				return true;
			} catch (ProtocolException) {
				return false;
			}
		}
	}
}
