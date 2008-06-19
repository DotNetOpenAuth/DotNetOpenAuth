using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.RelyingParty;
using System.Net;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class XriIdentifierTests {
		string goodXri = "=Andrew*Arnott";
		string badXri = "some\\wacky%^&*()non-XRI";

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNull() {
			new XriIdentifier(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorBlank() {
			new XriIdentifier(string.Empty);
		}

		[Test, ExpectedException(typeof(FormatException))]
		public void CtorBadXri() {
			new XriIdentifier(badXri);
		}

		[Test]
		public void CtorGoodXri() {
			var xri = new XriIdentifier(goodXri);
			Assert.AreEqual(goodXri, xri.OriginalXri);
			Assert.AreEqual(goodXri, xri.CanonicalXri); // assumes 'goodXri' is canonical already
		}

		[Test]
		public void IsValid() {
			Assert.IsTrue(XriIdentifier.IsValidXri(goodXri));
			Assert.IsFalse(XriIdentifier.IsValidXri(badXri));
		}

		/// <summary>
		/// Verifies 2.0 spec section 7.2#1
		/// </summary>
		[Test]
		public void StripXriScheme() {
			var xri = new XriIdentifier("xri://" + goodXri);
			Assert.AreEqual("xri://" + goodXri, xri.OriginalXri);
			Assert.AreEqual(goodXri, xri.CanonicalXri);
		}

		[Test]
		public void ToStringTest() {
			Assert.AreEqual(goodXri, new XriIdentifier(goodXri).ToString());
		}

		[Test]
		public void EqualsTest() {
			Assert.AreEqual(new XriIdentifier(goodXri), new XriIdentifier(goodXri));
			Assert.AreNotEqual(new XriIdentifier(goodXri), new XriIdentifier(goodXri + "a"));
			Assert.AreNotEqual(null, new XriIdentifier(goodXri));
			Assert.AreNotEqual(goodXri, new XriIdentifier(goodXri));
		}

		private ServiceEndpoint verifyCanonicalId(Identifier iname, string expectedClaimedIdentifier) {
			// This test requires a network connection
			ServiceEndpoint se = null;
			try {
				se = iname.Discover();
			} catch (WebException ex) {
				if (ex.Message.Contains("remote name could not be resolved"))
					Assert.Ignore("This test requires a network connection.");
			}
			if (expectedClaimedIdentifier != null) {
				Assert.IsNotNull(se);
				Assert.AreEqual(expectedClaimedIdentifier, se.ClaimedIdentifier.ToString(), "i-name {0} discovery resulted in unexpected CanonicalId", iname);
				Assert.AreEqual(1, se.ProviderSupportedServiceTypeUris.Length);
			} else {
				Assert.IsNull(se);
			}
			return se;
		}

		[Test]
		public void Discover() {
			string expectedCanonicalId = "=!9B72.7DD1.50A9.5CCD";
			ServiceEndpoint se = verifyCanonicalId("=Arnott", expectedCanonicalId);
			Assert.AreEqual(Protocol.v10, se.Protocol);
			Assert.AreEqual("http://1id.com/sso", se.ProviderEndpoint.ToString());
			Assert.AreEqual(se.ClaimedIdentifier, se.ProviderLocalIdentifier);
		}

		[Test]
		public void DiscoverCommunityIname() {
			verifyCanonicalId("=Web", "=!91F2.8153.F600.AE24");
			//verifyCanonicalId("=Web*andrew.arnott", null);
			verifyCanonicalId("@llli", "@!72CD.A072.157E.A9C6");
			verifyCanonicalId("@llli*area", "@!72CD.A072.157E.A9C6!0000.0000.3B9A.CA0C");
			verifyCanonicalId("@llli*area*canada.unattached", "@!72CD.A072.157E.A9C6!0000.0000.3B9A.CA0C!0000.0000.3B9A.CA41");
			verifyCanonicalId("@llli*area*canada.unattached*ada", "@!72CD.A072.157E.A9C6!0000.0000.3B9A.CA0C!0000.0000.3B9A.CA41!0000.0000.3B9A.CA01");
			
		}
	}
}
