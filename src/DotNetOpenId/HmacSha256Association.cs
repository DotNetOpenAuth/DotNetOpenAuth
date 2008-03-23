using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace DotNetOpenId {
	class HmacSha256Association : Association {
		public HmacSha256Association(string handle, byte[] secret, TimeSpan totalLifeLength)
			: base(handle, secret, totalLifeLength, DateTime.UtcNow) {
		}

		protected internal override string AssociationType {
			get { return Protocol.Constants.SignatureAlgorithms.HMAC_SHA256; }
		}

		protected internal override byte[] Sign(IDictionary<string, string> data, IList<string> keyOrder) {
			using (var hmac = new HMACSHA256(SecretKey)) {
				return hmac.ComputeHash(ProtocolMessages.KeyValueForm.GetBytes(data, keyOrder));
			}
		}
	}
}
