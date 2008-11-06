//-----------------------------------------------------------------------
// <copyright file="AssociateDiffieHellmanRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// An OpenID direct request from Relying Party to Provider to initiate an association that uses Diffie-Hellman encryption.
	/// </summary>
	internal class AssociateDiffieHellmanRequest : AssociateRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateDiffieHellmanRequest"/> class.
		/// </summary>
		/// <param name="providerEndpoint">The OpenID Provider endpoint.</param>
		internal AssociateDiffieHellmanRequest(Uri providerEndpoint)
			: base(providerEndpoint) {
		}

		/// <summary>
		/// Gets or sets the openid.dh_modulus value.
		/// </summary>
		[MessagePart("openid.dh_modulus", IsRequired = false, AllowEmpty = false)]
		internal byte[] DiffieHellmanModulus { get; set; }

		/// <summary>
		/// Gets or sets the openid.dh_gen value.
		/// </summary>
		[MessagePart("openid.dh_gen", IsRequired = false, AllowEmpty = false)]
		internal byte[] DiffieHellmanGen { get; set; }

		/// <summary>
		/// Gets or sets the openid.dh_consumer_public value.
		/// </summary>
		[MessagePart("openid.dh_consumer_public", IsRequired = true, AllowEmpty = false)]
		internal byte[] DiffieHellmanConsumerPublic { get; set; }
	}
}
