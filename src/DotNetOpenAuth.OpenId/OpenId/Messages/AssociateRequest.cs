//-----------------------------------------------------------------------
// <copyright file="AssociateRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An OpenID direct request from Relying Party to Provider to initiate an association.
	/// </summary>
	[DebuggerDisplay("OpenID {Version} {Mode} {AssociationType} {SessionType}")]
	internal abstract class AssociateRequest : RequestBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateRequest"/> class.
		/// </summary>
		/// <param name="version">The OpenID version this message must comply with.</param>
		/// <param name="providerEndpoint">The OpenID Provider endpoint.</param>
		protected AssociateRequest(Version version, Uri providerEndpoint)
			: base(version, providerEndpoint, GetProtocolConstant(version, p => p.Args.Mode.associate), MessageTransport.Direct) {
		}

		/// <summary>
		/// Gets or sets the preferred association type. The association type defines the algorithm to be used to sign subsequent messages. 
		/// </summary>
		/// <value>Value: A valid association type from Section 8.3.</value>
		[MessagePart("openid.assoc_type", IsRequired = true, AllowEmpty = false)]
		internal string AssociationType { get; set; }

		/// <summary>
		/// Gets or sets the preferred association session type. This defines the method used to encrypt the association's MAC key in transit. 
		/// </summary>
		/// <value>Value: A valid association session type from Section 8.4 (Association Session Types). </value>
		/// <remarks>Note: Unless using transport layer encryption, "no-encryption" MUST NOT be used. </remarks>
		[MessagePart("openid.session_type", IsRequired = false, AllowEmpty = true)]
		[MessagePart("openid.session_type", IsRequired = true, AllowEmpty = false, MinVersion = "2.0")]
		internal string SessionType { get; set; }

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
				!string.Equals(this.SessionType, Protocol.Args.SessionType.NoEncryption, StringComparison.Ordinal) || this.Recipient.IsTransportSecure(),
				OpenIdStrings.NoEncryptionSessionRequiresHttps,
				this);
		}
	}
}
