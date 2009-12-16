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
		const string nicknameTypeUri = WellKnownAttributes.Name.Alias;
		const string emailTypeUri = WellKnownAttributes.Contact.Email;
		const string incrementingAttribute = "http://incatt";
		int incrementingAttributeValue = 1;

		[Test]
		public void None() {
			var fetchResponse = ParameterizedTest<FetchResponse>(
				TestSupport.Scenarios.ExtensionFullCooperation, Version, null);
			Assert.IsNull(fetchResponse);
			var storeResponse = ParameterizedTest<StoreResponse>(
				TestSupport.Scenarios.ExtensionFullCooperation, Version, null);
			Assert.IsNull(storeResponse);
		}

		[Test]
		public void Fetch() {
			var request = new FetchRequest();
			request.AddAttribute(new AttributeRequest(nicknameTypeUri));
			request.AddAttribute(new AttributeRequest(emailTypeUri, false, int.MaxValue));
			var response = ParameterizedTest<FetchResponse>(
				TestSupport.Scenarios.ExtensionFullCooperation, Version, request);
			Assert.IsNotNull(response);
			var att = response.GetAttribute(nicknameTypeUri);
			Assert.IsNotNull(att);
			Assert.AreEqual(nicknameTypeUri, att.TypeUri);
			Assert.AreEqual(1, att.Values.Count);
			Assert.AreEqual("Andrew", att.Values[0]);
			att = response.GetAttribute(emailTypeUri);
			Assert.IsNotNull(att);
			Assert.AreEqual(emailTypeUri, att.TypeUri);
			Assert.AreEqual(2, att.Values.Count);
			Assert.AreEqual("a@a.com", att.Values[0]);
			Assert.AreEqual("b@b.com", att.Values[1]);
		}

		[Test]
		public void FetchLimitEmails() {
			var request = new FetchRequest();
			request.AddAttribute(new AttributeRequest { TypeUri = emailTypeUri, Count = 1 });
			var response = ParameterizedTest<FetchResponse>(
				TestSupport.Scenarios.ExtensionFullCooperation, Version, request);
			Assert.IsNotNull(response);
			var att = response.GetAttribute(emailTypeUri);
			Assert.IsNotNull(att);
			Assert.AreEqual(emailTypeUri, att.TypeUri);
			Assert.AreEqual(1, att.Values.Count);
			Assert.AreEqual("a@a.com", att.Values[0]);
		}

		[Test]
		public void Store() {
			var request = new StoreRequest();
			var newAttribute = new AttributeValues(incrementingAttribute,
				"val" + (incrementingAttributeValue++).ToString(), 
				"val" + (incrementingAttributeValue++).ToString()
			);
			request.AddAttribute(newAttribute);

			var response = ParameterizedTest<StoreResponse>(
				TestSupport.Scenarios.ExtensionFullCooperation, Version, request);
			Assert.IsNotNull(response);
			Assert.IsTrue(response.Succeeded);
			Assert.IsNull(response.FailureReason);

			var fetchRequest = new FetchRequest();
			fetchRequest.AddAttribute(new AttributeRequest { TypeUri = incrementingAttribute });
			var fetchResponse = ParameterizedTest<FetchResponse>(
				TestSupport.Scenarios.ExtensionFullCooperation, Version, fetchRequest);
			Assert.IsNotNull(fetchResponse);
			var att = fetchResponse.GetAttribute(incrementingAttribute);
			Assert.IsNotNull(att);
			Assert.AreEqual(newAttribute.Values.Count, att.Values.Count);
			for (int i = 0; i < newAttribute.Values.Count; i++)
				Assert.AreEqual(newAttribute.Values[i], att.Values[i]);
		}

		/// <summary>
		/// Tests that two extensions that use the same namespace cannot 
		/// both be added to the message at once, per the spec.
		/// </summary>
		[Test, ExpectedException(typeof(OpenIdException))]
		public void FetchAndStore() {
			var request = TestSupport.CreateRelyingPartyRequest(false, TestSupport.Scenarios.ExtensionFullCooperation, Version, false);
			request.AddExtension(new FetchRequest());
			request.AddExtension(new StoreRequest());
		}
	}
}
