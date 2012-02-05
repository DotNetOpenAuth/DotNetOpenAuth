//-----------------------------------------------------------------------
// <copyright file="PositiveAssertionResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
	[DebuggerDisplay("OpenID {Version} {Mode} {LocalIdentifier}")]
	[Serializable]
	internal class PositiveAssertionResponse : IndirectSignedResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="PositiveAssertionResponse"/> class.
		/// </summary>
		/// <param name="request">
		/// The authentication request that caused this assertion to be generated.
		/// </param>
		internal PositiveAssertionResponse(CheckIdRequest request)
			: base(request) {
			this.ClaimedIdentifier = request.ClaimedIdentifier;
			this.LocalIdentifier = request.LocalIdentifier;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PositiveAssertionResponse"/> class
		/// for unsolicited assertions.
		/// </summary>
		/// <param name="version">The OpenID version to use.</param>
		/// <param name="relyingPartyReturnTo">The return_to URL of the Relying Party.
		/// This value will commonly be from <see cref="SignedResponseRequest.ReturnTo"/>,
		/// but for unsolicited assertions may come from the Provider performing RP discovery
		/// to find the appropriate return_to URL to use.</param>
		internal PositiveAssertionResponse(Version version, Uri relyingPartyReturnTo)
			: base(version, relyingPartyReturnTo) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PositiveAssertionResponse"/> class.
		/// </summary>
		/// <param name="relyingParty">The relying party return_to endpoint that will receive this positive assertion.</param>
		internal PositiveAssertionResponse(RelyingPartyEndpointDescription relyingParty)
			: this(relyingParty.Protocol.Version, relyingParty.ReturnToEndpoint) {
		}

		/// <summary>
		/// Gets or sets the Claimed Identifier.
		/// </summary>
		/// <remarks>
		/// <para>"openid.claimed_id" and "openid.identity" SHALL be either both present or both absent. 
		/// If neither value is present, the assertion is not about an identifier, 
		/// and will contain other information in its payload, using extensions (Extensions). </para>
		/// </remarks>
		[MessagePart("openid.claimed_id", IsRequired = true, AllowEmpty = false, RequiredProtection = ProtectionLevel.Sign, MinVersion = "2.0")]
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
		[MessagePart("openid.identity", IsRequired = true, AllowEmpty = false, RequiredProtection = ProtectionLevel.Sign)]
		internal Identifier LocalIdentifier { get; set; }
	}
}
