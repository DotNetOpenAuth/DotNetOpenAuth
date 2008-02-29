using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace DotNetOpenId.Test
{
	[TestFixture]
	public class AssociationTestSuite {
		static readonly TimeSpan deltaDateTime = TimeSpan.FromSeconds(2);

		[Test]
		public void Properties() {
			string handle = "somehandle";
			byte[] key = new byte[] { 0x33, 0x55 };
			TimeSpan lifetime = TimeSpan.FromMinutes(2);
			Association assoc = new HmacSha1Association(handle, key, lifetime);
			Assert.IsFalse(assoc.IsExpired);
			Assert.IsTrue(Math.Abs((DateTime.Now - assoc.Issued.ToLocalTime()).TotalSeconds) < deltaDateTime.TotalSeconds);
			Assert.IsTrue(Math.Abs((DateTime.Now.ToLocalTime() + lifetime - assoc.Expires.ToLocalTime()).TotalSeconds) < deltaDateTime.TotalSeconds);
			Assert.AreEqual(handle, assoc.Handle);
			Assert.IsTrue(Math.Abs(lifetime.TotalSeconds - assoc.SecondsTillExpiration) < deltaDateTime.TotalSeconds);
			Assert.IsTrue(Util.ArrayEquals(key, assoc.SecretKey));
			Assert.AreEqual(0, assoc.Issued.Millisecond, "No milliseconds because this can be cut off in conversions.");
		}

		[Test]
		public void Sign() {
			Association assoc1 = new HmacSha1Association("h1", Encoding.ASCII.GetBytes("secret1"), TimeSpan.FromMinutes(2));
			Association assoc2 = new HmacSha1Association("h2", Encoding.ASCII.GetBytes("secret2"), TimeSpan.FromMinutes(2));

			var dict = new Dictionary<string, string>();
			dict.Add("a", "b");
			dict.Add("c", "d");

			// sign once and verify that it's sane
			byte[] signature1 = assoc1.Sign(dict);
			Assert.IsNotNull(signature1);
			Assert.AreNotEqual(0, signature1.Length);

			// sign again and make sure it's different
			byte[] signature2 = assoc2.Sign(dict);
			Assert.IsNotNull(signature2);
			Assert.AreNotEqual(0, signature2.Length);
			Assert.IsFalse(Util.ArrayEquals(signature1, signature2));

			// sign again with the same secret and make sure it's the same.
			Assert.IsTrue(Util.ArrayEquals(signature1, assoc1.Sign(dict)));

			// now add data and make sure signature changes
			dict.Add("g", "h");
			Assert.IsFalse(Util.ArrayEquals(signature1, assoc1.Sign(dict)));

			// now change existing data.
			dict.Remove("g");
			dict["c"] = "e";
			Assert.IsFalse(Util.ArrayEquals(signature1, assoc1.Sign(dict)));
			dict.Remove("c");
			Assert.IsFalse(Util.ArrayEquals(signature1, assoc1.Sign(dict)));
		}

		[Test]
		public void SignSome() {
			Association assoc = new HmacSha1Association("h1", Encoding.ASCII.GetBytes("secret1"), TimeSpan.FromMinutes(2));
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
