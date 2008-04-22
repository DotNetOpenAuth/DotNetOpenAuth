using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Extensions.AttributeExchange;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class AttributeExchangeTests : ExtensionTestBase {
		const string nicknameTypeUri = AttributeExchangeConstants.Name.Alias;
		const string emailTypeUri = AttributeExchangeConstants.Contact.Email;
		const string incrementingAttribute = "http://incatt";
		int incrementingAttributeValue = 1;

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
			var newAttribute = new AttributeValues {
				TypeUri = incrementingAttribute,
				Values = new[] { 
					"val" + (incrementingAttributeValue++).ToString(), 
					"val" + (incrementingAttributeValue++).ToString()
				}
			};
			request.AddAttribute(newAttribute);

			var response = ParameterizedTest<AttributeExchangeStoreResponse>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), request);
			Assert.IsNotNull(response);
			Assert.IsTrue(response.Succeeded);
			Assert.IsNull(response.FailureReason);

			var fetchRequest = new AttributeExchangeFetchRequest();
			fetchRequest.AddAttribute(new AttributeRequest { TypeUri = incrementingAttribute });
			var fetchResponse = ParameterizedTest<AttributeExchangeFetchResponse>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), fetchRequest);
			Assert.IsNotNull(fetchResponse);
			var att = fetchResponse.GetAttribute(incrementingAttribute);
			Assert.IsNotNull(att);
			Assert.AreEqual(newAttribute.Values.Length, att.Values.Length);
			for (int i = 0; i < newAttribute.Values.Length; i++)
				Assert.AreEqual(newAttribute.Values[i], att.Values[i]);
		}

		/// <summary>
		/// Tests that two extensions that use the same namespace cannot 
		/// both be added to the message at once, per the spec.
		/// </summary>
		[Test, ExpectedException(typeof(OpenIdException))]
		public void FetchAndStore() {
			var identityUrl = TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version);
			var returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);
			var realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
			var consumer = new OpenIdRelyingParty(AppStore, null);
			var request = consumer.CreateRequest(identityUrl, realm, returnTo);
			request.AddExtension(new AttributeExchangeFetchRequest());
			request.AddExtension(new AttributeExchangeStoreRequest());
		}
	}
}
