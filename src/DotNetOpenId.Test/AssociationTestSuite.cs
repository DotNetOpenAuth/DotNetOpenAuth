using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace DotNetOpenId.Test
{
	[TestFixture]
	public class AssociationTestSuite {
		static readonly TimeSpan deltaDateTime = TimeSpan.FromSeconds(2);
		byte[] sha1Secret = new byte[CryptUtil.Sha1.HashSize / 8];
		byte[] sha1Secret2 = new byte[CryptUtil.Sha1.HashSize / 8];

		public AssociationTestSuite() {
			// just a little something to make it at all interesting.
			sha1Secret[0] = 0x33;
			sha1Secret[1] = 0x55;

			sha1Secret2[0] = 0x88;
			sha1Secret2[1] = 0xcc;
		}

		[Test]
		public void Properties() {
			string handle = "somehandle";
			TimeSpan lifetime = TimeSpan.FromMinutes(2);
			Association assoc = new HmacSha1Association(handle, sha1Secret, lifetime);
			Assert.IsFalse(assoc.IsExpired);
			Assert.IsTrue(Math.Abs((DateTime.Now - assoc.Issued.ToLocalTime()).TotalSeconds) < deltaDateTime.TotalSeconds);
			Assert.IsTrue(Math.Abs((DateTime.Now.ToLocalTime() + lifetime - assoc.Expires.ToLocalTime()).TotalSeconds) < deltaDateTime.TotalSeconds);
			Assert.AreEqual(handle, assoc.Handle);
			Assert.IsTrue(Math.Abs(lifetime.TotalSeconds - assoc.SecondsTillExpiration) < deltaDateTime.TotalSeconds);
			Assert.IsTrue(Util.ArrayEquals(sha1Secret, assoc.SecretKey));
			Assert.AreEqual(0, assoc.Issued.Millisecond, "No milliseconds because this can be cut off in conversions.");
		}

		[Test]
		public void Sign() {
			Association assoc1 = new HmacSha1Association("h1", sha1Secret, TimeSpan.FromMinutes(2));
			Association assoc2 = new HmacSha1Association("h2", sha1Secret2, TimeSpan.FromMinutes(2));

			var dict = new Dictionary<string, string>();
			dict.Add("a", "b");
			dict.Add("c", "d");
			var keys = new List<string>();
			keys.Add("a");
			keys.Add("c");

			// sign once and verify that it's sane
			byte[] signature1 = assoc1.Sign(dict, keys);
			Assert.IsNotNull(signature1);
			Assert.AreNotEqual(0, signature1.Length);

			// sign again and make sure it's different
			byte[] signature2 = assoc2.Sign(dict, keys);
			Assert.IsNotNull(signature2);
			Assert.AreNotEqual(0, signature2.Length);
			Assert.IsFalse(Util.ArrayEquals(signature1, signature2));

			// sign again with the same secret and make sure it's the same.
			Assert.IsTrue(Util.ArrayEquals(signature1, assoc1.Sign(dict, keys)));

			// now add data and make sure signature changes
			dict.Add("g", "h");
			keys.Add("g");
			Assert.IsFalse(Util.ArrayEquals(signature1, assoc1.Sign(dict, keys)));

			// now change existing data.
			dict.Remove("g");
			keys.Remove("g");
			dict["c"] = "e";
			Assert.IsFalse(Util.ArrayEquals(signature1, assoc1.Sign(dict, keys)));
			dict.Remove("c");
			keys.Remove("c");
			Assert.IsFalse(Util.ArrayEquals(signature1, assoc1.Sign(dict, keys)));
		}

		[Test]
		public void SignSome() {
			Association assoc = new HmacSha1Association("h1", sha1Secret, TimeSpan.FromMinutes(2));
			const string prefix = "q.";

			var dict = new Dictionary<string, string>();
			dict.Add("q.a", "b");
			dict.Add("q.c", "d");
			dict.Add("q.e", "f");

			var signKeys = new List<string> {"a", "c"}; // don't sign e

			byte[] sig1 = assoc.Sign(dict, signKeys, prefix);

			// change the unsigned value and verify that the sig doesn't change
			dict["q.e"] = "g";
			Assert.IsTrue(Util.ArrayEquals(sig1, assoc.Sign(dict, signKeys, prefix)));
			// change a signed value and verify that the sig does change.
			dict["q.c"] = "D";
			Assert.IsFalse(Util.ArrayEquals(sig1, assoc.Sign(dict, signKeys, prefix)));

			// change the ordering of signed fields and verify that the signature changes.
			dict["q.c"] = "d"; // put this back first.
			Assert.IsTrue(Util.ArrayEquals(sig1, assoc.Sign(dict, signKeys, prefix)));
			signKeys.Insert(0, signKeys[1]);
			signKeys.RemoveAt(2);
			Assert.IsFalse(Util.ArrayEquals(sig1, assoc.Sign(dict, signKeys, prefix)));
		}
	}
}
