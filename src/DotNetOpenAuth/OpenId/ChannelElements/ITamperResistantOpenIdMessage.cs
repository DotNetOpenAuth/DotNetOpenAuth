//-----------------------------------------------------------------------
// <copyright file="ITamperResistantOpenIdMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An interface that OAuth messages implement to support signing.
	/// </summary>
	internal interface ITamperResistantOpenIdMessage : ITamperResistantProtocolMessage {
		/// <summary>
		/// Gets or sets the association handle used to sign the message.
		/// </summary>
		string AssociationHandle { get; set; }

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
		string SignedParameterOrder { get; set; }
	}
}
