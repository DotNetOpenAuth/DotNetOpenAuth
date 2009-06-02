//-----------------------------------------------------------------------
// <copyright file="USGovernmentLevel1.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.SecurityProfiles {
	using System;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// Implements the GSA level 1 OpenID profile.
	/// </summary>
	/// <remarks>
	/// <para>Relying parties that include this profile are always held to the terms required by the profile,
	/// but Providers are only affected by the special behaviors of the profile when the RP specifically
	/// indicates that they want to use this profile. </para>
	/// </remarks>
	public sealed class USGovernmentLevel1 : IRelyingPartySecurityProfile, IProviderSecurityProfile {
		/// <summary>
		/// The maximum time a shared association can live.
		/// </summary>
		private static readonly TimeSpan MaximumAssociationLifetime = TimeSpan.FromSeconds(86400);

		/// <summary>
		/// Initializes a new instance of the <see cref="USGovernmentLevel1"/> class.
		/// </summary>
		public USGovernmentLevel1() {
		}

		/// <summary>
		/// Gets or sets the provider for generating PPID identifiers.
		/// </summary>
		public static IDirectedIdentityIdentifierProvider PpidIdentifierProvider { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether PII is allowed to be requested or received via OpenID.
		/// </summary>
		/// <value>The default value is <c>false</c>.</value>
		public static bool AllowPersonallyIdentifiableInformation { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to ignore the SSL requirement (for testing purposes only).
		/// </summary>
		public static bool DisableSslRequirement { get; set; }

		#region IRelyingPartySecurityProfile Members

		/// <summary>
		/// Applies a well known set of security requirements.
		/// </summary>
		/// <param name="securitySettings">The security settings to enhance with the requirements of this profile.</param>
		/// <remarks>
		/// Care should be taken to never decrease security when applying a profile.
		/// Profiles should only enhance security requirements to avoid being
		/// incompatible with each other.
		/// </remarks>
		void IRelyingPartySecurityProfile.ApplySecuritySettings(RelyingPartySecuritySettings securitySettings) {
			ErrorUtilities.VerifyArgumentNotNull(securitySettings, "securitySettings");

			if (securitySettings.MaximumHashBitLength < 256) {
				securitySettings.MaximumHashBitLength = 256;
			}

			securitySettings.RequireSsl = !DisableSslRequirement;
			securitySettings.RequireDirectedIdentity = true;
			securitySettings.RequireAssociation = true;
			securitySettings.RejectDelegatingIdentifiers = true;
			securitySettings.IgnoreUnsignedExtensions = true;
			securitySettings.MinimumRequiredOpenIdVersion = ProtocolVersion.V20;
		}

		/// <summary>
		/// Called when an authentication request is about to be sent.
		/// </summary>
		/// <param name="request">The request.</param>
		void IRelyingPartySecurityProfile.OnOutgoingAuthenticationRequest(RelyingParty.IAuthenticationRequest request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			RelyingParty.AuthenticationRequest requestInternal = (RelyingParty.AuthenticationRequest)request;
			ErrorUtilities.VerifyProtocol(string.Equals(request.Realm.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal) || DisableSslRequirement, SecurityProfileStrings.RealmMustBeHttps);

			var pape = requestInternal.AppliedExtensions.OfType<PolicyRequest>().SingleOrDefault();
			if (pape == null) {
				request.AddExtension(pape = new PolicyRequest());
			}

			if (!pape.PreferredPolicies.Contains(AuthenticationPolicies.PrivatePersonalIdentifier)) {
				pape.PreferredPolicies.Add(AuthenticationPolicies.PrivatePersonalIdentifier);
			}

			if (!pape.PreferredPolicies.Contains(AuthenticationPolicies.USGovernmentTrustLevel1)) {
				pape.PreferredPolicies.Add(AuthenticationPolicies.USGovernmentTrustLevel1);
			}

			if (!AllowPersonallyIdentifiableInformation && !pape.PreferredPolicies.Contains(AuthenticationPolicies.NoPersonallyIdentifiableInformation)) {
				pape.PreferredPolicies.Add(AuthenticationPolicies.NoPersonallyIdentifiableInformation);
			}

			if (pape.PreferredPolicies.Contains(AuthenticationPolicies.NoPersonallyIdentifiableInformation)) {
				ErrorUtilities.VerifyProtocol(
					(!requestInternal.AppliedExtensions.OfType<ClaimsRequest>().Any() &&
					!requestInternal.AppliedExtensions.OfType<FetchRequest>().Any()),
					SecurityProfileStrings.PiiIncludedWithNoPiiPolicy);
			}
		}

		/// <summary>
		/// Called when an incoming positive assertion is received.
		/// </summary>
		/// <param name="assertion">The positive assertion.</param>
		void IRelyingPartySecurityProfile.OnIncomingPositiveAssertion(IAuthenticationResponse assertion) {
			ErrorUtilities.VerifyArgumentNotNull(assertion, "assertion");

			PolicyResponse pape = assertion.GetExtension<PolicyResponse>();
			ErrorUtilities.VerifyProtocol(
				pape != null &&
				pape.ActualPolicies.Contains(AuthenticationPolicies.USGovernmentTrustLevel1) &&
				pape.ActualPolicies.Contains(AuthenticationPolicies.PrivatePersonalIdentifier),
				SecurityProfileStrings.PapeResponseOrRequiredPoliciesMissing);

			ErrorUtilities.VerifyProtocol(AllowPersonallyIdentifiableInformation || pape.ActualPolicies.Contains(AuthenticationPolicies.NoPersonallyIdentifiableInformation), SecurityProfileStrings.PapeResponseOrRequiredPoliciesMissing);

			if (pape.ActualPolicies.Contains(AuthenticationPolicies.NoPersonallyIdentifiableInformation)) {
				ErrorUtilities.VerifyProtocol(
					assertion.GetExtension<ClaimsResponse>() == null &&
					assertion.GetExtension<FetchResponse>() == null,
					SecurityProfileStrings.PiiIncludedWithNoPiiPolicy);
			}
		}

		#endregion

		#region IProviderSecurityProfile Members

		/// <summary>
		/// Called when a request is received by the Provider.
		/// </summary>
		/// <param name="request">The incoming request.</param>
		/// <returns>
		/// 	<c>true</c> if this security profile owns this request and wants to stop other security profiles
		/// from handling it; <c>false</c> to allow other security profiles to process this request.
		/// </returns>
		/// <remarks>
		/// Implementations may set a new value to <see cref="IRequest.SecuritySettings"/> but
		/// should not change the properties on the instance of <see cref="ProviderSecuritySettings"/>
		/// itself as that instance may be shared across many requests.
		/// </remarks>
		bool IProviderSecurityProfile.OnIncomingRequest(IRequest request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			var hostProcessedRequest = request as IHostProcessedRequest;
			if (hostProcessedRequest != null) {
				// Only apply our special policies if the RP requested it.
				var papeRequest = request.GetExtension<PolicyRequest>();
				if (papeRequest != null) {
					if (papeRequest.PreferredPolicies.Contains(AuthenticationPolicies.USGovernmentTrustLevel1)) {
						// Whenever we see this GSA policy requested, we MUST also see the PPID policy requested.
						ErrorUtilities.VerifyProtocol(papeRequest.PreferredPolicies.Contains(AuthenticationPolicies.PrivatePersonalIdentifier), SecurityProfileStrings.PapeRequestMissingRequiredPolicies);
						ErrorUtilities.VerifyProtocol(string.Equals(hostProcessedRequest.Realm.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal) || DisableSslRequirement, SecurityProfileStrings.RealmMustBeHttps);

						request.SecuritySettings = GetProviderSecuritySettings(request.SecuritySettings);
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Called when the Provider is preparing to send a response to an authentication request.
		/// </summary>
		/// <param name="request">The request that is configured to generate the outgoing response.</param>
		/// <returns>
		/// 	<c>true</c> if this security profile owns this request and wants to stop other security profiles
		/// from handling it; <c>false</c> to allow other security profiles to process this request.
		/// </returns>
		bool IProviderSecurityProfile.OnOutgoingResponse(Provider.IAuthenticationRequest request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			bool result = false;

			// Nothing to do for negative assertions.
			if (!request.IsAuthenticated.Value) {
				return result;
			}

			var requestInternal = (Provider.AuthenticationRequest)request;
			var responseMessage = (IProtocolMessageWithExtensions)requestInternal.Response;

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
					ErrorUtilities.VerifyHost(requestInternal.HasRealmDiscoveryBeenPerformed, SecurityProfileStrings.RealmDiscoveryNotPerformed);
				}

				if (papeRequest.PreferredPolicies.Contains(AuthenticationPolicies.PrivatePersonalIdentifier)) {
					ErrorUtilities.VerifyProtocol(request.ClaimedIdentifier == request.LocalIdentifier, OpenIdStrings.DelegatingIdentifiersNotAllowed);

					// Mask the user's identity with a PPID.
					ErrorUtilities.VerifyHost(PpidIdentifierProvider != null, SecurityProfileStrings.PpidProviderNotGiven);
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
						SecurityProfileStrings.PiiIncludedWithNoPiiPolicy);

					// If no PII is given in extensions, and the claimed_id is a PPID, then we can state we issue no PII.
					if (papeResponse.ActualPolicies.Contains(AuthenticationPolicies.PrivatePersonalIdentifier)) {
						if (!papeResponse.ActualPolicies.Contains(AuthenticationPolicies.NoPersonallyIdentifiableInformation)) {
							papeResponse.ActualPolicies.Add(AuthenticationPolicies.NoPersonallyIdentifiableInformation);
						}
					}
				}
			}

			return result;
		}

		#endregion

		/// <summary>
		/// Adapts the default security settings to the requirements of this security profile.
		/// </summary>
		/// <param name="originalSecuritySettings">The original security settings.</param>
		/// <returns>A new security settings instance that should be used for all qualifying incoming requests.</returns>
		private static ProviderSecuritySettings GetProviderSecuritySettings(ProviderSecuritySettings originalSecuritySettings) {
			var securitySettings = originalSecuritySettings.Clone();

			if (securitySettings.MaximumHashBitLength < 256) {
				securitySettings.MaximumHashBitLength = 256;
			}

			securitySettings.RequireSsl = !DisableSslRequirement;
			SetMaximumAssociationLifetimeToNotExceed(Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA256, MaximumAssociationLifetime, securitySettings);
			SetMaximumAssociationLifetimeToNotExceed(Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA1, MaximumAssociationLifetime, securitySettings);

			return securitySettings;
		}

		/// <summary>
		/// Ensures the maximum association lifetime does not exceed a given limit.
		/// </summary>
		/// <param name="associationType">Type of the association.</param>
		/// <param name="maximumLifetime">The maximum lifetime.</param>
		/// <param name="securitySettings">The security settings to adjust.</param>
		private static void SetMaximumAssociationLifetimeToNotExceed(string associationType, TimeSpan maximumLifetime, ProviderSecuritySettings securitySettings) {
			Contract.RequiresAlways(!String.IsNullOrEmpty(associationType));
			Contract.RequiresAlways(maximumLifetime.TotalSeconds > 0);
			if (!securitySettings.AssociationLifetimes.ContainsKey(associationType) ||
				securitySettings.AssociationLifetimes[associationType] > maximumLifetime) {
				securitySettings.AssociationLifetimes[associationType] = maximumLifetime;
			}
		}
	}
}
