namespace DotNetOpenAuth.OAuth2.Crypto {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class JweHeader : JwtHeader {
		private JweHeader() {
		}

		internal JweHeader(string algorithm, string encryptionMethod) {
			Requires.NotNullOrEmpty(algorithm, "algorithm");
			Requires.NotNullOrEmpty(encryptionMethod, "encryptionMethod");
			this.Algorithm = algorithm;
			this.EncryptionMethod = encryptionMethod;
		}

		/// <summary>
		/// Gets or sets a value that identifies the cryptographic algorithm used to secure the JWS.
		/// A list of defined alg values is presented in Section 3, Table 1 of the JSON Web Algorithms (JWA) [JWA]
		/// specification. The processing of the alg header parameter requires that the value MUST be one that is
		/// both supported and for which there exists a key for use with that algorithm associated with the party
		/// that digitally signed or HMACed the content. The alg parameter value is case sensitive.
		/// This header parameter is REQUIRED.
		/// </summary>
		[MessagePart("alg", IsRequired = true, AllowEmpty = false)]
		internal string Algorithm { get; set; }

		/// <summary>
		/// Gets or sets a value that identifies the symmetric encryption algorithm used to secure the Ciphertext.
		/// A list of defined enc values is presented in Section 4, Table 3 of the JSON Web Algorithms (JWA) [JWA]
		/// specification. The processing of the enc (encryption method) header parameter requires that the value
		/// MUST be one that is supported. The enc value is case sensitive. This header parameter is REQUIRED.
		/// </summary>
		[MessagePart("enc", IsRequired = true, AllowEmpty = false)]
		internal string EncryptionMethod { get; set; }

		/// <summary>
		/// Gets or sets a value that identifies the cryptographic algorithm used to safeguard the integrity of the
		/// Ciphertext and the parameters used to create it. The int parameter uses the same values as the JWS alg
		/// parameter; a list of defined JWS alg values is presented in Section 3, Table 1 of the JSON Web Algorithms
		/// (JWA) [JWA] specification. This header parameter is REQUIRED when an AEAD algorithm is not used to encrypt
		/// the Plaintext and MUST NOT be present when an AEAD algorithm is used.
		/// </summary>
		[MessagePart("int")]
		internal string IntegrityAlgorithm { get; set; }

		/// <summary>
		/// Gets or sets a hint indicating which specific key owned by the signer should be used to validate the digital signature.
		/// This allows signers to explicitly signal a change of key to recipients. The interpretation of the contents of the kid
		/// parameter is unspecified. This header parameter is OPTIONAL.
		/// </summary>
		[MessagePart("kid")]
		internal string KeyIdentity { get; set; }

		/// <summary>
		/// Gets or sets the initialization Vector (iv) value for algorithms requiring it, represented as a base64url encoded string.
		/// This header parameter is OPTIONAL.
		/// </summary>
		[MessagePart("iv", Encoder = typeof(Base64WebEncoder))]
		internal byte[] IV { get; set; }

		/// <summary>
		/// Gets or sets the compression algorithm (zip) applied to the Plaintext before encryption, if any.
		/// This specification defines the value GZIP to refer to the encoding format produced by the file
		/// compression program "gzip" (GNU zip) as described in [RFC1952]; this format is a Lempel-Ziv coding
		/// (LZ77) with a 32 bit CRC. If no zip parameter is present, or its value is none, no compression is
		/// applied to the Plaintext before encryption. The zip value is case sensitive. This header parameter is OPTIONAL.
		/// </summary>
		[MessagePart("zip")]
		internal string CompressionAlgorithm { get; set; }
	}
}
