namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using System.IO;
	using System.Net;

	internal class Accept400ErrorDirectWebRequestHandlerWrapper : IDirectWebRequestHandler {
		private IDirectWebRequestHandler wrappedHandler;

		internal Accept400ErrorDirectWebRequestHandlerWrapper(IDirectWebRequestHandler wrappedHandler) {
			ErrorUtilities.VerifyArgumentNotNull(wrappedHandler, "wrappedHandler");
			this.wrappedHandler = wrappedHandler;
		}

		internal IDirectWebRequestHandler WrappedHandler {
			get { return this.wrappedHandler; }
		}

		#region IDirectWebRequestHandler Members

		public Stream GetRequestStream(HttpWebRequest request) {
			return this.wrappedHandler.GetRequestStream(request);
		}

		public DirectWebResponse GetResponse(HttpWebRequest request) {
			try {
				return this.wrappedHandler.GetResponse(request);
			} catch (ProtocolException ex) {
				WebException innerWeb = ex.InnerException as WebException;
				if (innerWeb != null && innerWeb.Status == WebExceptionStatus.ProtocolError) {
					HttpWebResponse httpResponse = innerWeb.Response as HttpWebResponse;
					if (httpResponse != null && httpResponse.StatusCode == HttpStatusCode.BadRequest) {
						// This is OK.  The OpenID spec says that server errors be returned as HTTP 400,
						// So we'll just swallow the exception and generate the message that's in the
						// error response.
					}
				}

				// This isn't a recognized acceptable case.
				throw;
			}
		}

		#endregion
	}

	internal class Accept400ErrorDirectSslWebRequestHandlerWrapper : Accept400ErrorDirectWebRequestHandlerWrapper, IDirectSslWebRequestHandler {
		private IDirectSslWebRequestHandler wrappedHandler;

		internal Accept400ErrorDirectSslWebRequestHandlerWrapper(IDirectSslWebRequestHandler wrappedHandler)
			: base(wrappedHandler) {
			this.wrappedHandler = wrappedHandler;
		}

		#region IDirectSslWebRequestHandler Members

		public Stream GetRequestStream(HttpWebRequest request, bool requireSsl) {
			return this.wrappedHandler.GetRequestStream(request, requireSsl);
		}

		public DirectWebResponse GetResponse(HttpWebRequest request, bool requireSsl) {
			try {
				return this.wrappedHandler.GetResponse(request, requireSsl);
			} catch (ProtocolException ex) {
				WebException innerWeb = ex.InnerException as WebException;
				if (innerWeb != null && innerWeb.Status == WebExceptionStatus.ProtocolError) {
					HttpWebResponse httpResponse = innerWeb.Response as HttpWebResponse;
					if (httpResponse != null && httpResponse.StatusCode == HttpStatusCode.BadRequest) {
						// This is OK.  The OpenID spec says that server errors be returned as HTTP 400,
						// So we'll just swallow the exception and generate the message that's in the
						// error response.
					}
				}

				// This isn't a recognized acceptable case.
				throw;
			}
		}

		#endregion
	}
}
