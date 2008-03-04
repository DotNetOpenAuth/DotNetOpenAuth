using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace DotNetOpenId {
	/// <summary>
	/// Conversion to and from the HTTP Encoding defined by
	/// OpenID Authentication 2.0 section 4.1.2.
	/// http://openid.net/specs/openid-authentication-2_0.html#anchor4
	/// </summary>
	internal class HttpEncoding : IProtocolMessageEncoding {
		public byte[] GetBytes(IDictionary<string, string> dictionary) {
			return Encoding.ASCII.GetBytes(UriUtil.CreateQueryString(dictionary));
		}

		public IDictionary<string, string> GetDictionary(byte[] bytes) {
			return GetDictionary(bytes, 0, bytes.Length);
		}

		public IDictionary<string, string> GetDictionary(byte[] bytes, int offset, int count) {
			return Util.NameValueCollectionToDictionary(
				HttpUtility.ParseQueryString(Encoding.ASCII.GetString(bytes, offset, count))
			);
		}
	}
}
