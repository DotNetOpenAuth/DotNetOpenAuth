using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Provider;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class TrustRootTestSuite {
		[Test]
		public void ValidTrustRootsTest() {
			// Just create these.  If any are determined to be invalid,
			// an exception should be thrown that would fail this test.
			new TrustRoot("http://www.myopenid.com");
			new TrustRoot("http://www.myopenid.com/");
			new TrustRoot("http://www.myopenid.com:5221/");
			new TrustRoot("https://www.myopenid.com");
			new TrustRoot("http://www.myopenid.com/abc");
			new TrustRoot("http://www.myopenid.com/abc/");
			new TrustRoot("http://*.myopenid.com/");
			new TrustRoot("http://*.com/");
			new TrustRoot("http://*.guest.myopenid.com/");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void InvalidTrustRootNull() {
			new TrustRoot(null);
		}

		[Test]
		[ExpectedException(typeof(MalformedTrustRootException))]
		public void InvalidTrustRootEmpty() {
			new TrustRoot("");
		}

		[Test]
		[ExpectedException(typeof(MalformedTrustRootException))]
		public void InvalidTrustRootBadProtocol() {
			new TrustRoot("asdf://www.microsoft.com/");
		}

		[Test]
		[ExpectedException(typeof(MalformedTrustRootException))]
		public void InvalidTrustRootNoScheme() {
			new TrustRoot("www.guy.com");
		}

		[Test]
		[ExpectedException(typeof(MalformedTrustRootException))]
		public void InvalidTrustRootBadWildcard() {
			new TrustRoot("http://*www.my.com");
		}

		[Test]
		[ExpectedException(typeof(MalformedTrustRootException))]
		public void InvalidTrustRootTwoWildcards1() {
			new TrustRoot("http://**.my.com");
		}

		[Test]
		[ExpectedException(typeof(MalformedTrustRootException))]
		public void InvalidTrustRootTwoWildcards2() {
			new TrustRoot("http://*.*.my.com");
		}

		[Test]
		public void IsSaneTest() {
			Assert.IsTrue(new TrustRoot("http://www.myopenid.com").IsSane);
			Assert.IsTrue(new TrustRoot("http://myopenid.com").IsSane);
			Assert.IsTrue(new TrustRoot("http://localhost").IsSane);
			Assert.IsTrue(new TrustRoot("http://localhost:33532/dab").IsSane);
			Assert.IsTrue(new TrustRoot("http://www.myopenid.com").IsSane);

			Assert.IsFalse(new TrustRoot("http://*.com").IsSane);
			Assert.IsFalse(new TrustRoot("http://*.co.uk").IsSane);
		}

		[Test]
		public void IsValidRootTests() {
			/* 
			 * The openid.return_to URL MUST descend from the openid.trust_root, or the 
			 * Identity Provider SHOULD return an error. Namely, the URL scheme and port 
			 * MUST match. The path, if present, MUST be equal to or below the value of 
			 * openid.trust_root, and the domains on both MUST match, or, the 
			 * openid.trust_root value contain a wildcard like http://*.example.com. 
			 * The wildcard SHALL only be at the beginning. It is RECOMMENDED Identity 
			 * Provider's protect their End Users from requests for things like 
			 * http://*.com/ or http://*.co.uk/.
			 */

			// Schemes must match
			Assert.IsFalse(new TrustRoot("https://www.my.com/").ValidateUrl("http://www.my.com/"));
			Assert.IsFalse(new TrustRoot("http://www.my.com/").ValidateUrl("https://www.my.com/"));

			// Ports must match
			Assert.IsTrue(new TrustRoot("http://www.my.com/").ValidateUrl("http://www.my.com:80/boo"));
			Assert.IsTrue(new TrustRoot("http://www.my.com:80/").ValidateUrl("http://www.my.com/boo"));
			Assert.IsFalse(new TrustRoot("http://www.my.com:79/").ValidateUrl("http://www.my.com/boo"));
			Assert.IsFalse(new TrustRoot("https://www.my.com/").ValidateUrl("http://www.my.com:79/boo"));

			// Path must be (at or) below trust root
			Assert.IsTrue(new TrustRoot("http://www.my.com/").ValidateUrl("http://www.my.com/"));
			Assert.IsTrue(new TrustRoot("http://www.my.com/").ValidateUrl("http://www.my.com/boo"));
			Assert.IsTrue(new TrustRoot("http://www.my.com/bah").ValidateUrl("http://www.my.com/bah/bah"));
			Assert.IsTrue(new TrustRoot("http://www.my.com/bah").ValidateUrl("http://www.my.com/bah/bah"));
			Assert.IsTrue(new TrustRoot("http://www.my.com/bah.html").ValidateUrl("http://www.my.com/bah.html/bah"));
			Assert.IsFalse(new TrustRoot("http://www.my.com/bah").ValidateUrl("http://www.my.com/bahbah"));
			Assert.IsTrue(new TrustRoot("http://www.my.com/bah").ValidateUrl("http://www.my.com/bah?q=a"));
			Assert.IsTrue(new TrustRoot("http://www.my.com/bah?q=a").ValidateUrl("http://www.my.com/bah?q=a"));
			Assert.IsTrue(new TrustRoot("http://www.my.com/bah?a=b&c=d").ValidateUrl("http://www.my.com/bah?a=b&c=d&e=f"));
			Assert.IsFalse(new TrustRoot("http://www.my.com/bah?a=b&c=d").ValidateUrl("http://www.my.com/bah?a=b"));

			// Domains MUST match
			Assert.IsFalse(new TrustRoot("http://www.my.com/").ValidateUrl("http://yours.com/"));
			Assert.IsFalse(new TrustRoot("http://www.my.com/").ValidateUrl("http://www.yours.com/"));
			Assert.IsFalse(new TrustRoot("http://www.my.com/").ValidateUrl("http://q.www.my.com/"));
			Assert.IsFalse(new TrustRoot("http://www.my.com/").ValidateUrl("http://wwww.my.com/"));
			Assert.IsFalse(new TrustRoot("http://www.my.com/").ValidateUrl("http://www.my.com.uk/"));
			Assert.IsFalse(new TrustRoot("http://www.my.com/").ValidateUrl("http://www.my.comm/"));

			// Allow for wildcards
			Assert.IsTrue(new TrustRoot("http://*.www.my.com/").ValidateUrl("http://bah.www.my.com/"));
			Assert.IsTrue(new TrustRoot("http://*.www.my.com/").ValidateUrl("http://bah.www.my.com/boo"));
			// These are tested against by the constructor test, as these are invalid wildcard positions.
			//Assert.IsFalse(new TrustRoot("http://*www.my.com/").ValidateUrl("http://bah.www.my.com/"));
			//Assert.IsFalse(new TrustRoot("http://*www.my.com/").ValidateUrl("http://wwww.my.com/"));

			// Among those that should return true, mix up character casing to test for case sensitivity.
			// Host names should be case INSENSITIVE, but paths should probably be case SENSITIVE,
			// because in some systems they are case sensitive and to ignore this would open
			// security holes.
			Assert.IsTrue(new TrustRoot("http://www.my.com/").ValidateUrl("http://WWW.MY.COM/"));
			Assert.IsFalse(new TrustRoot("http://www.my.com/abc").ValidateUrl("http://www.my.com/ABC"));
		}
	}
}
