//-----------------------------------------------------------------------
// <copyright file="AuthorizationApprovedResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.OAuth {
	using System;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The OAuth response that a Provider may include with a positive 
	/// OpenID identity assertion with an approved request token.
	/// </summary>
	[Serializable]
	public class AuthorizationApprovedResponse : ExtensionBase {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly StandardOpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage, isProviderRole) => {
			if (typeUri == Constants.TypeUri && !isProviderRole && data.ContainsKey(Constants.RequestTokenParameter)) {
				return new AuthorizationApprovedResponse();
			}

			return null;
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationApprovedResponse"/> class.
		/// </summary>
		public AuthorizationApprovedResponse()
			: base(new Version(1, 0), Constants.TypeUri, null) {
		}

		/// <summary>
		/// Gets or sets the user-approved request token.
		/// </summary>
		/// <value>The request token.</value>
		[MessagePart(Constants.RequestTokenParameter, IsRequired = true, AllowEmpty = false)]
		public string RequestToken { get; set; }

		/// <summary>
		/// Gets or sets a string that encodes, in a way possibly specific to the Combined Provider, one or more scopes that the returned request token is valid for. This will typically indicate a subset of the scopes requested in Section 8.
		/// </summary>
		[MessagePart("scope", IsRequired = false, AllowEmpty = true)]
		public string Scope { get; set; }
	}
}
