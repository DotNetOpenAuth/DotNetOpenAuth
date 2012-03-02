//-----------------------------------------------------------------------
// <copyright file="AssociationTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
	using NUnit.Framework;

	[TestFixture]
	public class AssociationTests : OpenIdTestBase {
		private static readonly TimeSpan deltaDateTime = TimeSpan.FromSeconds(2);
		private static readonly HashAlgorithm sha1 = DiffieHellmanUtilities.Lookup(Protocol.Default, Protocol.Default.Args.SessionType.DH_SHA1);
		private byte[] sha1Secret;
		private byte[] sha1Secret2;

		[SetUp]
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

		[Test]
		public void Properties() {
			string handle = "somehandle";
			TimeSpan lifetime = TimeSpan.FromMinutes(2);
			Association assoc = HmacShaAssociation.Create(Protocol.Default, Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA1, handle, this.sha1Secret, lifetime);
			Assert.IsFalse(assoc.IsExpired);
			Assert.That(assoc.Issued, Is.EqualTo(DateTime.UtcNow).Within(deltaDateTime));
			Assert.That(assoc.Expires, Is.EqualTo(DateTime.UtcNow + lifetime).Within(deltaDateTime));
			Assert.That(assoc.Handle, Is.EqualTo(handle));
			Assert.That(assoc.SecondsTillExpiration, Is.EqualTo(lifetime.TotalSeconds).Within(deltaDateTime.TotalSeconds));
			Assert.That(assoc.SecretKey, Is.EqualTo(this.sha1Secret));
			Assert.That(assoc.Issued.Millisecond, Is.EqualTo(0), "No milliseconds because this can be cut off in conversions.");
		}

		[Test]
		public void Sign() {
			Association assoc1 = HmacShaAssociation.Create(Protocol.Default, Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA1, "h1", this.sha1Secret, TimeSpan.FromMinutes(2));
			Association assoc2 = HmacShaAssociation.Create(Protocol.Default, Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA1, "h2", this.sha1Secret2, TimeSpan.FromMinutes(2));

			var data = new byte[] { 0xdd, 0xcc };

			// sign once and verify that it's sane
			byte[] signature1 = assoc1.Sign(data);
			Assert.That(signature1, Is.Not.Null);
			Assert.That(signature1.Length, Is.Not.EqualTo(0));

			// sign again and make sure it's different
			byte[] signature2 = assoc2.Sign(data);
			Assert.That(signature2, Is.Not.Null);
			Assert.That(signature2.Length, Is.Not.EqualTo(0));
			Assert.That(signature1, Is.Not.EqualTo(signature2));

			// sign again with the same secret and make sure it's the same.
			Assert.That(assoc1.Sign(data), Is.EqualTo(signature1));

			// now change the data and make sure signature changes
			data[1] = 0xee;
			Assert.That(assoc1.Sign(data), Is.Not.EqualTo(signature1));
		}
	}
}
