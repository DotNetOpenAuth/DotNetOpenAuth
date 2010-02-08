//-----------------------------------------------------------------------
// <copyright file="RichAppRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages.RichApp {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class RichAppRequest : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="RichAppRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal RichAppRequest(Uri authorizationServer, Version version)
			: base(version, MessageTransport.Indirect, authorizationServer) {
		}

		/// <summary>
		/// Gets or sets the client identifier previously obtained from the Authorization Server.
		/// </summary>
		/// <value>The client identifier.</value>
		[MessagePart(Protocol.wrap_client_id, IsRequired = true, AllowEmpty = false)]
		internal string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the callback URL.
		/// </summary>
		/// <value>
		/// An absolute URL to which the Authorization Server will redirect the User back after
		/// the user has approved the authorization request.
		/// </value>
		/// <remarks>
		/// Authorization Servers MAY require that the wrap_callback URL match the previously
		/// registered value for the Client Identifier.
		/// </remarks>
		[MessagePart(Protocol.wrap_callback, IsRequired = false, AllowEmpty = false)]
		internal Uri Callback { get; set; }

		/// <summary>
		/// Gets or sets state of the client that should be sent back with the authorization response.
		/// </summary>
		/// <value>
		/// An opaque value that Clients can use to maintain state associated with this request. 
		/// </value>
		/// <remarks>
		/// If this value is present, the Authorization Server MUST return it to the Client's Callback URL.
		/// </remarks>
		[MessagePart(Protocol.wrap_client_state, IsRequired = false, AllowEmpty = true)]
		internal string ClientState { get; set; }

		/// <summary>
		/// Gets or sets the scope.
		/// </summary>
		/// <value>The Authorization Server MAY define authorization scope values for the Client to include.</value>
		[MessagePart(Protocol.wrap_scope, IsRequired = false, AllowEmpty = true)]
		internal string Scope { get; set; }
	}
}
