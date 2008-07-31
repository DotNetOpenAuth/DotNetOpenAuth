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
	[DebuggerDisplay("Nonce: {Nonce}, Endpoint: {Endpoint.ClaimedIdentifier}")]
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
			using (MemoryStream dataStream = new MemoryStream()) {
				if (!persistSignature(store)) {
					Debug.Assert(!persistNonce(Endpoint, store), "Without a signature, a nonce is meaningless.");
					dataStream.WriteByte(0); // there will be NO signature.
					StreamWriter writer = new StreamWriter(dataStream);
					Endpoint.Serialize(writer);
					writer.Flush();
					return Convert.ToBase64String(dataStream.ToArray());
				} else {
					using (HashAlgorithm shaHash = createHashAlgorithm(store))
					using (CryptoStream shaStream = new CryptoStream(dataStream, shaHash, CryptoStreamMode.Write)) {
						StreamWriter writer = new StreamWriter(shaStream);
						Endpoint.Serialize(writer);
						if (persistNonce(Endpoint, store))
							writer.WriteLine(Nonce.Code);
						
						writer.Flush();
						shaStream.Flush();
						shaStream.FlushFinalBlock();

						byte[] hash = shaHash.Hash;
						byte[] data = new byte[1 + hash.Length + dataStream.Length];
						data[0] = 1; // there is a signature
						Buffer.BlockCopy(hash, 0, data, 1, hash.Length);
						Buffer.BlockCopy(dataStream.ToArray(), 0, data, 1 + hash.Length, (int)dataStream.Length);

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
			if (tok.Length < 1) throw new OpenIdException(Strings.InvalidSignature);
			bool signaturePresent = tok[0] == 1;
			bool signatureVerified = false;
			MemoryStream dataStream;

			if (signaturePresent) {
				if (persistSignature(store)) {
					// Verify the signature to guarantee that our state hasn't been
					// tampered with in transit or on the provider.
					HashAlgorithm hmac = createHashAlgorithm(store);
					int signatureLength = hmac.HashSize / 8;
					dataStream = new MemoryStream(tok, 1 + signatureLength, tok.Length - 1 - signatureLength);
					byte[] newSig = hmac.ComputeHash(dataStream);
					dataStream.Position = 0;
					if (tok.Length - 1 < newSig.Length)
						throw new OpenIdException(Strings.InvalidSignature);
					for (int i = 0; i < newSig.Length; i++)
						if (tok[i + 1] != newSig[i])
							throw new OpenIdException(Strings.InvalidSignature);
					signatureVerified = true;
				} else {
					// Oops, we have no application state, so we have no way of validating the signature.
					throw new OpenIdException(Strings.InconsistentAppState);
				}
			} else {
				dataStream = new MemoryStream(tok, 1, tok.Length - 1);
			}

			StreamReader reader = new StreamReader(dataStream);
			ServiceEndpoint endpoint = ServiceEndpoint.Deserialize(reader);
			Nonce nonce = null;
			if (signatureVerified && persistNonce(endpoint, store)) {
				nonce = new Nonce(reader.ReadLine(), false);
				nonce.Consume(store);
			}
			if (!signatureVerified) {
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
			// If the user entered an OP Identifier then the ClaimedIdentifier will be the special
			// identifier that we can't perform discovery on.  We need to be careful about that.
			Identifier identifierToDiscover;
			if (endpoint.ClaimedIdentifier == endpoint.Protocol.ClaimedIdentifierForOPIdentifier) {
				identifierToDiscover = endpoint.UserSuppliedIdentifier;
			} else {
				identifierToDiscover = endpoint.ClaimedIdentifier;
			}
			var discoveredEndpoints = new List<ServiceEndpoint>(identifierToDiscover.Discover());
			if (!discoveredEndpoints.Contains(endpoint)) {
				throw new OpenIdException(Strings.InvalidSignature);
			}
		}
	}
}
