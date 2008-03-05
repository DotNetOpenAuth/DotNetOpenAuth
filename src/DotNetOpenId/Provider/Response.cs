using System;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Web;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// A response to an OpenID request in terms a web server understands.
	/// </summary>
	class Response : IResponse {
		internal Response(HttpStatusCode code, NameValueCollection headers, byte[] body) {
			Code = code;
			Headers = headers ?? new NameValueCollection();
			Body = body;
		}

		public HttpStatusCode Code { get; private set; }
		public NameValueCollection Headers { get; private set; }
		public byte[] Body { get; private set; }

		/// <summary>
		/// Sends this response to the user agent or OpenId consumer.
		/// </summary>
		/// <remarks>
		/// This method requires a current ASP.NET HttpContext.
		/// </remarks>
		public void Send() {
			if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);
			HttpContext.Current.Response.Clear();
			HttpContext.Current.Response.StatusCode = (int)Code;
			foreach (string headerName in Headers)
				HttpContext.Current.Response.AddHeader(headerName, Headers[headerName]);
			if (Body != null && Body.Length > 0) {
				HttpContext.Current.Response.OutputStream.Write(Body, 0, Body.Length);
				HttpContext.Current.Response.OutputStream.Flush();
			}
			HttpContext.Current.Response.OutputStream.Close();
		}
	}
}
