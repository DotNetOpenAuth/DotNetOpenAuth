//-----------------------------------------------------------------------
// <copyright file="BinaryDataBagFormatter.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging.Bindings;

	[ContractClass(typeof(IStreamSerializingMessageContract))]
	internal interface IStreamSerializingMessage {
		void Serialize(Stream stream);

		void Deserialize(Stream stream);
	}

	[ContractClassFor(typeof(IStreamSerializingMessage))]
	internal abstract class IStreamSerializingMessageContract : IStreamSerializingMessage {
		void IStreamSerializingMessage.Serialize(Stream stream) {
			Contract.Requires(stream != null);
			Contract.Requires(stream.CanWrite);
			throw new NotImplementedException();
		}

		void IStreamSerializingMessage.Deserialize(Stream stream) {
			Contract.Requires(stream != null);
			Contract.Requires(stream.CanRead);
			throw new NotImplementedException();
		}
	}


	internal class BinaryDataBagFormatter<T> : DataBagFormatterBase<T> where T : DataBag, IStreamSerializingMessage, new() {
		/// <summary>
		/// Initializes a new instance of the <see cref="UriStyleMessageFormatter&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="signingKey">The crypto service provider with the asymmetric key to use for signing or verifying the token.</param>
		/// <param name="encryptingKey">The crypto service provider with the asymmetric key to use for encrypting or decrypting the token.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <see cref="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		protected internal BinaryDataBagFormatter(RSACryptoServiceProvider signingKey = null, RSACryptoServiceProvider encryptingKey = null, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
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
		protected internal BinaryDataBagFormatter(byte[] symmetricSecret = null, bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: base(symmetricSecret, signed, encrypted, compressed, maximumAge, decodeOnceOnly) {
			Contract.Requires<ArgumentException>(symmetricSecret != null || (!signed && !encrypted), "A secret is required when signing or encrypting is required.");
		}

		protected override byte[] SerializeCore(T message) {
			var stream = new MemoryStream();
			message.Serialize(stream);
			return stream.ToArray();
		}

		protected override void DeserializeCore(T message, byte[] data) {
			var stream = new MemoryStream(data);
			message.Deserialize(stream);

			// Perform basic validation on message that the MessageSerializer would have normally performed.
			var messageDescription = MessageDescriptions.Get(message);
			var dictionary = messageDescription.GetDictionary(message);
			messageDescription.EnsureMessagePartsPassBasicValidation(dictionary);
			IMessage m = message;
			m.EnsureValidMessage();
		}
	}
}
