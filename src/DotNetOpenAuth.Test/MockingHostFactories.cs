//-----------------------------------------------------------------------
// <copyright file="MockingHostFactories.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Linq;
	using Validation;

	internal class MockingHostFactories : IHostFactories {
		private readonly List<TestBase.Handler> handlers;

		public MockingHostFactories(List<TestBase.Handler> handlers = null) {
			this.handlers = handlers ?? new List<TestBase.Handler>();
			this.CookieContainer = new CookieContainer();
		}

		public List<TestBase.Handler> Handlers {
			get { return this.handlers; }
		}

		public CookieContainer CookieContainer { get; set; }

		public HttpMessageHandler CreateHttpMessageHandler() {
			return new CookieDelegatingHandler(new ForwardingMessageHandler(this.handlers, this), this.CookieContainer);
		}

		public HttpClient CreateHttpClient(HttpMessageHandler handler = null) {
			return new HttpClient(handler ?? this.CreateHttpMessageHandler());
		}

		private class ForwardingMessageHandler : HttpMessageHandler {
			private readonly IEnumerable<TestBase.Handler> handlers;

			private readonly IHostFactories hostFactories;

			public ForwardingMessageHandler(IEnumerable<TestBase.Handler> handlers, IHostFactories hostFactories) {
				Requires.NotNull(handlers, "handlers");

				this.handlers = handlers;
				this.hostFactories = hostFactories;
			}

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
				foreach (var handler in this.handlers) {
					if (handler.Uri.IsBaseOf(request.RequestUri) && handler.Uri.AbsolutePath == request.RequestUri.AbsolutePath) {
						var response = await handler.MessageHandler(request);
						if (response != null) {
							if (response.RequestMessage == null) {
								response.RequestMessage = request;
							}

							return response;
						}
					}
				}

				return new HttpResponseMessage(HttpStatusCode.NotFound);
			}
		}
	}
}