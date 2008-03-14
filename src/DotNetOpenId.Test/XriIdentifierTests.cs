using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

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
	}
}
