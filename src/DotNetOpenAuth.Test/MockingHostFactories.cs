//-----------------------------------------------------------------------
// <copyright file="MockingHostFactories.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.OpenId;
	using Validation;

	internal class MockingHostFactories : IHostFactories {
		public MockingHostFactories(Dictionary<Uri, Func<HttpRequestMessage, Task<HttpResponseMessage>>> handlers = null) {
			this.Handlers = handlers ?? new Dictionary<Uri, Func<HttpRequestMessage, Task<HttpResponseMessage>>>();
			this.CookieContainer = new CookieContainer();
			this.AllowAutoRedirects = true;
		}

		public Dictionary<Uri, Func<HttpRequestMessage, Task<HttpResponseMessage>>> Handlers { get; private set; }

		public CookieContainer CookieContainer { get; set; }

		public bool AllowAutoRedirects { get; set; }

		public bool InstallUntrustedWebReqestHandler { get; set; }

		public HttpMessageHandler CreateHttpMessageHandler() {
			var forwardingMessageHandler = new ForwardingMessageHandler(this.Handlers, this);
			var cookieDelegatingHandler = new CookieDelegatingHandler(forwardingMessageHandler, this.CookieContainer);
			if (this.InstallUntrustedWebReqestHandler) {
				var untrustedHandler = new UntrustedWebRequestHandler(cookieDelegatingHandler);
				untrustedHandler.AllowAutoRedirect = this.AllowAutoRedirects;
				return untrustedHandler;
			} else if (this.AllowAutoRedirects) {
				return new AutoRedirectHandler(cookieDelegatingHandler);
			} else {
				return cookieDelegatingHandler;
			}
		}

		public HttpClient CreateHttpClient(HttpMessageHandler handler = null) {
			return new HttpClient(handler ?? this.CreateHttpMessageHandler());
		}

		private class ForwardingMessageHandler : HttpMessageHandler {
			private readonly Dictionary<Uri, Func<HttpRequestMessage, Task<HttpResponseMessage>>> handlers;

			private readonly IHostFactories hostFactories;

			public ForwardingMessageHandler(Dictionary<Uri, Func<HttpRequestMessage, Task<HttpResponseMessage>>> handlers, IHostFactories hostFactories) {
				Requires.NotNull(handlers, "handlers");

				this.handlers = handlers;
				this.hostFactories = hostFactories;
			}

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
				foreach (var pair in this.handlers) {
					if (pair.Key.IsBaseOf(request.RequestUri) && pair.Key.AbsolutePath == request.RequestUri.AbsolutePath) {
						var response = await pair.Value(request);
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