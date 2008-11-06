//-----------------------------------------------------------------------
// <copyright file="AssociateDiffieHellmanResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// The successful Diffie-Hellman association response message.
	/// </summary>
	/// <remarks>
	/// Association response messages are described in OpenID 2.0 section 8.2.  This type covers section 8.2.3.
	/// </remarks>
	internal class AssociateDiffieHellmanResponse : AssociateSuccessfulResponse {
		/// <summary>
		/// Gets or sets the OP's Diffie-Hellman public key. 
		/// </summary>
		/// <value>btwoc(g ^ xb mod p)</value>
		[MessagePart("dh_server_public", IsRequired = true, AllowEmpty = false)]
		internal byte[] ServerPublic { get; set; }

		/// <summary>
		/// Gets or sets the MAC key (shared secret), encrypted with the secret Diffie-Hellman value. H is either "SHA1" or "SHA256" depending on the session type. 
		/// </summary>
		/// <value>H(btwoc(g ^ (xa * xb) mod p)) XOR MAC key</value>
		[MessagePart("enc_mac_key", IsRequired = true, AllowEmpty = false)]
		internal byte[] EncodedMacKey { get; set; }
	}
}
