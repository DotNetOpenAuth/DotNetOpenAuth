using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetOAuth {
	/// <summary>
	/// Constants used in the OAuth protocol.
	/// </summary>
	/// <remarks>
	/// OAuth Protocol Parameter names and values are case sensitive. Each OAuth Protocol Parameters MUST NOT appear more than once per request, and are REQUIRED unless otherwise noted,
	/// per OAuth 1.0 section 5.
	/// </remarks>
	class Protocol {
		internal static readonly Protocol Default = V10;
		internal static readonly Protocol V10 = new Protocol {
		};

		internal const string DataContractNamespace = "http://oauth.net/core/1.0/";
		internal string ParameterPrefix = "oauth_";

		/// <summary>
		/// Strings that identify the various message schemes.
		/// </summary>
		/// <remarks>
		/// These strings should be checked with case INsensitivity.
		/// </remarks>
		internal Dictionary<MessageScheme, string> MessageSchemes = new Dictionary<MessageScheme,string> {
			{ MessageScheme.AuthorizationHeaderRequest, "OAuth" },
		};
	}
}
