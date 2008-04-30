using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Extensions.ProviderAuthenticationPolicy {
	/// <summary>
	/// OpenID Provider Authentication Policy extension constants.
	/// </summary>
	static class Constants {
		/// <summary>
		/// The namespace used by this extension in messages.
		/// </summary>
		internal const string TypeUri = "http://specs.openid.net/extensions/pape/1.0";
		/// <summary>
		/// Parameters to be included with PAPE requests.
		/// </summary>
		internal static class RequestParameters {
			/// <summary>
			/// Optional. If the End User has not actively authenticated to the OP within the number of seconds specified in a manner fitting the requested policies, the OP SHOULD authenticate the End User for this request.
			/// </summary>
			/// <value>Integer value greater than or equal to zero in seconds.</value>
			/// <remarks>
			/// The OP should realize that not adhering to the request for re-authentication most likely means that the End User will not be allowed access to the services provided by the RP. If this parameter is absent in the request, the OP should authenticate the user at its own discretion.
			/// </remarks>
			internal const string MaxAuthAge = "max_auth_age";
			/// <summary>
			/// Zero or more authentication policy URIs that the OP SHOULD conform to when authenticating the user. If multiple policies are requested, the OP SHOULD satisfy as many as it can.
			/// </summary>
			/// <value>Space separated list of authentication policy URIs.</value>
			/// <remarks>
			/// If no policies are requested, the RP may be interested in other information such as the authentication age.
			/// </remarks>
			internal const string PreferredAuthPolicies = "preferred_auth_policies";
		}
		/// <summary>
		/// Parameters to be included with PAPE responses.
		/// </summary>
		internal static class ResponseParameters {
			/// <summary>
			/// One or more authentication policy URIs that the OP conformed to when authenticating the End User.
			/// </summary>
			/// <value>Space separated list of authentication policy URIs.</value>
			/// <remarks>
			/// If no policies were met though the OP wishes to convey other information in the response, this parameter MUST be included with the value of "none".
			/// </remarks>
			internal const string AuthPolicies = "auth_policies";
			/// <summary>
			/// Optional. The most recent timestamp when the End User has actively authenticated to the OP in a manner fitting the asserted policies.
			/// </summary>
			/// <value>
			/// The timestamp MUST be formatted as specified in section 5.6 of [RFC3339] (Klyne, G. and C. Newman, “Date and Time on the Internet: Timestamps,” .), with the following restrictions:
			///  * All times must be in the UTC timezone, indicated with a "Z".
			///  * No fractional seconds are allowed
			/// For example: 2005-05-15T17:11:51Z
			/// </value>
			/// <remarks>
			/// If the RP's request included the "openid.max_auth_age" parameter then the OP MUST include "openid.auth_time" in its response. If "openid.max_auth_age" was not requested, the OP MAY choose to include "openid.auth_time" in its response.
			/// </remarks>
			internal const string AuthTime = "auth_time";
			/// <summary>
			/// Optional. The Assurance Level as defined by the National Institute of Standards and Technology (NIST) in Special Publication 800-63 (Burr, W., Dodson, D., and W. Polk, Ed., “Electronic Authentication Guideline,” April 2006.) [NIST_SP800‑63] corresponding to the authentication method and policies employed by the OP when authenticating the End User.
			/// </summary>
			/// <value>Integer value between 0 and 4 inclusive.</value>
			/// <remarks>
			/// Level 0 is not an assurance level defined by NIST, but rather SHOULD be used to signify that the OP recognizes the parameter and the End User authentication did not meet the requirements of Level 1. See Appendix A.1.2 (NIST Assurance Levels) for high-level example classifications of authentication methods within the defined levels.
			/// </remarks>
			internal const string NistAuthLevel = "nist_auth_level";
		}
	}
}
