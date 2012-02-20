//-----------------------------------------------------------------------
// <copyright file="AuthorizationServerDescription.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A description of an OAuth Authorization Server as seen by an OAuth Client.
	/// </summary>
	public class AuthorizationServerDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationServerDescription"/> class.
		/// </summary>
		public AuthorizationServerDescription() {
			this.ProtocolVersion = Protocol.Default.ProtocolVersion;
		}

		/// <summary>
		/// Gets or sets the Authorization Server URL from which an Access Token is requested by the Client.
		/// </summary>
		/// <value>An HTTPS URL.</value>
		/// <remarks>
		/// <para>After obtaining authorization from the resource owner, clients request an access token from the authorization server's token endpoint.</para>
		/// <para>The URI of the token endpoint can be found in the service documentation, or can be obtained by the client by making an unauthorized protected resource request (from the WWW-Authenticate response header token-uri (The 'authorization-uri' Attribute) attribute).</para>
		/// <para>The token endpoint advertised by the resource server MAY include a query component as defined by [RFC3986] (Berners-Lee, T., Fielding, R., and L. Masinter, “Uniform Resource Identifier (URI): Generic Syntax,” January 2005.) section 3.</para>
		/// <para>Since requests to the token endpoint result in the transmission of plain text credentials in the HTTP request and response, the authorization server MUST require the use of a transport-layer mechanism such as TLS/SSL (or a secure channel with equivalent protections) when sending requests to the token endpoints. </para>
		/// </remarks>
		public Uri TokenEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the Authorization Server URL where the Client (re)directs the User
		/// to make an authorization request.
		/// </summary>
		/// <value>An HTTPS URL.</value>
		/// <remarks>
		/// <para>Clients direct the resource owner to the authorization endpoint to approve their access request. Before granting access, the resource owner first authenticates with the authorization server. The way in which the authorization server authenticates the end-user (e.g. username and password login, OpenID, session cookies) and in which the authorization server obtains the end-user's authorization, including whether it uses a secure channel such as TLS/SSL, is beyond the scope of this specification. However, the authorization server MUST first verify the identity of the end-user.</para>
		/// <para>The URI of the authorization endpoint can be found in the service documentation, or can be obtained by the client by making an unauthorized protected resource request (from the WWW-Authenticate response header auth-uri (The 'authorization-uri' Attribute) attribute).</para>
		/// <para>The authorization endpoint advertised by the resource server MAY include a query component as defined by [RFC3986] (Berners-Lee, T., Fielding, R., and L. Masinter, “Uniform Resource Identifier (URI): Generic Syntax,” January 2005.) section 3.</para>
		/// <para>Since requests to the authorization endpoint result in user authentication and the transmission of sensitive values, the authorization server SHOULD require the use of a transport-layer mechanism such as TLS/SSL (or a secure channel with equivalent protections) when sending requests to the authorization endpoints.</para>
		/// </remarks>
		public Uri AuthorizationEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the OAuth version supported by the Authorization Server.
		/// </summary>
		public ProtocolVersion ProtocolVersion { get; set; }

		/// <summary>
		/// Gets the version of the OAuth protocol to use with this Authorization Server.
		/// </summary>
		/// <value>The version.</value>
		internal Version Version {
			get { return Protocol.Lookup(this.ProtocolVersion).Version; }
		}
	}
}
