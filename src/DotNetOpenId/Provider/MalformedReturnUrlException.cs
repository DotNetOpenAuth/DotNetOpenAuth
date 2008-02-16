using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace DotNetOpenId.Provider {
	public class MalformedReturnUrlException : ProtocolException {
		internal MalformedReturnUrlException(NameValueCollection query, string return_to)
			: base(query, "") {
			ReturnTo = return_to;
		}

		public string ReturnTo { get; private set; }
	}
}