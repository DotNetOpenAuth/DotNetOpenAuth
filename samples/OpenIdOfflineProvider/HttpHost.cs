//-----------------------------------------------------------------------
// <copyright file="HttpHost.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;
	using Validation;

	/// <summary>
	/// An HTTP Listener that dispatches incoming requests for handling.
	/// </summary>
	internal class HttpHost : IDisposable {
		/// <summary>
		/// The HttpListener that waits for incoming requests.
		/// </summary>
		private readonly HttpListener listener;

		/// <summary>
		/// The thread that listens for incoming HTTP requests and dispatches them
		/// to the <see cref="handler"/>.
		/// </summary>
		private Thread listenerThread;

		/// <summary>
		/// The handler for incoming HTTP requests.
		/// </summary>
		private RequestHandlerAsync handler;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpHost"/> class.
		/// </summary>
		/// <param name="handler">The handler for incoming HTTP requests.</param>
		private HttpHost(RequestHandlerAsync handler) {
			Requires.NotNull(handler, "handler");

			this.Port = 45235;
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

			this.ProcessRequestsAsync();
		}

		/// <summary>
		/// The request handler delegate.
		/// </summary>
		/// <param name="context">Information on the incoming HTTP request.</param>
		/// <returns>A task that completes with the async operation.</returns>
		internal delegate Task RequestHandlerAsync(HttpListenerContext context);

		/// <summary>
		/// Gets the port that HTTP requests are being listened for on.
		/// </summary>
		public int Port { get; private set; }

		/// <summary>
		/// Gets the base URI for all incoming web requests that will be received.
		/// </summary>
		public Uri BaseUri {
			get { return new Uri("http://localhost:" + this.Port.ToString() + "/"); }
		}

		/// <summary>
		/// Creates the HTTP host.
		/// </summary>
		/// <param name="handler">The handler for incoming HTTP requests.</param>
		/// <returns>The instantiated host.</returns>
		public static HttpHost CreateHost(RequestHandlerAsync handler) {
			Requires.NotNull(handler, "handler");

			return new HttpHost(handler);
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				this.listener.Close();
				this.listenerThread.Join(1000);
				this.listenerThread.Abort();
			}
		}

		#endregion

		/// <summary>
		/// The HTTP listener thread body.
		/// </summary>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		private async Task ProcessRequestsAsync() {
			Assumes.True(this.listener != null);

			while (true) {
				try {
					HttpListenerContext context = await this.listener.GetContextAsync();
					await this.handler(context);
				} catch (HttpListenerException ex) {
					if (this.listener.IsListening) {
						App.Logger.Error("Unexpected exception.", ex);
					} else {
						// the listener is probably being shut down
						App.Logger.Warn("HTTP listener is closing down.", ex);
						break;
					}
				}
			}
		}
	}
}
