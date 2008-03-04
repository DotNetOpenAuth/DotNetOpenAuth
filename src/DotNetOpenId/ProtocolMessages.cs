using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId {
	internal static class ProtocolMessages {
		public readonly static HttpEncoding Http = new HttpEncoding();
		public readonly static KeyValueFormEncoding KeyValueForm = new KeyValueFormEncoding();
	}

	internal interface IProtocolMessageEncoding {
		byte[] GetBytes(IDictionary<string, string> dictionary);
		IDictionary<string, string> GetDictionary(byte[] bytes);
		IDictionary<string, string> GetDictionary(byte[] bytes, int offset, int count);
	}
}
