using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class UriIdentifierTests {
		string goodUri = "http://blog.nerdbank.net/";
		string badUri = "som%-)830w8vf/?.<>,ewackedURI";

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

		[Test]
		public void IsValid() {
			Assert.IsTrue(UriIdentifier.IsValidUri(goodUri));
			Assert.IsFalse(UriIdentifier.IsValidUri(badUri));
		}

		[Test]
		public void ToStringTest() {
			Assert.AreEqual(goodUri, new UriIdentifier(goodUri).ToString());
		}

		[Test]
		public void EqualsTest() {
			Assert.AreEqual(new UriIdentifier(goodUri), new UriIdentifier(goodUri));
			Assert.AreNotEqual(new UriIdentifier(goodUri), new UriIdentifier(goodUri + "a"));
			Assert.AreNotEqual(null, new UriIdentifier(goodUri));
			Assert.AreNotEqual(goodUri, new UriIdentifier(goodUri));
		}

		void discover(string page, ProtocolVersion version, Identifier expectedLocalId, bool useRedirect) {
			Protocol protocol = Protocol.Lookup(version);
			UriIdentifier claimedId = TestSupport.GetFullUrl("/htmldiscovery/" + page);
			UriIdentifier userSuppliedIdentifier = TestSupport.GetFullUrl(
				"htmldiscovery/redirect.aspx?target=" + page);
			if (expectedLocalId == null) expectedLocalId = claimedId;
			ServiceEndpoint se = useRedirect ? userSuppliedIdentifier.Discover() : claimedId.Discover();
			Assert.IsNotNull(se, page + " failed to be discovered.");
			Assert.AreSame(protocol, se.Protocol);
			Assert.AreEqual(claimedId, se.ClaimedIdentifier);
			Assert.AreEqual(expectedLocalId, se.ProviderLocalIdentifier);
			Assert.AreEqual(1, se.ProviderSupportedServiceTypeUris.Length);
			Assert.AreEqual(protocol.ClaimedIdentifierServiceTypeURI, se.ProviderSupportedServiceTypeUris[0]);
		}
		void discover(string scenario, ProtocolVersion version, Identifier expectedLocalId) {
			string page = scenario + ".aspx";
			discover(page, version, expectedLocalId, false);
			discover(page, version, expectedLocalId, true);
		}
		void failDiscovery(string scenario) {
			string page = scenario + ".aspx";
			UriIdentifier userSuppliedId = TestSupport.GetFullUrl("htmldiscovery/" + page);
			Assert.IsNull(userSuppliedId.Discover());
		}
		[Test]
		public void HtmlDiscover_11() {
			discover("html10prov", ProtocolVersion.V11, null);
			discover("html10both", ProtocolVersion.V11, "http://c/d");
			failDiscovery("html10del");
		}
		[Test]
		public void HtmlDiscover_20() {
			discover("html20prov", ProtocolVersion.V20, null);
			discover("html20both", ProtocolVersion.V20, "http://c/d");
			failDiscovery("html20del");
			discover("html2010", ProtocolVersion.V20, "http://c/d");
			discover("html1020", ProtocolVersion.V20, "http://c/d");
			discover("html2010combinedA", ProtocolVersion.V20, "http://c/d");
			discover("html2010combinedB", ProtocolVersion.V20, "http://c/d");
			discover("html2010combinedC", ProtocolVersion.V20, "http://c/d");
		}
	}
}
