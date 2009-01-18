//-----------------------------------------------------------------------
// <copyright file="AuthenticationPolicies.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy {
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Well-known authentication policies defined in the PAPE extension spec or by a recognized
	/// standards body.
	/// </summary>
	/// <remarks>
	/// This is a class of constants rather than a flags enum because policies may be
	/// freely defined and used by anyone, just by using a new Uri.
	/// </remarks>
	public static class AuthenticationPolicies {
		/// <summary>
		/// An authentication mechanism where the End User does not provide a shared secret to a party potentially under the control of the Relying Party. (Note that the potentially malicious Relying Party controls where the User-Agent is redirected to and thus may not send it to the End User's actual OpenID Provider).
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Phishing", Justification = "By design")]
		public const string PhishingResistant = "http://schemas.openid.net/pape/policies/2007/06/phishing-resistant";

		/// <summary>
		/// An authentication mechanism where the End User authenticates to the OpenID Provider by providing over one authentication factor. Common authentication factors are something you know, something you have, and something you are. An example would be authentication using a password and a software token or digital certificate.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "By design")]
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiFactor", Justification = "By design")]
		public const string MultiFactor = "http://schemas.openid.net/pape/policies/2007/06/multi-factor";

		/// <summary>
		/// An authentication mechanism where the End User authenticates to the OpenID Provider by providing over one authentication factor where at least one of the factors is a physical factor such as a hardware device or biometric. Common authentication factors are something you know, something you have, and something you are. This policy also implies the Multi-Factor Authentication policy (http://schemas.openid.net/pape/policies/2007/06/multi-factor) and both policies MAY BE specified in conjunction without conflict. An example would be authentication using a password and a hardware token.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiFactor", Justification = "By design")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "By design")]
		public const string PhysicalMultiFactor = "http://schemas.openid.net/pape/policies/2007/06/multi-factor-physical";

		/// <summary>
		/// Used in a PAPE response to indicate that no PAPE authentication policies could be satisfied.
		/// </summary>
		/// <remarks>
		/// Used internally by the PAPE extension, so that users don't have to know about it.
		/// </remarks>
		internal const string None = "http://schemas.openid.net/pape/policies/2007/06/none";
	}
}
