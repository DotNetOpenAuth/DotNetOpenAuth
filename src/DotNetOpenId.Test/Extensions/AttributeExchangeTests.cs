using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class AttributeExchangeTests : ExtensionTestBase {
		const string nicknameTypeUri = "http://axschema.org/namePerson/friendly";
		const string emailTypeUri = "http://axschema.org/contact/email";

		[Test]
		public void None() {
			var fetchResponse = ParameterizedTest<AttributeExchangeFetchResponse>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), null);
			Assert.IsNull(fetchResponse);
			var storeResponse = ParameterizedTest<AttributeExchangeStoreResponse>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), null);
			Assert.IsNull(storeResponse);
		}

		[Test]
		public void Fetch() {
			var request = new AttributeExchangeFetchRequest();
			request.AddAttribute(new AttributeRequest { TypeUri = nicknameTypeUri });
			request.AddAttribute(new AttributeRequest { TypeUri = emailTypeUri, Count = int.MaxValue });
			var response = ParameterizedTest<AttributeExchangeFetchResponse>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), request);
			Assert.IsNotNull(response);
			var att = response.GetAttribute(nicknameTypeUri);
			Assert.IsNotNull(att);
			Assert.AreEqual(nicknameTypeUri, att.TypeUri);
			Assert.AreEqual(1, att.Values.Length);
			Assert.AreEqual("Andrew", att.Values[0]);
			att = response.GetAttribute(emailTypeUri);
			Assert.IsNotNull(att);
			Assert.AreEqual(emailTypeUri, att.TypeUri);
			Assert.AreEqual(2, att.Values.Length);
			Assert.AreEqual("a@a.com", att.Values[0]);
			Assert.AreEqual("b@b.com", att.Values[1]);
		}

		[Test]
		public void FetchLimitEmails() {
			var request = new AttributeExchangeFetchRequest();
			request.AddAttribute(new AttributeRequest { TypeUri = emailTypeUri, Count = 1 });
			var response = ParameterizedTest<AttributeExchangeFetchResponse>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), request);
			Assert.IsNotNull(response);
			var att = response.GetAttribute(emailTypeUri);
			Assert.IsNotNull(att);
			Assert.AreEqual(emailTypeUri, att.TypeUri);
			Assert.AreEqual(1, att.Values.Length);
			Assert.AreEqual("a@a.com", att.Values[0]);
		}

		[Test]
		public void Store() {
			var request = new AttributeExchangeStoreRequest();
			var response = ParameterizedTest<AttributeExchangeStoreResponse>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), request);
			Assert.IsNotNull(response);
		}
	}
}
