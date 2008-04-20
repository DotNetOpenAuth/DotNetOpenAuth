using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.RelyingParty;
using System.Globalization;

namespace DotNetOpenId {
	/// <summary>
	/// Represents some unique value that can help prevent replay attacks.
	/// </summary>
	/// <remarks>
	/// When persisting nonce instances, only the <see cref="Code"/> and <see cref="ExpirationDate"/>
	/// properties are significant.  Nonces never need to be deserialized.
	/// </remarks>
	public class Nonce {
		const uint UniqueFragmentLength = 8;
		/// <summary>
		/// These are the characters that may be chosen from when forming a random nonce,
		/// per the OpenID 2.0 Authentication spec section 10.1.  
		/// </summary>
		/// <remarks>
		/// The following characters are allowed in the spec, but because they can cause validation
		/// failures with ASP.NET query validation (XSS-detection) they are deliberately left out of
		/// the set of characters we choose from: &lt; &amp;
		/// </remarks>
		const string allowedCharacters =
			@"!""#$%'()*+,-./0123456789:;=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
		// This array of formats is not yet a complete list.
		static readonly string[] PermissibleDateTimeFormats = { "yyyy-MM-ddTHH:mm:ssZ" };

		internal Nonce() : this(DateTime.UtcNow, generateUniqueFragment(), false) { }
		/// <summary>
		/// Deserializes a nonce from a string passed to us.
		/// </summary>
		/// <param name="code">
		/// A nonce in the format described by the OpenID Authentication 2.0
		/// spec section 10.1.  Specifically, it should be in the format:
		/// 2005-05-15T17:11:51ZUNIQUE
		/// </param>
		/// <param name="remoteServerOrigin"></param>
		internal Nonce(string code, bool remoteServerOrigin) {
			if (string.IsNullOrEmpty(code)) throw new ArgumentNullException("code");
			Code = code;
			int indexOfDateEnd = code.IndexOf("Z", StringComparison.Ordinal);
			if (indexOfDateEnd < 0) throw new FormatException(Strings.InvalidNonce);
			CreationDate = DateTime.Parse(code.Substring(0, indexOfDateEnd + 1), CultureInfo.InvariantCulture);
			UniqueFragment = code.Substring(indexOfDateEnd + 1);
			this.remoteServerOrigin = remoteServerOrigin;
		}
		internal Nonce(DateTime creation, string uniqueFragment, bool remoteServerOrigin) {
			Code = creation.ToUniversalTime().ToString(PermissibleDateTimeFormats[0], CultureInfo.InvariantCulture) + uniqueFragment;
			CreationDate = creation.ToUniversalTime();
			UniqueFragment = uniqueFragment;
			this.remoteServerOrigin = remoteServerOrigin;
		}

		/// <summary>
		/// The string form of the nonce that can be transmitted with an authentication
		/// request or response.
		/// </summary>
		public string Code { get; internal set; }
		/// <summary>
		/// The UTC date/time this nonce was generated.
		/// </summary>
		internal DateTime CreationDate { get; set; }
		internal string UniqueFragment { get; set; }
		bool remoteServerOrigin;
		TimeSpan maximumLifetime {
			get {
				return remoteServerOrigin ?
					Protocol.MaximumUserAgentAuthenticationTime + Protocol.MaximumAllowableTimeSkew :
					Protocol.MaximumUserAgentAuthenticationTime;
			}
		}

		internal TimeSpan Age { get { return DateTime.UtcNow - CreationDate.ToUniversalTime(); } }
		/// <summary>
		/// Gets whether this nonce is so old it no longer needs to be stored.
		/// </summary>
		public bool IsExpired { get { return Age > maximumLifetime; } }
		/// <summary>
		/// Gets the UTC date beyond which this nonce is no longer valid, so storing
		/// a nonce for replay attack protection is only necessary until this time.
		/// </summary>
		public DateTime ExpirationDate { get { return CreationDate + maximumLifetime; } }

		static Random generator = new Random();
		static string generateUniqueFragment() {
			char[] nonce = new char[UniqueFragmentLength];
			for (int i = 0; i < nonce.Length; i++) {
				nonce[i] = allowedCharacters[generator.Next(allowedCharacters.Length)];
			}
			return new string(nonce);
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

		/// <summary>
		/// Tests equality of two <see cref="Nonce"/> objects.
		/// </summary>
		public override bool Equals(object obj) {
			Nonce other = obj as Nonce;
			if (other == null) return false;
			return Code == other.Code;
		}
		/// <summary>
		/// Gets the hash code.
		/// </summary>
		public override int GetHashCode() {
			return Code.GetHashCode();
		}
		/// <summary>
		/// Returns the string representation of the <see cref="Nonce"/>.
		/// This is the <see cref="Code"/> property.
		/// </summary>
		public override string ToString() {
			return Code;
		}
	}
}
