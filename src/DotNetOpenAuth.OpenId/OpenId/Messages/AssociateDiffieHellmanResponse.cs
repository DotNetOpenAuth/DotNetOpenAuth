//-----------------------------------------------------------------------
// <copyright file="AssociateDiffieHellmanResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Security.Cryptography;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using Org.Mentalis.Security.Cryptography;

	/// <summary>
	/// The successful Diffie-Hellman association response message.
	/// </summary>
	/// <remarks>
	/// Association response messages are described in OpenID 2.0 section 8.2.  This type covers section 8.2.3.
	/// </remarks>
	internal abstract class AssociateDiffieHellmanResponse : AssociateSuccessfulResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateDiffieHellmanResponse"/> class.
		/// </summary>
		/// <param name="responseVersion">The OpenID version of the response message.</param>
		/// <param name="originatingRequest">The originating request.</param>
		internal AssociateDiffieHellmanResponse(Version responseVersion, AssociateDiffieHellmanRequest originatingRequest)
			: base(responseVersion, originatingRequest) {
		}

		/// <summary>
		/// Gets or sets the Provider's Diffie-Hellman public key. 
		/// </summary>
		/// <value>btwoc(g ^ xb mod p)</value>
		[MessagePart("dh_server_public", IsRequired = true, AllowEmpty = false)]
		internal byte[] DiffieHellmanServerPublic { get; set; }

		/// <summary>
		/// Gets or sets the MAC key (shared secret), encrypted with the secret Diffie-Hellman value.
		/// </summary>
		/// <value>H(btwoc(g ^ (xa * xb) mod p)) XOR MAC key. H is either "SHA1" or "SHA256" depending on the session type. </value>
		[MessagePart("enc_mac_key", IsRequired = true, AllowEmpty = false)]
		internal byte[] EncodedMacKey { get; set; }
	}
}
