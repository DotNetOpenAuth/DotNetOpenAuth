//-----------------------------------------------------------------------
// <copyright file="RealmTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class RealmTests {
		[Test]
		public void ValidRealmsTest() {
			// Just create these.  If any are determined to be invalid,
			// an exception should be thrown that would fail this test.
			new Realm("http://www.myopenid.com");
			new Realm("http://www.myopenid.com/");
			new Realm("http://www.myopenid.com:5221/");
			new Realm("https://www.myopenid.com");
			new Realm("http://www.myopenid.com/abc");
			new Realm("http://www.myopenid.com/abc/");
			new Realm("http://*.myopenid.com/");
			new Realm("http://*.com/");
			new Realm("http://*.guest.myopenid.com/");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void InvalidRealmNullString() {
			new Realm((string)null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void InvalidRealmNullUri() {
			new Realm((Uri)null);
		}

		[Test]
		[ExpectedException(typeof(UriFormatException))]
		public void InvalidRealmEmpty() {
			new Realm(string.Empty);
		}

		[Test]
		[ExpectedException(typeof(UriFormatException))]
		public void InvalidRealmBadProtocol() {
			new Realm("asdf://www.microsoft.com/");
		}

		[Test]
		[ExpectedException(typeof(UriFormatException))]
		public void InvalidRealmNoScheme() {
			new Realm("www.guy.com");
		}

		[Test]
		[ExpectedException(typeof(UriFormatException))]
		public void InvalidRealmBadWildcard1() {
			new Realm("http://*www.my.com");
		}

		[Test]
		[ExpectedException(typeof(UriFormatException))]
		public void InvalidRealmBadWildcard2() {
			new Realm("http://www.*.com");
		}

		[Test]
		[ExpectedException(typeof(UriFormatException))]
		public void InvalidRealmBadWildcard3() {
			new Realm("http://www.my.*/");
		}

		[Test]
		[ExpectedException(typeof(UriFormatException))]
		public void InvalidRealmTwoWildcards1() {
			new Realm("http://**.my.com");
		}

		[Test]
		[ExpectedException(typeof(UriFormatException))]
		public void InvalidRealmTwoWildcards2() {
			new Realm("http://*.*.my.com");
		}

		[Test]
		public void IsSaneTest() {
			Assert.IsTrue(new Realm("http://www.myopenid.com").IsSane);
			Assert.IsTrue(new Realm("http://myopenid.com").IsSane);
			Assert.IsTrue(new Realm("http://localhost").IsSane);
			Assert.IsTrue(new Realm("http://localhost:33532/dab").IsSane);
			Assert.IsTrue(new Realm("http://www.myopenid.com").IsSane);

			Assert.IsFalse(new Realm("http://*.com").IsSane);
			Assert.IsFalse(new Realm("http://*.co.uk").IsSane);
		}

		[Test]
		public void IsUrlWithinRealmTests() {
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
			Assert.IsFalse(new Realm("https://www.my.com/").Contains("http://www.my.com/"));
			Assert.IsFalse(new Realm("http://www.my.com/").Contains("https://www.my.com/"));

			// Ports must match
			Assert.IsTrue(new Realm("http://www.my.com/").Contains("http://www.my.com:80/boo"));
			Assert.IsTrue(new Realm("http://www.my.com:80/").Contains("http://www.my.com/boo"));
			Assert.IsFalse(new Realm("http://www.my.com:79/").Contains("http://www.my.com/boo"));
			Assert.IsFalse(new Realm("https://www.my.com/").Contains("http://www.my.com:79/boo"));

			// Path must be (at or) below trust root
			Assert.IsTrue(new Realm("http://www.my.com/").Contains("http://www.my.com/"));
			Assert.IsTrue(new Realm("http://www.my.com/").Contains("http://www.my.com/boo"));
			Assert.IsTrue(new Realm("http://www.my.com/p/").Contains("http://www.my.com/p/l"));
			Assert.IsTrue(new Realm("http://www.my.com/bah").Contains("http://www.my.com/bah/bah"));
			Assert.IsTrue(new Realm("http://www.my.com/bah").Contains("http://www.my.com/bah/bah"));
			Assert.IsTrue(new Realm("http://www.my.com/bah.html").Contains("http://www.my.com/bah.html/bah"));
			Assert.IsFalse(new Realm("http://www.my.com/bah").Contains("http://www.my.com/bahbah"));
			Assert.IsTrue(new Realm("http://www.my.com/bah").Contains("http://www.my.com/bah?q=a"));
			Assert.IsTrue(new Realm("http://www.my.com/bah?q=a").Contains("http://www.my.com/bah?q=a"));
			Assert.IsTrue(new Realm("http://www.my.com/bah?a=b&c=d").Contains("http://www.my.com/bah?a=b&c=d&e=f"));
			Assert.IsFalse(new Realm("http://www.my.com/bah?a=b&c=d").Contains("http://www.my.com/bah?a=b"));

			// Domains MUST match
			Assert.IsFalse(new Realm("http://www.my.com/").Contains("http://yours.com/"));
			Assert.IsFalse(new Realm("http://www.my.com/").Contains("http://www.yours.com/"));
			Assert.IsFalse(new Realm("http://www.my.com/").Contains("http://q.www.my.com/"));
			Assert.IsFalse(new Realm("http://www.my.com/").Contains("http://wwww.my.com/"));
			Assert.IsFalse(new Realm("http://www.my.com/").Contains("http://www.my.com.uk/"));
			Assert.IsFalse(new Realm("http://www.my.com/").Contains("http://www.my.comm/"));

			// Allow for wildcards
			Assert.IsTrue(new Realm("http://*.www.my.com/").Contains("http://bah.www.my.com/"));
			Assert.IsTrue(new Realm("http://*.www.my.com/").Contains("http://bah.WWW.MY.COM/"));
			Assert.IsTrue(new Realm("http://*.www.my.com/").Contains("http://bah.www.my.com/boo"));
			Assert.IsTrue(new Realm("http://*.my.com/").Contains("http://bah.www.my.com/boo"));
			Assert.IsTrue(new Realm("http://*.my.com/").Contains("http://my.com/boo"));
			Assert.IsFalse(new Realm("http://*.my.com/").Contains("http://ohmeohmy.com/"));
			Assert.IsFalse(new Realm("http://*.my.com/").Contains("http://me.com/"));
			Assert.IsFalse(new Realm("http://*.my.com/").Contains("http://my.co/"));
			Assert.IsFalse(new Realm("http://*.my.com/").Contains("http://com/"));
			Assert.IsFalse(new Realm("http://*.www.my.com/").Contains("http://my.com/"));
			Assert.IsFalse(new Realm("http://*.www.my.com/").Contains("http://zzz.my.com/"));
			// These are tested against by the constructor test, as these are invalid wildcard positions.
			////Assert.IsFalse(new Realm("http://*www.my.com/").ValidateUrl("http://bah.www.my.com/"));
			////Assert.IsFalse(new Realm("http://*www.my.com/").ValidateUrl("http://wwww.my.com/"));

			// Among those that should return true, mix up character casing to test for case sensitivity.
			// Host names should be case INSENSITIVE, but paths should probably be case SENSITIVE,
			// because in some systems they are case sensitive and to ignore this would open
			// security holes.
			Assert.IsTrue(new Realm("http://www.my.com/").Contains("http://WWW.MY.COM/"));
			Assert.IsFalse(new Realm("http://www.my.com/abc").Contains("http://www.my.com/ABC"));
		}

		[Test]
		public void ImplicitConversionFromStringTests() {
			Realm realm = "http://host";
			Assert.AreEqual("host", realm.Host);
			realm = (string)null;
			Assert.IsNull(realm);
		}

		[Test]
		public void ImplicitConversionToStringTests() {
			Realm realm = new Realm("http://host/");
			string realmString = realm;
			Assert.AreEqual("http://host/", realmString);
			realm = null;
			realmString = realm;
			Assert.IsNull(realmString);
		}

		[Test]
		public void ImplicitConverstionFromUriTests() {
			Uri uri = new Uri("http://host");
			Realm realm = uri;
			Assert.AreEqual(uri.Host, realm.Host);
			uri = null;
			realm = uri;
			Assert.IsNull(realm);
		}

		[Test]
		public void EqualsTest() {
			Realm testRealm1a = new Realm("http://www.yahoo.com");
			Realm testRealm1b = new Realm("http://www.yahoo.com");
			Realm testRealm2 = new Realm("http://www.yahoo.com/b");
			Realm testRealm3 = new Realm("http://*.www.yahoo.com");

			Assert.AreEqual(testRealm1a, testRealm1b);
			Assert.AreNotEqual(testRealm1a, testRealm2);
			Assert.AreNotEqual(testRealm1a, null);
			Assert.AreNotEqual(testRealm1a, testRealm1a.ToString(), "Although the URLs are equal, different object types shouldn't be equal.");
			Assert.AreNotEqual(testRealm3, testRealm1a, "Wildcard difference ignored by Equals");
		}

		[Test]
		public void MessagePartConvertibility() {
			var message = new MessageWithRealm();
			var messageDescription = new MessageDescription(message.GetType(), new Version(1, 0));
			var messageDictionary = new MessageDictionary(message, messageDescription, false);
			messageDictionary["Realm"] = OpenId.OpenIdTestBase.RPRealmUri.AbsoluteUri;
			Assert.That(messageDictionary["Realm"], Is.EqualTo(OpenId.OpenIdTestBase.RPRealmUri.AbsoluteUri));
		}

		private class MessageWithRealm : TestMessage {
			[MessagePart]
			internal Realm Realm { get; set; }
		}
	}
}
