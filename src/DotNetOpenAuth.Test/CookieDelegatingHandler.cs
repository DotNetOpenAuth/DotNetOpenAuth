//-----------------------------------------------------------------------
// <copyright file="CookieDelegatingHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	internal class CookieDelegatingHandler : DelegatingHandler {
		internal CookieDelegatingHandler(HttpMessageHandler innerHandler, CookieContainer cookieContainer = null)
			: base(innerHandler) {
			this.Container = cookieContainer ?? new CookieContainer();
		}

		public CookieContainer Container { get; set; }

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			this.Container.ApplyCookies(request);
			var response = await base.SendAsync(request, cancellationToken);
			this.Container.SetCookies(response);
			return response;
		}
	}
}
