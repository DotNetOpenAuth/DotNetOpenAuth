//-----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy {
	using System;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// OpenID Provider Authentication Policy extension constants.
	/// </summary>
	internal static class Constants {
		/// <summary>
		/// The namespace used by this extension in messages.
		/// </summary>
		internal const string TypeUri = "http://specs.openid.net/extensions/pape/1.0";

		/// <summary>
		/// The namespace alias to use for OpenID 1.x interop, where aliases are not defined in the message.
		/// </summary>
		internal const string CompatibilityAlias = "pape";

		/// <summary>
		/// The string to prepend on an Auth Level Type alias definition.
		/// </summary>
		internal const string AuthLevelNamespaceDeclarationPrefix = "auth_level.ns.";

		/// <summary>
		/// Well-known assurance level Type URIs.
		/// </summary>
		internal static class AssuranceLevels {
			/// <summary>
			/// A mapping between the PAPE TypeURI and the alias to use if 
			/// possible for backward compatibility reasons.
			/// </summary>
			internal static readonly IDictionary<string, string> PreferredTypeUriToAliasMap = new Dictionary<string, string> {
				{ NistTypeUri, "nist" },
			};

			/// <summary>
			/// The Type URI of the NIST assurance level.
			/// </summary>
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
	}
}
