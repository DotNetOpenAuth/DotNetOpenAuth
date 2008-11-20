//-----------------------------------------------------------------------
// <copyright file="DirectErrorResponseTests.cs" company="Andrew Arnott">
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
	public class DirectErrorResponseTests : OpenIdTestBase {
		private DirectErrorResponse response;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			var request = new AssociateUnencryptedRequest(Protocol.V20.Version, new Uri("http://host"));
			this.response = new DirectErrorResponse(request);
		}

		[TestMethod]
		public void ParameterNames() {
			this.response.ErrorMessage = "Some Error";
			this.response.Contact = "Andrew Arnott";
			this.response.Reference = "http://blog.nerdbank.net/";

			MessageSerializer serializer = MessageSerializer.Get(this.response.GetType());
			var fields = serializer.Serialize(this.response);
			Assert.AreEqual(Protocol.OpenId2Namespace, fields["ns"]);
			Assert.AreEqual("Some Error", fields["error"]);
			Assert.AreEqual("Andrew Arnott", fields["contact"]);
			Assert.AreEqual("http://blog.nerdbank.net/", fields["reference"]);
		}
	}
}