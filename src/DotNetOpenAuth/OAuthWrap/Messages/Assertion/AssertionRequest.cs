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
		[MessagePart(Protocol.wrap_assertion_format, IsRequired = true, AllowEmpty = false)]
		internal string AssertionFormat { get; set; }

		/// <summary>
		/// Gets or sets the assertion.
		/// </summary>
		/// <value>The assertion.</value>
		[MessagePart(Protocol.wrap_assertion, IsRequired = true, AllowEmpty = false)]
		internal string Assertion { get; set; }

		/// <summary>
		/// Gets or sets an optional authorization scope as defined by the Authorization Server.
		/// </summary>
		[MessagePart(Protocol.wrap_scope, IsRequired = false, AllowEmpty = true)]
		internal string Scope { get; set; }

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
