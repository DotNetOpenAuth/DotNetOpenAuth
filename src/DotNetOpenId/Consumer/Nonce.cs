using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Consumer {
	public class Nonce {
		static TimeSpan maximumLifetime {
			get { return OpenIdConsumer.MaximumUserAgentAuthenticationTime; }
		}

		public Nonce() : this(CryptUtil.CreateNonce(), DateTime.UtcNow) {}
		public Nonce(string code, DateTime creationDate) {
			if (string.IsNullOrEmpty(code)) throw new ArgumentNullException("code");
			Code = code;
			CreationDate = creationDate;
		}

		public DateTime CreationDate { get; internal set; }
		public string Code { get; internal set; }

		public TimeSpan Age { get { return DateTime.UtcNow - CreationDate.ToUniversalTime(); } }
		public bool IsExpired { get { return Age > maximumLifetime; } }
		/// <summary>
		/// Gets the date past which this nonce is no longer valid, so storing a nonce for replay attack
		/// protection is only necessary until this time.
		/// </summary>
		public DateTime ExpirationDate { get { return CreationDate + maximumLifetime; } }

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
