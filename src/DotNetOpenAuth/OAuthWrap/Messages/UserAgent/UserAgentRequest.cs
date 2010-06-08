//-----------------------------------------------------------------------
// <copyright file="UserAgentRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A message requesting user authorization to access protected data on behalf
	/// of an installed application or browser-hosted Javascript.
	/// </summary>
	internal class UserAgentRequest : MessageBase, IRequestWithRedirectUri {
		/// <summary>
		/// The type of message.
		/// </summary>
		[MessagePart(Protocol.type, IsRequired = true)]
		private const string Type = "user_agent";

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentRequest"/> class.
		/// </summary>
		/// <param name="authorizationEndpoint">The authorization endpoint.</param>
		/// <param name="version">The version.</param>
		internal UserAgentRequest(Uri authorizationEndpoint, Version version)
			: base(version, MessageTransport.Indirect, authorizationEndpoint) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal UserAgentRequest(AuthorizationServerDescription authorizationServer)
			: this(authorizationServer.AuthorizationEndpoint, authorizationServer.Version) {
		}

		/// <summary>
		/// Gets or sets state of the client that should be sent back with the authorization response.
		/// </summary>
		/// <value>
		/// An opaque value that Clients can use to maintain state associated with this request. 
		/// </value>
		[MessagePart(Protocol.state, IsRequired = false, AllowEmpty = true)]
		public string ClientState { get; set; }

		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		[MessagePart(Protocol.client_id, IsRequired = true, AllowEmpty = false)]
		public string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the callback URL.
		/// </summary>
		/// <value>
		/// An absolute URL to which the Authorization Server will redirect the User back after
		/// the user has approved the authorization request.
		/// </value>
		/// <remarks>
		/// REQUIRED unless a redirection URI has been established between the client and authorization server via other means. An absolute URI to which the authorization server will redirect the user-agent to when the end-user authorization step is completed. The authorization server MAY require the client to pre-register their redirection URI. The redirection URI MUST NOT include a query component as defined by [RFC3986]  (Berners-Lee, T., Fielding, R., and L. Masinter, “Uniform Resource Identifier (URI): Generic Syntax,” January 2005.) section 3 if the state parameter is present. 
		/// </remarks>
		[MessagePart(Protocol.redirect_uri, IsRequired = false, AllowEmpty = false)]
		public Uri Callback { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the authorization server is
		/// required to redirect the browser back to the client immediately.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. The parameter value must be set to true or false. If set to true, the authorization server MUST NOT prompt the end-user to authenticate or approve access. Instead, the authorization server attempts to establish the end-user's identity via other means (e.g. browser cookies) and checks if the end-user has previously approved an identical access request by the same client and if that access grant is still active. If the authorization server does not support an immediate check or if it is unable to establish the end-user's identity or approval status, it MUST deny the request without prompting the end-user. Defaults to false  if omitted. 
		/// </remarks>
		[MessagePart(Protocol.immediate, IsRequired = false, AllowEmpty = false)]
		internal bool? Immediate { get; set; }

		/// <summary>
		/// Gets or sets the scope.
		/// </summary>
		/// <value>The scope.</value>
		/// <remarks>
		/// OPTIONAL. The scope of the access request expressed as a list of space-delimited strings. The value of the scope parameter is defined by the authorization server. If the value contains multiple space-delimited strings, their order does not matter, and each string adds additional access range to the requested scope. 
		/// </remarks>
		[MessagePart(Protocol.scope, IsRequired = false, AllowEmpty = true)]
		internal string Scope { get; set; }

		/// <summary>
		/// Gets or sets the type of the secret.
		/// </summary>
		/// <value>The type of the secret.</value>
		/// <remarks>
		/// OPTIONAL. The access token secret type as described by Section 5.3  (Cryptographic Tokens Requests). If omitted, the authorization server will issue a bearer token (an access token without a matching secret) as described by Section 5.2  (Bearer Token Requests). 
		/// </remarks>
		[MessagePart(Protocol.secret_type, IsRequired = false, AllowEmpty = false)]
		internal string SecretType { get; set; }
	}
}
