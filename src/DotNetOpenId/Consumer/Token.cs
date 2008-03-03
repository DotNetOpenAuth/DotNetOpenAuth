using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Diagnostics;

namespace DotNetOpenId.Consumer {
	/// <summary>
	/// A state-containing bit of non-confidential data that is sent to the 
	/// user agent as part of the return_to URL so we can read from it later.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", 
		"CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "CryptoStream is not stored in a field.")]
	class Token {
		public static readonly string TokenKey = "token";
		/// <summary>
		/// The URL given as the OpenId URL, which may not be the same as the Provider-issued
		/// OpenId URL.
		/// This points to the page with the &lt;LINK&gt; tag with openid.server in it.
		/// </summary>
		public Uri IdentityUrl { get; private set; }
		/// <summary>
		/// The DelegateUrl if supplied, otherwise the IdentityUrl.
		/// </summary>
		public Uri ServerId { get; private set; }
		/// <summary>
		/// The OpenId provider URL used for programmatic authentication.
		/// </summary>
		public Uri ServerUrl { get; private set; }
		public Nonce Nonce { get; set; }

		public Token(ServiceEndpoint serviceEndpoint)
			: this(new Nonce(), serviceEndpoint.IdentityUrl, serviceEndpoint.ServerId, serviceEndpoint.ServerUrl) {
		}

		Token(Nonce nonce, Uri identityUrl, Uri serverId, Uri serverUrl) {
			this.Nonce = nonce;
			IdentityUrl = identityUrl;
			ServerId = serverId;
			ServerUrl = serverUrl;
		}

		delegate void DataWriter(string data, bool writeSeparator);

		/// <summary>
		/// Serializes this <see cref="Token"/> instance as a string that can be
		/// included as part of a return_to variable in a querystring. 
		/// This string is cryptographically signed to protect against tampering.
		/// </summary>
		public string Serialize(INonceStore store) {
			string timestamp = Nonce.CreationDate.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture);

			using (MemoryStream ms = new MemoryStream())
			using (HashAlgorithm sha1 = new HMACSHA1(store.SecretSigningKey))
			using (CryptoStream sha1Stream = new CryptoStream(ms, sha1, CryptoStreamMode.Write)) {
				DataWriter writeData = delegate(string value, bool writeSeparator) {
					byte[] buffer = Encoding.ASCII.GetBytes(value);
					sha1Stream.Write(buffer, 0, buffer.Length);

					if (writeSeparator)
						sha1Stream.WriteByte(0);
				};

				writeData(timestamp, true);
				writeData(Nonce.Code, true);
				writeData(IdentityUrl.AbsoluteUri, true);
				writeData(ServerId.AbsoluteUri, true);
				writeData(ServerUrl.AbsoluteUri, false);

				sha1Stream.Flush();
				sha1Stream.FlushFinalBlock();

				byte[] hash = sha1.Hash;

				byte[] data = new byte[sha1.HashSize / 8 + ms.Length];
				Buffer.BlockCopy(hash, 0, data, 0, hash.Length);
				Buffer.BlockCopy(ms.ToArray(), 0, data, hash.Length, (int)ms.Length);

				return CryptUtil.ToBase64String(data);
			}
		}

		public static Token Deserialize(string token, INonceStore store) {
			byte[] tok = Convert.FromBase64String(token);

			if (tok.Length < 20)
				throw new OpenIdException("Failed while reading token.");

			byte[] sig = new byte[20];
			Buffer.BlockCopy(tok, 0, sig, 0, 20);

			HMACSHA1 hmac = new HMACSHA1(store.SecretSigningKey);
			byte[] newSig = hmac.ComputeHash(tok, 20, tok.Length - 20);

			for (int i = 0; i < sig.Length; i++)
				if (sig[i] != newSig[i])
					throw new OpenIdException("Token failed signature verification.");

			List<string> items = new List<string>();

			int prev = 20;
			int idx;

			while ((idx = Array.IndexOf<byte>(tok, 0, prev)) > -1) {
				items.Add(Encoding.ASCII.GetString(tok, prev, idx - prev));

				prev = idx + 1;
			}

			if (prev < tok.Length)
				items.Add(Encoding.ASCII.GetString(tok, prev, tok.Length - prev));

			//# Check if timestamp has expired
			DateTime ts = DateTime.FromFileTimeUtc(Convert.ToInt64(items[0], CultureInfo.InvariantCulture));
			Nonce nonce = new Nonce(items[1], ts);
			consumeNonce(nonce, store);

			return new Token(nonce, new Uri(items[2]), new Uri(items[3]), new Uri(items[4]));
		}

		static void consumeNonce(Nonce nonce, INonceStore store) {
			if (nonce.IsExpired)
				throw new OpenIdException(Strings.ExpiredNonce);

			// We could store unused nonces and remove them as they are used, or
			// we could store used nonces and check that they do not previously exist.
			// To protect against DoS attacks, it's cheaper to store fully-used ones
			// than half-used ones because it costs the user agent more to get that far.
			lock (store) {
				// Replay detection
				if (store.ContainsNonce(nonce)) {
					// We've used this nonce before!  Replay attack!
					throw new OpenIdException(Strings.ReplayAttackDetected);
				}
				store.StoreNonce(nonce);
				store.ClearExpiredNonces();
			}
		}

		public override bool Equals(object obj) {
			Token other = obj as Token;
			if (other == null) return false;
			Debug.Assert(this.Nonce != null && other.Nonce != null, "No Token should be constructed with null Nonces.");
			// This should pretty much always return false, since every token should
			// have it's own unique nonce.
			return
				this.IdentityUrl == other.IdentityUrl &&
				this.Nonce.Equals(other.Nonce) &&
				this.ServerId == other.ServerId &&
				this.ServerUrl == other.ServerUrl;
		}
	}
}
