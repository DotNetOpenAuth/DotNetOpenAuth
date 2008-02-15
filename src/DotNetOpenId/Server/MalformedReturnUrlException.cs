using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace DotNetOpenId.Server {
	public class MalformedReturnUrlException : ProtocolException {
		public MalformedReturnUrlException(NameValueCollection query, string return_to)
			: base(query, "") {
			ReturnTo = return_to;
		}

		public string ReturnTo { get; private set; }
	}
}