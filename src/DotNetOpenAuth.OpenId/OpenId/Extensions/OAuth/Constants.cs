//-----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.OAuth {
	/// <summary>
	/// Constants used in the OpenID OAuth extension.
	/// </summary>
	internal static class Constants {
		/// <summary>
		/// The TypeURI for the OpenID OAuth extension.
		/// </summary>
		internal const string TypeUri = "http://specs.openid.net/extensions/oauth/1.0";

		/// <summary>
		/// The name of the parameter that carries the request token in the response.
		/// </summary>
		internal const string RequestTokenParameter = "request_token";
	}
}
