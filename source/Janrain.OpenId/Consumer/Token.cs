using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Janrain.OpenId.Consumer {
	/// <summary>
	/// A state-containing bit of non-confidential data that is sent to the 
	/// user agent as part of the return_to URL so we can read from it later.
	/// </summary>
	class Token {
		static readonly TimeSpan maximumLifetime = TimeSpan.FromMinutes(5);
		public static readonly string TokenKey = "token";
		public Uri IdentityUrl;
		public Uri ServerId;
		public Uri ServerUrl;

		public Token(ServiceEndpoint serviceEndpoint)
			: this(serviceEndpoint.IdentityUrl, serviceEndpoint.ServerId, serviceEndpoint.ServerUrl) {
		}

		Token(Uri identityUrl, Uri serverId, Uri serverUrl) {
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
		public string Serialize(byte[] signingSecretKey) {
			string timestamp = DateTime.UtcNow.ToFileTimeUtc().ToString();

			using (MemoryStream ms = new MemoryStream())
			using (HashAlgorithm sha1 = new HMACSHA1(signingSecretKey))
			using (CryptoStream sha1Stream = new CryptoStream(ms, sha1, CryptoStreamMode.Write)) {
				DataWriter writeData = delegate(string value, bool writeSeparator) {
					byte[] buffer = Encoding.ASCII.GetBytes(value);
					sha1Stream.Write(buffer, 0, buffer.Length);

					if (writeSeparator)
						sha1Stream.WriteByte(0);
				};

				writeData(timestamp, true);
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

		public static Token Deserialize(string token, byte[] signingSecretKey) {
			byte[] tok = Convert.FromBase64String(token);

			if (tok.Length < 20)
				throw new FailureException(null, "Failed while reading token.");

			byte[] sig = new byte[20];
			Buffer.BlockCopy(tok, 0, sig, 0, 20);

			HMACSHA1 hmac = new HMACSHA1(signingSecretKey);
			byte[] newSig = hmac.ComputeHash(tok, 20, tok.Length - 20);

			for (int i = 0; i < sig.Length; i++)
				if (sig[i] != newSig[i])
					throw new FailureException(null, "Token failed signature verification.");

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
			DateTime ts = DateTime.FromFileTimeUtc(Convert.ToInt64(items[0]));
			ts += maximumLifetime;

			if (ts < DateTime.UtcNow)
				throw new FailureException(null, "Token has expired.");

			items.RemoveAt(0);

			return new Token(new Uri(items[0]), new Uri(items[1]), new Uri(items[2]));
		}

	}
}
