//-----------------------------------------------------------------------
// <copyright file="DiffieHellmanTests.cs" company="Jason Alexander, Outercurve Foundation">
//     Copyright (c) Jason Alexander, Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.IO;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using NUnit.Framework;
	using Org.Mentalis.Security.Cryptography;

	[TestFixture]
	public class DiffieHellmanTests : OpenIdTestBase {
		[Test]
		public void Test() {
			string s1 = Test1();
			string s2 = Test1();

			Assert.AreNotEqual(s1, s2, "Secret keys should NOT be the same.");
		}

		[Test, Timeout(15000), Category("Slow"), Category("Performance")]
		public void TestPublic() {
			TextReader reader = new StringReader(OpenIdTestBase.LoadEmbeddedFile("dhpriv.txt"));

			try {
				string line;
				int lineNumber = 0;
				while ((line = reader.ReadLine()) != null) {
					TestUtilities.TestLogger.InfoFormat("\tLine {0}", ++lineNumber);
					string[] parts = line.Trim().Split(' ');
					byte[] x = Convert.FromBase64String(parts[0]);
					DiffieHellmanManaged dh = new DiffieHellmanManaged(AssociateDiffieHellmanRequest.DefaultMod, AssociateDiffieHellmanRequest.DefaultGen, x);
					byte[] pub = dh.CreateKeyExchange();
					byte[] y = Convert.FromBase64String(parts[1]);

					if (y[0] == 0 && y[1] <= 127) {
						y.CopyTo(y, 1);
					}

					Assert.AreEqual(
						Convert.ToBase64String(y),
						Convert.ToBase64String(DiffieHellmanUtilities.EnsurePositive(pub)),
						line);
				}
			} finally {
				reader.Close();
			}
		}

		private static string Test1() {
			DiffieHellman dh1 = new DiffieHellmanManaged();
			DiffieHellman dh2 = new DiffieHellmanManaged();

			string secret1 = Convert.ToBase64String(dh1.DecryptKeyExchange(dh2.CreateKeyExchange()));
			string secret2 = Convert.ToBase64String(dh2.DecryptKeyExchange(dh1.CreateKeyExchange()));

			Assert.AreEqual(secret1, secret2, "Secret keys do not match for some reason.");

			return secret1;
		}
	}
}
