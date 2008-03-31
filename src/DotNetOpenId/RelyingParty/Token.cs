using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// A state-containing bit of non-confidential data that is sent to the 
	/// user agent as part of the return_to URL so we can read from it later.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
		"CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "CryptoStream is not stored in a field.")]
	class Token {
		public static readonly string TokenKey = "token";
		/// <summary>
		/// This nonce will only be used if the provider is pre-2.0.
		/// </summary>
		public Nonce Nonce { get; set; }
		public ServiceEndpoint Endpoint { get; private set; }

		public Token(ServiceEndpoint provider)
			: this(new Nonce(), provider) {
		}

		Token(Nonce nonce, ServiceEndpoint provider) {
			Nonce = nonce;
			Endpoint = provider;
		}

		/// <summary>
		/// Serializes this <see cref="Token"/> instance as a string that can be
		/// included as part of a return_to variable in a querystring. 
		/// This string is cryptographically signed to protect against tampering.
		/// </summary>
		public string Serialize(INonceStore store) {
			using (MemoryStream ms = new MemoryStream()) {
				if (!persistSignature(store)) {
					Debug.Assert(!persistNonce(Endpoint, store), "Without a signature, a nonce is meaningless.");
					StreamWriter writer = new StreamWriter(ms);
					Endpoint.Serialize(writer);
					writer.Flush();
					return Convert.ToBase64String(ms.ToArray());
				} else {
					using (HashAlgorithm shaHash = createHashAlgorithm(store))
					using (CryptoStream shaStream = new CryptoStream(ms, shaHash, CryptoStreamMode.Write)) {
						StreamWriter writer = new StreamWriter(shaStream);

						Endpoint.Serialize(writer);
						if (persistNonce(Endpoint, store))
							writer.WriteLine(Nonce.Code);
						
						writer.Flush();
						shaStream.Flush();
						shaStream.FlushFinalBlock();

						byte[] hash = shaHash.Hash;
						byte[] data = new byte[hash.Length + ms.Length];
						Buffer.BlockCopy(hash, 0, data, 0, hash.Length);
						Buffer.BlockCopy(ms.ToArray(), 0, data, hash.Length, (int)ms.Length);

						return Convert.ToBase64String(data);
					}
				}
			}
		}

		/// <summary>
		/// Deserializes a token returned to us from the provider and verifies its integrity.
		/// </summary>
		/// <remarks>
		/// As part of deserialization, the signature is verified to check
		/// for tampering, and the nonce (if included by the RP) is also checked.
		/// If no signature is present (due to stateless mode), the endpoint is verified
		/// by discovery (slow but secure).
		/// </remarks>
		public static Token Deserialize(string token, INonceStore store) {
			byte[] tok = Convert.FromBase64String(token);
			MemoryStream ms;

			if (persistSignature(store)) {
				// Verify the signature to guarantee that our state hasn't been
				// tampered with in transit or on the provider.
				HashAlgorithm hmac = createHashAlgorithm(store);
				byte[] sig = new byte[hmac.HashSize / 8];
				if (tok.Length < sig.Length)
					throw new OpenIdException(Strings.InvalidSignature);
				Buffer.BlockCopy(tok, 0, sig, 0, sig.Length);
				ms = new MemoryStream(tok, sig.Length, tok.Length - sig.Length);
				byte[] newSig = hmac.ComputeHash(ms);
				ms.Seek(0, SeekOrigin.Begin);
				for (int i = 0; i < sig.Length; i++)
					if (sig[i] != newSig[i])
						throw new OpenIdException(Strings.InvalidSignature);
			} else {
				ms = new MemoryStream(tok);
			}

			StreamReader reader = new StreamReader(ms);
			ServiceEndpoint endpoint = ServiceEndpoint.Deserialize(reader);
			Nonce nonce = null;
			if (persistNonce(endpoint, store)) {
				nonce = new Nonce(reader.ReadLine(), false);
				nonce.Consume(store);
			}
			if (!persistSignature(store)) {
				verifyEndpointByDiscovery(endpoint);
			}

			return new Token(nonce, endpoint);
		}

		static HashAlgorithm createHashAlgorithm(INonceStore store) {
			return new HMACSHA256(store.SecretSigningKey);
		}
		/// <summary>
		/// Whether a relying party-side nonce should be used to protect
		/// against replay attacks.
		/// </summary>
		/// <remarks>
		/// When communicating with an OP using OpenID 2.0, the provider takes
		/// care of the nonce, so we don't have to.
		/// 
		/// If operating under stateless mode, nonces can't be used on the RP
		/// side, so we rely on the Provider to be using some nonce mechanism.
		/// In OpenID 2.0, this is guaranteed, but in 1.x it's just an 
		/// assumption, which allows for replay attacks if the assumption is false.
		/// </remarks>
		static bool persistNonce(ServiceEndpoint endpoint, INonceStore store) {
			return endpoint.Protocol.Version.Major < 2 && persistSignature(store);
		}
		/// <summary>
		/// Whether to sign a token.
		/// </summary>
		/// <remarks>
		/// If an application store exists, we should sign the token.  If it doesn't,
		/// we haven't any means to keep a secret, so we can't sign the token.
		/// </remarks>
		static bool persistSignature(INonceStore store) {
			return store != null;
		}
		/// <summary>
		/// Performs discovery on the information in the token to detect any tampering.
		/// </summary>
		/// <remarks>
		/// Manual re-discovery of a Claimed Identifier is the slow way to perform verification.
		/// The best way is to check a signature on a deserialized token.  That is the primary method,
		/// but when stateless mode is used and no place exists to store a secret for signature
		/// verification, this is the only alternative.
		/// </remarks>
		static void verifyEndpointByDiscovery(ServiceEndpoint endpoint) {
			if (!endpoint.Equals(endpoint.ClaimedIdentifier.Discover())) {
				throw new OpenIdException(Strings.InvalidSignature);
			}
		}
	}
}
