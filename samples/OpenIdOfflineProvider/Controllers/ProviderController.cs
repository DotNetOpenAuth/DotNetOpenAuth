//-----------------------------------------------------------------------
// <copyright file="ProviderController.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web.Http;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;

	public class ProviderController : ApiController {
		private static readonly ICryptoKeyAndNonceStore store = new MemoryCryptoKeyAndNonceStore();

	    private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
		private MainWindow MainWindow {
			get { return MainWindow.Instance; }
		}

		public Task<HttpResponseMessage> Get() {
			return this.HandleAsync();
		}

		public Task<HttpResponseMessage> Post() {
			return this.HandleAsync();
		}

		private async Task<HttpResponseMessage> HandleAsync() {
			var provider = new OpenIdProvider(store);
			IRequest request = await provider.GetRequestAsync(this.Request);
			if (request == null) {
                Logger.Error("A request came in that did not carry an OpenID message.");
				return new HttpResponseMessage(HttpStatusCode.BadRequest) {
					Content = new StringContent("<html><body>This is an OpenID Provider endpoint.</body></html>", Encoding.UTF8, "text/html"),
				};
			}

			return await await this.MainWindow.Dispatcher.InvokeAsync(async delegate {
				if (!request.IsResponseReady) {
					var authRequest = request as IAuthenticationRequest;
					if (authRequest != null) {
						string userIdentityPageBase = this.Url.Link("default", new { controller = "user" }) + "/";
						var userIdentityPageBaseUri = new Uri(userIdentityPageBase);
						switch (this.MainWindow.checkidRequestList.SelectedIndex) {
							case 0:
								if (authRequest.IsDirectedIdentity) {
									if (this.MainWindow.capitalizedHostName.IsChecked.Value) {
										userIdentityPageBase = (userIdentityPageBaseUri.Scheme + Uri.SchemeDelimiter + userIdentityPageBaseUri.Authority).ToUpperInvariant() + userIdentityPageBaseUri.PathAndQuery;
									}
									string leafPath = "directedidentity";
									if (this.MainWindow.directedIdentityTrailingPeriodsCheckbox.IsChecked.Value) {
										leafPath += ".";
									}
									authRequest.ClaimedIdentifier = Identifier.Parse(userIdentityPageBase + leafPath, true);
									authRequest.LocalIdentifier = authRequest.ClaimedIdentifier;
								}
								authRequest.IsAuthenticated = true;
								break;
							case 1:
								authRequest.IsAuthenticated = false;
								break;
							case 2:
								IntPtr oldForegroundWindow = NativeMethods.GetForegroundWindow();
								bool stoleFocus = NativeMethods.SetForegroundWindow(this.MainWindow);
								await CheckIdWindow.ProcessAuthenticationAsync(userIdentityPageBaseUri, authRequest, CancellationToken.None);
								if (stoleFocus) {
									NativeMethods.SetForegroundWindow(oldForegroundWindow);
								}
								break;
						}
					}
				}

				var responseMessage = await provider.PrepareResponseAsync(request);
				return responseMessage;
			});
		}
	}
}
