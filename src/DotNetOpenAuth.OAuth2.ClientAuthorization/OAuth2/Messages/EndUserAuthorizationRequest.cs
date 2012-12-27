//-----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using Validation;

	/// <summary>
	/// A message sent by a web application Client to the AuthorizationServer
	/// via the user agent to obtain authorization from the user and prepare
	/// to issue an access token to the client if permission is granted.
	/// </summary>
	[Serializable]
	public class EndUserAuthorizationRequest : MessageBase {
		/// <summary>
		/// Gets the grant type that the client expects of the authorization server.
		/// </summary>
		/// <value>Always <see cref="EndUserAuthorizationResponseType.AuthorizationCode"/>.  Other response types are not supported.</value>
		[MessagePart(Protocol.response_type, IsRequired = true, Encoder = typeof(EndUserAuthorizationResponseTypeEncoder))]
		private const EndUserAuthorizationResponseType ResponseTypeConst = EndUserAuthorizationResponseType.AuthorizationCode;

		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationRequest"/> class.
		/// </summary>
		/// <param name="authorizationEndpoint">The Authorization Server's user authorization URL to direct the user to.</param>
		/// <param name="version">The protocol version.</param>
		protected EndUserAuthorizationRequest(Uri authorizationEndpoint, Version version)
			: base(version, MessageTransport.Indirect, authorizationEndpoint) {
			Requires.NotNull(authorizationEndpoint, "authorizationEndpoint");
			Requires.NotNull(version, "version");
			this.HttpMethods = HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.PostRequest;
			this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
		}

		/// <summary>
		/// Gets the grant type that the client expects of the authorization server.
		/// </summary>
		public virtual EndUserAuthorizationResponseType ResponseType {
			get { return ResponseTypeConst; }
		}

		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		[MessagePart(Protocol.client_id, IsRequired = true)]
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
		[MessagePart(Protocol.redirect_uri, IsRequired = false)]
		public Uri Callback { get; set; }

		/// <summary>
		/// Gets or sets state of the client that should be sent back with the authorization response.
		/// </summary>
		/// <value>
		/// An opaque value that Clients can use to maintain state associated with this request. 
		/// </value>
		/// <remarks>
		/// This data is proprietary to the client and should be considered an opaque string to the
		/// authorization server.
		/// </remarks>
		[MessagePart(Protocol.state, IsRequired = false)]
		public string ClientState { get; set; }

		/// <summary>
		/// Gets the scope of access being requested.
		/// </summary>
		/// <value>The scope of the access request expressed as a list of space-delimited strings. The value of the scope parameter is defined by the authorization server. If the value contains multiple space-delimited strings, their order does not matter, and each string adds an additional access range to the requested scope.</value>
		[MessagePart(Protocol.scope, IsRequired = false, Encoder = typeof(ScopeEncoder))]
		public HashSet<string> Scope { get; private set; }

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();

			ErrorUtilities.VerifyProtocol(
				DotNetOpenAuthSection.Messaging.RelaxSslRequirements || this.Recipient.IsTransportSecure(),
				OAuthStrings.HttpsRequired);
			ErrorUtilities.VerifyProtocol(this.Callback == null || this.Callback.IsAbsoluteUri, this, OAuthStrings.AbsoluteUriRequired, Protocol.redirect_uri);
		}
	}
}
