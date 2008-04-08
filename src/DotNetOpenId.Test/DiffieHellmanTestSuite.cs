using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Org.Mentalis.Security.Cryptography;
using NUnit.Framework;

namespace DotNetOpenId.Test {
	public static class DHTestUtil {
		public static string Test1() {
			DiffieHellman dh1 = CryptUtil.CreateDiffieHellman();
			DiffieHellman dh2 = CryptUtil.CreateDiffieHellman();

			string secret1 = Convert.ToBase64String(dh1.DecryptKeyExchange(dh2.CreateKeyExchange()));
			string secret2 = Convert.ToBase64String(dh2.DecryptKeyExchange(dh1.CreateKeyExchange()));

			Assert.AreEqual(secret1, secret2, "Secret keys do not match for some reason.");

			return secret1;
		}
	}

	[TestFixture]
	public class DiffieHellmanTestSuite {

		[Test]
		public void Test() {
			string s1 = DHTestUtil.Test1();
			string s2 = DHTestUtil.Test1();

			Assert.AreNotEqual(s1, s2, "Secret keys should NOT be the same.");
		}

		[Test, Explicit("Test is slow.")]
		public void TestPublic() {
			StreamReader sr = new StreamReader(@"..\..\src\DotNetOpenId.Test\dhpriv.txt");

			try {
				string line;
				while ((line = sr.ReadLine()) != null) {
					string[] parts = line.Trim().Split(' ');
					byte[] x = Convert.FromBase64String(parts[0]);
					DiffieHellmanManaged dh = new DiffieHellmanManaged(CryptUtil.DEFAULT_MOD, CryptUtil.DEFAULT_GEN, x);
					byte[] pub = dh.CreateKeyExchange();
					byte[] y = Convert.FromBase64String(parts[1]);

					if (y[0] == 0 && y[1] <= 127)
						y.CopyTo(y, 1);

					Assert.AreEqual(y, Convert.FromBase64String(CryptUtil.UnsignedToBase64(pub)), line);
				}
			} finally {
				sr.Close();
			}
		}

	}

}
