//-----------------------------------------------------------------------
// <copyright file="AssociateDiffieHellmanRequestTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using NUnit.Framework;

	[TestFixture]
	public class AssociateDiffieHellmanRequestTests {
		private static readonly Uri Recipient = new Uri("http://host");
		private AssociateDiffieHellmanRequest request;

		[SetUp]
		public void Setup() {
			this.request = new AssociateDiffieHellmanRequest(Protocol.V20.Version, Recipient);
		}

		[Test]
		public void Ctor() {
			Assert.AreEqual(Recipient, this.request.Recipient);
		}

		[Test]
		public void Mode() {
			Assert.AreEqual("associate", this.request.Mode);
		}
	}
}
