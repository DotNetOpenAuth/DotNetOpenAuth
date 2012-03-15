//-----------------------------------------------------------------------
// <copyright file="ITamperResistantOpenIdMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// An interface that OAuth messages implement to support signing.
	/// </summary>
	internal interface ITamperResistantOpenIdMessage : ITamperResistantProtocolMessage, IReplayProtectedProtocolMessage {
		/// <summary>
		/// Gets or sets the association handle used to sign the message.
		/// </summary>
		/// <value>The handle for the association that was used to sign this assertion. </value>
		string AssociationHandle { get; set; }

		/// <summary>
		/// Gets or sets the association handle that the Provider wants the Relying Party to not use any more.
		/// </summary>
		/// <value>If the Relying Party sent an invalid association handle with the request, it SHOULD be included here.</value>
		string InvalidateHandle { get; set; }

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
		[SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1630:DocumentationTextMustContainWhitespace", Justification = "The samples are string literals.")]
		string SignedParameterOrder { get; set; } // TODO: make sure we have a unit test to verify that an incoming message with fewer signed fields than required will be rejected.
	}
}
