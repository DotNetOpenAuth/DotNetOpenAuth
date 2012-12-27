//-----------------------------------------------------------------------
// <copyright file="HmacShaAssociationProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;
	using Validation;

	/// <summary>
	/// OpenID Provider utility methods for HMAC-SHA* associations.
	/// </summary>
	internal static class HmacShaAssociationProvider {
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
			Requires.NotNull(protocol, "protocol");
			Requires.NotNullOrEmpty(associationType, "associationType");
			Requires.NotNull(associationStore, "associationStore");
			Requires.NotNull(securitySettings, "securitySettings");

			int secretLength = HmacShaAssociation.GetSecretLength(protocol, associationType);

			// Generate the secret that will be used for signing
			byte[] secret = MessagingUtilities.GetCryptoRandomData(secretLength);

			TimeSpan lifetime;
			if (associationUse == AssociationRelyingPartyType.Smart) {
				if (!securitySettings.AssociationLifetimes.TryGetValue(associationType, out lifetime)) {
					lifetime = DefaultMaximumLifetime;
				}
			} else {
				lifetime = HmacShaAssociation.DumbSecretLifetime;
			}

			string handle = associationStore.Serialize(secret, DateTime.UtcNow + lifetime, associationUse == AssociationRelyingPartyType.Dumb);

			Assumes.True(protocol != null); // All the way up to the method call, the condition holds, yet we get a Requires failure next
			Assumes.True(secret != null);
			Assumes.True(!string.IsNullOrEmpty(associationType));
			var result = HmacShaAssociation.Create(protocol, associationType, handle, secret, lifetime);
			return result;
		}
	}
}
