using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId {
	public class Nonce {
		static readonly uint NonceLength = 8;
		static readonly byte[] AllowedCharacters;
		// This array of formats is not yet a complete list.
		static readonly string[] PermissibleDateTimeFormats = { "yyyy-MM-ddTHH:mm:ssZ" };
		static Nonce() {
			// Valid nonce characters are ASCII codes 33 - 126 per 2.0 spec section 10.1
			AllowedCharacters = new byte[126 - 33 + 1];
			int i = 0;
			for (byte j = 33; j <= 126; j++)
				AllowedCharacters[i++] = j;
		}

		public Nonce() : this(DateTime.UtcNow, generateUniqueFragment(), false) { }
		/// <summary>
		/// Deserializes a nonce from a string passed to us.
		/// </summary>
		/// <param name="code">
		/// A nonce in the format described by the OpenID Authentication 2.0
		/// spec section 10.1.  Specifically, it should be in the format:
		/// 2005-05-15T17:11:51ZUNIQUE
		/// </param>
		/// <param name="remoteServerOrigin"></param>
		public Nonce(string code, bool remoteServerOrigin) {
			if (string.IsNullOrEmpty(code)) throw new ArgumentNullException("code");
			Code = code;
			int indexOfDateEnd = code.IndexOf("Z", StringComparison.Ordinal);
			if (indexOfDateEnd < 0) throw new FormatException(Strings.InvalidNonce);
			CreationDate = DateTime.Parse(code.Substring(0, indexOfDateEnd + 1));
			UniqueFragment = code.Substring(indexOfDateEnd + 1);
			this.remoteServerOrigin = remoteServerOrigin;
		}
		internal Nonce(DateTime creation, string uniqueFragment, bool remoteServerOrigin) {
			Code = creation.ToUniversalTime().ToString(PermissibleDateTimeFormats[0]) + uniqueFragment;
			CreationDate = creation;
			UniqueFragment = uniqueFragment;
			this.remoteServerOrigin = remoteServerOrigin;
		}

		public string Code { get; internal set; }
		public DateTime CreationDate { get; internal set; }
		internal string UniqueFragment { get; set; }
		bool remoteServerOrigin;
		TimeSpan maximumLifetime {
			get {
				return remoteServerOrigin ?
					Protocol.MaximumUserAgentAuthenticationTime + Protocol.MaximumAllowableTimeSkew :
					Protocol.MaximumUserAgentAuthenticationTime;
			}
		}

		public TimeSpan Age { get { return DateTime.UtcNow - CreationDate.ToUniversalTime(); } }
		public bool IsExpired { get { return Age > maximumLifetime; } }
		/// <summary>
		/// Gets the date past which this nonce is no longer valid, so storing a nonce for replay attack
		/// protection is only necessary until this time.
		/// </summary>
		public DateTime ExpirationDate { get { return CreationDate + maximumLifetime; } }

		static Random generator = new Random();
		static void randomSelection(ref byte[] tofill, byte[] choices) {
			if (choices.Length <= 0) throw new ArgumentException("Invalid input passed to RandomSelection. Array must have something in it.", "choices");

			byte[] rand = new byte[1];
			for (int i = 0; i < tofill.Length; i++) {
				generator.NextBytes(rand);
				tofill[i] = choices[(Convert.ToInt32(rand[0]) % choices.Length)];
			}
		}
		static string generateUniqueFragment() {
			byte[] nonce = new byte[NonceLength];
			randomSelection(ref nonce, AllowedCharacters);
			return ASCIIEncoding.ASCII.GetString(nonce);
		}

		internal void Consume(INonceStore store) {
			if (IsExpired)
				throw new OpenIdException(Strings.ExpiredNonce);

			// We could store unused nonces and remove them as they are used, or
			// we could store used nonces and check that they do not previously exist.
			// To protect against DoS attacks, it's cheaper to store fully-used ones
			// than half-used ones because it costs the user agent more to get that far.
			lock (store) {
				// Replay detection
				if (store.ContainsNonce(this)) {
					// We've used this nonce before!  Replay attack!
					throw new OpenIdException(Strings.ReplayAttackDetected);
				}
				store.StoreNonce(this);
				store.ClearExpiredNonces();
			}
		}

		public override bool Equals(object obj) {
			Nonce other = obj as Nonce;
			if (other == null) return false;
			return Code == other.Code;
		}
		public override int GetHashCode() {
			return Code.GetHashCode();
		}
	}
}
