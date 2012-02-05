//-----------------------------------------------------------------------
// <copyright file="AuthorizationDeclinedResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.OAuth {
	using System;

	/// <summary>
	/// The OAuth response that a Provider should include with a positive 
	/// OpenID identity assertion when OAuth authorization was declined.
	/// </summary>
	[Serializable]
	public class AuthorizationDeclinedResponse : ExtensionBase {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly StandardOpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage, isProviderRole) => {
			if (typeUri == Constants.TypeUri && !isProviderRole && !data.ContainsKey(Constants.RequestTokenParameter)) {
				return new AuthorizationDeclinedResponse();
			}

			return null;
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationDeclinedResponse"/> class.
		/// </summary>
		public AuthorizationDeclinedResponse()
			: base(new Version(1, 0), Constants.TypeUri, null) {
		}
	}
}
