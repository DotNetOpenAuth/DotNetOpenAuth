//-----------------------------------------------------------------------
// <copyright file="PositiveAssertionResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using System.Net.Security;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.ChannelElements;

	/// <summary>
	/// An identity assertion from a Provider to a Relying Party, stating that the
	/// user operating the user agent is in fact some specific user known to the Provider.
	/// </summary>
	[DebuggerDisplay("OpenID {ProtocolVersion} {Mode} {LocalIdentifier}")]
	internal class PositiveAssertionResponse : IndirectResponseBase, ITamperResistantOpenIdMessage {
		/// <summary>
		/// The allowed date/time formats for the response_nonce parameter.
		/// </summary>
		/// <remarks>
		/// This array of formats is not yet a complete list.
		/// </remarks>
		private static readonly string[] PermissibleDateTimeFormats = { "yyyy-MM-ddTHH:mm:ssZ" };

		/// <summary>
		/// Backing field for the <see cref="IExpiringProtocolMessage.UtcCreationDate"/> property.
		/// </summary>
		private DateTime creationDateUtc;

		/// <summary>
		/// Initializes a new instance of the <see cref="PositiveAssertionResponse"/> class.
		/// </summary>
		/// <param name="request">
		/// The authentication request that caused this assertion to be generated.
		/// </param>
		internal PositiveAssertionResponse(CheckIdRequest request)
			: base(request, Protocol.Lookup(GetVersion(request)).Args.Mode.id_res) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			this.ReturnTo = request.ReturnTo;
			this.ProviderEndpoint = request.Recipient;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PositiveAssertionResponse"/> class
		/// for unsolicited assertions.
		/// </summary>
		/// <param name="version">The OpenID version to use.</param>
		/// <param name="relyingPartyReturnTo">The return_to URL of the Relying Party.
		/// This value will commonly be from <see cref="CheckIdRequest.ReturnTo"/>,
		/// but for unsolicited assertions may come from the Provider performing RP discovery
		/// to find the appropriate return_to URL to use.</param>
		internal PositiveAssertionResponse(Version version, Uri relyingPartyReturnTo)
			: base(version, relyingPartyReturnTo, Protocol.Lookup(version).Args.Mode.id_res) {
		}

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		/// <value><see cref="MessageProtections.All"/></value>
		public override MessageProtections RequiredProtection {
			get { return MessageProtections.All; }
		}

		/// <summary>
		/// Gets or sets the message signature.
		/// </summary>
		/// <value>Base 64 encoded signature calculated as specified in Section 6 (Generating Signatures).</value>
		[MessagePart("openid.sig", IsRequired = true, AllowEmpty = false)]
		string ITamperResistantProtocolMessage.Signature { get; set; }

		/// <summary>
		/// Gets or sets the signed parameter order.
		/// </summary>
		/// <value>Comma-separated list of signed fields.</value>
		/// <example>"op_endpoint,identity,claimed_id,return_to,assoc_handle,response_nonce"</example>
		/// <remarks>
		/// This entry consists of the fields without the "openid." prefix that the signature covers.
		/// This list MUST contain at least "op_endpoint", "return_to" "response_nonce" and "assoc_handle",
		/// and if present in the response, "claimed_id" and "identity".
		/// Additional keys MAY be signed as part of the message. See Generating Signatures.
		/// </remarks>
		[MessagePart("openid.signed", IsRequired = true, AllowEmpty = false)]
		string ITamperResistantOpenIdMessage.SignedParameterOrder { get; set; }

		/// <summary>
		/// Gets or sets the association handle used to sign the message.
		/// </summary>
		/// <value>The handle for the association that was used to sign this assertion. </value>
		[MessagePart("openid.assoc_handle", IsRequired = true, AllowEmpty = false, RequiredProtection = ProtectionLevel.Sign)]
		string ITamperResistantOpenIdMessage.AssociationHandle { get; set; }

		/// <summary>
		/// Gets or sets the nonce that will protect the message from replay attacks.
		/// </summary>
		string IReplayProtectedProtocolMessage.Nonce { get; set; }

		/// <summary>
		/// Gets or sets the UTC date/time the message was originally sent onto the network.
		/// </summary>
		/// <remarks>
		/// The property setter should ensure a UTC date/time,
		/// and throw an exception if this is not possible.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown when a DateTime that cannot be converted to UTC is set.
		/// </exception>
		DateTime IExpiringProtocolMessage.UtcCreationDate {
			get { return this.creationDateUtc; }
			set { this.creationDateUtc = value.ToUniversalTime(); }
		}

		/// <summary>
		/// Gets or sets the association handle that the Provider wants the Relying Party to not use any more.
		/// </summary>
		/// <value>If the Relying Party sent an invalid association handle with the request, it SHOULD be included here.</value>
		[MessagePart("openid.invalidate_handle", IsRequired = false, AllowEmpty = false)]
		string ITamperResistantOpenIdMessage.InvalidateHandle { get; set; }

		/// <summary>
		/// Gets or sets the Provider Endpoint URI.
		/// </summary>
		[MessagePart("openid.op_endpoint", IsRequired = true, AllowEmpty = false, RequiredProtection = ProtectionLevel.Sign, MinVersion = "2.0")]
		internal Uri ProviderEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the Claimed Identifier.
		/// </summary>
		/// <remarks>
		/// <para>"openid.claimed_id" and "openid.identity" SHALL be either both present or both absent. 
		/// If neither value is present, the assertion is not about an identifier, 
		/// and will contain other information in its payload, using extensions (Extensions). </para>
		/// </remarks>
		[MessagePart("openid.claimed_id", IsRequired = false, AllowEmpty = false, RequiredProtection = ProtectionLevel.Sign, MinVersion = "2.0")]
		internal Identifier ClaimedIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the OP Local Identifier.
		/// </summary>
		/// <value>The OP-Local Identifier. </value>
		/// <remarks>
		/// <para>OpenID Providers MAY assist the end user in selecting the Claimed 
		/// and OP-Local Identifiers about which the assertion is made. 
		/// The openid.identity field MAY be omitted if an extension is in use that 
		/// makes the response meaningful without it (see openid.claimed_id above). </para>
		/// </remarks>
		[MessagePart("openid.identity", IsRequired = false, AllowEmpty = false, RequiredProtection = ProtectionLevel.Sign)]
		internal Identifier LocalIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the return_to parameter as the relying party provided
		/// it in <see cref="CheckIdRequest.ReturnTo"/>.
		/// </summary>
		/// <value>Verbatim copy of the return_to URL parameter sent in the
		/// request, before the Provider modified it. </value>
		[MessagePart("openid.return_to", IsRequired = true, AllowEmpty = false, RequiredProtection = ProtectionLevel.Sign, Encoder = typeof(OriginalStringUriEncoder))]
		internal Uri ReturnTo { get; set; }

		/// <summary>
		/// Gets or sets the nonce that will protect the message from replay attacks.
		/// </summary>
		/// <value>
		/// <para>A string 255 characters or less in length, that MUST be unique to 
		/// this particular successful authentication response. The nonce MUST start 
		/// with the current time on the server, and MAY contain additional ASCII 
		/// characters in the range 33-126 inclusive (printable non-whitespace characters), 
		/// as necessary to make each response unique. The date and time MUST be 
		/// formatted as specified in section 5.6 of [RFC3339] 
		/// (Klyne, G. and C. Newman, “Date and Time on the Internet: Timestamps,” .), 
		/// with the following restrictions:</para>
		/// <list type="bullet">
		///   <item>All times must be in the UTC timezone, indicated with a "Z".</item>
		///   <item>No fractional seconds are allowed</item>
		/// </list>
		/// </value>
		/// <example>2005-05-15T17:11:51ZUNIQUE</example>
		[MessagePart("openid.response_nonce", IsRequired = true, AllowEmpty = false, RequiredProtection = ProtectionLevel.Sign, MinVersion = "2.0")]
		private string ResponseNonce {
			get {
				string uniqueFragment = ((IReplayProtectedProtocolMessage)this).Nonce;
				return this.creationDateUtc.ToString(PermissibleDateTimeFormats[0], CultureInfo.InvariantCulture) + uniqueFragment;
			}

			set {
				if (value == null) {
					((IReplayProtectedProtocolMessage)this).Nonce = null;
				} else {
					int indexOfZ = value.IndexOf("Z", StringComparison.Ordinal);
					ErrorUtilities.VerifyProtocol(indexOfZ >= 0, MessagingStrings.UnexpectedMessagePartValue, Protocol.openid.response_nonce, value);
					this.creationDateUtc = DateTime.Parse(value.Substring(0, indexOfZ + 1), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
					((IReplayProtectedProtocolMessage)this).Nonce = value.Substring(indexOfZ + 1);
				}
			}
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

			if (this.ProtocolVersion.Major >= 2) {
				// Ensure that claimed_id and identity are either both present or both absent.
				ErrorUtilities.VerifyProtocol((this.ClaimedIdentifier == null) == (this.LocalIdentifier == null), OpenIdStrings.ClaimedIdAndLocalIdMustBothPresentOrAbsent);
			}
		}
	}
}
