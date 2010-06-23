//-----------------------------------------------------------------------
// <copyright file="AuthorizationDataBag.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// A data bag that stores authorization data.
	/// </summary>
	[Serializable]
	internal abstract class AuthorizationDataBag : DataBag, IAuthorizationDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationDataBag"/> class.
		/// </summary>
		/// <param name="secret">The symmetric secret to use for signing and encrypting.</param>
		/// <param name="signed">A value indicating whether the data in this instance will be protected against tampering.</param>
		/// <param name="encrypted">A value indicating whether the data in this instance will be protected against eavesdropping.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <see cref="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		protected AuthorizationDataBag(byte[] secret, bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: base(secret, signed, encrypted, compressed, maximumAge, decodeOnceOnly) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationDataBag"/> class.
		/// </summary>
		/// <param name="signingKey">The asymmetric private key to use for signing the token.</param>
		/// <param name="encryptingKey">The asymmetric public key to use for encrypting the token.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <see cref="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		protected AuthorizationDataBag(RSAParameters? signingKey = null, RSAParameters? encryptingKey = null, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: base(signingKey, encryptingKey, compressed, maximumAge, decodeOnceOnly) {
		}

		/// <summary>
		/// Gets or sets the identifier of the client authorized to access protected data.
		/// </summary>
		/// <value></value>
		[MessagePart]
		public string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets the date this authorization was established or the token was issued.
		/// </summary>
		/// <value>A date/time expressed in UTC.</value>
		public DateTime UtcIssued {
			get { return this.UtcCreationDate; }
		}

		/// <summary>
		/// Gets or sets the name on the account whose data on the resource server is accessible using this authorization.
		/// </summary>
		[MessagePart]
		public string User { get; set; }

		/// <summary>
		/// Gets or sets the scope of operations the client is allowed to invoke.
		/// </summary>
		[MessagePart]
		public string Scope { get; set; }
	}
}
