//-----------------------------------------------------------------------
// <copyright file="CheckIdRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
	internal class CheckIdRequest : SignedResponseRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckIdRequest"/> class.
		/// </summary>
		/// <param name="version">The OpenID version to use.</param>
		/// <param name="providerEndpoint">The Provider endpoint that receives this message.</param>
		/// <param name="immediate">
		/// <c>true</c> for asynchronous javascript clients; 
		/// <c>false</c> to allow the Provider to interact with the user in order to complete authentication.
		/// </param>
		internal CheckIdRequest(Version version, Uri providerEndpoint, bool immediate) :
			base(version, providerEndpoint, immediate) {
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
	}
}
