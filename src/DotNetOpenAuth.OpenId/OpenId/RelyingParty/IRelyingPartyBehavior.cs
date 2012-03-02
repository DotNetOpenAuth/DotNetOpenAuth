//-----------------------------------------------------------------------
// <copyright file="IRelyingPartyBehavior.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// Applies a custom security policy to certain OpenID security settings and behaviors.
	/// </summary>
	[ContractClass(typeof(IRelyingPartyBehaviorContract))]
	public interface IRelyingPartyBehavior {
		/// <summary>
		/// Applies a well known set of security requirements to a default set of security settings.
		/// </summary>
		/// <param name="securitySettings">The security settings to enhance with the requirements of this profile.</param>
		/// <remarks>
		/// Care should be taken to never decrease security when applying a profile.
		/// Profiles should only enhance security requirements to avoid being
		/// incompatible with each other.
		/// </remarks>
		void ApplySecuritySettings(RelyingPartySecuritySettings securitySettings);

		/// <summary>
		/// Called when an authentication request is about to be sent.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <remarks>
		/// Implementations should be prepared to be called multiple times on the same outgoing message
		/// without malfunctioning.
		/// </remarks>
		void OnOutgoingAuthenticationRequest(IAuthenticationRequest request);

		/// <summary>
		/// Called when an incoming positive assertion is received.
		/// </summary>
		/// <param name="assertion">The positive assertion.</param>
		void OnIncomingPositiveAssertion(IAuthenticationResponse assertion);
	}

	/// <summary>
	/// Contract class for the <see cref="IRelyingPartyBehavior"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IRelyingPartyBehavior))]
	internal abstract class IRelyingPartyBehaviorContract : IRelyingPartyBehavior {
		/// <summary>
		/// Prevents a default instance of the <see cref="IRelyingPartyBehaviorContract"/> class from being created.
		/// </summary>
		private IRelyingPartyBehaviorContract() {
		}

		#region IRelyingPartyBehavior Members

		/// <summary>
		/// Applies a well known set of security requirements to a default set of security settings.
		/// </summary>
		/// <param name="securitySettings">The security settings to enhance with the requirements of this profile.</param>
		/// <remarks>
		/// Care should be taken to never decrease security when applying a profile.
		/// Profiles should only enhance security requirements to avoid being
		/// incompatible with each other.
		/// </remarks>
		void IRelyingPartyBehavior.ApplySecuritySettings(RelyingPartySecuritySettings securitySettings) {
			Requires.NotNull(securitySettings, "securitySettings");
		}

		/// <summary>
		/// Called when an authentication request is about to be sent.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <remarks>
		/// Implementations should be prepared to be called multiple times on the same outgoing message
		/// without malfunctioning.
		/// </remarks>
		void IRelyingPartyBehavior.OnOutgoingAuthenticationRequest(IAuthenticationRequest request) {
			Requires.NotNull(request, "request");
		}

		/// <summary>
		/// Called when an incoming positive assertion is received.
		/// </summary>
		/// <param name="assertion">The positive assertion.</param>
		void IRelyingPartyBehavior.OnIncomingPositiveAssertion(IAuthenticationResponse assertion) {
			Requires.NotNull(assertion, "assertion");
		}

		#endregion
	}
}
