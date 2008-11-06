//-----------------------------------------------------------------------
// <copyright file="AssociateRequestTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AssociateRequestTests {
		private Uri secureRecipient = new Uri("https://hi");
		private Uri insecureRecipient = new Uri("http://hi");
		private AssociateRequest request;

		[TestInitialize]
		public void Setup() {
			this.request = new AssociateRequest(this.secureRecipient);
		}

		[TestMethod]
		public void ConstructorTest() {
			Assert.AreEqual(this.secureRecipient, this.request.Recipient);
		}

		[TestMethod]
		public void MessagePartsTest() {
			this.request.AssociationType = "HMAC-SHA1";
			this.request.SessionType = "no-encryption";

			Assert.AreEqual("associate", this.request.Mode);
			Assert.AreEqual("HMAC-SHA1", this.request.AssociationType);
			Assert.AreEqual("no-encryption", this.request.SessionType);

			var dict = new MessageDictionary(this.request);
			Assert.AreEqual(Protocol.OpenId2Namespace, dict["openid.ns"]);
			Assert.AreEqual("associate", dict["openid.mode"]);
			Assert.AreEqual("HMAC-SHA1", dict["openid.assoc_type"]);
			Assert.AreEqual("no-encryption", dict["openid.session_type"]);
		}

		[TestMethod]
		public void ValidMessageTest() {
			this.request = new AssociateRequest(this.secureRecipient);
			this.request.AssociationType = "HMAC-SHA1";
			this.request.SessionType = "no-encryption";
			this.request.EnsureValidMessage();
		}

		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void InvalidMessageTest() {
			this.request = new AssociateRequest(this.insecureRecipient);
			this.request.AssociationType = "HMAC-SHA1";
			this.request.SessionType = "no-encryption";
			this.request.EnsureValidMessage(); // no-encryption only allowed for secure channels.
		}
	}
}
