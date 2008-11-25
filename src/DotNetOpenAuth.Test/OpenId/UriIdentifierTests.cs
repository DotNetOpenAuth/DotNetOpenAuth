//-----------------------------------------------------------------------
// <copyright file="UriIdentifierTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Linq;
	using System.Net;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class UriIdentifierTests : OpenIdTestBase {
		private string goodUri = "http://blog.nerdbank.net/";
		private string relativeUri = "host/path";
		private string badUri = "som%-)830w8vf/?.<>,ewackedURI";

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullUri() {
			new UriIdentifier((Uri)null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullString() {
			new UriIdentifier((string)null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void CtorBlank() {
			new UriIdentifier(string.Empty);
		}

		[TestMethod, ExpectedException(typeof(UriFormatException))]
		public void CtorBadUri() {
			new UriIdentifier(this.badUri);
		}

		[TestMethod]
		public void CtorGoodUri() {
			var uri = new UriIdentifier(this.goodUri);
			Assert.AreEqual(new Uri(this.goodUri), uri.Uri);
			Assert.IsFalse(uri.SchemeImplicitlyPrepended);
			Assert.IsFalse(uri.IsDiscoverySecureEndToEnd);
		}

		[TestMethod]
		public void CtorStringNoSchemeSecure() {
			var uri = new UriIdentifier("host/path", true);
			Assert.AreEqual("https://host/path", uri.Uri.AbsoluteUri);
			Assert.IsTrue(uri.IsDiscoverySecureEndToEnd);
		}

		[TestMethod]
		public void CtorStringHttpsSchemeSecure() {
			var uri = new UriIdentifier("https://host/path", true);
			Assert.AreEqual("https://host/path", uri.Uri.AbsoluteUri);
			Assert.IsTrue(uri.IsDiscoverySecureEndToEnd);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void CtorStringHttpSchemeSecure() {
			new UriIdentifier("http://host/path", true);
		}

		[TestMethod]
		public void CtorUriHttpsSchemeSecure() {
			var uri = new UriIdentifier(new Uri("https://host/path"), true);
			Assert.AreEqual("https://host/path", uri.Uri.AbsoluteUri);
			Assert.IsTrue(uri.IsDiscoverySecureEndToEnd);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
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
		[TestMethod]
		public void DoesNotStripFragment() {
			Uri original = new Uri("http://a/b#c");
			UriIdentifier identifier = new UriIdentifier(original);
			Assert.AreEqual(original.Fragment, identifier.Uri.Fragment);
		}

		[TestMethod]
		public void IsValid() {
			Assert.IsTrue(UriIdentifier.IsValidUri(this.goodUri));
			Assert.IsFalse(UriIdentifier.IsValidUri(this.badUri));
			Assert.IsTrue(UriIdentifier.IsValidUri(relativeUri), "URL lacking http:// prefix should have worked anyway.");
		}

		[TestMethod]
		public void TrimFragment() {
			Identifier noFragment = UriIdentifier.Parse("http://a/b");
			Identifier fragment = UriIdentifier.Parse("http://a/b#c");
			Assert.AreSame(noFragment, noFragment.TrimFragment());
			Assert.AreEqual(noFragment, fragment.TrimFragment());
		}

		[TestMethod]
		public void ToStringTest() {
			Assert.AreEqual(this.goodUri, new UriIdentifier(this.goodUri).ToString());
		}

		[TestMethod]
		public void EqualsTest() {
			Assert.AreEqual(new UriIdentifier(this.goodUri), new UriIdentifier(this.goodUri));
			// This next test is an interesting side-effect of passing off to Uri.Equals.  But it's probably ok.
			Assert.AreEqual(new UriIdentifier(this.goodUri), new UriIdentifier(this.goodUri + "#frag"));
			Assert.AreNotEqual(new UriIdentifier(this.goodUri), new UriIdentifier(this.goodUri + "a"));
			Assert.AreNotEqual(null, new UriIdentifier(this.goodUri));
			Assert.AreEqual(this.goodUri, new UriIdentifier(this.goodUri));
		}

		[TestMethod]
		public void UnicodeTest() {
			string unicodeUrl = "http://nerdbank.org/opaffirmative/崎村.aspx";
			Assert.IsTrue(UriIdentifier.IsValidUri(unicodeUrl));
			Identifier id;
			Assert.IsTrue(UriIdentifier.TryParse(unicodeUrl, out id));
			Assert.AreEqual("/opaffirmative/%E5%B4%8E%E6%9D%91.aspx", ((UriIdentifier)id).Uri.AbsolutePath);
			Assert.AreEqual(Uri.EscapeUriString(unicodeUrl), id.ToString());
		}

		[TestMethod]
		public void HtmlDiscover_11() {
			this.DiscoverHtml("html10prov", ProtocolVersion.V11, null);
			this.DiscoverHtml("html10both", ProtocolVersion.V11, "http://c/d");
			this.FailDiscoverHtml("html10del");
		}

		[TestMethod]
		public void HtmlDiscover_20() {
			this.DiscoverHtml("html20prov", ProtocolVersion.V20, null);
			this.DiscoverHtml("html20both", ProtocolVersion.V20, "http://c/d");
			this.FailDiscoverHtml("html20del");
			this.DiscoverHtml("html2010", ProtocolVersion.V20, "http://c/d");
			this.DiscoverHtml("html1020", ProtocolVersion.V20, "http://c/d");
			this.DiscoverHtml("html2010combinedA", ProtocolVersion.V20, "http://c/d");
			this.DiscoverHtml("html2010combinedB", ProtocolVersion.V20, "http://c/d");
			this.DiscoverHtml("html2010combinedC", ProtocolVersion.V20, "http://c/d");
			this.FailDiscoverHtml("html20relative");
		}

		[TestMethod]
		public void XrdsDiscoveryFromHead() {
			this.mockResponder.RegisterMockResponse(new Uri("http://localhost/xrds1020.xml"),
				"application/xrds+xml", TestSupport.LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds1020.xml"));
			this.DiscoverXrds("XrdsReferencedInHead.html", ProtocolVersion.V10, null);
		}

		[TestMethod]
		public void XrdsDiscoveryFromHttpHeader() {
			WebHeaderCollection headers = new WebHeaderCollection();
			headers.Add("X-XRDS-Location", TestSupport.GetFullUrl("http://localhost/xrds1020.xml").AbsoluteUri);
			this.mockResponder.RegisterMockResponse(new Uri("http://localhost/xrds1020.xml"), "application/xrds+xml", TestSupport.LoadEmbeddedFile("/Discovery/xrdsdiscovery/xrds1020.xml"));
			this.DiscoverXrds("XrdsReferencedInHttpHeader.html", ProtocolVersion.V10, null, headers);
		}

		[TestMethod]
		public void XrdsDirectDiscovery_10() {
			this.FailDiscoverXrds("xrds-irrelevant");
			this.DiscoverXrds("xrds10", ProtocolVersion.V10, null);
			this.DiscoverXrds("xrds11", ProtocolVersion.V11, null);
			this.DiscoverXrds("xrds1020", ProtocolVersion.V10, null);
		}

		[TestMethod]
		public void XrdsDirectDiscovery_20() {
			this.DiscoverXrds("xrds20", ProtocolVersion.V20, null);
			this.DiscoverXrds("xrds2010a", ProtocolVersion.V20, null);
			this.DiscoverXrds("xrds2010b", ProtocolVersion.V20, null);
		}

		[TestMethod]
		public void NormalizeCase() {
			// only the host name can be normalized in casing safely.
			Identifier id = "http://HOST:80/PaTH?KeY=VaLUE#fRag";
			Assert.AreEqual("http://host/PaTH?KeY=VaLUE#fRag", id.ToString());
			// make sure https is preserved, along with port 80, which is NON-default for https
			id = "https://HOST:80/PaTH?KeY=VaLUE#fRag";
			Assert.AreEqual("https://host:80/PaTH?KeY=VaLUE#fRag", id.ToString());
		}

		[TestMethod]
		public void HttpSchemePrepended() {
			UriIdentifier id = new UriIdentifier("www.yahoo.com");
			Assert.AreEqual("http://www.yahoo.com/", id.ToString());
			Assert.IsTrue(id.SchemeImplicitlyPrepended);
		}

		////[TestMethod, Ignore("The spec says http:// must be prepended in this case, but that just creates an invalid URI.  Our UntrustedWebRequest will stop disallowed schemes.")]
		public void CtorDisallowedScheme() {
			UriIdentifier id = new UriIdentifier(new Uri("ftp://host/path"));
			Assert.AreEqual("http://ftp://host/path", id.ToString());
			Assert.IsTrue(id.SchemeImplicitlyPrepended);
		}

		[TestMethod]
		public void DiscoveryWithRedirects() {
			Identifier claimedId = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, this.mockResponder, ProtocolVersion.V20);

			// Add a couple of chained redirect pages that lead to the claimedId.
			Uri userSuppliedUri = TestSupport.GetFullUrl("/someSecurePage", null, true);
			Uri insecureMidpointUri = TestSupport.GetFullUrl("/insecureStop");
			this.mockResponder.RegisterMockRedirect(userSuppliedUri, insecureMidpointUri);
			this.mockResponder.RegisterMockRedirect(insecureMidpointUri, new Uri(claimedId.ToString()));

			// don't require secure SSL discovery for this test.
			Identifier userSuppliedIdentifier = new UriIdentifier(userSuppliedUri, false);
			Assert.AreEqual(1, userSuppliedIdentifier.Discover(this.requestHandler).Count());
		}

		[TestMethod]
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
			Assert.AreEqual(0, secureId.Discover(this.requestHandler).Count());

			id = new UriIdentifier("http://www.yahoo.com");
			Assert.IsFalse(id.TryRequireSsl(out secureId));
			Assert.IsFalse(secureId.IsDiscoverySecureEndToEnd);
			Assert.AreEqual("http://www.yahoo.com/", secureId.ToString());
			Assert.AreEqual(0, secureId.Discover(this.requestHandler).Count());
		}

		[TestMethod]
		public void DiscoverRequireSslWithSecureRedirects() {
			Identifier claimedId = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, this.mockResponder, ProtocolVersion.V20, true);

			// Add a couple of chained redirect pages that lead to the claimedId.
			// All redirects should be secure.
			Uri userSuppliedUri = TestSupport.GetFullUrl("/someSecurePage", null, true);
			Uri secureMidpointUri = TestSupport.GetFullUrl("/secureStop", null, true);
			this.mockResponder.RegisterMockRedirect(userSuppliedUri, secureMidpointUri);
			this.mockResponder.RegisterMockRedirect(secureMidpointUri, new Uri(claimedId.ToString()));

			Identifier userSuppliedIdentifier = new UriIdentifier(userSuppliedUri, true);
			Assert.AreEqual(1, userSuppliedIdentifier.Discover(this.requestHandler).Count());
		}

		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void DiscoverRequireSslWithInsecureRedirect() {
			Identifier claimedId = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, this.mockResponder, ProtocolVersion.V20, true);

			// Add a couple of chained redirect pages that lead to the claimedId.
			// Include an insecure HTTP jump in those redirects to verify that
			// the ultimate endpoint is never found as a result of high security profile.
			Uri userSuppliedUri = TestSupport.GetFullUrl("/someSecurePage", null, true);
			Uri insecureMidpointUri = TestSupport.GetFullUrl("/insecureStop");
			this.mockResponder.RegisterMockRedirect(userSuppliedUri, insecureMidpointUri);
			this.mockResponder.RegisterMockRedirect(insecureMidpointUri, new Uri(claimedId.ToString()));

			Identifier userSuppliedIdentifier = new UriIdentifier(userSuppliedUri, true);
			userSuppliedIdentifier.Discover(this.requestHandler);
		}

		[TestMethod]
		public void DiscoveryRequireSslWithInsecureXrdsInSecureHtmlHead() {
			var insecureXrdsSource = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, this.mockResponder, ProtocolVersion.V20, false);
			Uri secureClaimedUri = TestSupport.GetFullUrl("/secureId", null, true);

			string html = string.Format("<html><head><meta http-equiv='X-XRDS-Location' content='{0}'/></head><body></body></html>", insecureXrdsSource);
			this.mockResponder.RegisterMockResponse(secureClaimedUri, "text/html", html);

			Identifier userSuppliedIdentifier = new UriIdentifier(secureClaimedUri, true);
			Assert.AreEqual(0, userSuppliedIdentifier.Discover(this.requestHandler).Count());
		}

		[TestMethod]
		public void DiscoveryRequireSslWithInsecureXrdsInSecureHttpHeader() {
			var insecureXrdsSource = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, this.mockResponder, ProtocolVersion.V20, false);
			Uri secureClaimedUri = TestSupport.GetFullUrl("/secureId", null, true);

			string html = "<html><head></head><body></body></html>";
			WebHeaderCollection headers = new WebHeaderCollection {
				{ "X-XRDS-Location", insecureXrdsSource }
			};
			this.mockResponder.RegisterMockResponse(secureClaimedUri, secureClaimedUri, "text/html", headers, html);

			Identifier userSuppliedIdentifier = new UriIdentifier(secureClaimedUri, true);
			Assert.AreEqual(0, userSuppliedIdentifier.Discover(this.requestHandler).Count());
		}

		[TestMethod]
		public void DiscoveryRequireSslWithInsecureXrdsButSecureLinkTags() {
			var insecureXrdsSource = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, this.mockResponder, ProtocolVersion.V20, false);
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
				HttpUtility.HtmlEncode(localIdForLinkTag.ToString()));
			this.mockResponder.RegisterMockResponse(secureClaimedUri, "text/html", html);

			Identifier userSuppliedIdentifier = new UriIdentifier(secureClaimedUri, true);
			Assert.AreEqual(localIdForLinkTag, userSuppliedIdentifier.Discover(this.requestHandler).Single().ProviderLocalIdentifier);
		}

		[TestMethod]
		public void DiscoveryRequiresSslIgnoresInsecureEndpointsInXrds() {
			var insecureEndpoint = TestSupport.GetServiceEndpoint(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20, 10, false);
			var secureEndpoint = TestSupport.GetServiceEndpoint(TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V20, 20, true);
			UriIdentifier secureClaimedId = new UriIdentifier(TestSupport.GetFullUrl("/claimedId", null, true), true);
			this.mockResponder.RegisterMockXrdsResponse(secureClaimedId, new ServiceEndpoint[] { insecureEndpoint, secureEndpoint });
			Assert.AreEqual(secureEndpoint.ProviderLocalIdentifier, secureClaimedId.Discover(this.requestHandler).Single().ProviderLocalIdentifier);
		}

		private void Discover(string url, ProtocolVersion version, Identifier expectedLocalId, bool expectSreg, bool useRedirect) {
			this.Discover(url, version, expectedLocalId, expectSreg, useRedirect, null);
		}

		private void Discover(string url, ProtocolVersion version, Identifier expectedLocalId, bool expectSreg, bool useRedirect, WebHeaderCollection headers) {
			Protocol protocol = Protocol.Lookup(version);
			UriIdentifier claimedId = TestSupport.GetFullUrl(url);
			UriIdentifier userSuppliedIdentifier = TestSupport.GetFullUrl(
				"Discovery/htmldiscovery/redirect.aspx?target=" + url);
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
			this.mockResponder.RegisterMockResponse(new Uri(idToDiscover), claimedId, contentType, headers ?? new WebHeaderCollection(), TestSupport.LoadEmbeddedFile(url));

			ServiceEndpoint se = idToDiscover.Discover(this.requestHandler).FirstOrDefault();
			Assert.IsNotNull(se, url + " failed to be discovered.");
			Assert.AreSame(protocol, se.Protocol);
			Assert.AreEqual(claimedId, se.ClaimedIdentifier);
			Assert.AreEqual(expectedLocalId, se.ProviderLocalIdentifier);
			Assert.AreEqual(expectSreg ? 2 : 1, se.ProviderSupportedServiceTypeUris.Length);
			Assert.IsTrue(Array.IndexOf(se.ProviderSupportedServiceTypeUris, protocol.ClaimedIdentifierServiceTypeURI) >= 0);

			// TODO: re-enable this line once extensions support is added back in.
			////Assert.AreEqual(expectSreg, se.IsExtensionSupported(new ClaimsRequest()));
		}

		private void DiscoverXrds(string page, ProtocolVersion version, Identifier expectedLocalId) {
			this.DiscoverXrds(page, version, expectedLocalId, null);
		}

		private void DiscoverXrds(string page, ProtocolVersion version, Identifier expectedLocalId, WebHeaderCollection headers) {
			if (!page.Contains(".")) page += ".xml";
			this.Discover("/Discovery/xrdsdiscovery/" + page, version, expectedLocalId, true, false, headers);
			this.Discover("/Discovery/xrdsdiscovery/" + page, version, expectedLocalId, true, true, headers);
		}

		private void DiscoverHtml(string page, ProtocolVersion version, Identifier expectedLocalId, bool useRedirect) {
			this.Discover("/Discovery/htmldiscovery/" + page, version, expectedLocalId, false, useRedirect);
		}

		private void DiscoverHtml(string scenario, ProtocolVersion version, Identifier expectedLocalId) {
			string page = scenario + ".html";
			this.DiscoverHtml(page, version, expectedLocalId, false);
			this.DiscoverHtml(page, version, expectedLocalId, true);
		}

		private void FailDiscover(string url) {
			UriIdentifier userSuppliedId = TestSupport.GetFullUrl(url);

			this.mockResponder.RegisterMockResponse(new Uri(userSuppliedId), userSuppliedId, "text/html", TestSupport.LoadEmbeddedFile(url));

			Assert.AreEqual(0, userSuppliedId.Discover(this.requestHandler).Count()); // ... but that no endpoint info is discoverable
		}

		private void FailDiscoverHtml(string scenario) {
			this.FailDiscover("/Discovery/htmldiscovery/" + scenario + ".html");
		}

		private void FailDiscoverXrds(string scenario) {
			this.FailDiscover("/Discovery/xrdsdiscovery/" + scenario + ".xml");
		}
	}
}
