//-----------------------------------------------------------------------
// <copyright file="DirectErrorResponseTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using NUnit.Framework;

	[TestFixture]
	public class DirectErrorResponseTests : OpenIdTestBase {
		private DirectErrorResponse response;

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			var request = new AssociateUnencryptedRequest(Protocol.V20.Version, new Uri("http://host"));
			this.response = new DirectErrorResponse(request.Version, request);
		}

		[Test]
		public void ParameterNames() {
			this.response.ErrorMessage = "Some Error";
			this.response.Contact = "Andrew Arnott";
			this.response.Reference = "http://blog.nerdbank.net/";

			MessageSerializer serializer = MessageSerializer.Get(this.response.GetType());
			var fields = this.MessageDescriptions.GetAccessor(this.response).Serialize();
			Assert.AreEqual(Protocol.OpenId2Namespace, fields["ns"]);
			Assert.AreEqual("Some Error", fields["error"]);
			Assert.AreEqual("Andrew Arnott", fields["contact"]);
			Assert.AreEqual("http://blog.nerdbank.net/", fields["reference"]);
		}

		/// <summary>
		/// Verifies that error messages are created as HTTP 400 errors,
		/// per OpenID 2.0 section 5.1.2.2.
		/// </summary>
		[Test]
		public void ErrorMessagesAsHttp400() {
			var httpStatusMessage = (IHttpDirectResponse)this.response;
			Assert.AreEqual(HttpStatusCode.BadRequest, httpStatusMessage.HttpStatusCode);
		}
	}
}