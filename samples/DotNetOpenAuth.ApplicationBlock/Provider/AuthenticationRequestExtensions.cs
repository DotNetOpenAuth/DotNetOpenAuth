using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.OpenId;

namespace DotNetOpenAuth.ApplicationBlock.Provider {
	public static class AuthenticationRequestExtensions {
		/// <summary>
		/// Removes all personally identifiable information from the positive assertion.
		/// </summary>
		/// <remarks>
		/// The openid.claimed_id and openid.identity values are hashed.
		/// </remarks>
		public static void ScrubPersonallyIdentifiableInformation(this IAuthenticationRequest request, Identifier localIdentifier, AnonymousIdentifierProviderBase anonymousIdentifierProvider, bool pairwiseUnique) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}
			if (!request.IsDirectedIdentity) {
				throw new InvalidOperationException("This operation is supported only under identifier select (directed identity) scenarios.");
			}
			if (anonymousIdentifierProvider == null) {
				throw new ArgumentNullException("anonymousIdentifierProvider");
			}
			if (localIdentifier == null) {
				throw new ArgumentNullException("localIdentifier");
			}

			// When generating the anonymous identifiers, the openid.identity and openid.claimed_id
			// will always end up with matching values.
			var anonymousIdentifier = anonymousIdentifierProvider.GetAnonymousIdentifier(localIdentifier, pairwiseUnique ? request.Realm : null);
			request.ClaimedIdentifier = anonymousIdentifier;
			request.LocalIdentifier = anonymousIdentifier;
		}
	}
}
