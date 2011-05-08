//-----------------------------------------------------------------------
// <copyright file="UriStyleMessageFormatter.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// A serializer for <see cref="DataBag"/>-derived types
	/// </summary>
	/// <typeparam name="T">The DataBag-derived type that is to be serialized/deserialized.</typeparam>
	internal class UriStyleMessageFormatter<T> : DataBagFormatterBase<T> where T : DataBag, new() {
		/// <summary>
		/// Initializes a new instance of the <see cref="UriStyleMessageFormatter&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="signingKey">The crypto service provider with the asymmetric key to use for signing or verifying the token.</param>
		/// <param name="encryptingKey">The crypto service provider with the asymmetric key to use for encrypting or decrypting the token.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <see cref="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		protected internal UriStyleMessageFormatter(RSACryptoServiceProvider signingKey = null, RSACryptoServiceProvider encryptingKey = null, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: base(signingKey, encryptingKey, compressed, maximumAge, decodeOnceOnly) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UriStyleMessageFormatter&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="symmetricSecret">The symmetric secret to use for signing and encrypting.</param>
		/// <param name="signed">A value indicating whether the data in this instance will be protected against tampering.</param>
		/// <param name="encrypted">A value indicating whether the data in this instance will be protected against eavesdropping.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <see cref="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		protected internal UriStyleMessageFormatter(byte[] symmetricSecret = null, bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: base(symmetricSecret, signed, encrypted, compressed, maximumAge, decodeOnceOnly) {
			Contract.Requires<ArgumentException>(symmetricSecret != null || (!signed && !encrypted), "A secret is required when signing or encrypting is required.");
		}

		protected override byte[] SerializeCore(T message) {
			var fields = MessageSerializer.Get(message.GetType()).Serialize(MessageDescriptions.GetAccessor(message));
			string value = MessagingUtilities.CreateQueryString(fields);
			return Encoding.UTF8.GetBytes(value);
		}

		protected override void DeserializeCore(T message, byte[] data) {
			string value = Encoding.UTF8.GetString(data);

			// Deserialize into message newly created instance.
			var serializer = MessageSerializer.Get(message.GetType());
			var fields = MessageDescriptions.GetAccessor(message);
			serializer.Deserialize(HttpUtility.ParseQueryString(value).ToDictionary(), fields);
		}
	}
}
