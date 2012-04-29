namespace DotNetOpenAuth.OAuth2.Crypto {
	internal static class JsonWebSignatureAlgorithms {
		/// <summary>
		/// HMAC using SHA-256 hash algorithm.
		/// </summary>
		internal const string HmacSha256 = "HS256";

		/// <summary>
		/// HMAC using SHA-384 hash algorithm.
		/// </summary>
		internal const string HmacSha384 = "HS384";

		/// <summary>
		/// HMAC using SHA-512 hash algorithm.
		/// </summary>
		internal const string HmacSha512 = "HS512";

		/// <summary>
		/// RSA using SHA-256 hash algorithm.
		/// </summary>
		internal const string RsaSha256 = "RS256";

		/// <summary>
		/// RSA using SHA-384 hash algorithm.
		/// </summary>
		internal const string RsaSha384 = "RS384";

		/// <summary>
		/// RSA using SHA-512 hash algorithm.
		/// </summary>
		internal const string RsaSha512 = "RS512";

		/// <summary>
		/// ECDSA using P-256 curve and SHA-256 hash algorithm.
		/// </summary>
		internal const string ECDsaSha256 = "ES256";

		/// <summary>
		/// ECDSA using P-384 curve and SHA-384 hash algorithm.
		/// </summary>
		internal const string ECDsaSha384 = "ES384";

		/// <summary>
		/// ECDSA using P-521 curve and SHA-512 hash algorithm.
		/// </summary>
		internal const string ECDsaSha512 = "ES512";

		/// <summary>
		/// No digital signature or HMAC value included.
		/// </summary>
		internal const string None = "none";
	}

	/// <summary>
	/// The set of alg (algorithm) header parameter values that are defined by this
	/// specification for use with JWE. These algorithms are used to encrypt the CEK,
	/// which produces the JWE Encrypted Key.
	/// </summary>
	/// <remarks>
	/// http://self-issued.info/docs/draft-ietf-jose-json-web-algorithms-01.html#EncAlgTable
	/// </remarks>
	internal static class JsonWebEncryptionAlgorithms {
		/// <summary>
		/// RSA using RSA-PKCS1-1.5 padding, as defined in RFC 3447 [RFC3447]
		/// </summary>
		internal const string RSA1_5 = "RSA1_5";

		/// <summary>
		/// RSA using Optimal Asymmetric Encryption Padding (OAEP), as defined in RFC 3447 [RFC3447]
		/// </summary>
		internal const string RSA_OAEP = "RSA-OAEP";

		/// <summary>
		/// Elliptic Curve Diffie-Hellman Ephemeral Static, as defined in RFC 6090
		/// [RFC6090], and using the Concat KDF, as defined in [NIST‑800‑56A],
		/// where the Digest Method is SHA-256 and all OtherInfo parameters are
		/// the empty bit string
		/// </summary>
		internal const string ECDH_ES = "ECDH-ES";

		/// <summary>
		/// Advanced Encryption Standard (AES) Key Wrap Algorithm using 128 bit keys,
		/// as defined in RFC 3394 [RFC3394]
		/// </summary>
		internal const string A128KW = "A128KW";

		/// <summary>
		/// Advanced Encryption Standard (AES) Key Wrap Algorithm using 256 bit keys,
		/// as defined in RFC 3394 [RFC3394]
		/// </summary>
		internal const string A256KW = "A256KW";

		/// <summary>
		/// Advanced Encryption Standard (AES) Key Wrap Algorithm using 512 bit keys,
		/// as defined in RFC 3394 [RFC3394]
		/// </summary>
		internal const string A512KW = "A512KW";

		/// <summary>
		/// Advanced Encryption Standard (AES) using 128 bit keys in Galois/Counter
		/// Mode, as defined in [FIPS‑197] and [NIST‑800‑38D]
		/// </summary>
		internal const string A128GCM = "A128GCM";

		/// <summary>
		/// Advanced Encryption Standard (AES) using 256 bit keys in Galois/Counter
		/// Mode, as defined in [FIPS‑197] and [NIST‑800‑38D]
		/// </summary>
		internal const string A256GCM = "A256GCM";
	}

	/// <summary>
	/// The set of enc (encryption method) header parameter values that are defined
	/// by this specification for use with JWE. These algorithms are used to encrypt
	/// the Plaintext, which produces the Ciphertext.
	/// </summary>
	/// <remarks>
	/// http://self-issued.info/docs/draft-ietf-jose-json-web-algorithms-01.html#EncTable
	/// </remarks>
	internal static class JsonWebEncryptionMethods {
		/// <summary>
		/// Advanced Encryption Standard (AES) using 128 bit keys in Cipher Block Chaining mode, as defined in [FIPS‑197] and [NIST‑800‑38A]
		/// </summary>
		internal const string A128CBC = "A128CBC";

		/// <summary>
		/// Advanced Encryption Standard (AES) using 256 bit keys in Cipher Block Chaining mode, as defined in [FIPS‑197] and [NIST‑800‑38A]
		/// </summary>
		internal const string A256CBC = "A256CBC";

		/// <summary>
		/// Advanced Encryption Standard (AES) using 128 bit keys in Galois/Counter Mode, as defined in [FIPS‑197] and [NIST‑800‑38D]
		/// </summary>
		internal const string A128GCM = "A128GCM";

		/// <summary>
		/// Advanced Encryption Standard (AES) using 256 bit keys in Galois/Counter Mode, as defined in [FIPS‑197] and [NIST‑800‑38D]
		/// </summary>
		internal const string A256GCM = "A256GCM";
	}
}
