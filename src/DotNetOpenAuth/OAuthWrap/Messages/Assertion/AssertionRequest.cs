//-----------------------------------------------------------------------
// <copyright file="AssertionRequest.cs" company="Andrew Arnott">
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
	/// A request from a Client to an Authorization Server with some assertion for an access token.
	/// </summary>
	internal class AssertionRequest : MessageBase {
		/// <summary>
		/// The type of message.
		/// </summary>
		[MessagePart(Protocol.type, IsRequired = true)]
		private const string Type = "assertion";

		/// <summary>
		/// Initializes a new instance of the <see cref="AssertionRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal AssertionRequest(Uri authorizationServer, Version version)
			: base(version, MessageTransport.Direct, authorizationServer) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		/// <summary>
		/// Gets or sets the format of the assertion as defined by the Authorization Server.
		/// </summary>
		/// <value>The assertion format.</value>
		[MessagePart(Protocol.format, IsRequired = true, AllowEmpty = false)]
		internal string AssertionFormat { get; set; }

		/// <summary>
		/// Gets or sets the assertion.
		/// </summary>
		/// <value>The assertion.</value>
		[MessagePart(Protocol.assertion, IsRequired = true, AllowEmpty = false)]
		internal string Assertion { get; set; }

		/// <summary>
		/// Gets or sets an optional authorization scope as defined by the Authorization Server.
		/// </summary>
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
