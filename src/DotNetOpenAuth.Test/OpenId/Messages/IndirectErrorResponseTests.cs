//-----------------------------------------------------------------------
// <copyright file="IndirectErrorResponseTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class IndirectErrorResponseTests {
		private IndirectErrorResponse response;
		private Uri recipient = new Uri("http://host");

		[TestInitialize]
		public void Setup() {
			this.response = new IndirectErrorResponse(Protocol.V20.Version, this.recipient);
		}

		[TestMethod]
		public void Ctor() {
			Assert.AreEqual(this.recipient, this.response.Recipient);
		}

		[TestMethod]
		public void ParameterNames() {
			this.response.ErrorMessage = "Some Error";
			this.response.Contact = "Andrew Arnott";
			this.response.Reference = "http://blog.nerdbank.net/";

			MessageSerializer serializer = MessageSerializer.Get(this.response.GetType());
			var fields = serializer.Serialize(this.response);
			Assert.AreEqual(Protocol.OpenId2Namespace, fields["openid.ns"]);
			Assert.AreEqual("Some Error", fields["openid.error"]);
			Assert.AreEqual("Andrew Arnott", fields["openid.contact"]);
			Assert.AreEqual("http://blog.nerdbank.net/", fields["openid.reference"]);
		}
	}
}