//-----------------------------------------------------------------------
// <copyright file="HttpHost.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Threading;
	using DotNetOpenAuth.OpenId.Provider;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;

	internal class HttpHost : IDisposable {
		private readonly HttpListener listener;
		private Thread listenerThread;
		private RequestHandler handler;

		private HttpHost(RequestHandler handler) {
			Contract.Requires(handler != null);

			this.Port = 59687;
			this.handler = handler;
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

		internal delegate void RequestHandler(HttpListenerContext context);

		public static HttpHost CreateHost(RequestHandler handler) {
			Contract.Requires(handler != null);
			Contract.Ensures(Contract.Result<HttpHost>() != null);

			return new HttpHost(handler);
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
			Contract.Requires(this.listener != null);

			try {
				while (true) {
					HttpListenerContext context = this.listener.GetContext();
					this.handler(context);
				}
			} catch (HttpListenerException ex) {
				// the listener is probably being shut down
				App.Logger.Warn("HTTP listener is closing down.", ex);
			}
		}
	}
}
