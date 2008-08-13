using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;

namespace DotNetOpenId {
	class HmacSha384Association : Association {
		public HmacSha384Association(string handle, byte[] secret, TimeSpan totalLifeLength)
			: base(handle, secret, totalLifeLength, DateTime.UtcNow) {
			Debug.Assert(secret.Length == CryptUtil.Sha384.HashSize / 8);
		}

		protected override HashAlgorithm CreateHasher() {
			return new HMACSHA384(SecretKey);
		}

		internal override string GetAssociationType(Protocol protocol) {
			return protocol.Args.SignatureAlgorithm.HMAC_SHA384;
		}
	}
}
