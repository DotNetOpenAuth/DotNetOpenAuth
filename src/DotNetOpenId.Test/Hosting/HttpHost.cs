using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Globalization;
using System.Threading;
using System.IO;

namespace DotNetOpenId.Test.Hosting {
	class HttpHost : IDisposable {
		HttpListener listener;
		public int Port { get; private set; }
		Thread listenerThread;
		AspNetHost aspNetHost;

		public HttpHost(string webDirectory) {
			aspNetHost = AspNetHost.CreateHost(webDirectory);

			listener = new HttpListener();
			Port = 59687;
			listener.Prefixes.Add(string.Format(CultureInfo.InvariantCulture,
				"http://localhost:{0}/", Port));
			listener.Start();
			listenerThread = new Thread(processRequests);
			listenerThread.Start();
		}

		void processRequests() {
			try {
				while (true) {
					var context = listener.GetContext();
					using (TextWriter writer = new StreamWriter(context.Response.OutputStream)) {
						aspNetHost.ProcessRequest(
							context.Request.Url.LocalPath.TrimStart('/'),
							context.Request.Url.Query,
							context.Request.InputStream,
							writer);
					}
				}
			} catch (HttpListenerException) {
				// the listener is probably being shut down
			}
		}

		#region IDisposable Members

		public void Dispose() {
			listener.Close();
			listenerThread.Join(1000);
			listenerThread.Abort();
		}

		#endregion
	}
}
