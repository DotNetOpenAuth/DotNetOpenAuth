//-----------------------------------------------------------------------
// <copyright file="AssociationsTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.OpenId;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AssociationsTests : OpenIdTestBase {
		private static readonly HashAlgorithm sha1 = DiffieHellmanUtilities.Lookup(Protocol.Default, Protocol.Default.Args.SessionType.DH_SHA1);
		private byte[] sha1Secret;
		private Associations assocs;

		[TestInitialize]
		public override void SetUp() {
			this.sha1Secret = new byte[sha1.HashSize / 8];
			this.assocs = new Associations();
		}

		[TestMethod]
		public void GetNonexistentHandle() {
			Assert.IsNull(this.assocs.Get("someinvalidhandle"));
		}

		[TestMethod]
		public void RemoveNonexistentHandle() {
			Assert.IsFalse(this.assocs.Remove("someinvalidhandle"));
		}

		[TestMethod]
		public void HandleLifecycle() {
			Association a = HmacShaAssociation.Create(
				Protocol.Default,
				Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA1,
				"somehandle",
				this.sha1Secret,
				TimeSpan.FromDays(1));
			this.assocs.Set(a);
			Assert.AreSame(a, this.assocs.Get(a.Handle));
			Assert.IsTrue(this.assocs.Remove(a.Handle));
			Assert.IsNull(this.assocs.Get(a.Handle));
			Assert.IsFalse(this.assocs.Remove(a.Handle));
		}

		[TestMethod]
		public void Best() {
			Association a = HmacShaAssociation.Create(
				Protocol.Default,
				Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA1,
				"h1",
				this.sha1Secret,
				TimeSpan.FromHours(1));
			Association b = HmacShaAssociation.Create(
				Protocol.Default,
				Protocol.Default.Args.SignatureAlgorithm.HMAC_SHA1,
				"h2",
				this.sha1Secret,
				TimeSpan.FromHours(1));

			this.assocs.Set(a);
			this.assocs.Set(b);

			// make b the best by making a older
			a.Issued -= TimeSpan.FromHours(1);
			Assert.AreSame(b, this.assocs.Best.FirstOrDefault());
			// now make a the best
			b.Issued -= TimeSpan.FromHours(2);
			Assert.AreSame(a, this.assocs.Best.FirstOrDefault());
		}
	}
}
