using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Extensions.AuthenticationPolicy {
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
		public const string PhishingResistantAuthentication = "http://schemas.openid.net/pape/policies/2007/06/phishing-resistant";
		/// <summary>
		/// An authentication mechanism where the End User authenticates to the OpenID Provider by providing over one authentication factor. Common authentication factors are something you know, something you have, and something you are. An example would be authentication using a password and a software token or digital certificate.
		/// </summary>
		public const string MultiFactorAuthentication = "http://schemas.openid.net/pape/policies/2007/06/multi-factor";
		/// <summary>
		/// An authentication mechanism where the End User authenticates to the OpenID Provider by providing over one authentication factor where at least one of the factors is a physical factor such as a hardware device or biometric. Common authentication factors are something you know, something you have, and something you are. This policy also implies the Multi-Factor Authentication policy (http://schemas.openid.net/pape/policies/2007/06/multi-factor) and both policies MAY BE specified in conjunction without conflict. An example would be authentication using a password and a hardware token.
		/// </summary>
		public const string PhysicalMultiFactorAuthentication = "http://schemas.openid.net/pape/policies/2007/06/multi-factor-physical";
	}
}
