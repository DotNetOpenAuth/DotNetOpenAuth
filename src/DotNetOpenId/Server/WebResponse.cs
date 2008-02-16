using System;
using System.Collections.Specialized;
using System.Text;
using System.Net;

namespace DotNetOpenId.Server {
	/// <summary>
	/// A response to an OpenID request in terms a web server understands.
	/// </summary>
	public class WebResponse {
		internal WebResponse(HttpStatusCode code, NameValueCollection headers, byte[] body) {
			Code = code;
			Headers = headers ?? new NameValueCollection();
			Body = body;
		}

		public HttpStatusCode Code { get; private set; }
		public NameValueCollection Headers { get; private set; }
		public byte[] Body { get; private set; }
	}
}
