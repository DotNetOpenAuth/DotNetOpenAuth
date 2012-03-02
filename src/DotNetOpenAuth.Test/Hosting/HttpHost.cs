//-----------------------------------------------------------------------
// <copyright file="HttpHost.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Hosting {
	using System;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Threading;

	internal class HttpHost : IDisposable {
		private readonly HttpListener listener;
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
			this.listenerThread = new Thread(this.ProcessRequests);
			this.listenerThread.Start();
		}

		public int Port { get; private set; }

		public Uri BaseUri {
			get { return new Uri("http://localhost:" + this.Port.ToString() + "/"); }
		}

		public static HttpHost CreateHost(AspNetHost aspNetHost) {
			return new HttpHost(aspNetHost);
		}

		public static HttpHost CreateHost(string webDirectory) {
			return new HttpHost(AspNetHost.CreateHost(webDirectory));
		}

		public string ProcessRequest(string url) {
			return this.ProcessRequest(url, null);
		}

		public string ProcessRequest(string url, string body) {
			WebRequest request = WebRequest.Create(new Uri(this.BaseUri, url));
			if (body != null) {
				request.Method = "POST";
				request.ContentLength = body.Length;
				using (StreamWriter sw = new StreamWriter(request.GetRequestStream())) {
					sw.Write(body);
				}
			}
			try {
				using (WebResponse response = request.GetResponse()) {
					using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
						return sr.ReadToEnd();
					}
				}
			} catch (WebException ex) {
				Logger.Http.Error("Exception in HttpHost", ex);
				using (StreamReader sr = new StreamReader(ex.Response.GetResponseStream())) {
					string streamContent = sr.ReadToEnd();
					Logger.Http.ErrorFormat("Error content stream follows: {0}", streamContent);
				}
				throw;
			}
		}

		#region IDisposable Members

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				this.listener.Close();
				this.listenerThread.Join(1000);
				this.listenerThread.Abort();
			}
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
