using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using DotNetOpenId.Extensions.SimpleRegistration;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Test.Mocks;
using NUnit.Framework;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class UriIdentifierTests {
		string goodUri = "http://blog.nerdbank.net/";
		string relativeUri = "host/path";
		string badUri = "som%-)830w8vf/?.<>,ewackedURI";

		[SetUp]
		public void SetUp() {
			if (!UntrustedWebRequest.WhitelistHosts.Contains("localhost"))
				UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		[TearDown]
		public void TearDown() {
			Mocks.MockHttpRequest.Reset();
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullUri() {
			new UriIdentifier((Uri)null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullString() {
			new UriIdentifier((string)null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorBlank() {
			new UriIdentifier(string.Empty);
		}

		[Test, ExpectedException(typeof(UriFormatException))]
		public void CtorBadUri() {
			new UriIdentifier(badUri);
		}

		[Test]
		public void CtorGoodUri() {
			var uri = new UriIdentifier(goodUri);
			Assert.AreEqual(new Uri(goodUri), uri.Uri);
			Assert.IsFalse(uri.SchemeImplicitlyPrepended);
			Assert.IsFalse(uri.IsDiscoverySecureEndToEnd);
		}

		[Test]
		public void CtorStringNoSchemeSecure() {
			var uri = new UriIdentifier("host/path", true);
			Assert.AreEqual("https://host/path", uri.Uri.AbsoluteUri);
			Assert.IsTrue(uri.IsDiscoverySecureEndToEnd);
		}

		[Test]
		public void CtorStringHttpsSchemeSecure() {
			var uri = new UriIdentifier("https://host/path", true);
			Assert.AreEqual("https://host/path", uri.Uri.AbsoluteUri);
			Assert.IsTrue(uri.IsDiscoverySecureEndToEnd);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void CtorStringHttpSchemeSecure() {
			new UriIdentifier("http://host/path", true);
		}

		[Test]
		public void CtorUriHttpsSchemeSecure() {
			var uri = new UriIdentifier(new Uri("https://host/path"), true);
			Assert.AreEqual("https://host/path", uri.Uri.AbsoluteUri);
			Assert.IsTrue(uri.IsDiscoverySecureEndToEnd);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void CtorUriHttpSchemeSecure() {
			new UriIdentifier(new Uri("http://host/path"), true);
		}

		/// <summary>
		/// Verifies that the fragment is not stripped from an Identifier.
		/// </summary>
		/// <remarks>
		/// Although fragments should be stripped from user supplied identifiers, 
		/// they should NOT be stripped from claimed identifiers.  So the UriIdentifier
		/// class, which serves both identifier types, must not do the stripping.
		/// </remarks>
		[Test]
		public void DoesNotStripFragment() {
			Uri original = new Uri("http://a/b#c");
			UriIdentifier identifier = new UriIdentifier(original);
			Assert.AreEqual(original.Fragment, identifier.Uri.Fragment);
		}

		[Test]
		public void IsValid() {
			Assert.IsTrue(UriIdentifier.IsValidUri(goodUri));
			Assert.IsFalse(UriIdentifier.IsValidUri(badUri));
			Assert.IsTrue(UriIdentifier.IsValidUri(relativeUri), "URL lacking http:// prefix should have worked anyway.");
		}

		[Test]
		public void TrimFragment() {
			Identifier noFragment = UriIdentifier.Parse("http://a/b");
			Identifier fragment = UriIdentifier.Parse("http://a/b#c");
			Assert.AreSame(noFragment, noFragment.TrimFragment());
			Assert.AreEqual(noFragment, fragment.TrimFragment());
		}

		[Test]
		public void ToStringTest() {
			Assert.AreEqual(goodUri, new UriIdentifier(goodUri).ToString());
		}

		[Test]
		public void EqualsTest() {
			Assert.AreEqual(new UriIdentifier(goodUri), new UriIdentifier(goodUri));
			// This next test is an interesting side-effect of passing off to Uri.Equals.  But it's probably ok.
			Assert.AreEqual(new UriIdentifier(goodUri), new UriIdentifier(goodUri + "#frag"));
			Assert.AreNotEqual(new UriIdentifier(goodUri), new UriIdentifier(goodUri + "a"));
			Assert.AreNotEqual(null, new UriIdentifier(goodUri));
			Assert.AreNotEqual(goodUri, new UriIdentifier(goodUri));
		}

		[Test]
		public void UnicodeTest() {
			string unicodeUrl = "http://nerdbank.org/opaffirmative/崎村.aspx";
			Assert.IsTrue(UriIdentifier.IsValidUri(unicodeUrl));
			Identifier id;
			Assert.IsTrue(UriIdentifier.TryParse(unicodeUrl, out id));
			Assert.AreEqual("/opaffirmative/%E5%B4%8E%E6%9D%91.aspx", ((UriIdentifier)id).Uri.AbsolutePath);
			Assert.AreEqual(Uri.EscapeUriString(unicodeUrl), id.ToString());
		}

		void discover(string url, ProtocolVersion version, Identifier expectedLocalId, Uri providerEndpoint, bool expectSreg, bool useRedirect) {
			discover(url, version, expectedLocalId, providerEndpoint, expectSreg, useRedirect, null);
		}
		void discover(string url, ProtocolVersion version, Identifier expectedLocalId, Uri providerEndpoint, bool expectSreg, bool useRedirect, WebHeaderCollection headers) {
			Protocol protocol = Protocol.Lookup(version);
			UriIdentifier claimedId = TestSupport.GetFullUrl(url);
			UriIdentifier userSuppliedIdentifier = TestSupport.GetFullUrl(
				"Discovery/htmldiscovery/redirect.aspx?target=" + url);
			if (expectedLocalId == null) expectedLocalId = claimedId;
			Identifier idToDiscover = useRedirect ? userSuppliedIdentifier : claimedId;

			string contentType;
			if (url.EndsWith("html")) {
				contentType = "text/html";
			} else if (url.EndsWith("xml")) {
				contentType = "application/xrds+xml";
			} else {
				throw new InvalidOperationException();
			}
			MockHttpRequest.RegisterMockResponse(new Uri(idToDiscover), claimedId, contentType,
				headers ?? new WebHeaderCollection(), TestSupport.LoadEmbeddedFile(url));

			ServiceEndpoint expected = ServiceEndpoint.CreateForClaimedIdentifier(
				claimedId,
				expectedLocalId,
				providerEndpoint,
				new string[] { protocol.ClaimedIdentifierServiceTypeURI }, // this isn't checked by Equals
				null,
				null);

			ServiceEndpoint se = idToDiscover.Discover().FirstOrDefault(ep => ep.Equals(expected));
			Assert.IsNotNull(se, url + " failed to be discovered.");

			// Do extra checking of service type URIs, which aren't included in 
			// the ServiceEndpoint.Equals method.
			Assert.AreEqual(expectSreg ? 2 : 1, se.ProviderSupportedServiceTypeUris.Length);
			Assert.IsTrue(Array.IndexOf(se.ProviderSupportedServiceTypeUris, protocol.ClaimedIdentifierServiceTypeURI) >= 0);
			Assert.AreEqual(expectSreg, se.IsExtensionSupported(new ClaimsRequest()));
		}
		void discoverXrds(string page, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint) {
			discoverXrds(page, version, expectedLocalId, providerEndpoint, null);
		}
		void discoverXrds(string page, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint, WebHeaderCollection headers) {
			if (!page.Contains(".")) page += ".xml";
			discover("/Discovery/xrdsdiscovery/" + page, version, expectedLocalId, new Uri(providerEndpoint), true, false, headers);
			discover("/Discovery/xrdsdiscovery/" + page, version, expectedLocalId, new Uri(providerEndpoint), true, true, headers);
		}
		void discoverHtml(string page, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint, bool useRedirect) {
			discover("/Discovery/htmldiscovery/" + page, version, expectedLocalId, new Uri(providerEndpoint), false, useRedirect);
		}
		void discoverHtml(string scenario, ProtocolVersion version, Identifier expectedLocalId, string providerEndpoint) {
			string page = scenario + ".html";
			discoverHtml(page, version, expectedLocalId, providerEndpoint, false);
			discoverHtml(page, version, expectedLocalId, providerEndpoint, true);
		}
		void failDiscover(string url) {
			UriIdentifier userSuppliedId = TestSupport.GetFullUrl(url);

			Mocks.MockHttpRequest.RegisterMockResponse(new Uri(userSuppliedId), userSuppliedId, "text/html",
				TestSupport.LoadEmbeddedFile(url));

			Assert.AreEqual(0, userSuppliedId.Discover().Count()); // ... but that no endpoint info is discoverable
		}
		void failDiscoverHtml(string scenario) {
			failDiscover("/Discovery/htmldiscovery/" + scenario + ".html");
		}
		void failDiscoverXrds(string scenario) {
			failDiscover("/Discovery/xrdsdiscovery/" + scenario + ".xml");
		}
		[Test]
		public void HtmlDiscover_11() {
			discoverHtml("html10prov", ProtocolVersion.V11, null, "http://a/b");
			discoverHtml("html10both", ProtocolVersion.V11, "http://c/d", "http://a/b");
			failDiscoverHtml("html10del");

			// Verify that HTML discovery generates the 1.x endpoints when appropriate
			discoverHtml("html2010", ProtocolVersion.V11, "http://g/h", "http://e/f");
			discoverHtml("html1020", ProtocolVersion.V11, "http://g/h", "http://e/f");
			discoverHtml("html2010combinedA", ProtocolVersion.V11, "http://c/d", "http://a/b");
			discoverHtml("html2010combinedB", ProtocolVersion.V11, "http://c/d", "http://a/b");
			discoverHtml("html2010combinedC", ProtocolVersion.V11, "http://c/d", "http://a/b");
		}
		[Test]
		public void HtmlDiscover_20() {
			discoverHtml("html20prov", ProtocolVersion.V20, null, "http://a/b");
			discoverHtml("html20both", ProtocolVersion.V20, "http://c/d", "http://a/b");
			failDiscoverHtml("html20del");
			discoverHtml("html2010", ProtocolVersion.V20, "http://c/d", "http://a/b");
			discoverHtml("html1020", ProtocolVersion.V20, "http://c/d", "http://a/b");
			discoverHtml("html2010combinedA", ProtocolVersion.V20, "http://c/d", "http://a/b");
			discoverHtml("html2010combinedB", ProtocolVersion.V20, "http://c/d", "http://a/b");
			discoverHtml("html2010combinedC", ProtocolVersion.V20, "http://c/d", "http://a/b");
			failDiscoverHtml("html20relative");
		}
		[Test]
		public void XrdsDiscoveryFromHead() {
			Mocks.MockHttpRequest.RegisterMockResponse(new Uri("http://localhost/xrds1020.xml"),
				"application/xrds+xml", TestSupport.LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds1020.xml"));
			discoverXrds("XrdsReferencedInHead.html", ProtocolVersion.V10, null, "http://a/b");
		}
		[Test]
		public void XrdsDiscoveryFromHttpHeader() {
			WebHeaderCollection headers = new WebHeaderCollection();
			headers.Add("X-XRDS-Location", TestSupport.GetFullUrl("http://localhost/xrds1020.xml").AbsoluteUri);
			Mocks.MockHttpRequest.RegisterMockResponse(new Uri("http://localhost/xrds1020.xml"),
				"application/xrds+xml", TestSupport.LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds1020.xml"));
			discoverXrds("XrdsReferencedInHttpHeader.html", ProtocolVersion.V10, null, "http://a/b", headers);
		}
		[Test]
		public void XrdsDirectDiscovery_10() {
			failDiscoverXrds("xrds-irrelevant");
			discoverXrds("xrds10", ProtocolVersion.V10, null, "http://a/b");
			discoverXrds("xrds11", ProtocolVersion.V11, null, "http://a/b");
			discoverXrds("xrds1020", ProtocolVersion.V10, null, "http://a/b");
		}
		[Test]
		public void XrdsDirectDiscovery_20() {
			discoverXrds("xrds20", ProtocolVersion.V20, null, "http://a/b");
			discoverXrds("xrds2010a", ProtocolVersion.V20, null, "http://a/b");
			discoverXrds("xrds2010b", ProtocolVersion.V20, null, "http://a/b");
		}

		[Test]
		public void NormalizeCase() {
			// only the host name can be normalized in casing safely.
			Identifier id = "http://HOST:80/PaTH?KeY=VaLUE#fRag";
			Assert.AreEqual("http://host/PaTH?KeY=VaLUE#fRag", id.ToString());
			// make sure https is preserved, along with port 80, which is NON-default for https
			id = "https://HOST:80/PaTH?KeY=VaLUE#fRag";
			Assert.AreEqual("https://host:80/PaTH?KeY=VaLUE#fRag", id.ToString());
		}

		[Test]
		public void HttpSchemePrepended() {
			UriIdentifier id = new UriIdentifier("www.yahoo.com");
			Assert.AreEqual("http://www.yahoo.com/", id.ToString());
			Assert.IsTrue(id.SchemeImplicitlyPrepended);
		}

		//[Test, Ignore("The spec says http:// must be prepended in this case, but that just creates an invalid URI.  Our UntrustedWebRequest will stop disallowed schemes.")]
		public void CtorDisallowedScheme() {
			UriIdentifier id = new UriIdentifier(new Uri("ftp://host/path"));
			Assert.AreEqual("http://ftp://host/path", id.ToString());
			Assert.IsTrue(id.SchemeImplicitlyPrepended);
		}

		[Test]
		public void DiscoveryWithRedirects() {
			MockHttpRequest.Reset();
			Identifier claimedId = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);

			// Add a couple of chained redirect pages that lead to the claimedId.
			Uri userSuppliedUri = TestSupport.GetFullUrl("/someSecurePage", null, true);
			Uri insecureMidpointUri = TestSupport.GetFullUrl("/insecureStop");
			MockHttpRequest.RegisterMockRedirect(userSuppliedUri, insecureMidpointUri);
			MockHttpRequest.RegisterMockRedirect(insecureMidpointUri, new Uri(claimedId.ToString()));

			// don't require secure SSL discovery for this test.
			Identifier userSuppliedIdentifier = new UriIdentifier(userSuppliedUri, false);
			Assert.AreEqual(1, userSuppliedIdentifier.Discover().Count());
		}

		[Test]
		public void TryRequireSslAdjustsIdentifier() {
			Identifier secureId;
			// Try Parse and ctor without explicit scheme
			var id = Identifier.Parse("www.yahoo.com");
			Assert.AreEqual("http://www.yahoo.com/", id.ToString());
			Assert.IsTrue(id.TryRequireSsl(out secureId));
			Assert.IsTrue(secureId.IsDiscoverySecureEndToEnd);
			Assert.AreEqual("https://www.yahoo.com/", secureId.ToString());

			id = new UriIdentifier("www.yahoo.com");
			Assert.AreEqual("http://www.yahoo.com/", id.ToString());
			Assert.IsTrue(id.TryRequireSsl(out secureId));
			Assert.IsTrue(secureId.IsDiscoverySecureEndToEnd);
			Assert.AreEqual("https://www.yahoo.com/", secureId.ToString());

			// Try Parse and ctor with explicit http:// scheme
			id = Identifier.Parse("http://www.yahoo.com");
			Assert.IsFalse(id.TryRequireSsl(out secureId));
			Assert.IsTrue(secureId.IsDiscoverySecureEndToEnd, "Although the TryRequireSsl failed, the created identifier should retain the Ssl status.");
			Assert.AreEqual("http://www.yahoo.com/", secureId.ToString());
			Assert.AreEqual(0, secureId.Discover().Count(), "Since TryRequireSsl failed, the created Identifier should never discover anything.");

			id = new UriIdentifier("http://www.yahoo.com");
			Assert.IsFalse(id.TryRequireSsl(out secureId));
			Assert.IsTrue(secureId.IsDiscoverySecureEndToEnd);
			Assert.AreEqual("http://www.yahoo.com/", secureId.ToString());
			Assert.AreEqual(0, secureId.Discover().Count());
		}

		[Test]
		public void DiscoverRequireSslWithSecureRedirects() {
			MockHttpRequest.Reset();
			Identifier claimedId = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20, true);

			// Add a couple of chained redirect pages that lead to the claimedId.
			// All redirects should be secure.
			Uri userSuppliedUri = TestSupport.GetFullUrl("/someSecurePage", null, true);
			Uri secureMidpointUri = TestSupport.GetFullUrl("/secureStop", null, true);
			MockHttpRequest.RegisterMockRedirect(userSuppliedUri, secureMidpointUri);
			MockHttpRequest.RegisterMockRedirect(secureMidpointUri, new Uri(claimedId.ToString()));

			Identifier userSuppliedIdentifier = new UriIdentifier(userSuppliedUri, true);
			Assert.AreEqual(1, userSuppliedIdentifier.Discover().Count());
		}

		[Test, ExpectedException(typeof(OpenIdException))]
		public void DiscoverRequireSslWithInsecureRedirect() {
			MockHttpRequest.Reset();
			Identifier claimedId = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20, true);

			// Add a couple of chained redirect pages that lead to the claimedId.
			// Include an insecure HTTP jump in those redirects to verify that
			// the ultimate endpoint is never found as a result of high security profile.
			Uri userSuppliedUri = TestSupport.GetFullUrl("/someSecurePage", null, true);
			Uri insecureMidpointUri = TestSupport.GetFullUrl("/insecureStop");
			MockHttpRequest.RegisterMockRedirect(userSuppliedUri, insecureMidpointUri);
			MockHttpRequest.RegisterMockRedirect(insecureMidpointUri, new Uri(claimedId.ToString()));

			Identifier userSuppliedIdentifier = new UriIdentifier(userSuppliedUri, true);
			userSuppliedIdentifier.Discover();
		}

		[Test]
		public void DiscoveryRequireSslWithInsecureXrdsInSecureHtmlHead() {
			var insecureXrdsSource = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20, false);
			Uri secureClaimedUri = TestSupport.GetFullUrl("/secureId", null, true);

			string html = string.Format("<html><head><meta http-equiv='X-XRDS-Location' content='{0}'/></head><body></body></html>",
				insecureXrdsSource);
			MockHttpRequest.RegisterMockResponse(secureClaimedUri, "text/html", html);

			Identifier userSuppliedIdentifier = new UriIdentifier(secureClaimedUri, true);
			Assert.AreEqual(0, userSuppliedIdentifier.Discover().Count());
		}

		[Test]
		public void DiscoveryRequireSslWithInsecureXrdsInSecureHttpHeader() {
			var insecureXrdsSource = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20, false);
			Uri secureClaimedUri = TestSupport.GetFullUrl("/secureId", null, true);

			string html = "<html><head></head><body></body></html>";
			WebHeaderCollection headers = new WebHeaderCollection {
				{ "X-XRDS-Location", insecureXrdsSource }
			};
			MockHttpRequest.RegisterMockResponse(secureClaimedUri, secureClaimedUri, "text/html", headers, html);

			Identifier userSuppliedIdentifier = new UriIdentifier(secureClaimedUri, true);
			Assert.AreEqual(0, userSuppliedIdentifier.Discover().Count());
		}

		[Test]
		public void DiscoveryRequireSslWithInsecureXrdsButSecureLinkTags() {
			var insecureXrdsSource = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20, false);
			Uri secureClaimedUri = TestSupport.GetFullUrl("/secureId", null, true);

			Identifier localIdForLinkTag = TestSupport.GetDelegateUrl(TestSupport.Scenarios.AlwaysDeny, true);
			string html = string.Format(@"
	<html><head>
		<meta http-equiv='X-XRDS-Location' content='{0}'/> <!-- this one will be insecure and ignored -->
		<link rel='openid2.provider' href='{1}' />
		<link rel='openid2.local_id' href='{2}' />
	</head><body></body></html>",
				HttpUtility.HtmlEncode(insecureXrdsSource),
				HttpUtility.HtmlEncode(TestSupport.GetFullUrl("/" + TestSupport.ProviderPage, null, true).AbsoluteUri),
				HttpUtility.HtmlEncode(localIdForLinkTag.ToString())
				);
			MockHttpRequest.RegisterMockResponse(secureClaimedUri, "text/html", html);

			Identifier userSuppliedIdentifier = new UriIdentifier(secureClaimedUri, true);
			Assert.AreEqual(localIdForLinkTag, userSuppliedIdentifier.Discover().Single().ProviderLocalIdentifier);
		}

		[Test]
		public void DiscoveryRequiresSslIgnoresInsecureEndpointsInXrds() {
			var insecureEndpoint = TestSupport.GetServiceEndpoint(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20, 10, false);
			var secureEndpoint = TestSupport.GetServiceEndpoint(TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V20, 20, true);
			UriIdentifier secureClaimedId = new UriIdentifier(TestSupport.GetFullUrl("/claimedId", null, true), true);
			MockHttpRequest.RegisterMockXrdsResponse(secureClaimedId, new ServiceEndpoint[] { insecureEndpoint, secureEndpoint });
			Assert.AreEqual(secureEndpoint.ProviderLocalIdentifier, secureClaimedId.Discover().Single().ProviderLocalIdentifier);
		}
	}
}
