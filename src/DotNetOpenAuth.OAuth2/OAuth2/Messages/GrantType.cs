//-----------------------------------------------------------------------
// <copyright file="GrantType.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	/// <summary>
	/// The types of authorizations that a client can use to obtain
	/// a refresh token and/or an access token.
	/// </summary>
	internal enum GrantType {
		/// <summary>
		/// The client is providing the authorization code previously obtained from an end user authorization response.
		/// </summary>
		AuthorizationCode,

		/// <summary>
		/// The client is providing the end user's username and password to the authorization server.
		/// </summary>
		Password,

		/// <summary>
		/// The client is providing an assertion it obtained from another source.
		/// </summary>
		Assertion,

		/// <summary>
		/// The client is providing a refresh token.
		/// </summary>
		RefreshToken,

		/// <summary>
		/// No authorization to access a user's data has been given.  The client is requesting
		/// an access token authorized for its own private data.  This fits the classic OAuth 1.0(a) "2-legged OAuth" scenario.
		/// </summary>
		/// <remarks>
		/// When requesting an access token using the none access grant type (no access grant is included), the client is requesting access to the protected resources under its control, or those of another resource owner which has been previously arranged with the authorization server (the method of which is beyond the scope of this specification).
		/// </remarks>
		ClientCredentials,
	}
}
