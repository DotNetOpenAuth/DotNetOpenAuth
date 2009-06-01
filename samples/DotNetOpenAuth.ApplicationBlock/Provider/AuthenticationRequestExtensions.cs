namespace DotNetOpenAuth.ApplicationBlock.Provider {
	using System;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;

	public static class AuthenticationRequestExtensions {
		/// <summary>
		/// Removes all personally identifiable information from the positive assertion.
		/// </summary>
		/// <param name="request">The incoming authentication request.</param>
		/// <param name="localIdentifier">The OP local identifier, before the anonymous hash is applied to it.</param>
		/// <param name="anonymousIdentifierProvider">The anonymous identifier provider.</param>
		/// <remarks>
		/// The openid.claimed_id and openid.identity values are hashed.
		/// </remarks>
		public static void ScrubPersonallyIdentifiableInformation(this IAuthenticationRequest request, Identifier localIdentifier, IDirectedIdentityIdentifierProvider anonymousIdentifierProvider) {
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
			var anonymousIdentifier = anonymousIdentifierProvider.GetIdentifier(localIdentifier, request.Realm);
			request.ClaimedIdentifier = anonymousIdentifier;
			request.LocalIdentifier = anonymousIdentifier;
		}
	}
}
