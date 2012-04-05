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
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class UriDiscoveryServiceTests : OpenIdTestBase {
		[Test]
		public void DiscoveryWithRedirects() {
			Identifier claimedId = this.GetMockIdentifier(ProtocolVersion.V20, false);

			// Add a couple of chained redirect pages that lead to the claimedId.
			Uri userSuppliedUri = new Uri("https://localhost/someSecurePage");
			Uri insecureMidpointUri = new Uri("http://localhost/insecureStop");
			this.MockResponder.RegisterMockRedirect(userSuppliedUri, insecureMidpointUri);
			this.MockResponder.RegisterMockRedirect(insecureMidpointUri, new Uri(claimedId.ToString()));

			// don't require secure SSL discovery for this test.
			Identifier userSuppliedIdentifier = new UriIdentifier(userSuppliedUri, false);
			Assert.AreEqual(1, this.Discover(userSuppliedIdentifier).Count());
		}

		[Test]
		public void DiscoverRequireSslWithSecureRedirects() {
			Identifier claimedId = this.GetMockIdentifier(ProtocolVersion.V20, true);

			// Add a couple of chained redirect pages that lead to the claimedId.
			// All redirects should be secure.
			Uri userSuppliedUri = new Uri("https://localhost/someSecurePage");
			Uri secureMidpointUri = new Uri("https://localhost/secureStop");
			this.MockResponder.RegisterMockRedirect(userSuppliedUri, secureMidpointUri);
			this.MockResponder.RegisterMockRedirect(secureMidpointUri, new Uri(claimedId.ToString()));

			Identifier userSuppliedIdentifier = new UriIdentifier(userSuppliedUri, true);
			Assert.AreEqual(1, this.Discover(userSuppliedIdentifier).Count());
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public void DiscoverRequireSslWithInsecureRedirect() {
			Identifier claimedId = this.GetMockIdentifier(ProtocolVersion.V20, true);

			// Add a couple of chained redirect pages that lead to the claimedId.
			// Include an insecure HTTP jump in those redirects to verify that
			// the ultimate endpoint is never found as a result of high security profile.
			Uri userSuppliedUri = new Uri("https://localhost/someSecurePage");
			Uri insecureMidpointUri = new Uri("http://localhost/insecureStop");
			this.MockResponder.RegisterMockRedirect(userSuppliedUri, insecureMidpointUri);
			this.MockResponder.RegisterMockRedirect(insecureMidpointUri, new Uri(claimedId.ToString()));

			Identifier userSuppliedIdentifier = new UriIdentifier(userSuppliedUri, true);
			this.Discover(userSuppliedIdentifier);
		}

		[Test]
		public void DiscoveryRequireSslWithInsecureXrdsInSecureHtmlHead() {
			var insecureXrdsSource = this.GetMockIdentifier(ProtocolVersion.V20, false);
			Uri secureClaimedUri = new Uri("https://localhost/secureId");

			string html = string.Format("<html><head><meta http-equiv='X-XRDS-Location' content='{0}'/></head><body></body></html>", insecureXrdsSource);
			this.MockResponder.RegisterMockResponse(secureClaimedUri, "text/html", html);

			Identifier userSuppliedIdentifier = new UriIdentifier(secureClaimedUri, true);
			Assert.AreEqual(0, this.Discover(userSuppliedIdentifier).Count());
		}

		[Test]
		public void DiscoveryRequireSslWithInsecureXrdsInSecureHttpHeader() {
			var insecureXrdsSource = this.GetMockIdentifier(ProtocolVersion.V20, false);

			string html = "<html><head></head><body></body></html>";
			WebHeaderCollection headers = new WebHeaderCollection {
				{ "X-XRDS-Location", insecureXrdsSource }
			};
			this.MockResponder.RegisterMockResponse(VanityUriSsl, VanityUriSsl, "text/html", headers, html);

			Identifier userSuppliedIdentifier = new UriIdentifier(VanityUriSsl, true);
			Assert.AreEqual(0, this.Discover(userSuppliedIdentifier).Count());
		}

		[Test]
		public void DiscoveryRequireSslWithInsecureXrdsButSecureLinkTags() {
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
			this.MockResponder.RegisterMockResponse(VanityUriSsl, "text/html", html);

			Identifier userSuppliedIdentifier = new UriIdentifier(VanityUriSsl, true);

			// We verify that the XRDS was ignored and the LINK tags were used
			// because the XRDS OP-LocalIdentifier uses different local identifiers.
			Assert.AreEqual(OPLocalIdentifiersSsl[1].AbsoluteUri, this.Discover(userSuppliedIdentifier).Single().ProviderLocalIdentifier.ToString());
		}

		[Test]
		public void DiscoveryRequiresSslIgnoresInsecureEndpointsInXrds() {
			var insecureEndpoint = GetServiceEndpoint(0, ProtocolVersion.V20, 10, false);
			var secureEndpoint = GetServiceEndpoint(1, ProtocolVersion.V20, 20, true);
			UriIdentifier secureClaimedId = new UriIdentifier(VanityUriSsl, true);
			this.MockResponder.RegisterMockXrdsResponse(secureClaimedId, new IdentifierDiscoveryResult[] { insecureEndpoint, secureEndpoint });
			Assert.AreEqual(secureEndpoint.ProviderLocalIdentifier, this.Discover(secureClaimedId).Single().ProviderLocalIdentifier);
		}

		[Test]
		public void XrdsDirectDiscovery_10() {
			this.FailDiscoverXrds("xrds-irrelevant");
			this.DiscoverXrds("xrds10", ProtocolVersion.V10, null, "http://a/b");
			this.DiscoverXrds("xrds11", ProtocolVersion.V11, null, "http://a/b");
			this.DiscoverXrds("xrds1020", ProtocolVersion.V10, null, "http://a/b");
		}

		[Test]
		public void XrdsDirectDiscovery_20() {
			this.DiscoverXrds("xrds20", ProtocolVersion.V20, null, "http://a/b");
			this.DiscoverXrds("xrds2010a", ProtocolVersion.V20, null, "http://a/b");
			this.DiscoverXrds("xrds2010b", ProtocolVersion.V20, null, "http://a/b");
		}

		[Test]
		public void HtmlDiscover_11() {
			this.DiscoverHtml("html10prov", ProtocolVersion.V11, null, "http://a/b");
			this.DiscoverHtml("html10both", ProtocolVersion.V11, "http://c/d", "http://a/b");
			this.FailDiscoverHtml("html10del");

			// Verify that HTML discovery generates the 1.x endpoints when appropriate
			this.DiscoverHtml("html2010", ProtocolVersion.V11, "http://g/h", "http://e/f");
			this.DiscoverHtml("html1020", ProtocolVersion.V11, "http://g/h", "http://e/f");
			this.DiscoverHtml("html2010combinedA", ProtocolVersion.V11, "http://c/d", "http://a/b");
			this.DiscoverHtml("html2010combinedB", ProtocolVersion.V11, "http://c/d", "http://a/b");
			this.DiscoverHtml("html2010combinedC", ProtocolVersion.V11, "http://c/d", "http://a/b");
		}

		[Test]
		public void HtmlDiscover_20() {
			this.DiscoverHtml("html20prov", ProtocolVersion.V20, null, "http://a/b");
			this.DiscoverHtml("html20both", ProtocolVersion.V20, "http://c/d", "http://a/b");
			this.FailDiscoverHtml("html20del");
			this.DiscoverHtml("html2010", ProtocolVersion.V20, "http://c/d", "http://a/b");
			this.DiscoverHtml("html1020", ProtocolVersion.V20, "http://c/d", "http://a/b");
			this.DiscoverHtml("html2010combinedA", ProtocolVersion.V20, "http://c/d", "http://a/b");
			this.DiscoverHtml("html2010combinedB", ProtocolVersion.V20, "http://c/d", "http://a/b");
			this.DiscoverHtml("html2010combinedC", ProtocolVersion.V20, "http://c/d", "http://a/b");
			this.FailDiscoverHtml("html20relative");
		}

		[Test]
		public void XrdsDiscoveryFromHead() {
			this.MockResponder.RegisterMockResponse(new Uri("http://localhost/xrds1020.xml"), "application/xrds+xml", LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds1020.xml"));
			this.DiscoverXrds("XrdsReferencedInHead.html", ProtocolVersion.V10, null, "http://a/b");
		}

		[Test]
		public void XrdsDiscoveryFromHttpHeader() {
			WebHeaderCollection headers = new WebHeaderCollection();
			headers.Add("X-XRDS-Location", new Uri("http://localhost/xrds1020.xml").AbsoluteUri);
			this.MockResponder.RegisterMockResponse(new Uri("http://localhost/xrds1020.xml"), "application/xrds+xml", LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds1020.xml"));
			this.DiscoverXrds("XrdsReferencedInHttpHeader.html", ProtocolVersion.V10, null, "http://a/b", headers);
		}

		/// <summary>
		/// Verifies HTML discovery proceeds if an XRDS document is referenced that doesn't contain OpenID endpoints.
		/// </summary>
		[Test]
		public void HtmlDiscoveryProceedsIfXrdsIsEmpty() {
			this.MockResponder.RegisterMockResponse(new Uri("http://localhost/xrds-irrelevant.xml"), "application/xrds+xml", LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds-irrelevant.xml"));
			this.DiscoverHtml("html20provWithEmptyXrds", ProtocolVersion.V20, null, "http://a/b");
		}

		/// <summary>
		/// Verifies HTML discovery proceeds if the XRDS that is referenced cannot be found.
		/// </summary>
		[Test]
		public void HtmlDiscoveryProceedsIfXrdsIsBadOrMissing() {
			this.DiscoverHtml("html20provWithBadXrds", ProtocolVersion.V20, null, "http://a/b");
		}

		/// <summary>
		/// Verifies that a dual identifier yields only one service endpoint by default.
		/// </summary>
		[Test]
		public void DualIdentifierOffByDefault() {
			this.MockResponder.RegisterMockResponse(VanityUri, "application/xrds+xml", LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds20dual.xml"));
			var results = this.Discover(VanityUri).ToList();
			Assert.AreEqual(1, results.Count(r => r.ClaimedIdentifier == r.Protocol.ClaimedIdentifierForOPIdentifier), "OP Identifier missing from discovery results.");
			Assert.AreEqual(1, results.Count, "Unexpected additional services discovered.");
		}

		/// <summary>
		/// Verifies that a dual identifier yields two service endpoints when that feature is turned on.
		/// </summary>
		[Test]
		public void DualIdentifier() {
			this.MockResponder.RegisterMockResponse(VanityUri, "application/xrds+xml", LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds20dual.xml"));
			var rp = this.CreateRelyingParty(true);
			rp.Channel.WebRequestHandler = this.RequestHandler;
			rp.SecuritySettings.AllowDualPurposeIdentifiers = true;
			var results = rp.Discover(VanityUri).ToList();
			Assert.AreEqual(1, results.Count(r => r.ClaimedIdentifier == r.Protocol.ClaimedIdentifierForOPIdentifier), "OP Identifier missing from discovery results.");
			Assert.AreEqual(1, results.Count(r => r.ClaimedIdentifier == VanityUri), "Claimed identifier missing from discovery results.");
			Assert.AreEqual(2, results.Count, "Unexpected additional services discovered.");
		}

		private void Discover(string url, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint, bool expectSreg, bool useRedirect) {
			this.Discover(url, version, expectedLocalId, providerEndpoint, expectSreg, useRedirect, null);
		}

		private void Discover(string url, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint, bool expectSreg, bool useRedirect, WebHeaderCollection headers) {
			Protocol protocol = Protocol.Lookup(version);
			Uri baseUrl = new Uri("http://localhost/");
			UriIdentifier claimedId = new Uri(baseUrl, url);
			UriIdentifier userSuppliedIdentifier = new Uri(baseUrl, "Discovery/htmldiscovery/redirect.aspx?target=" + url);
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
			this.MockResponder.RegisterMockResponse(new Uri(idToDiscover), claimedId, contentType, headers ?? new WebHeaderCollection(), LoadEmbeddedFile(url));

			IdentifierDiscoveryResult expected = IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				claimedId,
				expectedLocalId,
				new ProviderEndpointDescription(new Uri(providerEndpoint), new string[] { protocol.ClaimedIdentifierServiceTypeURI }), // services aren't checked by Equals
				null,
				null);

			IdentifierDiscoveryResult se = this.Discover(idToDiscover).FirstOrDefault(ep => ep.Equals(expected));
			Assert.IsNotNull(se, url + " failed to be discovered.");

			// Do extra checking of service type URIs, which aren't included in 
			// the ServiceEndpoint.Equals method.
			Assert.AreEqual(expectSreg ? 2 : 1, se.Capabilities.Count);
			Assert.IsTrue(se.Capabilities.Contains(protocol.ClaimedIdentifierServiceTypeURI));
			Assert.AreEqual(expectSreg, se.IsExtensionSupported<ClaimsRequest>());
		}

		private void DiscoverXrds(string page, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint) {
			this.DiscoverXrds(page, version, expectedLocalId, providerEndpoint, null);
		}

		private void DiscoverXrds(string page, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint, WebHeaderCollection headers) {
			if (!page.Contains(".")) {
				page += ".xml";
			}
			this.Discover("/Discovery/xrdsdiscovery/" + page, version, expectedLocalId, providerEndpoint, true, false, headers);
			this.Discover("/Discovery/xrdsdiscovery/" + page, version, expectedLocalId, providerEndpoint, true, true, headers);
		}

		private void DiscoverHtml(string page, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint, bool useRedirect) {
			this.Discover("/Discovery/htmldiscovery/" + page, version, expectedLocalId, providerEndpoint, false, useRedirect);
		}

		private void DiscoverHtml(string scenario, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint) {
			string page = scenario + ".html";
			this.DiscoverHtml(page, version, expectedLocalId, providerEndpoint, false);
			this.DiscoverHtml(page, version, expectedLocalId, providerEndpoint, true);
		}

		private void FailDiscover(string url) {
			UriIdentifier userSuppliedId = new Uri(new Uri("http://localhost"), url);

			this.MockResponder.RegisterMockResponse(new Uri(userSuppliedId), userSuppliedId, "text/html", LoadEmbeddedFile(url));

			Assert.AreEqual(0, this.Discover(userSuppliedId).Count()); // ... but that no endpoint info is discoverable
		}

		private void FailDiscoverHtml(string scenario) {
			this.FailDiscover("/Discovery/htmldiscovery/" + scenario + ".html");
		}

		private void FailDiscoverXrds(string scenario) {
			this.FailDiscover("/Discovery/xrdsdiscovery/" + scenario + ".xml");
		}
	}
}
