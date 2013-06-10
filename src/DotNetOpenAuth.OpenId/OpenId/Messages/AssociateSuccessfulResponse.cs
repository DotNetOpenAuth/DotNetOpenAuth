//-----------------------------------------------------------------------
// <copyright file="AssociateSuccessfulResponse.cs" company="Outercurve Foundation">
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
	/// The base class that all successful association response messages derive from.
	/// </summary>
	/// <remarks>
	/// Association response messages are described in OpenID 2.0 section 8.2.  This type covers section 8.2.1.
	/// </remarks>
	[DebuggerDisplay("OpenID {Version} associate response {AssociationHandle} {AssociationType} {SessionType}")]
	internal abstract class AssociateSuccessfulResponse : DirectResponseBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateSuccessfulResponse"/> class.
		/// </summary>
		/// <param name="responseVersion">The OpenID version of the response message.</param>
		/// <param name="originatingRequest">The originating request.</param>
		internal AssociateSuccessfulResponse(Version responseVersion, AssociateRequest originatingRequest)
			: base(responseVersion, originatingRequest) {
		}

		/// <summary>
		/// Gets or sets the association handle is used as a key to refer to this association in subsequent messages. 
		/// </summary>
		/// <value>A string 255 characters or less in length. It MUST consist only of ASCII characters in the range 33-126 inclusive (printable non-whitespace characters). </value>
		[MessagePart("assoc_handle", IsRequired = true, AllowEmpty = false)]
		internal string AssociationHandle { get; set; }

		/// <summary>
		/// Gets or sets the preferred association type. The association type defines the algorithm to be used to sign subsequent messages. 
		/// </summary>
		/// <value>Value: A valid association type from Section 8.3.</value>
		[MessagePart("assoc_type", IsRequired = true, AllowEmpty = false)]
		internal string AssociationType { get; set; }

		/// <summary>
		/// Gets or sets the value of the "openid.session_type" parameter from the request. 
		/// If the OP is unwilling or unable to support this association type, it MUST return an 
		/// unsuccessful response (Unsuccessful Response Parameters). 
		/// </summary>
		/// <value>Value: A valid association session type from Section 8.4 (Association Session Types). </value>
		/// <remarks>Note: Unless using transport layer encryption, "no-encryption" MUST NOT be used. </remarks>
		[MessagePart("session_type", IsRequired = false, AllowEmpty = true)]
		[MessagePart("session_type", IsRequired = true, AllowEmpty = false, MinVersion = "2.0")]
		internal string SessionType { get; set; }

		/// <summary>
		/// Gets or sets the lifetime, in seconds, of this association. The Relying Party MUST NOT use the association after this time has passed. 
		/// </summary>
		/// <value>An integer, represented in base 10 ASCII. </value>
		[MessagePart("expires_in", IsRequired = true)]
		internal long ExpiresIn { get; set; }

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

			if (this.Version.Major < 2) {
				ErrorUtilities.VerifyProtocol(
					string.IsNullOrEmpty(this.SessionType) || string.Equals(this.SessionType, this.Protocol.Args.SessionType.DH_SHA1, StringComparison.Ordinal),
					MessagingStrings.UnexpectedMessagePartValueForConstant,
					GetType().Name,
					Protocol.openid.session_type,
					this.Protocol.Args.SessionType.DH_SHA1,
					this.SessionType);
			}
		}
	}
}
