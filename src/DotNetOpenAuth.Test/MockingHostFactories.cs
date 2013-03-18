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
		private readonly List<CoordinatorBase.Handler> handlers;

		public MockingHostFactories(List<CoordinatorBase.Handler> handlers = null) {
			this.handlers = handlers ?? new List<CoordinatorBase.Handler>();
		}

		public List<CoordinatorBase.Handler> Handlers {
			get { return this.handlers; }
		}

		public HttpMessageHandler CreateHttpMessageHandler() {
			return new ForwardingMessageHandler(this.handlers, this);
		}

		public HttpClient CreateHttpClient(HttpMessageHandler handler = null) {
			return new HttpClient(handler ?? this.CreateHttpMessageHandler());
		}

		private class ForwardingMessageHandler : HttpMessageHandler {
			private readonly IEnumerable<CoordinatorBase.Handler> handlers;

			private readonly IHostFactories hostFactories;

			public ForwardingMessageHandler(IEnumerable<CoordinatorBase.Handler> handlers, IHostFactories hostFactories) {
				Requires.NotNull(handlers, "handlers");

				this.handlers = handlers;
				this.hostFactories = hostFactories;
			}

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
				foreach (var handler in this.handlers) {
					if (handler.Uri.IsBaseOf(request.RequestUri) && handler.Uri.AbsolutePath == request.RequestUri.AbsolutePath) {
						var response = await handler.MessageHandler(this.hostFactories, request, cancellationToken);
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