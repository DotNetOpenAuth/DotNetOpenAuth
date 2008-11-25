//-----------------------------------------------------------------------
// <copyright file="HttpHost.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Hosting {
	using System;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Threading;

	class HttpHost : IDisposable {
		private HttpListener listener;
		private Thread listenerThread;
		private AspNetHost aspNetHost;

		private HttpHost(AspNetHost aspNetHost) {
			this.aspNetHost = aspNetHost;

			this.Port = 59687;
			Random r = new Random();
		tryAgain:
			try {
				this.listener = new HttpListener();
				this.listener.Prefixes.Add(string.Format(CultureInfo.InvariantCulture, "http://localhost:{0}/", this.Port));
				this.listener.Start();
			} catch (HttpListenerException ex) {
				if (ex.Message.Contains("conflicts")) {
					this.Port += r.Next(1, 20);
					goto tryAgain;
				}
				throw;
			}
			this.listenerThread = new Thread(ProcessRequests);
			this.listenerThread.Start();
		}

		public int Port { get; private set; }

		public static HttpHost CreateHost(AspNetHost aspNetHost) {
			return new HttpHost(aspNetHost);
		}

		public static HttpHost CreateHost(string webDirectory) {
			return new HttpHost(AspNetHost.CreateHost(webDirectory));
		}
		
		public Uri BaseUri {
			get { return new Uri("http://localhost:" + this.Port.ToString() + "/"); }
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
			try {
				using (WebResponse response = request.GetResponse()) {
					using (StreamReader sr = new StreamReader(response.GetResponseStream()))
						return sr.ReadToEnd();
				}
			} catch (WebException ex) {
				Logger.Error("Exception in HttpHost", ex);
				using (StreamReader sr = new StreamReader(ex.Response.GetResponseStream())) {
					string streamContent = sr.ReadToEnd();
					Logger.ErrorFormat("Error content stream follows: {0}", streamContent);
				}
				throw;
			}
		}

		#region IDisposable Members

		public void Dispose() {
			listener.Close();
			listenerThread.Join(1000);
			listenerThread.Abort();
		}

		#endregion

		private void ProcessRequests() {
			try {
				while (true) {
					var context = this.listener.GetContext();
					this.aspNetHost.BeginProcessRequest(context);
				}
			} catch (HttpListenerException) {
				// the listener is probably being shut down
			}
		}
	}
}
