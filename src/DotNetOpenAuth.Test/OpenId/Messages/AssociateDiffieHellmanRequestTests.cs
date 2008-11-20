//-----------------------------------------------------------------------
// <copyright file="AssociateDiffieHellmanRequestTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AssociateDiffieHellmanRequestTests {
		private static readonly Uri Recipient = new Uri("http://host");
		private AssociateDiffieHellmanRequest request;

		[TestInitialize]
		public void Setup() {
			this.request = new AssociateDiffieHellmanRequest(Protocol.V20.Version, Recipient);
		}

		[TestMethod]
		public void Ctor() {
			Assert.AreEqual(Recipient, this.request.Recipient);
		}

		[TestMethod]
		public void Mode() {
			Assert.AreEqual("associate", this.request.Mode);
		}
	}
}
