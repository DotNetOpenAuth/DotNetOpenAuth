//-----------------------------------------------------------------------
// <copyright file="BinaryDataBagFormatter.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging.Bindings;
	using Validation;

	/// <summary>
	/// A compact binary <see cref="DataBag"/> serialization class.
	/// </summary>
	/// <typeparam name="T">The <see cref="DataBag"/>-derived type to serialize/deserialize.</typeparam>
	internal class BinaryDataBagFormatter<T> : DataBagFormatterBase<T> where T : DataBag, IStreamSerializingDataBag, new() {
		/// <summary>
		/// Initializes a new instance of the <see cref="BinaryDataBagFormatter&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="signingKey">The crypto service provider with the asymmetric key to use for signing or verifying the token.</param>
		/// <param name="encryptingKey">The crypto service provider with the asymmetric key to use for encrypting or decrypting the token.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <paramref name="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		protected internal BinaryDataBagFormatter(RSACryptoServiceProvider signingKey = null, RSACryptoServiceProvider encryptingKey = null, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: base(signingKey, encryptingKey, compressed, maximumAge, decodeOnceOnly) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BinaryDataBagFormatter&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="cryptoKeyStore">The crypto key store used when signing or encrypting.</param>
		/// <param name="bucket">The bucket in which symmetric keys are stored for signing/encrypting data.</param>
		/// <param name="signed">A value indicating whether the data in this instance will be protected against tampering.</param>
		/// <param name="encrypted">A value indicating whether the data in this instance will be protected against eavesdropping.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="minimumAge">The minimum age.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <paramref name="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		protected internal BinaryDataBagFormatter(ICryptoKeyStore cryptoKeyStore = null, string bucket = null, bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? minimumAge = null, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: base(cryptoKeyStore, bucket, signed, encrypted, compressed, minimumAge, maximumAge, decodeOnceOnly) {
			Requires.That((cryptoKeyStore != null && bucket != null) || (!signed && !encrypted), null, "Signing or encryption requires a crypto key store and bucket.");
		}

		/// <summary>
		/// Serializes the <see cref="DataBag"/> instance to a buffer.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>The buffer containing the serialized data.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		protected override byte[] SerializeCore(T message) {
			using (var stream = new MemoryStream()) {
				message.Serialize(stream);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Deserializes the <see cref="DataBag"/> instance from a buffer.
		/// </summary>
		/// <param name="message">The message instance to initialize with data from the buffer.</param>
		/// <param name="data">The data buffer.</param>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		protected override void DeserializeCore(T message, byte[] data) {
			using (var stream = new MemoryStream(data)) {
				message.Deserialize(stream);
			}

			// Perform basic validation on message that the MessageSerializer would have normally performed.
			var messageDescription = MessageDescriptions.Get(message);
			var dictionary = messageDescription.GetDictionary(message);
			messageDescription.EnsureMessagePartsPassBasicValidation(dictionary);
			IMessage m = message;
			m.EnsureValidMessage();
		}
	}
}
