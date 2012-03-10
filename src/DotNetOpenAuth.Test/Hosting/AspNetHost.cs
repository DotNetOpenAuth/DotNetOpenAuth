//-----------------------------------------------------------------------
// <copyright file="AspNetHost.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Hosting {
	using System;
	using System.IO;
	using System.Net;
	using System.Threading;
	using System.Web;
	using System.Web.Hosting;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Test.OpenId;

	/// <summary>
	/// Hosts an ASP.NET web site for placing ASP.NET controls and services on
	/// for more complete end-to-end testing.
	/// </summary>
	internal class AspNetHost : MarshalByRefObject, IDisposable {
		private HttpHost httpHost;

		/// <summary>
		/// Initializes a new instance of the <see cref="AspNetHost"/> class.
		/// </summary>
		/// <remarks>
		/// DO NOT CALL DIRECTLY.  This is only here for ASP.NET to call.
		/// Call the static <see cref="AspNetHost.CreateHost"/> method instead.
		/// </remarks>
		[Obsolete("Use the CreateHost static method instead.")]
		public AspNetHost() {
			this.httpHost = HttpHost.CreateHost(this);
		}

		public Uri BaseUri {
			get { return this.httpHost.BaseUri; }
		}

		public static AspNetHost CreateHost(string webDirectory) {
			AspNetHost host = (AspNetHost)ApplicationHost.CreateApplicationHost(typeof(AspNetHost), "/", webDirectory);
			return host;
		}

		public string ProcessRequest(string url) {
			return this.httpHost.ProcessRequest(url);
		}

		public string ProcessRequest(string url, string body) {
			return this.httpHost.ProcessRequest(url, body);
		}

		public void BeginProcessRequest(HttpListenerContext context) {
			ThreadPool.QueueUserWorkItem(state => { ProcessRequest(context); });
		}

		public void ProcessRequest(HttpListenerContext context) {
			try {
				using (TextWriter tw = new StreamWriter(context.Response.OutputStream)) {
					HttpRuntime.ProcessRequest(new TestingWorkerRequest(context, tw));
				}
			} catch (Exception ex) {
				Logger.Http.Error("Exception in AspNetHost", ex);
				throw;
			}
		}

		public void CloseHttp() {
			this.httpHost.Dispose();
		}

		#region IDisposable Members

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing) {
			if (disposing) {
				this.CloseHttp();
			}
		}

		#endregion
	}
}
