using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenId.Provider;
using System.Diagnostics;

namespace DotNetOpenId.Test.Hosting {
	/// <remarks>
	/// This should be instantiated in the test app domain,
	/// and passed to the ASP.NET host app domain.
	/// </remarks>
	public class EncodingInterceptor : MarshalByRefObject {
		/// <summary>
		/// Forwards a call from the ASP.NET host on to any interested test.
		/// </summary>
		/// <param name="message"></param>
		internal void OnSigningMessage(IEncodable message) {
			if (SigningMessage != null) {
				try {
					SigningMessage(message);
				} catch (Exception e) {
					Trace.TraceWarning("Unhandled exception in cross app-domain event handler: {0}", e);
				}
			}
		}
		internal delegate void InterceptorHandler(IEncodable message);
		internal InterceptorHandler SigningMessage;
	}
}
