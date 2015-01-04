//-----------------------------------------------------------------------
// <copyright file="GsaIcamProfile.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider.Behaviors {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Behaviors;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Validation;

	/// <summary>
	/// Implements the Identity, Credential, &amp; Access Management (ICAM) OpenID 2.0 Profile
	/// for the General Services Administration (GSA).
	/// </summary>
	/// <remarks>
	/// <para>Relying parties that include this profile are always held to the terms required by the profile,
	/// but Providers are only affected by the special behaviors of the profile when the RP specifically
	/// indicates that they want to use this profile. </para>
	/// </remarks>
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Icam", Justification = "Acronym")]
	public sealed class GsaIcamProfile : GsaIcamProfileBase, IProviderBehavior {
		/// <summary>
		/// The maximum time a shared association can live.
		/// </summary>
		private static readonly TimeSpan MaximumAssociationLifetime = TimeSpan.FromSeconds(86400);

		/// <summary>
		/// Initializes a new instance of the <see cref="GsaIcamProfile"/> class.
		/// </summary>
		public GsaIcamProfile() {
			if (DisableSslRequirement) {
				Logger.OpenId.Warn("GSA level 1 behavior has its RequireSsl requirement disabled.");
			}
		}

		/// <summary>
		/// Gets or sets the provider for generating PPID identifiers.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ppid", Justification = "Acronym")]
		public static IDirectedIdentityIdentifierProvider PpidIdentifierProvider { get; set; }

		#region IProviderBehavior Members

		/// <summary>
		/// Adapts the default security settings to the requirements of this behavior.
		/// </summary>
		/// <param name="securitySettings">The original security settings.</param>
		void IProviderBehavior.ApplySecuritySettings(ProviderSecuritySettings securitySettings) {
			if (securitySettings.MaximumHashBitLength < 256) {
				securitySettings.MaximumHashBitLength = 256;
			}

			SetMaximumAssociationLifetimeToNotExceed(Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA256, MaximumAssociationLifetime, securitySettings);
			SetMaximumAssociationLifetimeToNotExceed(Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA1, MaximumAssociationLifetime, securitySettings);
		}

		/// <summary>
		/// Called when a request is received by the Provider.
		/// </summary>
		/// <param name="request">The incoming request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// 	<c>true</c> if this behavior owns this request and wants to stop other behaviors
		/// from handling it; <c>false</c> to allow other behaviors to process this request.
		/// </returns>
		/// <remarks>
		/// Implementations may set a new value to <see cref="IRequest.SecuritySettings"/> but
		/// should not change the properties on the instance of <see cref="ProviderSecuritySettings"/>
		/// itself as that instance may be shared across many requests.
		/// </remarks>
		Task<bool> IProviderBehavior.OnIncomingRequestAsync(IRequest request, CancellationToken cancellationToken) {
			var hostProcessedRequest = request as IHostProcessedRequest;
			if (hostProcessedRequest != null) {
				// Only apply our special policies if the RP requested it.
				var papeRequest = request.GetExtension<PolicyRequest>();
				if (papeRequest != null) {
					if (papeRequest.PreferredPolicies.Contains(AuthenticationPolicies.USGovernmentTrustLevel1)) {
						// Whenever we see this GSA policy requested, we MUST also see the PPID policy requested.
						ErrorUtilities.VerifyProtocol(papeRequest.PreferredPolicies.Contains(AuthenticationPolicies.PrivatePersonalIdentifier), BehaviorStrings.PapeRequestMissingRequiredPolicies);
						ErrorUtilities.VerifyProtocol(string.Equals(hostProcessedRequest.Realm.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal) || DisableSslRequirement, BehaviorStrings.RealmMustBeHttps);

						// Apply GSA-specific security to this individual request.
						request.SecuritySettings.RequireSsl = !DisableSslRequirement;
						return Task.FromResult(true);
					}
				}
			}

			return Task.FromResult(false);
		}

		/// <summary>
		/// Called when the Provider is preparing to send a response to an authentication request.
		/// </summary>
		/// <param name="request">The request that is configured to generate the outgoing response.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// 	<c>true</c> if this behavior owns this request and wants to stop other behaviors
		/// from handling it; <c>false</c> to allow other behaviors to process this request.
		/// </returns>
		async Task<bool> IProviderBehavior.OnOutgoingResponseAsync(Provider.IAuthenticationRequest request, CancellationToken cancellationToken) {
			bool result = false;

			// Nothing to do for negative assertions.
			if (!request.IsAuthenticated.Value) {
				return result;
			}

			var requestInternal = (Provider.AuthenticationRequest)request;
			var responseMessage = (IProtocolMessageWithExtensions)await requestInternal.GetResponseAsync(cancellationToken);

			// Only apply our special policies if the RP requested it.
			var papeRequest = request.GetExtension<PolicyRequest>();
			if (papeRequest != null) {
				var papeResponse = responseMessage.Extensions.OfType<PolicyResponse>().SingleOrDefault();
				if (papeResponse == null) {
					request.AddResponseExtension(papeResponse = new PolicyResponse());
				}

				if (papeRequest.PreferredPolicies.Contains(AuthenticationPolicies.USGovernmentTrustLevel1)) {
					result = true;
					if (!papeResponse.ActualPolicies.Contains(AuthenticationPolicies.USGovernmentTrustLevel1)) {
						papeResponse.ActualPolicies.Add(AuthenticationPolicies.USGovernmentTrustLevel1);
					}

					// The spec requires that the OP perform discovery and if that fails, it must either sternly
					// warn the user of a potential threat or just abort the authentication.
					// We can't verify that the OP displayed anything to the user at this level, but we can
					// at least verify that the OP performed the discovery on the realm and halt things if it didn't.
					ErrorUtilities.VerifyHost(requestInternal.HasRealmDiscoveryBeenPerformed, BehaviorStrings.RealmDiscoveryNotPerformed);
				}

				if (papeRequest.PreferredPolicies.Contains(AuthenticationPolicies.PrivatePersonalIdentifier)) {
					ErrorUtilities.VerifyProtocol(request.ClaimedIdentifier == request.LocalIdentifier, OpenIdStrings.DelegatingIdentifiersNotAllowed);

					// Mask the user's identity with a PPID.
					ErrorUtilities.VerifyHost(PpidIdentifierProvider != null, BehaviorStrings.PpidProviderNotGiven);
					Identifier ppidIdentifier = PpidIdentifierProvider.GetIdentifier(request.LocalIdentifier, request.Realm);
					requestInternal.ResetClaimedAndLocalIdentifiers(ppidIdentifier);

					// Indicate that the RP is receiving a PPID claimed_id
					if (!papeResponse.ActualPolicies.Contains(AuthenticationPolicies.PrivatePersonalIdentifier)) {
						papeResponse.ActualPolicies.Add(AuthenticationPolicies.PrivatePersonalIdentifier);
					}
				}

				if (papeRequest.PreferredPolicies.Contains(AuthenticationPolicies.NoPersonallyIdentifiableInformation)) {
					ErrorUtilities.VerifyProtocol(
						!responseMessage.Extensions.OfType<ClaimsResponse>().Any() &&
						!responseMessage.Extensions.OfType<FetchResponse>().Any(),
						BehaviorStrings.PiiIncludedWithNoPiiPolicy);

					// If no PII is given in extensions, and the claimed_id is a PPID, then we can state we issue no PII.
					if (papeResponse.ActualPolicies.Contains(AuthenticationPolicies.PrivatePersonalIdentifier)) {
						if (!papeResponse.ActualPolicies.Contains(AuthenticationPolicies.NoPersonallyIdentifiableInformation)) {
							papeResponse.ActualPolicies.Add(AuthenticationPolicies.NoPersonallyIdentifiableInformation);
						}
					}
				}

				Reporting.RecordEventOccurrence(this, "OP");
			}

			return result;
		}

		#endregion

		/// <summary>
		/// Ensures the maximum association lifetime does not exceed a given limit.
		/// </summary>
		/// <param name="associationType">Type of the association.</param>
		/// <param name="maximumLifetime">The maximum lifetime.</param>
		/// <param name="securitySettings">The security settings to adjust.</param>
		private static void SetMaximumAssociationLifetimeToNotExceed(string associationType, TimeSpan maximumLifetime, ProviderSecuritySettings securitySettings) {
			Requires.NotNullOrEmpty(associationType, "associationType");
			Requires.That(maximumLifetime.TotalSeconds > 0, "maximumLifetime", "requires positive timespan");
			if (!securitySettings.AssociationLifetimes.ContainsKey(associationType) ||
				securitySettings.AssociationLifetimes[associationType] > maximumLifetime) {
				securitySettings.AssociationLifetimes[associationType] = maximumLifetime;
			}
		}
	}
}
