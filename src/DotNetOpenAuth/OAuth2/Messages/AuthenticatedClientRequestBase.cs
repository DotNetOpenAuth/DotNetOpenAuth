//-----------------------------------------------------------------------
// <copyright file="AuthenticatedClientRequestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;

	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A direct message from the client to the authorization server that includes the client's credentials.
	/// </summary>
	public abstract class AuthenticatedClientRequestBase : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticatedClientRequestBase"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The Authorization Server's access token endpoint URL.</param>
		/// <param name="version">The version.</param>
		protected AuthenticatedClientRequestBase(Uri tokenEndpoint, Version version)
			: base(version, MessageTransport.Direct, tokenEndpoint) {
		}

		/// <summary>
		/// Gets the client identifier previously obtained from the Authorization Server.
		/// </summary>
		/// <value>The client identifier.</value>
		[MessagePart(Protocol.client_id, IsRequired = true, AllowEmpty = false)]
		public string ClientIdentifier { get; internal set; }

		/// <summary>
		/// Gets the client secret.
		/// </summary>
		/// <value>The client secret.</value>
		/// <remarks>
		/// REQUIRED. The client secret as described in Section 2.1  (Client Credentials). OPTIONAL if no client secret was issued. 
		/// </remarks>
		[MessagePart(Protocol.client_secret, IsRequired = false, AllowEmpty = true)]
		public string ClientSecret { get; internal set; }
	}
}