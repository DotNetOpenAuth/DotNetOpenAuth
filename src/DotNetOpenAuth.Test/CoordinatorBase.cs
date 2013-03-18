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
	using System.Net.Http.Headers;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using DotNetOpenAuth.Test.OpenId;

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

		internal static Task RunAsync(Func<IHostFactories, CancellationToken, Task> driver, params Handler[] handlers) {
			var coordinator = new CoordinatorBase(driver, handlers);
			return coordinator.RunAsync();
		}

		protected internal virtual async Task RunAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			IHostFactories hostFactories = new MockingHostFactories(this.handlers);

			await this.driver(hostFactories, cancellationToken);
		}

		internal static Handler Handle(Uri uri) {
			return new Handler(uri);
		}

		internal static Func<IHostFactories, CancellationToken, Task> RelyingPartyDriver(Func<OpenIdRelyingParty, CancellationToken, Task> relyingPartyDriver) {
			return async (hostFactories, ct) => {
				var rp = new OpenIdRelyingParty(new StandardRelyingPartyApplicationStore(), hostFactories);
				await relyingPartyDriver(rp, ct);
			};
		}

		internal static Func<IHostFactories, CancellationToken, Task> ProviderDriver(Func<OpenIdProvider, CancellationToken, Task> providerDriver) {
			return async (hostFactories, ct) => {
				var op = new OpenIdProvider(new StandardProviderApplicationStore(), hostFactories);
				await providerDriver(op, ct);
			};
		}

		internal static Handler HandleProvider(Func<OpenIdProvider, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> provider) {
			return Handle(OpenIdTestBase.OPUri).By(async (req, ct) => {
				var op = new OpenIdProvider(new StandardProviderApplicationStore());
				return await provider(op, req, ct);
			});
		}

		internal struct Handler {
			internal Handler(Uri uri)
				: this() {
				this.Uri = uri;
			}

			public Uri Uri { get; private set; }

			public Func<IHostFactories, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> MessageHandler { get; private set; }

			internal Handler By(Func<IHostFactories, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) {
				return new Handler(this.Uri) { MessageHandler = handler };
			}

			internal Handler By(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) {
				return By((hf, req, ct) => handler(req, ct));
			}

			internal Handler By(Func<HttpRequestMessage, HttpResponseMessage> handler) {
				return By((req, ct) => Task.FromResult(handler(req)));
			}

			internal Handler By(string responseContent, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK) {
				return By(
					req => {
						var response = new HttpResponseMessage(statusCode);
						response.Content = new StringContent(responseContent);
						response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
						return response;
					});
			}
		}
	}
}
