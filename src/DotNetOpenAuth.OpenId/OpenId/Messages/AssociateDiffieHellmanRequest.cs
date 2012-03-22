//-----------------------------------------------------------------------
// <copyright file="AssociateDiffieHellmanRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using Org.Mentalis.Security.Cryptography;

	/// <summary>
	/// An OpenID direct request from Relying Party to Provider to initiate an association that uses Diffie-Hellman encryption.
	/// </summary>
	internal class AssociateDiffieHellmanRequest : AssociateRequest {
		/// <summary>
		/// The (only) value we use for the X variable in the Diffie-Hellman algorithm.
		/// </summary>
		internal static readonly int DefaultX = 1024;

		/// <summary>
		/// The default gen value for the Diffie-Hellman algorithm.
		/// </summary>
		internal static readonly byte[] DefaultGen = { 2 };

		/// <summary>
		/// The default modulus value for the Diffie-Hellman algorithm.
		/// </summary>
		internal static readonly byte[] DefaultMod = {
			0, 220, 249, 58, 11, 136, 57, 114, 236, 14, 25, 152, 154, 197, 162,
			206, 49, 14, 29, 55, 113, 126, 141, 149, 113, 187, 118, 35, 115, 24,
			102, 230, 30, 247, 90, 46, 39, 137, 139, 5, 127, 152, 145, 194, 226,
			122, 99, 156, 63, 41, 182, 8, 20, 88, 28, 211, 178, 202, 57, 134, 210,
			104, 55, 5, 87, 125, 69, 194, 231, 229, 45, 200, 28, 122, 23, 24, 118,
			229, 206, 167, 75, 20, 72, 191, 223, 175, 24, 130, 142, 253, 37, 25,
			241, 78, 69, 227, 130, 102, 52, 175, 25, 73, 229, 181, 53, 204, 130,
			154, 72, 59, 138, 118, 34, 62, 93, 73, 10, 37, 127, 5, 189, 255, 22,
			242, 251, 34, 197, 131, 171 };

		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateDiffieHellmanRequest"/> class.
		/// </summary>
		/// <param name="version">The OpenID version this message must comply with.</param>
		/// <param name="providerEndpoint">The OpenID Provider endpoint.</param>
		internal AssociateDiffieHellmanRequest(Version version, Uri providerEndpoint)
			: base(version, providerEndpoint) {
			this.DiffieHellmanModulus = DefaultMod;
			this.DiffieHellmanGen = DefaultGen;
		}

		/// <summary>
		/// Gets or sets the openid.dh_modulus value.
		/// </summary>
		/// <value>May be null if the default value given in the OpenID spec is to be used.</value>
		[MessagePart("openid.dh_modulus", IsRequired = false, AllowEmpty = false)]
		internal byte[] DiffieHellmanModulus { get; set; }

		/// <summary>
		/// Gets or sets the openid.dh_gen value.
		/// </summary>
		/// <value>May be null if the default value given in the OpenID spec is to be used.</value>
		[MessagePart("openid.dh_gen", IsRequired = false, AllowEmpty = false)]
		internal byte[] DiffieHellmanGen { get; set; }

		/// <summary>
		/// Gets or sets the openid.dh_consumer_public value.
		/// </summary>
		/// <remarks>
		/// This property is initialized with a call to <see cref="InitializeRequest"/>.
		/// </remarks>
		[MessagePart("openid.dh_consumer_public", IsRequired = true, AllowEmpty = false)]
		internal byte[] DiffieHellmanConsumerPublic { get; set; }

		/// <summary>
		/// Gets the Diffie-Hellman algorithm.
		/// </summary>
		/// <remarks>
		/// This property is initialized with a call to <see cref="InitializeRequest"/>.
		/// </remarks>
		internal DiffieHellman Algorithm { get; private set; }

		/// <summary>
		/// Called by the Relying Party to initialize the Diffie-Hellman algorithm and consumer public key properties.
		/// </summary>
		internal void InitializeRequest() {
			if (this.DiffieHellmanModulus == null || this.DiffieHellmanGen == null) {
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, OpenIdStrings.DiffieHellmanRequiredPropertiesNotSet, string.Join(", ", new string[] { "DiffieHellmanModulus", "DiffieHellmanGen" })));
			}

			this.Algorithm = new DiffieHellmanManaged(this.DiffieHellmanModulus ?? DefaultMod, this.DiffieHellmanGen ?? DefaultGen, DefaultX);
			byte[] consumerPublicKeyExchange = this.Algorithm.CreateKeyExchange();
			this.DiffieHellmanConsumerPublic = DiffieHellmanUtilities.EnsurePositive(consumerPublicKeyExchange);
		}
	}
}
