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
		/// The namespace alias to use for OpenID 1.x interop, where aliases are not defined in the message.
		/// </summary>
		internal const string pape_compatibility_alias = "pape";
		/// <summary>
		/// The string to prepend on an Auth Level Type alias definition.
		/// </summary>
		internal const string AuthLevelNamespaceDeclarationPrefix = "auth_level.ns.";

		internal static class AuthenticationLevels {
			internal static readonly IDictionary<string, string> PreferredTypeUriToAliasMap = new Dictionary<string, string> {
				{ NistTypeUri, nist_compatibility_alias },
			};

			internal const string nist_compatibility_alias = "nist";
			internal const string NistTypeUri = "http://csrc.nist.gov/publications/nistpubs/800-63/SP800-63V1_0_2.pdf";
		}

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
			/// <summary>
			/// The space separated list of the name spaces of the custom Assurance Level that RP requests, in the order of its preference.
			/// </summary>
			internal const string PreferredAuthLevelTypes = "preferred_auth_level_types";
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
			/// The first part of a parameter name that gives the custom string value for
			/// the assurance level.  The second part of the parameter name is the alias for
			/// that assurance level.
			/// </summary>
			internal const string AuthLevelAliasPrefix = "auth_level.";
		}
	}
}
