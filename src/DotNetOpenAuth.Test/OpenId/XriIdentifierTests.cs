//-----------------------------------------------------------------------
// <copyright file="XriIdentifierTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class XriIdentifierTests : OpenIdTestBase {
		private string goodXri = "=Andrew*Arnott";
		private string badXri = "some\\wacky%^&*()non-XRI";

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void CtorNull() {
			new XriIdentifier(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void CtorBlank() {
			new XriIdentifier(string.Empty);
		}

		[TestMethod, ExpectedException(typeof(FormatException))]
		public void CtorBadXri() {
			new XriIdentifier(this.badXri);
		}

		[TestMethod]
		public void CtorGoodXri() {
			var xri = new XriIdentifier(this.goodXri);
			Assert.AreEqual(this.goodXri, xri.OriginalXri);
			Assert.AreEqual(this.goodXri, xri.CanonicalXri); // assumes 'goodXri' is canonical already
			Assert.IsFalse(xri.IsDiscoverySecureEndToEnd);
		}

		[TestMethod]
		public void CtorGoodXriSecure() {
			var xri = new XriIdentifier(this.goodXri, true);
			Assert.AreEqual(this.goodXri, xri.OriginalXri);
			Assert.AreEqual(this.goodXri, xri.CanonicalXri); // assumes 'goodXri' is canonical already
			Assert.IsTrue(xri.IsDiscoverySecureEndToEnd);
		}

		[TestMethod]
		public void IsValid() {
			Assert.IsTrue(XriIdentifier.IsValidXri(this.goodXri));
			Assert.IsFalse(XriIdentifier.IsValidXri(this.badXri));
		}

		/// <summary>
		/// Verifies 2.0 spec section 7.2#1
		/// </summary>
		[TestMethod]
		public void StripXriScheme() {
			var xri = new XriIdentifier("xri://" + this.goodXri);
			Assert.AreEqual("xri://" + this.goodXri, xri.OriginalXri);
			Assert.AreEqual(this.goodXri, xri.CanonicalXri);
		}

		[TestMethod]
		public void TrimFragment() {
			Identifier xri = new XriIdentifier(this.goodXri);
			Assert.AreSame(xri, xri.TrimFragment());
		}

		[TestMethod]
		public void ToStringTest() {
			Assert.AreEqual(this.goodXri, new XriIdentifier(this.goodXri).ToString());
		}

		[TestMethod]
		public void EqualsTest() {
			Assert.AreEqual(new XriIdentifier(this.goodXri), new XriIdentifier(this.goodXri));
			Assert.AreNotEqual(new XriIdentifier(this.goodXri), new XriIdentifier(this.goodXri + "a"));
			Assert.AreNotEqual(null, new XriIdentifier(this.goodXri));
			Assert.AreEqual(this.goodXri, new XriIdentifier(this.goodXri));
		}

		[TestMethod, Ignore] // XRI parsing and normalization is not implemented (yet).
		public void NormalizeCase() {
			Identifier id = "=!9B72.7dd1.50a9.5ccd";
			Assert.AreEqual("=!9B72.7DD1.50A9.5CCD", id.ToString());
		}
	}
}
