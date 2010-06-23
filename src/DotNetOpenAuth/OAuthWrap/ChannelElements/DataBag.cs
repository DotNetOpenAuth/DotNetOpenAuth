//-----------------------------------------------------------------------
// <copyright file="DataBag.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuthWrap.Messages;

	/// <summary>
	/// A collection of message parts that will be serialized into a single string,
	/// to be set into a larger message.
	/// </summary>
	[Serializable]
	internal abstract class DataBag : MessageBase {
		/// <summary>
		/// The message description cache to use for data bag types.
		/// </summary>
		private static readonly MessageDescriptionCollection MessageDescriptions = new MessageDescriptionCollection();

		/// <summary>
		/// The length of the nonce to include in tokens that can be decoded once only.
		/// </summary>
		private const int NonceLength = 6;

		/// <summary>
		/// The symmetric secret used for signing/encryption of verification codes and refresh tokens.
		/// </summary>
		[NonSerialized]
		private readonly byte[] symmetricSecret;

		/// <summary>
		/// The hashing algorithm to use while signing when using a symmetric secret.
		/// </summary>
		[NonSerialized]
		private readonly HashAlgorithm symmetricHasher;

		/// <summary>
		/// The crypto to use for signing access tokens.
		/// </summary>
		[NonSerialized]
		private readonly RSACryptoServiceProvider asymmetricSigning;

		/// <summary>
		/// The crypto to use for encrypting access tokens.
		/// </summary>
		[NonSerialized]
		private readonly RSACryptoServiceProvider asymmetricEncrypting;

		/// <summary>
		/// The hashing algorithm to use for asymmetric signatures.
		/// </summary>
		[NonSerialized]
		private readonly HashAlgorithm hasherForAsymmetricSigning;

		/// <summary>
		/// A value indicating whether the data in this instance will be protected against tampering.
		/// </summary>
		[NonSerialized]
		private readonly bool signed;

		/// <summary>
		/// The nonce store to use to ensure that this instance is only decoded once.
		/// </summary>
		[NonSerialized]
		private readonly INonceStore decodeOnceOnly;

		/// <summary>
		/// The maximum age of a token that can be decoded; useful only when <see cref="decodeOnceOnly"/> is <c>true</c>.
		/// </summary>
		[NonSerialized]
		private readonly TimeSpan? maximumAge;

		/// <summary>
		/// A value indicating whether the data in this instance will be protected against eavesdropping.
		/// </summary>
		[NonSerialized]
		private readonly bool encrypted;

		/// <summary>
		/// A value indicating whether the data in this instance will be GZip'd.
		/// </summary>
		[NonSerialized]
		private readonly bool compressed;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataBag"/> class.
		/// </summary>
		/// <param name="signed">A value indicating whether the data in this instance will be protected against tampering.</param>
		/// <param name="encrypted">A value indicating whether the data in this instance will be protected against eavesdropping.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <see cref="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		protected DataBag(bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: base(Protocol.Default.Version) {
			Contract.Requires<ArgumentException>(signed || decodeOnceOnly == null, "A signature must be applied if this data is meant to be decoded only once.");
			Contract.Requires<ArgumentException>(maximumAge.HasValue || decodeOnceOnly == null, "A maximum age must be given if a message can only be decoded once.");

			this.signed = signed;
			this.maximumAge = maximumAge;
			this.decodeOnceOnly = decodeOnceOnly;
			this.encrypted = encrypted;
			this.compressed = compressed;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataBag"/> class.
		/// </summary>
		/// <param name="signingKey">The asymmetric private key to use for signing the token.</param>
		/// <param name="encryptingKey">The asymmetric public key to use for encrypting the token.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <see cref="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		protected DataBag(RSAParameters? signingKey = null, RSAParameters? encryptingKey = null, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
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
		/// Initializes a new instance of the <see cref="DataBag"/> class.
		/// </summary>
		/// <param name="symmetricSecret">The symmetric secret to use for signing and encrypting.</param>
		/// <param name="signed">A value indicating whether the data in this instance will be protected against tampering.</param>
		/// <param name="encrypted">A value indicating whether the data in this instance will be protected against eavesdropping.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <see cref="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		protected DataBag(byte[] symmetricSecret = null, bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: this(signed, encrypted, compressed, maximumAge, decodeOnceOnly) {
			Contract.Requires<ArgumentException>(symmetricSecret != null || (!signed && !encrypted), "A secret is required when signing or encrypting is required.");

			if (symmetricSecret != null) {
				this.symmetricHasher = new HMACSHA256(symmetricSecret);
			}

			this.symmetricSecret = symmetricSecret;
		}

		/// <summary>
		/// Gets or sets the nonce.
		/// </summary>
		/// <value>The nonce.</value>
		[MessagePart]
		internal byte[] Nonce { get; set; }

		/// <summary>
		/// Gets or sets the UTC creation date of this token.
		/// </summary>
		/// <value>The UTC creation date.</value>
		[MessagePart("timestamp", IsRequired = true, Encoder = typeof(TimestampEncoder))]
		internal DateTime UtcCreationDate { get; set; }

		/// <summary>
		/// Gets the type of this instance.
		/// </summary>
		/// <value>The type of the bag.</value>
		/// <remarks>
		/// This ensures that one token cannot be misused as another kind of token.
		/// </remarks>
		[MessagePart("t", IsRequired = true, AllowEmpty = false)]
		private string BagType {
			get { return this.GetType().Name; }
		}

		/// <summary>
		/// Gets or sets the signature.
		/// </summary>
		/// <value>The signature.</value>
		[MessagePart("sig")]
		private byte[] Signature { get; set; }

		/// <summary>
		/// Serializes this instance as a string that can be sent as part of a larger message.
		/// </summary>
		/// <returns>The serialized version of the data in this instance.</returns>
		internal virtual string Encode() {
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

			this.UtcCreationDate = DateTime.UtcNow;

			if (this.decodeOnceOnly != null) {
				this.Nonce = MessagingUtilities.GetNonCryptoRandomData(NonceLength);
			}

			if (this.signed) {
				this.Signature = this.CalculateSignature();
			}

			var memoryStream = new MemoryStream();
			var formatter = new BinaryFormatter();
			formatter.Serialize(memoryStream, this);
			byte[] encoded = memoryStream.ToArray();

			if (this.compressed) {
				encoded = MessagingUtilities.Compress(encoded);
			}

			if (this.encrypted) {
				encoded = this.Encrypt(encoded);
			}

			return Convert.ToBase64String(encoded);
		}

		/// <summary>
		/// Populates this instance with data from a given string.
		/// </summary>
		/// <param name="value">The value to deserialize from.</param>
		/// <param name="containingMessage">The message that contained this token.</param>
		protected virtual void Decode(string value, IProtocolMessage containingMessage) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(value));
			Contract.Requires<ArgumentNullException>(containingMessage != null, "containingMessage");

			byte[] encoded = Convert.FromBase64String(value);

			if (this.encrypted) {
				encoded = this.Decrypt(encoded);
			}

			if (this.compressed) {
				encoded = MessagingUtilities.Decompress(encoded);
			}

			var dataStream = new MemoryStream(encoded);

			// Deserialize into this newly created instance.
			var formatter = new BinaryFormatter();
			var bag = (DataBag) formatter.Deserialize(dataStream);
			// TODO: deserialize into THIS instance

			if (this.signed) {
				// Verify that the verification code was issued by this authorization server.
				ErrorUtilities.VerifyProtocol(this.IsSignatureValid(), Protocol.bad_verification_code);
			}

			if (this.maximumAge.HasValue) {
				// Has this verification code expired?
				DateTime expirationDate = this.UtcCreationDate + this.maximumAge.Value;
				if (expirationDate < DateTime.UtcNow) {
					throw new ExpiredMessageException(expirationDate, containingMessage);
				}
			}

			// Has this verification code already been used to obtain an access/refresh token?
			if (this.decodeOnceOnly != null) {
				ErrorUtilities.VerifyInternal(this.maximumAge.HasValue, "Oops!  How can we validate a nonce without a maximum message age?");
				string context = "{" + GetType().FullName + "}";
				if (!this.decodeOnceOnly.StoreNonce(context, Convert.ToBase64String(this.Nonce), this.UtcCreationDate)) {
					Logger.OpenId.ErrorFormat("Replayed nonce detected ({0} {1}).  Rejecting message.", this.Nonce, this.UtcCreationDate);
					throw new ReplayedMessageException(containingMessage);
				}
			}
		}

		/// <summary>
		/// Determines whether the signature on this instance is valid.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if the signature is valid; otherwise, <c>false</c>.
		/// </returns>
		private bool IsSignatureValid() {
			if (this.asymmetricSigning != null) {
				byte[] bytesToSign = this.GetBytesToSign();
				return this.asymmetricSigning.VerifyData(bytesToSign, this.hasherForAsymmetricSigning, this.Signature);
			} else {
				return MessagingUtilities.AreEquivalent(this.Signature, this.CalculateSignature());
			}
		}

		/// <summary>
		/// Calculates the signature for the data in this verification code.
		/// </summary>
		/// <returns>The calculated signature.</returns>
		private byte[] CalculateSignature() {
			Contract.Requires<InvalidOperationException>(this.asymmetricSigning != null || this.symmetricHasher != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			byte[] bytesToSign = this.GetBytesToSign();
			if (this.asymmetricSigning != null) {
				return this.asymmetricSigning.SignData(bytesToSign, this.hasherForAsymmetricSigning);
			} else {
				return this.symmetricHasher.ComputeHash(bytesToSign);
			}
		}

		/// <summary>
		/// Gets the bytes to sign.
		/// </summary>
		/// <returns>A buffer of the bytes to sign.</returns>
		private byte[] GetBytesToSign() {
			// Sign the data, being sure to avoid any impact of the signature field itself.
			var fields = MessageDescriptions.GetAccessor(this);
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
