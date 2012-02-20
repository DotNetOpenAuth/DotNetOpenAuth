//-----------------------------------------------------------------------
// <copyright file="CheckIdRequest.cs" company="Outercurve Foundation">
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
	/// An authentication request from a Relying Party to a Provider.
	/// </summary>
	/// <remarks>
	/// This message type satisfies OpenID 2.0 section 9.1.
	/// </remarks>
	[DebuggerDisplay("OpenID {Version} {Mode} {ClaimedIdentifier}")]
	[Serializable]
	internal class CheckIdRequest : SignedResponseRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckIdRequest"/> class.
		/// </summary>
		/// <param name="version">The OpenID version to use.</param>
		/// <param name="providerEndpoint">The Provider endpoint that receives this message.</param>
		/// <param name="mode">
		/// <see cref="AuthenticationRequestMode.Immediate"/> for asynchronous javascript clients;
		/// <see cref="AuthenticationRequestMode.Setup"/>  to allow the Provider to interact with the user in order to complete authentication.
		/// </param>
		internal CheckIdRequest(Version version, Uri providerEndpoint, AuthenticationRequestMode mode) :
			base(version, providerEndpoint, mode) {
		}

		/// <summary>
		/// Gets or sets the Claimed Identifier.
		/// </summary>
		/// <remarks>
		/// <para>"openid.claimed_id" and "openid.identity" SHALL be either both present or both absent. 
		/// If neither value is present, the assertion is not about an identifier, 
		/// and will contain other information in its payload, using extensions (Extensions). </para>
		/// <para>It is RECOMMENDED that OPs accept XRI identifiers with or without the "xri://" prefix, as specified in the Normalization (Normalization) section. </para>
		/// </remarks>
		[MessagePart("openid.claimed_id", IsRequired = true, AllowEmpty = false, MinVersion = "2.0")]
		internal Identifier ClaimedIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the OP Local Identifier.
		/// </summary>
		/// <value>The OP-Local Identifier. </value>
		/// <remarks>
		/// <para>If a different OP-Local Identifier is not specified, the claimed 
		/// identifier MUST be used as the value for openid.identity.</para>
		/// <para>Note: If this is set to the special value 
		/// "http://specs.openid.net/auth/2.0/identifier_select" then the OP SHOULD 
		/// choose an Identifier that belongs to the end user. This parameter MAY 
		/// be omitted if the request is not about an identifier (for instance if 
		/// an extension is in use that makes the request meaningful without it; 
		/// see openid.claimed_id above). </para>
		/// </remarks>
		[MessagePart("openid.identity", IsRequired = true, AllowEmpty = false)]
		internal Identifier LocalIdentifier { get; set; }

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

			if (this.Protocol.ClaimedIdentifierForOPIdentifier != null) {
				// Ensure that the claimed_id and identity parameters are either both the 
				// special identifier_select value or both NOT that value.
				ErrorUtilities.VerifyProtocol(
					(this.LocalIdentifier == this.Protocol.ClaimedIdentifierForOPIdentifier) == (this.ClaimedIdentifier == this.Protocol.ClaimedIdentifierForOPIdentifier),
					OpenIdStrings.MatchingArgumentsExpected,
					Protocol.openid.claimed_id,
					Protocol.openid.identity,
					Protocol.ClaimedIdentifierForOPIdentifier);
			}
		}
	}
}
