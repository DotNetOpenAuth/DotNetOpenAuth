//-----------------------------------------------------------------------
// <copyright file="UriDiscoveryServiceTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.DiscoveryServices {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;

	using NUnit.Framework;

	[TestFixture]
	public class UriDiscoveryServiceTests : OpenIdTestBase {
		[Test]
		public async Task DiscoveryWithRedirects() {
			Identifier claimedId = this.GetMockIdentifier(ProtocolVersion.V20, false);

			// Add a couple of chained redirect pages that lead to the claimedId.
			Uri userSuppliedUri = new Uri("https://localhost/someSecurePage");
			Uri insecureMidpointUri = new Uri("http://localhost/insecureStop");
			this.RegisterMockRedirect(userSuppliedUri, insecureMidpointUri);
			this.RegisterMockRedirect(insecureMidpointUri, new Uri(claimedId.ToString()));

			// don't require secure SSL discovery for this test.
			Identifier userSuppliedIdentifier = new UriIdentifier(userSuppliedUri, false);
			var discoveryResult = await this.DiscoverAsync(userSuppliedIdentifier);
			Assert.AreEqual(1, discoveryResult.Count());
		}

		[Test]
		public async Task DiscoverRequireSslWithSecureRedirects() {
			Identifier claimedId = this.GetMockIdentifier(ProtocolVersion.V20, true);

			// Add a couple of chained redirect pages that lead to the claimedId.
			// All redirects should be secure.
			Uri userSuppliedUri = new Uri("https://localhost/someSecurePage");
			Uri secureMidpointUri = new Uri("https://localhost/secureStop");
			this.RegisterMockRedirect(userSuppliedUri, secureMidpointUri);
			this.RegisterMockRedirect(secureMidpointUri, new Uri(claimedId.ToString()));

			Identifier userSuppliedIdentifier = new UriIdentifier(userSuppliedUri, true);
			var discoveryResult = await this.DiscoverAsync(userSuppliedIdentifier);
			Assert.AreEqual(1, discoveryResult.Count());
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public async Task DiscoverRequireSslWithInsecureRedirect() {
			Identifier claimedId = this.GetMockIdentifier(ProtocolVersion.V20, true);

			// Add a couple of chained redirect pages that lead to the claimedId.
			// Include an insecure HTTP jump in those redirects to verify that
			// the ultimate endpoint is never found as a result of high security profile.
			Uri userSuppliedUri = new Uri("https://localhost/someSecurePage");
			Uri insecureMidpointUri = new Uri("http://localhost/insecureStop");
			this.RegisterMockRedirect(userSuppliedUri, insecureMidpointUri);
			this.RegisterMockRedirect(insecureMidpointUri, new Uri(claimedId.ToString()));

			Identifier userSuppliedIdentifier = new UriIdentifier(userSuppliedUri, true);
			await this.DiscoverAsync(userSuppliedIdentifier);
		}

		[Test]
		public async Task DiscoveryRequireSslWithInsecureXrdsInSecureHtmlHead() {
			var insecureXrdsSource = this.GetMockIdentifier(ProtocolVersion.V20, false);
			Uri secureClaimedUri = new Uri("https://localhost/secureId");

			string html = string.Format("<html><head><meta http-equiv='X-XRDS-Location' content='{0}'/></head><body></body></html>", insecureXrdsSource);
			this.RegisterMockResponse(secureClaimedUri, "text/html", html);

			Identifier userSuppliedIdentifier = new UriIdentifier(secureClaimedUri, true);
			var discoveryResult = await this.DiscoverAsync(userSuppliedIdentifier);
			Assert.AreEqual(0, discoveryResult.Count());
		}

		[Test]
		public async Task DiscoveryRequireSslWithInsecureXrdsInSecureHttpHeader() {
			var insecureXrdsSource = this.GetMockIdentifier(ProtocolVersion.V20, false);

			string html = "<html><head></head><body></body></html>";
			WebHeaderCollection headers = new WebHeaderCollection {
				{ "X-XRDS-Location", insecureXrdsSource }
			};
			this.RegisterMockResponse(VanityUriSsl, VanityUriSsl, "text/html", headers, html);

			Identifier userSuppliedIdentifier = new UriIdentifier(VanityUriSsl, true);
			var discoveryResult = await this.DiscoverAsync(userSuppliedIdentifier);
			Assert.AreEqual(0, discoveryResult.Count());
		}

		[Test]
		public async Task DiscoveryRequireSslWithInsecureXrdsButSecureLinkTags() {
			var insecureXrdsSource = this.GetMockIdentifier(ProtocolVersion.V20, false);
			string html = string.Format(
				@"
	<html><head>
		<meta http-equiv='X-XRDS-Location' content='{0}'/> <!-- this one will be insecure and ignored -->
		<link rel='openid2.provider' href='{1}' />
		<link rel='openid2.local_id' href='{2}' />
	</head><body></body></html>",
				HttpUtility.HtmlEncode(insecureXrdsSource),
				HttpUtility.HtmlEncode(OPUriSsl.AbsoluteUri),
				HttpUtility.HtmlEncode(OPLocalIdentifiersSsl[1].AbsoluteUri));
			this.Handle(VanityUriSsl).By(html, "text/html");

			Identifier userSuppliedIdentifier = new UriIdentifier(VanityUriSsl, true);

			// We verify that the XRDS was ignored and the LINK tags were used
			// because the XRDS OP-LocalIdentifier uses different local identifiers.
			var discoveryResult = await this.DiscoverAsync(userSuppliedIdentifier);
			Assert.AreEqual(OPLocalIdentifiersSsl[1].AbsoluteUri, discoveryResult.Single().ProviderLocalIdentifier.ToString());
		}

		[Test]
		public async Task DiscoveryRequiresSslIgnoresInsecureEndpointsInXrds() {
			var insecureEndpoint = GetServiceEndpoint(0, ProtocolVersion.V20, 10, false);
			var secureEndpoint = GetServiceEndpoint(1, ProtocolVersion.V20, 20, true);
			UriIdentifier secureClaimedId = new UriIdentifier(VanityUriSsl, true);
			this.RegisterMockXrdsResponse(secureClaimedId, new[] { insecureEndpoint, secureEndpoint });
			var discoverResult = await this.DiscoverAsync(secureClaimedId);
			Assert.AreEqual(secureEndpoint.ProviderLocalIdentifier, discoverResult.Single().ProviderLocalIdentifier);
		}

		[Test]
		public async Task XrdsDirectDiscovery_10() {
			await this.FailDiscoverXrdsAsync("xrds-irrelevant");
			await this.DiscoverXrdsAsync("xrds10", ProtocolVersion.V10, null, "http://a/b");
			await this.DiscoverXrdsAsync("xrds11", ProtocolVersion.V11, null, "http://a/b");
			await this.DiscoverXrdsAsync("xrds1020", ProtocolVersion.V10, null, "http://a/b");
		}

		[Test]
		public async Task XrdsDirectDiscovery_20() {
			await this.DiscoverXrdsAsync("xrds20", ProtocolVersion.V20, null, "http://a/b");
			await this.DiscoverXrdsAsync("xrds2010a", ProtocolVersion.V20, null, "http://a/b");
			await this.DiscoverXrdsAsync("xrds2010b", ProtocolVersion.V20, null, "http://a/b");
		}

		[Test]
		public async Task HtmlDiscover_11() {
			await this.DiscoverHtmlAsync("html10prov", ProtocolVersion.V11, null, "http://a/b");
			await this.DiscoverHtmlAsync("html10both", ProtocolVersion.V11, "http://c/d", "http://a/b");
			await this.FailDiscoverHtmlAsync("html10del");

			// Verify that HTML discovery generates the 1.x endpoints when appropriate
			await this.DiscoverHtmlAsync("html2010", ProtocolVersion.V11, "http://g/h", "http://e/f");
			await this.DiscoverHtmlAsync("html1020", ProtocolVersion.V11, "http://g/h", "http://e/f");
			await this.DiscoverHtmlAsync("html2010combinedA", ProtocolVersion.V11, "http://c/d", "http://a/b");
			await this.DiscoverHtmlAsync("html2010combinedB", ProtocolVersion.V11, "http://c/d", "http://a/b");
			await this.DiscoverHtmlAsync("html2010combinedC", ProtocolVersion.V11, "http://c/d", "http://a/b");
		}

		[Test]
		public async Task HtmlDiscover_20() {
			await this.DiscoverHtmlAsync("html20prov", ProtocolVersion.V20, null, "http://a/b");
			await this.DiscoverHtmlAsync("html20both", ProtocolVersion.V20, "http://c/d", "http://a/b");
			await this.FailDiscoverHtmlAsync("html20del");
			await this.DiscoverHtmlAsync("html2010", ProtocolVersion.V20, "http://c/d", "http://a/b");
			await this.DiscoverHtmlAsync("html1020", ProtocolVersion.V20, "http://c/d", "http://a/b");
			await this.DiscoverHtmlAsync("html2010combinedA", ProtocolVersion.V20, "http://c/d", "http://a/b");
			await this.DiscoverHtmlAsync("html2010combinedB", ProtocolVersion.V20, "http://c/d", "http://a/b");
			await this.DiscoverHtmlAsync("html2010combinedC", ProtocolVersion.V20, "http://c/d", "http://a/b");
			await this.FailDiscoverHtmlAsync("html20relative");
		}

		[Test]
		public async Task XrdsDiscoveryFromHead() {
			this.RegisterMockResponse(
				new Uri("http://localhost/xrds1020.xml"),
				"application/xrds+xml",
				LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds1020.xml"));
			await this.DiscoverXrdsAsync("XrdsReferencedInHead.html", ProtocolVersion.V10, null, "http://a/b");
		}

		[Test]
		public async Task XrdsDiscoveryFromHttpHeader() {
			WebHeaderCollection headers = new WebHeaderCollection();
			headers.Add("X-XRDS-Location", new Uri("http://localhost/xrds1020.xml").AbsoluteUri);
			this.RegisterMockResponse(new Uri("http://localhost/xrds1020.xml"), "application/xrds+xml", LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds1020.xml"));
			await this.DiscoverXrdsAsync("XrdsReferencedInHttpHeader.html", ProtocolVersion.V10, null, "http://a/b", headers);
		}

		/// <summary>
		/// Verifies HTML discovery proceeds if an XRDS document is referenced that doesn't contain OpenID endpoints.
		/// </summary>
		[Test]
		public async Task HtmlDiscoveryProceedsIfXrdsIsEmpty() {
			this.RegisterMockResponse(new Uri("http://localhost/xrds-irrelevant.xml"), "application/xrds+xml", LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds-irrelevant.xml"));
			await this.DiscoverHtmlAsync("html20provWithEmptyXrds", ProtocolVersion.V20, null, "http://a/b");
		}

		/// <summary>
		/// Verifies HTML discovery proceeds if the XRDS that is referenced cannot be found.
		/// </summary>
		[Test]
		public async Task HtmlDiscoveryProceedsIfXrdsIsBadOrMissing() {
			await this.DiscoverHtmlAsync("html20provWithBadXrds", ProtocolVersion.V20, null, "http://a/b");
		}

		/// <summary>
		/// Verifies that a dual identifier yields only one service endpoint by default.
		/// </summary>
		[Test]
		public async Task DualIdentifierOffByDefault() {
			this.RegisterMockResponse(VanityUri, "application/xrds+xml", LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds20dual.xml"));
			var results = (await this.DiscoverAsync(VanityUri)).ToList();
			Assert.AreEqual(1, results.Count(r => r.ClaimedIdentifier == r.Protocol.ClaimedIdentifierForOPIdentifier), "OP Identifier missing from discovery results.");
			Assert.AreEqual(1, results.Count, "Unexpected additional services discovered.");
		}

		/// <summary>
		/// Verifies that a dual identifier yields two service endpoints when that feature is turned on.
		/// </summary>
		[Test]
		public async Task DualIdentifier() {
			this.RegisterMockResponse(VanityUri, "application/xrds+xml", LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds20dual.xml"));
			var rp = this.CreateRelyingParty(true);
			rp.SecuritySettings.AllowDualPurposeIdentifiers = true;
			var results = (await rp.DiscoverAsync(VanityUri, CancellationToken.None)).ToList();
			Assert.AreEqual(1, results.Count(r => r.ClaimedIdentifier == r.Protocol.ClaimedIdentifierForOPIdentifier), "OP Identifier missing from discovery results.");
			Assert.AreEqual(1, results.Count(r => r.ClaimedIdentifier == VanityUri), "Claimed identifier missing from discovery results.");
			Assert.AreEqual(2, results.Count, "Unexpected additional services discovered.");
		}

		private async Task DiscoverAsync(string url, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint, bool expectSreg, bool useRedirect) {
			await this.DiscoverAsync(url, version, expectedLocalId, providerEndpoint, expectSreg, useRedirect, null);
		}

		private string RegisterDiscoveryRedirector(Uri baseUrl) {
			var redirectorUrl = new Uri(baseUrl, "Discovery/htmldiscovery/redirect.aspx");
			this.Handle(redirectorUrl).By(req => {
				string redirectTarget = HttpUtility.ParseQueryString(req.RequestUri.Query)["target"];
				var response = new HttpResponseMessage(HttpStatusCode.Redirect);
				response.Headers.Location = new Uri(redirectTarget, UriKind.RelativeOrAbsolute);
				response.RequestMessage = req;
				return response;
			});

			return redirectorUrl.AbsoluteUri + "?target=";
		}

		private async Task DiscoverAsync(string url, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint, bool expectSreg, bool useRedirect, WebHeaderCollection headers) {
			Protocol protocol = Protocol.Lookup(version);
			Uri baseUrl = new Uri("http://localhost/");
			string redirectBase = this.RegisterDiscoveryRedirector(baseUrl);
			UriIdentifier claimedId = new Uri(baseUrl, url);
			UriIdentifier userSuppliedIdentifier = new Uri(redirectBase + Uri.EscapeDataString(url));
			if (expectedLocalId == null) {
				expectedLocalId = claimedId;
			}
			Identifier idToDiscover = useRedirect ? userSuppliedIdentifier : claimedId;

			string contentType;
			if (url.EndsWith("html")) {
				contentType = "text/html";
			} else if (url.EndsWith("xml")) {
				contentType = "application/xrds+xml";
			} else {
				throw new InvalidOperationException();
			}
			this.RegisterMockResponse(claimedId, claimedId, contentType, headers ?? new WebHeaderCollection(), LoadEmbeddedFile(url));

			IdentifierDiscoveryResult expected = IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				claimedId,
				expectedLocalId,
				new ProviderEndpointDescription(new Uri(providerEndpoint), new string[] { protocol.ClaimedIdentifierServiceTypeURI }), // services aren't checked by Equals
				null,
				null);

			var discoveryResult = await this.DiscoverAsync(idToDiscover);
			IdentifierDiscoveryResult se = discoveryResult.FirstOrDefault(ep => ep.Equals(expected));
			Assert.IsNotNull(se, url + " failed to be discovered.");

			// Do extra checking of service type URIs, which aren't included in 
			// the ServiceEndpoint.Equals method.
			Assert.AreEqual(expectSreg ? 2 : 1, se.Capabilities.Count);
			Assert.IsTrue(se.Capabilities.Contains(protocol.ClaimedIdentifierServiceTypeURI));
			Assert.AreEqual(expectSreg, se.IsExtensionSupported<ClaimsRequest>());
		}

		private async Task DiscoverXrdsAsync(string page, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint) {
			await this.DiscoverXrdsAsync(page, version, expectedLocalId, providerEndpoint, null);
		}

		private async Task DiscoverXrdsAsync(string page, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint, WebHeaderCollection headers) {
			if (!page.Contains(".")) {
				page += ".xml";
			}
			await this.DiscoverAsync("/Discovery/xrdsdiscovery/" + page, version, expectedLocalId, providerEndpoint, true, false, headers);
			await this.DiscoverAsync("/Discovery/xrdsdiscovery/" + page, version, expectedLocalId, providerEndpoint, true, true, headers);
		}

		private async Task DiscoverHtmlAsync(string page, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint, bool useRedirect) {
			await this.DiscoverAsync("/Discovery/htmldiscovery/" + page, version, expectedLocalId, providerEndpoint, false, useRedirect);
		}

		private async Task DiscoverHtmlAsync(string scenario, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint) {
			string page = scenario + ".html";
			await this.DiscoverHtmlAsync(page, version, expectedLocalId, providerEndpoint, false);
			await this.DiscoverHtmlAsync(page, version, expectedLocalId, providerEndpoint, true);
		}

		private async Task FailDiscoverAsync(string url) {
			UriIdentifier userSuppliedId = new Uri(new Uri("http://localhost"), url);

			this.RegisterMockResponse(new Uri(userSuppliedId), userSuppliedId, "text/html", LoadEmbeddedFile(url));

			var discoveryResult = await this.DiscoverAsync(userSuppliedId);
			Assert.AreEqual(0, discoveryResult.Count()); // ... but that no endpoint info is discoverable
		}

		private async Task FailDiscoverHtmlAsync(string scenario) {
			await this.FailDiscoverAsync("/Discovery/htmldiscovery/" + scenario + ".html");
		}

		private async Task FailDiscoverXrdsAsync(string scenario) {
			await this.FailDiscoverAsync("/Discovery/xrdsdiscovery/" + scenario + ".xml");
		}
	}
}
