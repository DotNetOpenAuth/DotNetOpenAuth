using System;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace DotNetOpenId {
	internal class HmacSha1Association : Association {

		public HmacSha1Association(string handle, byte[] secret, TimeSpan totalLifeLength)
			: base(handle, secret, totalLifeLength, DateTime.UtcNow) {
		}

		internal override string GetAssociationType(Protocol protocol) {
			return protocol.Args.SignatureAlgorithm.HMAC_SHA1;
		}

		protected override HashAlgorithm CreateHasher() {
			return new HMACSHA1(SecretKey);
		}
	}
}