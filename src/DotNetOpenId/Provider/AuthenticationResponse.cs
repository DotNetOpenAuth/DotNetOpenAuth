using System;
using System.Collections.Specialized;
using System.Text;
using System.Net;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// A response to an OpenID request in terms a web server understands.
	/// </summary>
	public class AuthenticationResponse {
		internal AuthenticationResponse(HttpStatusCode code, NameValueCollection headers, byte[] body) {
			Code = code;
			Headers = headers ?? new NameValueCollection();
			Body = body;
		}

		public HttpStatusCode Code { get; private set; }
		public NameValueCollection Headers { get; private set; }
		public byte[] Body { get; private set; }
	}
}
