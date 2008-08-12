using System;
using System.Linq;
using System.Net;
using DotNetOpenId.Extensions.SimpleRegistration;
using DotNetOpenId.RelyingParty;
using NUnit.Framework;
using DotNetOpenId.Test.Mocks;

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

		void discover(string url, ProtocolVersion version, Identifier expectedLocalId, bool expectSreg, bool useRedirect) {
			discover(url, version, expectedLocalId, expectSreg, useRedirect, null);
		}
		void discover(string url, ProtocolVersion version, Identifier expectedLocalId, bool expectSreg, bool useRedirect, WebHeaderCollection headers) {
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
			Mocks.MockHttpRequest.RegisterMockResponse(new Uri(idToDiscover), claimedId, contentType,
				headers ?? new WebHeaderCollection(), TestSupport.LoadEmbeddedFile(url));

			ServiceEndpoint se = idToDiscover.Discover().FirstOrDefault();
			Assert.IsNotNull(se, url + " failed to be discovered.");
			Assert.AreSame(protocol, se.Protocol);
			Assert.AreEqual(claimedId, se.ClaimedIdentifier);
			Assert.AreEqual(expectedLocalId, se.ProviderLocalIdentifier);
			Assert.AreEqual(expectSreg ? 2 : 1, se.ProviderSupportedServiceTypeUris.Length);
			Assert.IsTrue(Array.IndexOf(se.ProviderSupportedServiceTypeUris, protocol.ClaimedIdentifierServiceTypeURI) >= 0);
			Assert.AreEqual(expectSreg, se.IsExtensionSupported(new ClaimsRequest()));
		}
		void discoverXrds(string page, ProtocolVersion version, Identifier expectedLocalId) {
			discoverXrds(page, version, expectedLocalId, null);
		}
		void discoverXrds(string page, ProtocolVersion version, Identifier expectedLocalId, WebHeaderCollection headers) {
			if (!page.Contains(".")) page += ".xml";
			discover("/Discovery/xrdsdiscovery/" + page, version, expectedLocalId, true, false, headers);
			discover("/Discovery/xrdsdiscovery/" + page, version, expectedLocalId, true, true, headers);
		}
		void discoverHtml(string page, ProtocolVersion version, Identifier expectedLocalId, bool useRedirect) {
			discover("/Discovery/htmldiscovery/" + page, version, expectedLocalId, false, useRedirect);
		}
		void discoverHtml(string scenario, ProtocolVersion version, Identifier expectedLocalId) {
			string page = scenario + ".html";
			discoverHtml(page, version, expectedLocalId, false);
			discoverHtml(page, version, expectedLocalId, true);
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
			discoverHtml("html10prov", ProtocolVersion.V11, null);
			discoverHtml("html10both", ProtocolVersion.V11, "http://c/d");
			failDiscoverHtml("html10del");
		}
		[Test]
		public void HtmlDiscover_20() {
			discoverHtml("html20prov", ProtocolVersion.V20, null);
			discoverHtml("html20both", ProtocolVersion.V20, "http://c/d");
			failDiscoverHtml("html20del");
			discoverHtml("html2010", ProtocolVersion.V20, "http://c/d");
			discoverHtml("html1020", ProtocolVersion.V20, "http://c/d");
			discoverHtml("html2010combinedA", ProtocolVersion.V20, "http://c/d");
			discoverHtml("html2010combinedB", ProtocolVersion.V20, "http://c/d");
			discoverHtml("html2010combinedC", ProtocolVersion.V20, "http://c/d");
			failDiscoverHtml("html20relative");
		}
		[Test]
		public void XrdsDiscoveryFromHead() {
			Mocks.MockHttpRequest.RegisterMockResponse(new Uri("http://localhost/xrds1020.xml"),
				"application/xrds+xml", TestSupport.LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds1020.xml"));
			discoverXrds("XrdsReferencedInHead.html", ProtocolVersion.V10, null);
		}
		[Test]
		public void XrdsDiscoveryFromHttpHeader() {
			WebHeaderCollection headers = new WebHeaderCollection();
			headers.Add("X-XRDS-Location", TestSupport.GetFullUrl("http://localhost/xrds1020.xml").AbsoluteUri);
			Mocks.MockHttpRequest.RegisterMockResponse(new Uri("http://localhost/xrds1020.xml"),
				"application/xrds+xml", TestSupport.LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds1020.xml"));
			discoverXrds("XrdsReferencedInHttpHeader.html", ProtocolVersion.V10, null, headers);
		}
		[Test]
		public void XrdsDirectDiscovery_10() {
			failDiscoverXrds("xrds-irrelevant");
			discoverXrds("xrds10", ProtocolVersion.V10, null);
			discoverXrds("xrds11", ProtocolVersion.V11, null);
			discoverXrds("xrds1020", ProtocolVersion.V10, null);
		}
		[Test]
		public void XrdsDirectDiscovery_20() {
			discoverXrds("xrds20", ProtocolVersion.V20, null);
			discoverXrds("xrds2010a", ProtocolVersion.V20, null);
			discoverXrds("xrds2010b", ProtocolVersion.V20, null);
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
			Assert.IsFalse(secureId.IsDiscoverySecureEndToEnd);
			Assert.AreEqual("http://www.yahoo.com/", secureId.ToString());
			Assert.AreEqual(0, secureId.Discover().Count());

			id = new UriIdentifier("http://www.yahoo.com");
			Assert.IsFalse(id.TryRequireSsl(out secureId));
			Assert.IsFalse(secureId.IsDiscoverySecureEndToEnd);
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
	}
}
