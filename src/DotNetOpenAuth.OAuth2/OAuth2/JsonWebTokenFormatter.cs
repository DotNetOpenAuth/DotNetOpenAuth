namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Json;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Crypto;

	internal class JsonWebTokenFormatter : DataBagFormatterBase<AccessToken> {
		private static readonly Encoding JwtCharacterEncoding = Encoding.UTF8;

		private static readonly MessageDescriptionCollection messageDescriptions = new MessageDescriptionCollection();

		private static readonly IMessageFactory jwtHeaderMessageFactory = ConstructJwtHeaderMessageFactory();

		private static IMessageFactory ConstructJwtHeaderMessageFactory() {
			var factory = new StandardMessageFactory();
			factory.AddMessageTypes(new MessageDescription[] {
				messageDescriptions.Get(typeof(JwsHeader), new Version(1, 0)),
				messageDescriptions.Get(typeof(JweHeader), new Version(1, 0)),
			});

			return factory;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonWebTokenFormatter"/> class.
		/// </summary>
		/// <param name="signingKey">The crypto service provider with the asymmetric key to use for signing or verifying the token.</param>
		/// <param name="encryptingKey">The crypto service provider with the asymmetric key to use for encrypting or decrypting the token.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <paramref name="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		protected internal JsonWebTokenFormatter(RSACryptoServiceProvider signingKey = null, RSACryptoServiceProvider encryptingKey = null, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: base(signingKey, encryptingKey, compressed, maximumAge, decodeOnceOnly) {
			this.UseOaepPadding = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonWebTokenFormatter"/> class.
		/// </summary>
		/// <param name="cryptoKeyStore">The crypto key store used when signing or encrypting.</param>
		/// <param name="bucket">The bucket in which symmetric keys are stored for signing/encrypting data.</param>
		/// <param name="signed">A value indicating whether the data in this instance will be protected against tampering.</param>
		/// <param name="encrypted">A value indicating whether the data in this instance will be protected against eavesdropping.</param>
		/// <param name="compressed">A value indicating whether the data in this instance will be GZip'd.</param>
		/// <param name="minimumAge">The minimum age.</param>
		/// <param name="maximumAge">The maximum age of a token that can be decoded; useful only when <paramref name="decodeOnceOnly"/> is <c>true</c>.</param>
		/// <param name="decodeOnceOnly">The nonce store to use to ensure that this instance is only decoded once.</param>
		protected internal JsonWebTokenFormatter(ICryptoKeyStore cryptoKeyStore = null, string bucket = null, bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? minimumAge = null, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: base(cryptoKeyStore, bucket, signed, encrypted, compressed, minimumAge, maximumAge, decodeOnceOnly) {
			Requires.True((cryptoKeyStore != null && !string.IsNullOrEmpty(bucket)) || (!signed && !encrypted), null);
			this.UseOaepPadding = true;
		}

		internal bool UseOaepPadding { get; set; }

		public override string Serialize(AccessToken message) {
			this.BeforeSerialize(message);

			var claimsSet = new JwtClaims() {
				IssuedAt = message.UtcIssued,
				Principal = message.User,
				Scope = message.Scope,
				Id = message.Nonce,
			};
			if (message.Lifetime.HasValue) {
				claimsSet.NotAfter = message.UtcIssued + message.Lifetime.Value;
			}

			byte[] encodedPayload = MessagingUtilities.SerializeAsJsonBytes(claimsSet, messageDescriptions);

			// First sign, then encrypt the payload, JWT style.
			string jwt = this.CreateJsonWebEncryptionToken(JwtCharacterEncoding.GetBytes(this.CreateJsonWebSignatureToken(encodedPayload)));
			return jwt;
		}

		protected override byte[] SerializeCore(AccessToken message) {
			throw new NotImplementedException();
		}

		public override void Deserialize(AccessToken message, IProtocolMessage containingMessage, string value, string messagePartName) {
			string[] segments = value.Split(new [] {'.'}, 4);
			ErrorUtilities.VerifyProtocol(segments.Length > 1, "Invalid JWT.  No periods found.");

			string encodedHeader = segments[0];
			byte[] decodedHeader = MessagingUtilities.FromBase64WebSafeString(encodedHeader);
			//jwtHeaderMessageFactory.GetNewRequestMessage(new MessageReceivingEndpoint(new Uri ("http://localhost/"), HttpDeliveryMethods.PostRequest), );
			var jwtHeader = new JwtHeader();
			MessagingUtilities.DeserializeFromJson(decodedHeader, jwtHeader, messageDescriptions, JwtCharacterEncoding);
			// TODO: instantiate the appropriate JwtHeader type.
			// TODO: Verify that ExtraData is empty.



			throw new NotImplementedException();

			this.AfterDeserialize(message, containingMessage);
		}

		protected override void DeserializeCore(AccessToken message, byte[] data) {
			throw new NotImplementedException();
		}

		private static string SerializeSegment(IMessage message) {
			return MessagingUtilities.ConvertToBase64WebSafeString(MessagingUtilities.SerializeAsJsonBytes(message, messageDescriptions, JwtCharacterEncoding));
		}

		private string CreateJsonWebSignatureToken(byte[] payload) {
			Requires.NotNull(payload, "payload");
			Requires.ValidState(this.SigningKey != null, "An RSA signing key must be set first.");

			string encodedPayload = MessagingUtilities.ConvertToBase64WebSafeString(payload);

			KeyValuePair<string, CryptoKey> handleAndKey = this.CryptoKeyStore.GetKeys(this.CryptoKeyBucket).First();
			using (var algorithm = JwtRsaShaSigningAlgorithm.Create(this.SigningKey, JwtRsaShaSigningAlgorithm.HashSize.Sha256)) {
				string encodedHeader = SerializeSegment(algorithm.Header);

				var builder = new StringBuilder(encodedHeader.Length + 1 + encodedPayload.Length);
				builder.Append(encodedHeader);
				builder.Append(".");
				builder.Append(encodedPayload);
				string securedInput = builder.ToString();

				string encodedSignature = MessagingUtilities.ConvertToBase64WebSafeString(algorithm.Sign(JwtCharacterEncoding.GetBytes(securedInput)));
				builder.Append(".");
				builder.Append(encodedSignature);

				return builder.ToString();
			}
		}

		private string CreateJsonWebEncryptionToken(byte[] payload) {
			Requires.NotNull(payload, "payload");
			ErrorUtilities.VerifyInternal(this.Encrypted, "We shouldn't generate a JWE if we're not encrypting!");
			ErrorUtilities.VerifySupported(this.EncryptingKey != null, "Only asymmetric encryption is supported.");

			string encodedPayload = MessagingUtilities.ConvertToBase64WebSafeString(payload);

			var header = new JweHeader(this.UseOaepPadding ? JsonWebEncryptionAlgorithms.RSA_OAEP : JsonWebEncryptionAlgorithms.RSA1_5, JsonWebEncryptionMethods.A256CBC);

			var symmetricAlgorithm = SymmetricAlgorithm.Create("AES");
			symmetricAlgorithm.KeySize = 256;
			symmetricAlgorithm.Mode = CipherMode.CBC;
			header.IV = symmetricAlgorithm.IV;

			byte[] contentMasterKey = symmetricAlgorithm.Key;
			byte[] encryptedKey = this.EncryptingKey.Encrypt(contentMasterKey, this.UseOaepPadding);
			string encodedEncryptedKey = MessagingUtilities.ConvertToBase64WebSafeString(encryptedKey);

			byte[] plaintext = payload;
			if (this.Compressed) {
				header.CompressionAlgorithm = "GZIP";
				plaintext = MessagingUtilities.Compress(payload, MessagingUtilities.CompressionMethod.Gzip);
			}

			var ciphertextStream = new MemoryStream();
			using (var encryptor = symmetricAlgorithm.CreateEncryptor()) {
				using (var cryptoStream = new CryptoStream(ciphertextStream, encryptor, CryptoStreamMode.Write)) {
					cryptoStream.Write(plaintext, 0, plaintext.Length);
					cryptoStream.Flush();
				}
			}

			string encodedCiphertext = MessagingUtilities.ConvertToBase64WebSafeString(ciphertextStream.ToArray());
			string encodedHeader = SerializeSegment(header);

			var builder = new StringBuilder(encodedHeader.Length + 1 + encodedPayload.Length);
			builder.Append(encodedHeader);
			builder.Append(".");
			builder.Append(encodedEncryptedKey);
			builder.Append(".");
			builder.Append(encodedCiphertext);
			builder.Append(".");
			builder.Append(String.Empty); // the Encoded JWE Integrity Value is always empty because we use an AEAD encryption algorithm.
			string securedInput = builder.ToString();

			return builder.ToString();
		}

		private class JwtClaims : JwtMessageBase {
			[MessagePart("exp", Encoder = typeof(TimestampEncoder))]
			internal DateTime NotAfter { get; set; }

			[MessagePart("nbf", Encoder = typeof(TimestampEncoder))]
			internal DateTime NotBefore { get; set; }

			[MessagePart("iat", Encoder = typeof(TimestampEncoder))]
			internal DateTime IssuedAt { get; set; }

			[MessagePart("iss")]
			internal string Issuer { get; set; }

			[MessagePart("aud")]
			internal string Audience { get; set; }

			[MessagePart("prn")]
			internal string Principal { get; set; }

			[MessagePart("jti")]
			internal byte[] Id { get; set; }

			[MessagePart("typ")]
			internal string Type { get; set; }

			[MessagePart("scope", Encoder = typeof(ScopeEncoder))]
			internal HashSet<string> Scope { get; set; }
		}
	}
}
