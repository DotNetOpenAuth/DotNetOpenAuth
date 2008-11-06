//-----------------------------------------------------------------------
// <copyright file="AssociateUnencryptedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// The successful unencrypted association response message.
	/// </summary>
	/// <remarks>
	/// Association response messages are described in OpenID 2.0 section 8.2.  This type covers section 8.2.2.
	/// </remarks>
	internal class AssociateUnencryptedResponse : AssociateSuccessfulResponse {
		/// <summary>
		/// Gets or sets the MAC key (shared secret) for this association, Base 64 (Josefsson, S., “The Base16, Base32, and Base64 Data Encodings,” .) [RFC3548] encoded. 
		/// </summary>
		[MessagePart("mac_key", IsRequired = true, AllowEmpty = false)]
		internal byte[] MacKey { get; set; }
	}
}
