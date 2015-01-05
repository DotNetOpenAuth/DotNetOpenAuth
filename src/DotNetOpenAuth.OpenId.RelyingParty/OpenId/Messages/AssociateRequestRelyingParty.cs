//-----------------------------------------------------------------------
// <copyright file="AssociateRequestRelyingParty.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Validation;

	/// <summary>
	/// Utility methods for requesting associations from the relying party.
	/// </summary>
	internal static class AssociateRequestRelyingParty {
		/// <summary>
		/// Creates an association request message that is appropriate for a given Provider.
		/// </summary>
		/// <param name="securityRequirements">The set of requirements the selected association type must comply to.</param>
		/// <param name="provider">The provider to create an association with.</param>
		/// <returns>
		/// The message to send to the Provider to request an association.
		/// Null if no association could be created that meet the security requirements
		/// and the provider OpenID version.
		/// </returns>
		internal static AssociateRequest Create(SecuritySettings securityRequirements, IProviderEndpoint provider) {
			Requires.NotNull(securityRequirements, "securityRequirements");
			Requires.NotNull(provider, "provider");

			// Apply our knowledge of the endpoint's transport, OpenID version, and
			// security requirements to decide the best association.
			bool unencryptedAllowed = provider.Uri.IsTransportSecure();
			bool useDiffieHellman = !unencryptedAllowed;
			string associationType, sessionType;
			if (!HmacShaAssociation.TryFindBestAssociation(Protocol.Lookup(provider.Version), true, securityRequirements, useDiffieHellman, out associationType, out sessionType)) {
				// There are no associations that meet all requirements.
				Logger.OpenId.Warn("Security requirements and protocol combination knock out all possible association types.  Dumb mode forced.");
				return null;
			}

			return Create(securityRequirements, provider, associationType, sessionType);
		}

		/// <summary>
		/// Creates an association request message that is appropriate for a given Provider.
		/// </summary>
		/// <param name="securityRequirements">The set of requirements the selected association type must comply to.</param>
		/// <param name="provider">The provider to create an association with.</param>
		/// <param name="associationType">Type of the association.</param>
		/// <param name="sessionType">Type of the session.</param>
		/// <returns>
		/// The message to send to the Provider to request an association.
		/// Null if no association could be created that meet the security requirements
		/// and the provider OpenID version.
		/// </returns>
		internal static AssociateRequest Create(SecuritySettings securityRequirements, IProviderEndpoint provider, string associationType, string sessionType) {
			Requires.NotNull(securityRequirements, "securityRequirements");
			Requires.NotNull(provider, "provider");
			Requires.NotNullOrEmpty(associationType, "associationType");
			Requires.NotNull(sessionType, "sessionType");

			bool unencryptedAllowed = provider.Uri.IsTransportSecure();
			if (unencryptedAllowed) {
				var associateRequest = new AssociateUnencryptedRequest(provider.Version, provider.Uri);
				associateRequest.AssociationType = associationType;
				return associateRequest;
			} else {
				if (OpenIdUtilities.IsDiffieHellmanPresent) {
					var associateRequest = new AssociateDiffieHellmanRequest(provider.Version, provider.Uri);
					associateRequest.AssociationType = associationType;
					associateRequest.SessionType = sessionType;
					associateRequest.InitializeRequest();
					return associateRequest;
				} else {
					return null;
				}
			}
		}
	}
}
