//-----------------------------------------------------------------------
// <copyright file="AssociateUnencryptedRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// Represents an association request that is sent using HTTPS and otherwise communicates the shared secret in plain text.
	/// </summary>
	internal class AssociateUnencryptedRequest : AssociateRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateUnencryptedRequest"/> class.
		/// </summary>
		/// <param name="version">The OpenID version this message must comply with.</param>
		/// <param name="providerEndpoint">The OpenID Provider endpoint.</param>
		internal AssociateUnencryptedRequest(Version version, Uri providerEndpoint)
			: base(version, providerEndpoint) {
			SessionType = Protocol.Args.SessionType.NoEncryption;
		}

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
		public override void EnsureValidMessage() {
			base.EnsureValidMessage();

			ErrorUtilities.VerifyProtocol(
				string.Equals(this.SessionType, Protocol.Args.SessionType.NoEncryption, StringComparison.Ordinal),
				MessagingStrings.UnexpectedMessagePartValueForConstant,
				GetType().Name,
				Protocol.openid.session_type,
				Protocol.Args.SessionType.NoEncryption,
				SessionType);
		}
	}
}
