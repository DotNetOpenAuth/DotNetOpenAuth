using System;
using System.Linq;
using System.Net;
using DotNetOpenId.Extensions.SimpleRegistration;
using DotNetOpenId.RelyingParty;
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
	}
}
