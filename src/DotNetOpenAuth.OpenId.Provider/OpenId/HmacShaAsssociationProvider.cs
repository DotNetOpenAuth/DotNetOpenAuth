//-----------------------------------------------------------------------
// <copyright file="HmacShaAsssociationProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;

	internal static class HmacShaAsssociationProvider : HmacShaAssociation {
		/// <summary>
		/// The default lifetime of a shared association when no lifetime is given
		/// for a specific association type.
		/// </summary>
		private static readonly TimeSpan DefaultMaximumLifetime = TimeSpan.FromDays(14);

		/// <summary>
		/// Creates a new association of a given type at an OpenID Provider.
		/// </summary>
		/// <param name="protocol">The protocol.</param>
		/// <param name="associationType">Type of the association (i.e. HMAC-SHA1 or HMAC-SHA256)</param>
		/// <param name="associationUse">A value indicating whether the new association will be used privately by the Provider for "dumb mode" authentication
		/// or shared with the Relying Party for "smart mode" authentication.</param>
		/// <param name="associationStore">The Provider's association store.</param>
		/// <param name="securitySettings">The security settings of the Provider.</param>
		/// <returns>
		/// The newly created association.
		/// </returns>
		/// <remarks>
		/// The new association is NOT automatically put into an association store.  This must be done by the caller.
		/// </remarks>
		internal static HmacShaAssociation Create(Protocol protocol, string associationType, AssociationRelyingPartyType associationUse, IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings) {
			Contract.Requires<ArgumentNullException>(protocol != null);
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(associationType));
			Contract.Requires<ArgumentNullException>(associationStore != null);
			Contract.Requires<ArgumentNullException>(securitySettings != null);
			Contract.Ensures(Contract.Result<HmacShaAssociation>() != null);

			int secretLength = GetSecretLength(protocol, associationType);

			// Generate the secret that will be used for signing
			byte[] secret = MessagingUtilities.GetCryptoRandomData(secretLength);

			TimeSpan lifetime;
			if (associationUse == AssociationRelyingPartyType.Smart) {
				if (!securitySettings.AssociationLifetimes.TryGetValue(associationType, out lifetime)) {
					lifetime = DefaultMaximumLifetime;
				}
			} else {
				lifetime = DumbSecretLifetime;
			}

			string handle = associationStore.Serialize(secret, DateTime.UtcNow + lifetime, associationUse == AssociationRelyingPartyType.Dumb);

			Contract.Assert(protocol != null); // All the way up to the method call, the condition holds, yet we get a Requires failure next
			Contract.Assert(secret != null);
			Contract.Assert(!String.IsNullOrEmpty(associationType));
			var result = Create(protocol, associationType, handle, secret, lifetime);
			return result;
		}
	}
}
