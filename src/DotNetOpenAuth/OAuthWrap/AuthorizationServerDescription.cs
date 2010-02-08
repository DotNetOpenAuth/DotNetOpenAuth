//-----------------------------------------------------------------------
// <copyright file="AuthorizationServerDescription.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A description of an OAuth WRAP Authorization Server.
	/// </summary>
	public class AuthorizationServerDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationServerDescription"/> class.
		/// </summary>
		public AuthorizationServerDescription() {
			this.ProtocolVersion = Protocol.Default.ProtocolVersion;
		}

		/// <summary>
		/// Gets or sets the Authorization Server URL at which an Access Token is requested by the Client.
		/// A refresh token may also be returned to the Client.
		/// </summary>
		/// <value>An HTTPS URL.</value>
		/// <remarks>
		/// Messages sent to this URL must always be sent by the POST HTTP method.
		/// </remarks>
		public Uri AccessTokenEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the Authorization Server URL at which a Refresh Token is presented in exchange
		/// for a new Access Token.
		/// </summary>
		/// <value>An HTTPS URL.</value>
		/// <remarks>
		/// Messages sent to this URL must always be sent by the POST HTTP method.
		/// </remarks>
		public Uri RefreshTokenEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the Authorization Server URL where the Client (re)directs the User
		/// to make an authorization request.
		/// </summary>
		/// <value>An HTTP or HTTPS URL.</value>
		public Uri UserAuthorizationEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the OAuth WRAP version supported by the Authorization Server.
		/// </summary>
		public ProtocolVersion ProtocolVersion { get; set; }

		/// <summary>
		/// Gets or sets the version of the OAuth WRAP protocol to use with this Authorization Server.
		/// </summary>
		/// <value>The version.</value>
		internal Version Version {
			get { return Protocol.Lookup(this.ProtocolVersion).Version; }
		}
	}
}
