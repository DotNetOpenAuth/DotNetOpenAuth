//-----------------------------------------------------------------------
// <copyright file="UserController.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web.Http;

	using Validation;

	public class UserController : ApiController {
		public HttpResponseMessage Get(string id) {
			string localId = null; // string.Format("http://localhost:{0}/user", context.Request.Url.Port);
			var opEndpoint = this.Url.Link("default", new { controller = "provider" });
			var opEndpointUri = new Uri(opEndpoint);
			return new HttpResponseMessage() {
				Content = new StringContent(GenerateHtmlDiscoveryDocument(opEndpointUri, localId), Encoding.UTF8, "text/html"),
			};
		}

		/// <summary>
		/// Generates HTML for an identity page.
		/// </summary>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="localId">The local id.</param>
		/// <returns>The HTML document to return to the RP.</returns>
		private static string GenerateHtmlDiscoveryDocument(Uri providerEndpoint, string localId) {
			Requires.NotNull(providerEndpoint, "providerEndpoint");

			const string DelegatedHtmlDiscoveryFormat = @"<html><head>
				<link rel=""openid.server"" href=""{0}"" />
				<link rel=""openid.delegate"" href=""{1}"" />
				<link rel=""openid2.provider"" href=""{0}"" />
				<link rel=""openid2.local_id"" href=""{1}"" />
			</head><body></body></html>";

			const string NonDelegatedHtmlDiscoveryFormat = @"<html><head>
				<link rel=""openid.server"" href=""{0}"" />
				<link rel=""openid2.provider"" href=""{0}"" />
			</head><body></body></html>";

			return string.Format(
				localId != null ? DelegatedHtmlDiscoveryFormat : NonDelegatedHtmlDiscoveryFormat,
				providerEndpoint.AbsoluteUri,
				localId);
		}
	}
}
