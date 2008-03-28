using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace DotNetOpenId {
	class HmacSha256Association : Association {
		public HmacSha256Association(string handle, byte[] secret, TimeSpan totalLifeLength)
			: base(handle, secret, totalLifeLength, DateTime.UtcNow) {
		}

		protected override HashAlgorithm CreateHasher() {
			return new HMACSHA256(SecretKey);
		}

		internal override string GetAssociationType(Protocol protocol) {
			return protocol.Args.SignatureAlgorithm.HMAC_SHA256;
		}
	}
}
