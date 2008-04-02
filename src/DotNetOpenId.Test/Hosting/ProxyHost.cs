using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Globalization;
using System.Threading;
using System.IO;

namespace DotNetOpenId.Test.Hosting {
	class ProxyHost : IDisposable {
		HttpListener listener;
		public int Port { get; private set; }
		Thread listenerThread;
		public delegate void Handler();
		Handler handler;

		public ProxyHost(Handler handler) {
			Port = 59688;
			Random r = new Random();
		tryAgain:
			try {
				listener = new HttpListener();
				listener.Prefixes.Add(string.Format(CultureInfo.InvariantCulture,
					"http://localhost:{0}/", Port));
				listener.Start();
			} catch (HttpListenerException ex) {
				if (ex.Message.Contains("conflicts")) {
					Port += r.Next(1, 20);
					goto tryAgain;
				}
				throw;
			}
			listenerThread = new Thread(processRequests);
			listenerThread.Start();
			this.handler = handler;
		}

		void processRequests() {
			try {
				while (true) {
					var context = listener.GetContext();
					
				}
			} catch (HttpListenerException) {
				// the listener is probably being shut down
			}
		}

		public Uri BaseUri {
			get { return new Uri("http://localhost:" + Port.ToString() + "/"); }
		}
		public string ProcessRequest(string url) {
			return ProcessRequest(url, null);
		}
		public string ProcessRequest(string url, string body) {
			WebRequest request = WebRequest.Create(new Uri(BaseUri, url));
			if (body != null) {
				request.Method = "POST";
				request.ContentLength = body.Length;
				using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
					sw.Write(body);
			}
			using (WebResponse response = request.GetResponse()) {
				using (StreamReader sr = new StreamReader(response.GetResponseStream()))
					return sr.ReadToEnd();
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
