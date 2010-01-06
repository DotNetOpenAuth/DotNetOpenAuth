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
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
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

		[TestMethod, ExpectedException(typeof(ArgumentException))]
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
			Assert.IsTrue(UriIdentifier.IsValidUri(this.relativeUri), "URL lacking http:// prefix should have worked anyway.");
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
			Assert.AreEqual(0, Discover(secureId).Count(), "Since TryRequireSsl failed, the created Identifier should never discover anything.");

			id = new UriIdentifier("http://www.yahoo.com");
			Assert.IsFalse(id.TryRequireSsl(out secureId));
			Assert.IsTrue(secureId.IsDiscoverySecureEndToEnd);
			Assert.AreEqual("http://www.yahoo.com/", secureId.ToString());
			Assert.AreEqual(0, Discover(secureId).Count());
		}
	}
}
