//-----------------------------------------------------------------------
// <copyright file="DirectErrorResponseTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class DirectErrorResponseTests {
		private DirectErrorResponse response;

		[TestInitialize]
		public void Setup() {
			this.response = new DirectErrorResponse();
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