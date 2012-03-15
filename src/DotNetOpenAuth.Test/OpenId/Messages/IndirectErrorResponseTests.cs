//-----------------------------------------------------------------------
// <copyright file="IndirectErrorResponseTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class IndirectErrorResponseTests : OpenIdTestBase {
		private IndirectErrorResponse response;

		[SetUp]
		public void Setup() {
			CheckIdRequest request = new CheckIdRequest(Protocol.V20.Version, OPUri, AuthenticationRequestMode.Immediate);
			request.ReturnTo = RPUri;
			this.response = new IndirectErrorResponse(request);
		}

		[Test]
		public void Ctor() {
			Assert.AreEqual(RPUri, this.response.Recipient);
		}

		[Test]
		public void ParameterNames() {
			this.response.ErrorMessage = "Some Error";
			this.response.Contact = "Andrew Arnott";
			this.response.Reference = "http://blog.nerdbank.net/";

			MessageSerializer serializer = MessageSerializer.Get(this.response.GetType());
			var fields = this.MessageDescriptions.GetAccessor(this.response).Serialize();
			Assert.AreEqual(Protocol.OpenId2Namespace, fields["openid.ns"]);
			Assert.AreEqual("Some Error", fields["openid.error"]);
			Assert.AreEqual("Andrew Arnott", fields["openid.contact"]);
			Assert.AreEqual("http://blog.nerdbank.net/", fields["openid.reference"]);
		}
	}
}