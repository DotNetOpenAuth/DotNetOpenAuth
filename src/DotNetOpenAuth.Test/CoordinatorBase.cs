//-----------------------------------------------------------------------
// <copyright file="CoordinatorBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;
	using Validation;

	using System.Linq;

	internal class CoordinatorBase {
		private Func<IHostFactories, CancellationToken, Task> driver;

		private Handler[] handlers;

		internal CoordinatorBase(Func<IHostFactories, CancellationToken, Task> driver, params Handler[] handlers) {
			Requires.NotNull(driver, "driver");
			Requires.NotNull(handlers, "handlers");

			this.driver = driver;
			this.handlers = handlers;
		}

		protected internal virtual async Task RunAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			IHostFactories hostFactories = new MyHostFactories(this.handlers);

			await this.driver(hostFactories, cancellationToken);
		}

		internal static Handler Handle(Uri uri) {
			return new Handler(uri);
		}

		internal struct Handler {
			internal Handler(Uri uri)
				: this() {
				this.Uri = uri;
			}

			public Uri Uri { get; private set; }

			public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> MessageHandler { get; private set; }

			internal Handler By(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) {
				return new Handler(this.Uri) { MessageHandler = handler };
			}
		}

		private class MyHostFactories : IHostFactories {
			private readonly Handler[] handlers;

			public MyHostFactories(Handler[] handlers) {
				this.handlers = handlers;
			}

			public HttpMessageHandler CreateHttpMessageHandler() {
				return new ForwardingMessageHandler(this.handlers);
			}

			public HttpClient CreateHttpClient(HttpMessageHandler handler = null) {
				return new HttpClient(handler ?? this.CreateHttpMessageHandler());
			}
		}

		private class ForwardingMessageHandler : HttpMessageHandler {
			private readonly Handler[] handlers;

			public ForwardingMessageHandler(Handler[] handlers) {
				this.handlers = handlers;
			}

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
				foreach (var handler in this.handlers) {
					if (handler.Uri.AbsolutePath == request.RequestUri.AbsolutePath) {
						var response = await handler.MessageHandler(request, cancellationToken);
						if (response != null) {
							return response;
						}
					}
				}

				return new HttpResponseMessage(HttpStatusCode.NotFound);
			}
		}
	}
}
