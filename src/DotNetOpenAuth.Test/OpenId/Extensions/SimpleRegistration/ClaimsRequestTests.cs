//-----------------------------------------------------------------------
// <copyright file="ClaimsRequestTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using NUnit.Framework;

	[TestFixture]
	public class ClaimsRequestTests : OpenIdTestBase {
		[Test]
		public void CreateResponse() {
			// some unofficial type URIs...
			this.ParameterizedTypeUriPreservedTest("http://openid.net/sreg/1.0");
			this.ParameterizedTypeUriPreservedTest("http://openid.net/sreg/1.1");
			// and the official one.
			this.ParameterizedTypeUriPreservedTest("http://openid.net/extensions/sreg/1.1");
		}

		[Test]
		public void RequiredOptionalLists() {
			ClaimsRequest req = new ClaimsRequest();
			MessageDictionary dictionary = this.MessageDescriptions.GetAccessor(req);
			Assert.AreEqual(string.Empty, dictionary["required"]);
			Assert.AreEqual(string.Empty, dictionary["optional"]);

			req.BirthDate = DemandLevel.Request;
			req.Nickname = DemandLevel.Require;
			Assert.AreEqual("dob", dictionary["optional"]);
			Assert.AreEqual("nickname", dictionary["required"]);

			req.PostalCode = DemandLevel.Require;
			req.Gender = DemandLevel.Request;
			Assert.AreEqual("dob,gender", dictionary["optional"]);
			Assert.AreEqual("nickname,postcode", dictionary["required"]);
		}

		[Test]
		public void EqualityTests() {
			ClaimsRequest req1 = new ClaimsRequest();
			ClaimsRequest req2 = new ClaimsRequest();
			Assert.AreEqual(req1, req2);

			req1.BirthDate = DemandLevel.Request;
			Assert.AreNotEqual(req1, req2);

			req2.BirthDate = DemandLevel.Request;
			req1.Country = DemandLevel.Request;
			Assert.AreNotEqual(req1, req2);
		}

		private void ParameterizedTypeUriPreservedTest(string typeUri) {
			ClaimsRequest request = new ClaimsRequest(typeUri);
			ClaimsResponse response = request.CreateResponse();
			Assert.AreEqual(typeUri, ((IOpenIdMessageExtension)response).TypeUri);
		}
	}
}
