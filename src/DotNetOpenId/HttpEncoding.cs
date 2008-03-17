using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;

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

		public IDictionary<string, string> GetDictionary(Stream data) {
			using (StreamReader sr = new StreamReader(data, Encoding.ASCII))
				return Util.NameValueCollectionToDictionary(
					HttpUtility.ParseQueryString(sr.ReadToEnd()));
		}
	}
}
