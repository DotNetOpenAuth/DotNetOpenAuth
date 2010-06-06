//-----------------------------------------------------------------------
// <copyright file="DataBag.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuthWrap.Messages;

	internal abstract class DataBag : MessageBase {
		private static readonly MessageDescriptionCollection MessageDescriptions = new MessageDescriptionCollection();

		private const int NonceLength = 6;

		private readonly byte[] symmetricSecret;

		private readonly RSACryptoServiceProvider asymmetricSigning;

		private readonly RSACryptoServiceProvider asymmetricEncrypting;

		private readonly HashAlgorithm hasherForAsymmetricSigning;

		private readonly bool signed;

		protected HashAlgorithm hasher;

		private readonly INonceStore decodeOnceOnly;

		private readonly TimeSpan? maximumAge;

		private readonly bool encrypted;

		private readonly bool compressed;

		[MessagePart("t", IsRequired = true, AllowEmpty = false)]
		private string BagType {
			get { return this.GetType().Name; }
		}

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

		protected DataBag(byte[] symmetricSecret = null, bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: this(signed, encrypted, compressed, maximumAge, decodeOnceOnly) {
			Contract.Requires<ArgumentException>(symmetricSecret != null || (signed == null && encrypted == null), "A secret is required when signing or encrypting is required.");

			if (symmetricSecret != null) {
				this.hasher = new HMACSHA256(symmetricSecret);
			}

			this.symmetricSecret = symmetricSecret;
		}

		[MessagePart("sig")]
		private string Signature { get; set; }

		[MessagePart]
		internal string Nonce { get; set; }

		[MessagePart("timestamp", IsRequired = true, Encoder = typeof(TimestampEncoder))]
		internal DateTime UtcCreationDate { get; set; }

		internal virtual string Encode() {
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

			this.UtcCreationDate = DateTime.UtcNow;

			if (decodeOnceOnly != null) {
				this.Nonce = Convert.ToBase64String(MessagingUtilities.GetNonCryptoRandomData(NonceLength));
			}

			if (signed) {
				this.Signature = this.CalculateSignature();
			}

			var fields = MessageSerializer.Get(this.GetType()).Serialize(MessageDescriptions.GetAccessor(this));
			string value = MessagingUtilities.CreateQueryString(fields);

			byte[] encoded = Encoding.UTF8.GetBytes(value);

			if (compressed) {
				encoded = MessagingUtilities.Compress(encoded);
			}

			if (encrypted) {
				encoded = this.Encrypt(encoded);
			}

			return Convert.ToBase64String(encoded);
		}

		protected virtual void Decode(string value, IProtocolMessage containingMessage = null) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(value));

			byte[] encoded = Convert.FromBase64String(value);

			if (encrypted) {
				encoded = this.Decrypt(encoded);
			}

			if (compressed) {
				encoded = MessagingUtilities.Decompress(encoded);
			}

			value = Encoding.UTF8.GetString(encoded);

			// Deserialize into this newly created instance.
			var serializer = MessageSerializer.Get(this.GetType());
			var fields = MessageDescriptions.GetAccessor(this);
			serializer.Deserialize(HttpUtility.ParseQueryString(value).ToDictionary(), fields);

			if (signed) {
				// Verify that the verification code was issued by this authorization server.
				ErrorUtilities.VerifyProtocol(this.IsSignatureValid(), Protocol.bad_verification_code);
			}

			if (maximumAge.HasValue) {
				// Has this verification code expired?
				DateTime expirationDate = this.UtcCreationDate + this.maximumAge.Value;
				if (expirationDate < DateTime.UtcNow) {
					throw new ExpiredMessageException(expirationDate, containingMessage);
				}
			}

			// Has this verification code already been used to obtain an access/refresh token?
			if (decodeOnceOnly != null) {
				ErrorUtilities.VerifyInternal(this.maximumAge.HasValue, "Oops!  How can we validate a nonce without a maximum message age?");
				string context = "{" + GetType().FullName + "}";
				if (!this.decodeOnceOnly.StoreNonce(context, this.Nonce, this.UtcCreationDate)) {
					Logger.OpenId.ErrorFormat("Replayed nonce detected ({0} {1}).  Rejecting message.", this.Nonce, this.UtcCreationDate);
					throw new ReplayedMessageException(containingMessage);
				}
			}
		}

		private bool IsSignatureValid() {
			if (this.asymmetricSigning != null) {
				byte[] bytesToSign = this.GetBytesToSign();
				byte[] signature = Convert.FromBase64String(this.Signature);
				return this.asymmetricSigning.VerifyData(bytesToSign, this.hasherForAsymmetricSigning, signature);
			} else {
				return string.Equals(this.Signature, this.CalculateSignature(), StringComparison.Ordinal);
			}
		}

		/// <summary>
		/// Calculates the signature for the data in this verification code.
		/// </summary>
		/// <returns>The calculated signature.</returns>
		private string CalculateSignature() {
			Contract.Requires<InvalidOperationException>(this.asymmetricSigning != null || this.hasher != null);

			byte[] bytesToSign = this.GetBytesToSign();
			if (this.asymmetricSigning != null) {
				byte[] signature = this.asymmetricSigning.SignData(bytesToSign, this.hasherForAsymmetricSigning);
				return Convert.ToBase64String(signature);
			} else {
				return Convert.ToBase64String(this.hasher.ComputeHash(bytesToSign));
			}
		}

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

		private byte[] Encrypt(byte[] value) {
			Contract.Requires<InvalidOperationException>(this.asymmetricEncrypting != null || this.symmetricSecret != null);

			if (this.asymmetricEncrypting != null) {
				return this.asymmetricEncrypting.EncryptWithRandomSymmetricKey(value);
			} else {
				return MessagingUtilities.Encrypt(value, this.symmetricSecret);
			}
		}

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
