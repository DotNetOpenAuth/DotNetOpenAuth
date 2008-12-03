//-----------------------------------------------------------------------
// <copyright file="AssociationTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AssociationTests : OpenIdTestBase {
		private static readonly TimeSpan deltaDateTime = TimeSpan.FromSeconds(2);
		private static readonly HashAlgorithm sha1 = DiffieHellmanUtilities.Lookup(Protocol.Default, Protocol.Default.Args.SessionType.DH_SHA1);
		private byte[] sha1Secret;
		private byte[] sha1Secret2;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			// just a little something to make it at all interesting.
			this.sha1Secret = new byte[sha1.HashSize / 8];
			this.sha1Secret[0] = 0x33;
			this.sha1Secret[1] = 0x55;

			this.sha1Secret2 = new byte[sha1.HashSize / 8];
			this.sha1Secret2[0] = 0x88;
			this.sha1Secret2[1] = 0xcc;
		}

		[TestMethod]
		public void Properties() {
			string handle = "somehandle";
			TimeSpan lifetime = TimeSpan.FromMinutes(2);
			Association assoc = HmacShaAssociation.Create(Protocol.Default, Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA1, handle, this.sha1Secret, lifetime);
			Assert.IsFalse(assoc.IsExpired);
			Assert.IsTrue(Math.Abs((DateTime.Now - assoc.Issued.ToLocalTime()).TotalSeconds) < deltaDateTime.TotalSeconds);
			Assert.IsTrue(Math.Abs((DateTime.Now.ToLocalTime() + lifetime - assoc.Expires.ToLocalTime()).TotalSeconds) < deltaDateTime.TotalSeconds);
			Assert.AreEqual(handle, assoc.Handle);
			Assert.IsTrue(Math.Abs(lifetime.TotalSeconds - assoc.SecondsTillExpiration) < deltaDateTime.TotalSeconds);
			Assert.IsTrue(MessagingUtilities.AreEquivalent(this.sha1Secret, assoc.SecretKey));
			Assert.AreEqual(0, assoc.Issued.Millisecond, "No milliseconds because this can be cut off in conversions.");
		}

		[TestMethod]
		public void Sign() {
			Association assoc1 = HmacShaAssociation.Create(Protocol.Default, Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA1, "h1", this.sha1Secret, TimeSpan.FromMinutes(2));
			Association assoc2 = HmacShaAssociation.Create(Protocol.Default, Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA1, "h2", this.sha1Secret2, TimeSpan.FromMinutes(2));

			var data = new byte[] { 0xdd, 0xcc };

			// sign once and verify that it's sane
			byte[] signature1 = assoc1.Sign(data);
			Assert.IsNotNull(signature1);
			Assert.AreNotEqual(0, signature1.Length);

			// sign again and make sure it's different
			byte[] signature2 = assoc2.Sign(data);
			Assert.IsNotNull(signature2);
			Assert.AreNotEqual(0, signature2.Length);
			Assert.IsFalse(MessagingUtilities.AreEquivalent(signature1, signature2));

			// sign again with the same secret and make sure it's the same.
			Assert.IsTrue(MessagingUtilities.AreEquivalent(signature1, assoc1.Sign(data)));

			// now change the data and make sure signature changes
			data[1] = 0xee;
			Assert.IsFalse(MessagingUtilities.AreEquivalent(signature1, assoc1.Sign(data)));
		}
	}
}
