//-----------------------------------------------------------------------
// <copyright file="GsaIcamProfile.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty.Behaviors {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Behaviors;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;

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
	public sealed class GsaIcamProfile : GsaIcamProfileBase, IRelyingPartyBehavior {
		/// <summary>
		/// Initializes a new instance of the <see cref="GsaIcamProfile"/> class.
		/// </summary>
		public GsaIcamProfile() {
			if (DisableSslRequirement) {
				Logger.OpenId.Warn("GSA level 1 behavior has its RequireSsl requirement disabled.");
			}
		}

		#region IRelyingPartyBehavior Members

		/// <summary>
		/// Applies a well known set of security requirements.
		/// </summary>
		/// <param name="securitySettings">The security settings to enhance with the requirements of this profile.</param>
		/// <remarks>
		/// Care should be taken to never decrease security when applying a profile.
		/// Profiles should only enhance security requirements to avoid being
		/// incompatible with each other.
		/// </remarks>
		void IRelyingPartyBehavior.ApplySecuritySettings(RelyingPartySecuritySettings securitySettings) {
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
		void IRelyingPartyBehavior.OnOutgoingAuthenticationRequest(RelyingParty.IAuthenticationRequest request) {
			RelyingParty.AuthenticationRequest requestInternal = (RelyingParty.AuthenticationRequest)request;
			ErrorUtilities.VerifyProtocol(string.Equals(request.Realm.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal) || DisableSslRequirement, BehaviorStrings.RealmMustBeHttps);

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
					BehaviorStrings.PiiIncludedWithNoPiiPolicy);
			}

			Reporting.RecordEventOccurrence(this, "RP");
		}

		/// <summary>
		/// Called when an incoming positive assertion is received.
		/// </summary>
		/// <param name="assertion">The positive assertion.</param>
		void IRelyingPartyBehavior.OnIncomingPositiveAssertion(IAuthenticationResponse assertion) {
			PolicyResponse pape = assertion.GetExtension<PolicyResponse>();
			ErrorUtilities.VerifyProtocol(
				pape != null &&
				pape.ActualPolicies.Contains(AuthenticationPolicies.USGovernmentTrustLevel1) &&
				pape.ActualPolicies.Contains(AuthenticationPolicies.PrivatePersonalIdentifier),
				BehaviorStrings.PapeResponseOrRequiredPoliciesMissing);

			ErrorUtilities.VerifyProtocol(AllowPersonallyIdentifiableInformation || pape.ActualPolicies.Contains(AuthenticationPolicies.NoPersonallyIdentifiableInformation), BehaviorStrings.PapeResponseOrRequiredPoliciesMissing);

			if (pape.ActualPolicies.Contains(AuthenticationPolicies.NoPersonallyIdentifiableInformation)) {
				ErrorUtilities.VerifyProtocol(
					assertion.GetExtension<ClaimsResponse>() == null &&
					assertion.GetExtension<FetchResponse>() == null,
					BehaviorStrings.PiiIncludedWithNoPiiPolicy);
			}
		}

		#endregion
	}
}
