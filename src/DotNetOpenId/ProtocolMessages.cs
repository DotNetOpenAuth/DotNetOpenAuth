using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DotNetOpenId {
	internal static class ProtocolMessages {
		public readonly static HttpEncoding Http = new HttpEncoding();
		public readonly static KeyValueFormEncoding KeyValueForm = new KeyValueFormEncoding();
	}

	internal interface IProtocolMessageEncoding {
		byte[] GetBytes(IDictionary<string, string> dictionary);
		IDictionary<string, string> GetDictionary(Stream data);
	}
}
