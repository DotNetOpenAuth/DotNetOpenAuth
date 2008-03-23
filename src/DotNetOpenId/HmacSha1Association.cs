using System;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace DotNetOpenId {
	internal class HmacSha1Association : Association {

		public HmacSha1Association(string handle, byte[] secret, TimeSpan totalLifeLength)
			: base(handle, secret, totalLifeLength, DateTime.UtcNow) {
		}

		protected internal override string AssociationType {
			get { return Protocol.Args.SignatureAlgorithm.HMAC_SHA1; }
		}

		protected internal override byte[] Sign(IDictionary<string, string> data, IList<string> keyOrder) {
			using (var hmac = new HMACSHA1(SecretKey)) {
				return hmac.ComputeHash(ProtocolMessages.KeyValueForm.GetBytes(data, keyOrder));
			}
		}
	}
}