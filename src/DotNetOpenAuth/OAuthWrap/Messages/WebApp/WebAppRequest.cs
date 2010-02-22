//-----------------------------------------------------------------------
// <copyright file="WebAppRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;

	/// <summary>
	/// A message sent by a web application Client to the AuthorizationServer
	/// via the user agent to obtain authorization from the user and prepare
	/// to issue an access token to the Consumer if permission is granted.
	/// </summary>
	internal class WebAppRequest : MessageBase, IMessageWithClientState {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppRequest"/> class.
		/// </summary>
		/// <param name="userAuthorizationEndpoint">The Authorization Server's user authorization URL to direct the user to.</param>
		/// <param name="version">The protocol version.</param>
		internal WebAppRequest(Uri userAuthorizationEndpoint, Version version)
			: base(version, MessageTransport.Indirect, userAuthorizationEndpoint) {
			Contract.Requires<ArgumentNullException>(userAuthorizationEndpoint != null);
			Contract.Requires<ArgumentNullException>(version != null);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal WebAppRequest(AuthorizationServerDescription authorizationServer)
			: this(authorizationServer.UserAuthorizationEndpoint, authorizationServer.Version) {
			Contract.Requires<ArgumentNullException>(authorizationServer != null);
			Contract.Requires<ArgumentException>(authorizationServer.Version != null);
			Contract.Requires<ArgumentException>(authorizationServer.UserAuthorizationEndpoint != null);
		}

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
		public string ClientState { get; set; }

		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
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
		[MessagePart(Protocol.wrap_callback, IsRequired = true, AllowEmpty = false)]
		internal Uri Callback { get; set; }

		/// <summary>
		/// Gets or sets the scope.
		/// </summary>
		/// <value>The Authorization Server MAY define authorization scope values for the Client to include.</value>
		[MessagePart(Protocol.wrap_scope, IsRequired = false, AllowEmpty = true)]
		internal string Scope { get; set; }
	}
}
