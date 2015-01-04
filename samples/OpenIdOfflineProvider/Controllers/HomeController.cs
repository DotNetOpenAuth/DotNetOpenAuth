//-----------------------------------------------------------------------
// <copyright file="HomeController.cs" company="Andrew Arnott">
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

	using DotNetOpenAuth.Logging;

	using Validation;

	public class HomeController : ApiController {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        public HttpResponseMessage Get() {
            Logger.Info("Discovery on OP Identifier detected.");
			var opEndpoint = this.Url.Link("default", new { controller = "provider" });
			var opEndpointUri = new Uri(opEndpoint);
			return new HttpResponseMessage() {
				Content = new StringContent(GenerateXrdsOPIdentifierDocument(opEndpointUri, Enumerable.Empty<string>()), Encoding.UTF8, "application/xrds+xml"),
			};
		}

		/// <summary>
		/// Generates the OP Identifier XRDS document.
		/// </summary>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="supportedExtensions">The supported extensions.</param>
		/// <returns>The content of the XRDS document.</returns>
		private static string GenerateXrdsOPIdentifierDocument(Uri providerEndpoint, IEnumerable<string> supportedExtensions) {
			Requires.NotNull(providerEndpoint, "providerEndpoint");
			Requires.NotNull(supportedExtensions, "supportedExtensions");

			const string OPIdentifierDiscoveryFormat = @"<xrds:XRDS
	xmlns:xrds='xri://$xrds'
	xmlns:openid='http://openid.net/xmlns/1.0'
	xmlns='xri://$xrd*($v*2.0)'>
	<XRD>
		<Service priority='10'>
			<Type>http://specs.openid.net/auth/2.0/server</Type>
			{1}
			<URI>{0}</URI>
		</Service>
	</XRD>
</xrds:XRDS>";

			string extensions = string.Join(
				"\n\t\t\t",
				supportedExtensions.Select(ext => "<Type>" + ext + "</Type>").ToArray());
			return string.Format(
				OPIdentifierDiscoveryFormat,
				providerEndpoint.AbsoluteUri,
				extensions);
		}
	}
}
