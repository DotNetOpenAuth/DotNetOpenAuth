//-----------------------------------------------------------------------
// <copyright file="UriIdentifierTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Linq;
	using System.Net;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class UriIdentifierTests : OpenIdTestBase {
		private string goodUri = "http://blog.nerdbank.net/";
		private string relativeUri = "host/path";
		private string badUri = "som%-)830w8vf/?.<>,ewackedURI";

		[SetUp]
		public override void SetUp() {
			base.SetUp();
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullUri() {
			new UriIdentifier((Uri)null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullString() {
			new UriIdentifier((string)null);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void CtorBlank() {
			new UriIdentifier(string.Empty);
		}

		[Test, ExpectedException(typeof(UriFormatException))]
		public void CtorBadUri() {
			new UriIdentifier(this.badUri);
		}

		[Test]
		public void CtorGoodUri() {
			var uri = new UriIdentifier(this.goodUri);
			Assert.AreEqual(new Uri(this.goodUri), uri.Uri);
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
			Assert.IsTrue(UriIdentifier.IsValidUri(this.goodUri));
			Assert.IsFalse(UriIdentifier.IsValidUri(this.badUri));
			Assert.IsTrue(UriIdentifier.IsValidUri(this.relativeUri), "URL lacking http:// prefix should have worked anyway.");
		}

		[Test]
		public void TrimFragment() {
			Identifier noFragment = UriIdentifier.Parse("http://a/b");
			Identifier fragment = UriIdentifier.Parse("http://a/b#c");
			Assert.AreSame(noFragment, noFragment.TrimFragment());
			Assert.AreEqual(noFragment.ToString(), fragment.TrimFragment().ToString());

			// Try the problematic ones
			TestAsFullAndPartialTrust(fullTrust => {
				Identifier noFrag = UriIdentifier.Parse("http://a/b./c");
				Identifier frag = UriIdentifier.Parse("http://a/b./c#d");
				Assert.AreSame(noFrag, noFrag.TrimFragment());
				Assert.AreEqual(noFrag.ToString(), frag.TrimFragment().ToString());
			});
		}

		[Test]
		public void ToStringTest() {
			Assert.AreEqual(this.goodUri, new UriIdentifier(this.goodUri).ToString());
			TestAsFullAndPartialTrust(fullTrust => {
				Assert.AreEqual("http://abc/D./e.?Qq#Ff", new UriIdentifier("HTTP://ABC/D./e.?Qq#Ff").ToString());
				Assert.AreEqual("http://abc/D./e.?Qq", new UriIdentifier("HTTP://ABC/D./e.?Qq").ToString());
				Assert.AreEqual("http://abc/D./e.#Ff", new UriIdentifier("HTTP://ABC/D./e.#Ff").ToString());
				Assert.AreEqual("http://abc/", new UriIdentifier("HTTP://ABC").ToString());
				Assert.AreEqual("http://abc/?q", new UriIdentifier("HTTP://ABC?q").ToString());
				Assert.AreEqual("http://abc/#f", new UriIdentifier("HTTP://ABC#f").ToString());

				Assert.AreEqual("http://blog.nerdbank.net/", new UriIdentifier("blog.nerdbank.net").ToString());
				Assert.AreEqual("http://blog.nerdbank.net/a", new UriIdentifier("BLOG.nerdbank.net/a").ToString());
				Assert.AreEqual("https://blog.nerdbank.net/", new UriIdentifier("blog.nerdbank.net", true).ToString());
			});
		}

		[Test]
		public void EqualsTest() {
			TestAsFullAndPartialTrust(fulltrust => {
				Assert.AreEqual(new UriIdentifier(this.goodUri), new UriIdentifier(this.goodUri));
				// This next test is an interesting side-effect of passing off to Uri.Equals.  But it's probably ok.
				Assert.AreEqual(new UriIdentifier(this.goodUri), new UriIdentifier(this.goodUri + "#frag"));
				Assert.AreEqual(new UriIdentifier("http://a/b./c."), new UriIdentifier("http://a/b./c.#frag"));
				Assert.AreNotEqual(new UriIdentifier(this.goodUri), new UriIdentifier(this.goodUri + "a"));
				Assert.AreNotEqual(null, new UriIdentifier(this.goodUri));
				Assert.IsTrue(new UriIdentifier(this.goodUri).Equals(this.goodUri));

				Assert.AreEqual(Identifier.Parse("HTTP://WWW.FOO.COM/abc", true), Identifier.Parse("http://www.foo.com/abc", true));
				Assert.AreEqual(Identifier.Parse("HTTP://WWW.FOO.COM/abc", true), Identifier.Parse("http://www.foo.com/abc", false));
				Assert.AreEqual(Identifier.Parse("HTTP://WWW.FOO.COM/abc", false), Identifier.Parse("http://www.foo.com/abc", false));
				Assert.AreNotEqual(Identifier.Parse("http://www.foo.com/abc", true), Identifier.Parse("http://www.foo.com/ABC", true));
				Assert.AreNotEqual(Identifier.Parse("http://www.foo.com/abc", true), Identifier.Parse("http://www.foo.com/ABC", false));
				Assert.AreNotEqual(Identifier.Parse("http://www.foo.com/abc", false), Identifier.Parse("http://www.foo.com/ABC", false));

				Assert.AreNotEqual(Identifier.Parse("http://a/b./c."), Identifier.Parse("http://a/b/c."));
				Assert.AreEqual(Identifier.Parse("http://a/b./c."), Identifier.Parse("http://a/b./c."));
			});
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

		[Test]
		public void NormalizeCase() {
			// only the host name can be normalized in casing safely.
			Identifier id = "http://HOST:80/PaTH?KeY=VaLUE#fRag";
			Assert.AreEqual("http://host/PaTH?KeY=VaLUE#fRag", id.ToString());
			// make sure https is preserved, along with port 80, which is NON-default for https
			id = "https://HOST:80/PaTH?KeY=VaLUE#fRag";
			Assert.AreEqual("https://host:80/PaTH?KeY=VaLUE#fRag", id.ToString());
		}

		/// <summary>
		/// Verifies that URIs that contain base64 encoded path segments (such as Yahoo) are properly preserved.
		/// </summary>
		/// <remarks>
		/// Yahoo includes a base64 encoded part as their last path segment,
		/// which may end with a period.  The default .NET Uri parser trims off
		/// trailing periods, which breaks OpenID unless special precautions are taken.
		/// </remarks>
		[Test]
		public void TrailingPeriodsNotTrimmed() {
			TestAsFullAndPartialTrust(fullTrust => {
				string claimedIdentifier = "https://me.yahoo.com/a/AsDf.#asdf";
				Identifier id = claimedIdentifier;
				Assert.AreEqual(claimedIdentifier, id.OriginalString);
				Assert.AreEqual(claimedIdentifier, id.ToString());

				UriIdentifier idUri = new UriIdentifier(claimedIdentifier);
				Assert.AreEqual(claimedIdentifier, idUri.OriginalString);
				Assert.AreEqual(claimedIdentifier, idUri.ToString());
				if (fullTrust) {
					Assert.AreEqual(claimedIdentifier, idUri.Uri.AbsoluteUri);
				}
				Assert.AreEqual(Uri.UriSchemeHttps, idUri.Uri.Scheme); // in case custom scheme tricks are played, this must still match
				Assert.AreEqual("https://me.yahoo.com/a/AsDf.", idUri.TrimFragment().ToString());
				Assert.AreEqual("https://me.yahoo.com/a/AsDf.", idUri.TrimFragment().OriginalString);
				Assert.AreEqual(id.ToString(), new UriIdentifier((Uri)idUri).ToString(), "Round tripping UriIdentifier->Uri->UriIdentifier failed.");

				idUri = new UriIdentifier(new Uri(claimedIdentifier));
				Assert.AreEqual(claimedIdentifier, idUri.OriginalString);
				Assert.AreEqual(claimedIdentifier, idUri.ToString());
				if (fullTrust) {
					Assert.AreEqual(claimedIdentifier, idUri.Uri.AbsoluteUri);
				}
				Assert.AreEqual(Uri.UriSchemeHttps, idUri.Uri.Scheme); // in case custom scheme tricks are played, this must still match
				Assert.AreEqual("https://me.yahoo.com/a/AsDf.", idUri.TrimFragment().ToString());
				Assert.AreEqual("https://me.yahoo.com/a/AsDf.", idUri.TrimFragment().OriginalString);
				Assert.AreEqual(id.ToString(), new UriIdentifier((Uri)idUri).ToString(), "Round tripping UriIdentifier->Uri->UriIdentifier failed.");

				claimedIdentifier = "https://me.yahoo.com:443/a/AsDf.#asdf";
				id = claimedIdentifier;
				Assert.AreEqual(claimedIdentifier, id.OriginalString);
				Assert.AreEqual("https://me.yahoo.com/a/AsDf.#asdf", id.ToString());
			});
		}

		[Test]
		public void HttpSchemePrepended() {
			UriIdentifier id = new UriIdentifier("www.yahoo.com");
			Assert.AreEqual("http://www.yahoo.com/", id.ToString());
			Assert.IsTrue(id.SchemeImplicitlyPrepended);
		}

		////[Test, Ignore("The spec says http:// must be prepended in this case, but that just creates an invalid URI.  Our UntrustedWebRequest will stop disallowed schemes.")]
		public void CtorDisallowedScheme() {
			UriIdentifier id = new UriIdentifier(new Uri("ftp://host/path"));
			Assert.AreEqual("http://ftp://host/path", id.ToString());
			Assert.IsTrue(id.SchemeImplicitlyPrepended);
		}

		[Test]
		public async Task TryRequireSslAdjustsIdentifier() {
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
			Assert.AreEqual(0, (await DiscoverAsync(secureId)).Count(), "Since TryRequireSsl failed, the created Identifier should never discover anything.");

			id = new UriIdentifier("http://www.yahoo.com");
			Assert.IsFalse(id.TryRequireSsl(out secureId));
			Assert.IsTrue(secureId.IsDiscoverySecureEndToEnd);
			Assert.AreEqual("http://www.yahoo.com/", secureId.ToString());
			Assert.AreEqual(0, (await DiscoverAsync(secureId)).Count());
		}

		/// <summary>
		/// Verifies that unicode hostnames are handled.
		/// </summary>
		[Test]
		public void UnicodeHostSupport() {
			var id = new UriIdentifier("http://server崎/村");
			Assert.AreEqual("server崎", id.Uri.Host);
		}

		/// <summary>
		/// Verifies SimpleUri behavior
		/// </summary>
		[Test]
		public void SimpleUri() {
			Assert.AreEqual("http://abc/D./e.?Qq#Ff", new UriIdentifier.SimpleUri("HTTP://ABC/D./e.?Qq#Ff").ToString());
			Assert.AreEqual("http://abc/D./e.?Qq", new UriIdentifier.SimpleUri("HTTP://ABC/D./e.?Qq").ToString());
			Assert.AreEqual("http://abc/D./e.#Ff", new UriIdentifier.SimpleUri("HTTP://ABC/D./e.#Ff").ToString());
			Assert.AreEqual("http://abc/", new UriIdentifier.SimpleUri("HTTP://ABC/").ToString());
			Assert.AreEqual("http://abc/", new UriIdentifier.SimpleUri("HTTP://ABC").ToString());
			Assert.AreEqual("http://abc/?q", new UriIdentifier.SimpleUri("HTTP://ABC?q").ToString());
			Assert.AreEqual("http://abc/#f", new UriIdentifier.SimpleUri("HTTP://ABC#f").ToString());

			Assert.AreEqual("http://abc/a//b", new UriIdentifier.SimpleUri("http://abc/a//b").ToString());
			Assert.AreEqual("http://abc/a%2Fb/c", new UriIdentifier.SimpleUri("http://abc/a%2fb/c").ToString());
			Assert.AreEqual("http://abc/A/c", new UriIdentifier.SimpleUri("http://abc/%41/c").ToString());
		}

		private static void TestAsFullAndPartialTrust(Action<bool> action) {
			// Test a bunch of interesting URLs both with scheme substitution on and off.
			Assert.IsTrue(UriIdentifier.SchemeSubstitutionTestHook, "Expected scheme substitution to be working.");
			action(true);

			UriIdentifier.SchemeSubstitutionTestHook = false;
			try {
				action(false);
			} finally {
				UriIdentifier.SchemeSubstitutionTestHook = true;
			}
		}
	}
}
