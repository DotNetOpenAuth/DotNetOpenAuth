//-----------------------------------------------------------------------
// <copyright file="ClientType.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	/// <summary>
	/// OAuth 2 Client types
	/// </summary>
	/// <remarks>
	/// <para>Based on their ability to
	/// authenticate securely with the authorization server (i.e. ability to
	/// maintain the confidentiality of their client credentials).</para>
	/// <para>The client type designation is based on the authorization server's
	/// definition of secure authentication and its acceptable exposure
	/// levels of client credentials.</para>
	/// <para>The authorization server SHOULD NOT make assumptions about the client
	/// type, nor accept the type information provided by the client
	/// developer without first establishing trust.</para>
	/// <para>A client application consisting of multiple components, each with its
	/// own client type (e.g. a distributed client with both a confidential
	/// server-based component and a public browser-based component), MUST
	/// register each component separately as a different client to ensure
	/// proper handling by the authorization server.  The authorization
	/// server MAY provider tools to manage such complex clients through a
	/// single administration interface.</para>
	/// </remarks>
	public enum ClientType {
		/// <summary>
		/// Clients capable of maintaining the confidentiality of their
		/// credentials (e.g. client implemented on a secure server with
		/// restricted access to the client credentials), or capable of secure
		/// client authentication using other means.
		/// </summary>
		Confidential,

		/// <summary>
		/// Clients incapable of maintaining the confidentiality of their
		/// credentials (e.g. clients executing on the device used by the
		/// resource owner such as an installed native application or a web
		/// browser-based application), and incapable of secure client
		/// authentication via any other means.
		/// </summary>
		Public,
	}
}
