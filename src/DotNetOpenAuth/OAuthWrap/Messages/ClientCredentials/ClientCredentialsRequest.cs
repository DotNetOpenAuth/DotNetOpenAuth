//-----------------------------------------------------------------------
// <copyright file="ClientCredentialsRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;

	/// <summary>
	/// A request for an access token for a client application that has its
	/// own (non-user affiliated) client name and password.
	/// </summary>
	/// <remarks>
	/// This is somewhat analogous to 2-legged OAuth.
	/// </remarks>
	internal class ClientCredentialsRequest : MessageBase, IAccessTokenRequest, IOAuthDirectResponseFormat {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClientCredentialsRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal ClientCredentialsRequest(Uri authorizationServer, Version version)
			: base(version, MessageTransport.Direct, authorizationServer) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		/// <summary>
		/// Gets or sets the account name.
		/// </summary>
		/// <value>The name on the account.</value>
		[MessagePart(Protocol.client_id, IsRequired = true, AllowEmpty = false)]
		public string ClientIdentifier { get; internal set; }

		/// <summary>
		/// Gets or sets the user's password.
		/// </summary>
		/// <value>The password.</value>
		[MessagePart(Protocol.client_secret, IsRequired = true, AllowEmpty = false)]
		public string ClientSecret { get; internal set; }

		/// <summary>
		/// Gets or sets the type of the secret.
		/// </summary>
		/// <value>The type of the secret.</value>
		/// <remarks>
		/// OPTIONAL. The access token secret type as described by Section 5.3  (Cryptographic Tokens Requests). If omitted, the authorization server will issue a bearer token (an access token without a matching secret) as described by Section 5.2  (Bearer Token Requests). 
		/// </remarks>
		[MessagePart(Protocol.secret_type, IsRequired = false, AllowEmpty = false)]
		public string SecretType { get; set; }

		/// <summary>
		/// Gets the format the client is requesting the authorization server should deliver the request in.
		/// </summary>
		/// <value>The format.</value>
		ResponseFormat IOAuthDirectResponseFormat.Format {
			get { return this.Format.HasValue ? this.Format.Value : ResponseFormat.Json; }
		}

		/// <summary>
		/// Gets or sets an optional authorization scope as defined by the Authorization Server.
		/// </summary>
		[MessagePart(Protocol.scope, IsRequired = false, AllowEmpty = true)]
		internal string Scope { get; set; }

		/// <summary>
		/// Gets or sets the format the client is requesting the authorization server should deliver the request in.
		/// </summary>
		/// <value>The format.</value>
		[MessagePart(Protocol.format, Encoder = typeof(ResponseFormatEncoder))]
		private ResponseFormat? Format { get; set; }

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// 	<para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the
		/// message to see if it conforms to the protocol.</para>
		/// 	<para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();
			ErrorUtilities.VerifyProtocol(this.Recipient.IsTransportSecure(), OAuthWrapStrings.HttpsRequired);
		}
	}
}
