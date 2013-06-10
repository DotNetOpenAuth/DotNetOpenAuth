//-----------------------------------------------------------------------
// <copyright file="AuthenticatedClientRequestBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Net;
	using System.Net.Http;

	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A direct message from the client to the authorization server that includes the client's credentials.
	/// </summary>
	public abstract class AuthenticatedClientRequestBase : MessageBase, IHttpDirectRequest {
		/// <summary>
		/// The backing for the <see cref="Headers"/> property.
		/// </summary>
		private readonly System.Net.Http.Headers.HttpRequestHeaders headers = new HttpRequestMessage().Headers;

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
		/// <remarks>
		/// Not required, because the client id may be communicate through alternate means like HTTP Basic authentication (the OAuth 2 spec allows a lot of freedom here).
		/// </remarks>
		[MessagePart(Protocol.client_id, IsRequired = false)]
		public string ClientIdentifier { get; internal set; }

		/// <summary>
		/// Gets the client secret.
		/// </summary>
		/// <value>The client secret.</value>
		/// <remarks>
		/// REQUIRED. The client secret as described in Section 2.1  (Client Credentials). OPTIONAL if no client secret was issued. 
		/// </remarks>
		[MessagePart(Protocol.client_secret, IsRequired = false, IsSecuritySensitive = true)]
		public string ClientSecret { get; internal set; }

		/// <summary>
		/// Gets the HTTP headers of the request.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		public System.Net.Http.Headers.HttpRequestHeaders Headers {
			get { return this.headers; }
		}
	}
}