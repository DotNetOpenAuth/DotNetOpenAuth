//-----------------------------------------------------------------------
// <copyright file="UriStyleMessageFormatter.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
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
	internal class UriStyleMessageFormatter<T> : IDataBagFormatter<T> where T : DataBag, new() {
		/// <summary>
		/// The length of the nonce to include in tokens that can be decoded once only.
		/// </summary>
		private const int NonceLength = 6;

		/// <summary>
		/// The message description cache to use for data bag types.
		/// </summary>
		private static readonly MessageDescriptionCollection MessageDescriptions = new MessageDescriptionCollection();

		/// <summary>
		/// The symmetric secret used for signing/encryption of verification codes and refresh tokens.
		/// </summary>
		private readonly byte[] symmetricSecret;

		/// <summary>
		/// The hashing algorithm to use while signing when using a symmetric secret.
		/// </summary>
		private readonly HashAlgorithm symmetricHasher;

		/// <summary>
		/// The crypto to use for signing access tokens.
		/// </summary>
		private readonly RSACryptoServiceProvider asymmetricSigning;

		/// <summary>
		/// The crypto to use for encrypting access tokens.
		/// </summary>
		private readonly RSACryptoServiceProvider asymmetricEncrypting;

		/// <summary>
		/// The hashing algorithm to use for asymmetric signatures.
		/// </summary>
		private readonly HashAlgorithm hasherForAsymmetricSigning;

		/// <summary>
		/// A value indicating whether the data in this instance will be protected against tampering.
		/// </summary>
		private readonly bool signed;

		/// <summary>
		/// The nonce store to use to ensure that this instance is only decoded once.
		/// </summary>
		private readonly INonceStore decodeOnceOnly;

		/// <summary>
		/// The maximum age of a token that can be decoded; useful only when <see cref="decodeOnceOnly"/> is <c>true</c>.
		/// </summary>
		private readonly TimeSpan? maximumAge;

		/// <summary>
		/// A value indicating whether the data in this instance will be protected against eavesdropping.
		/// </summary>
		private readonly bool encrypted;

		/// <summary>
		/// A value indicating whether the data in this instance will be GZip'd.
		/// </summary>
		private readonly bool compressed;

		/// <summary>
		/// Initializes a new instance of the <see cref="UriStyleMessageFormatter&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="signed">A value indicating whether the data in this instance will be protected against tampering.</param>
		/// <param name="encrypted">A value indicating whether the data in this instance will be protected against eavesdropping.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <see cref="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		internal UriStyleMessageFormatter(bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null) {
			Contract.Requires<ArgumentException>(signed || decodeOnceOnly == null, "A signature must be applied if this data is meant to be decoded only once.");
			Contract.Requires<ArgumentException>(maximumAge.HasValue || decodeOnceOnly == null, "A maximum age must be given if a message can only be decoded once.");

			this.signed = signed;
			this.maximumAge = maximumAge;
			this.decodeOnceOnly = decodeOnceOnly;
			this.encrypted = encrypted;
			this.compressed = compressed;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UriStyleMessageFormatter&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="signingKey">The asymmetric private key to use for signing the token.</param>
		/// <param name="encryptingKey">The asymmetric public key to use for encrypting the token.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <see cref="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		internal UriStyleMessageFormatter(RSAParameters? signingKey = null, RSAParameters? encryptingKey = null, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: this(signingKey.HasValue, encryptingKey.HasValue, compressed, maximumAge, decodeOnceOnly) {
			if (signingKey.HasValue) {
				this.asymmetricSigning = new RSACryptoServiceProvider();
				this.asymmetricSigning.ImportParameters(signingKey.Value);
			}

			if (encryptingKey.HasValue) {
				this.asymmetricEncrypting = new RSACryptoServiceProvider();
				this.asymmetricEncrypting.ImportParameters(encryptingKey.Value);
			}

			this.hasherForAsymmetricSigning = new SHA1CryptoServiceProvider();
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
		internal UriStyleMessageFormatter(byte[] symmetricSecret = null, bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: this(signed, encrypted, compressed, maximumAge, decodeOnceOnly) {
			Contract.Requires<ArgumentException>(symmetricSecret != null || (!signed && !encrypted), "A secret is required when signing or encrypting is required.");

			if (symmetricSecret != null) {
				this.symmetricHasher = new HMACSHA256(symmetricSecret);
			}

			this.symmetricSecret = symmetricSecret;
		}

		public string Serialize(T message) {
			message.UtcCreationDate = DateTime.UtcNow;

			if (this.decodeOnceOnly != null) {
				message.Nonce = MessagingUtilities.GetNonCryptoRandomData(NonceLength);
			}

			if (this.signed) {
				message.Signature = this.CalculateSignature(message);
			}

			var fields = MessageSerializer.Get(message.GetType()).Serialize(MessageDescriptions.GetAccessor(message));
			string value = MessagingUtilities.CreateQueryString(fields);

			byte[] encoded = Encoding.UTF8.GetBytes(value);

			if (this.compressed) {
				encoded = MessagingUtilities.Compress(encoded);
			}

			if (this.encrypted) {
				encoded = this.Encrypt(encoded);
			}

			return Convert.ToBase64String(encoded);
		}

		public T Deserialize(IProtocolMessage containingMessage, string value) {
			var message = new T { ContainingMessage = containingMessage };
			byte[] data = Convert.FromBase64String(value);

			if (this.encrypted) {
				data = this.Decrypt(data);
			}

			if (this.compressed) {
				data = MessagingUtilities.Decompress(data);
			}

			value = Encoding.UTF8.GetString(data);

			// Deserialize into message newly created instance.
			var serializer = MessageSerializer.Get(message.GetType());
			var fields = MessageDescriptions.GetAccessor(message);
			serializer.Deserialize(HttpUtility.ParseQueryString(value).ToDictionary(), fields);

			if (this.signed) {
				// Verify that the verification code was issued by message authorization server.
				ErrorUtilities.VerifyProtocol(this.IsSignatureValid(message), Protocol.bad_verification_code);
			}

			if (this.maximumAge.HasValue) {
				// Has message verification code expired?
				DateTime expirationDate = message.UtcCreationDate + this.maximumAge.Value;
				if (expirationDate < DateTime.UtcNow) {
					throw new ExpiredMessageException(expirationDate, containingMessage);
				}
			}

			// Has message verification code already been used to obtain an access/refresh token?
			if (this.decodeOnceOnly != null) {
				ErrorUtilities.VerifyInternal(this.maximumAge.HasValue, "Oops!  How can we validate a nonce without a maximum message age?");
				string context = "{" + GetType().FullName + "}";
				if (!this.decodeOnceOnly.StoreNonce(context, Convert.ToBase64String(message.Nonce), message.UtcCreationDate)) {
					Logger.OpenId.ErrorFormat("Replayed nonce detected ({0} {1}).  Rejecting message.", message.Nonce, message.UtcCreationDate);
					throw new ReplayedMessageException(containingMessage);
				}
			}

			((IMessage)message).EnsureValidMessage();

			return message;
		}

		/// <summary>
		/// Determines whether the signature on this instance is valid.
		/// </summary>
		/// <param name="message">The message whose signature is to be checked.</param>
		/// <returns>
		/// 	<c>true</c> if the signature is valid; otherwise, <c>false</c>.
		/// </returns>
		private bool IsSignatureValid(DataBag message) {
			Contract.Requires<ArgumentNullException>(message != null, "message");

			if (this.asymmetricSigning != null) {
				byte[] bytesToSign = this.GetBytesToSign(message);
				return this.asymmetricSigning.VerifyData(bytesToSign, this.hasherForAsymmetricSigning, message.Signature);
			} else {
				return MessagingUtilities.AreEquivalentConstantTime(message.Signature, this.CalculateSignature(message));
			}
		}

		/// <summary>
		/// Calculates the signature for the data in this verification code.
		/// </summary>
		/// <param name="message">The message whose signature is to be calculated.</param>
		/// <returns>The calculated signature.</returns>
		private byte[] CalculateSignature(DataBag message) {
			Contract.Requires<ArgumentNullException>(message != null, "message");
			Contract.Requires<InvalidOperationException>(this.asymmetricSigning != null || this.symmetricHasher != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			byte[] bytesToSign = this.GetBytesToSign(message);
			if (this.asymmetricSigning != null) {
				return this.asymmetricSigning.SignData(bytesToSign, this.hasherForAsymmetricSigning);
			} else {
				return this.symmetricHasher.ComputeHash(bytesToSign);
			}
		}

		/// <summary>
		/// Gets the bytes to sign.
		/// </summary>
		/// <param name="message">The message to be encoded as normalized bytes for signing.</param>
		/// <returns>A buffer of the bytes to sign.</returns>
		private byte[] GetBytesToSign(DataBag message) {
			Contract.Requires<ArgumentNullException>(message != null, "message");

			// Sign the data, being sure to avoid any impact of the signature field itself.
			var fields = MessageDescriptions.GetAccessor(message);
			var fieldsCopy = fields.ToDictionary();
			fieldsCopy.Remove("sig");

			var sortedData = new SortedDictionary<string, string>(fieldsCopy, StringComparer.OrdinalIgnoreCase);
			string value = MessagingUtilities.CreateQueryString(sortedData);
			byte[] bytesToSign = Encoding.UTF8.GetBytes(value);
			return bytesToSign;
		}

		/// <summary>
		/// Encrypts the specified value using either the symmetric or asymmetric encryption algorithm as appropriate.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The encrypted value.</returns>
		private byte[] Encrypt(byte[] value) {
			Contract.Requires<InvalidOperationException>(this.asymmetricEncrypting != null || this.symmetricSecret != null);

			if (this.asymmetricEncrypting != null) {
				return this.asymmetricEncrypting.EncryptWithRandomSymmetricKey(value);
			} else {
				return MessagingUtilities.Encrypt(value, this.symmetricSecret);
			}
		}

		/// <summary>
		/// Decrypts the specified value using either the symmetric or asymmetric encryption algorithm as appropriate.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The decrypted value.</returns>
		private byte[] Decrypt(byte[] value) {
			Contract.Requires<InvalidOperationException>(this.asymmetricEncrypting != null || this.symmetricSecret != null);

			if (this.asymmetricEncrypting != null) {
				return this.asymmetricEncrypting.DecryptWithRandomSymmetricKey(value);
			} else {
				return MessagingUtilities.Decrypt(value, this.symmetricSecret);
			}
		}
	}
}
