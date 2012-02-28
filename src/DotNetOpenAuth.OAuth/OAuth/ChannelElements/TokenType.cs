//-----------------------------------------------------------------------
// <copyright file="TokenType.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	/// <summary>
	/// The two types of tokens that exist in the OAuth protocol.
	/// </summary>
	public enum TokenType {
		/// <summary>
		/// A token that is freely issued to any known Consumer.
		/// It does not grant any authorization to access protected resources,
		/// but is used as a step in obtaining that access.
		/// </summary>
		RequestToken,

		/// <summary>
		/// A token only obtained after the owner of some protected resource(s)
		/// has approved a Consumer's access to said resource(s).
		/// </summary>
		AccessToken,

		/// <summary>
		/// An unrecognized, expired or invalid token.
		/// </summary>
		InvalidToken,
	}
}
