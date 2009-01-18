//-----------------------------------------------------------------------
// <copyright file="AttributeExchangeRoundtripTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AttributeExchangeRoundtripTests : OpenIdTestBase {
		private const string NicknameTypeUri = WellKnownAttributes.Name.Alias;
		private const string EmailTypeUri = WellKnownAttributes.Contact.Email;
		private const string IncrementingAttribute = "http://incatt";
		private int incrementingAttributeValue = 1;

		[TestMethod]
		public void Fetch() {
			var request = new FetchRequest();
			request.AddAttribute(new AttributeRequest(NicknameTypeUri));
			request.AddAttribute(new AttributeRequest(EmailTypeUri, false, int.MaxValue));

			var response = new FetchResponse();
			response.AddAttribute(new AttributeValues(NicknameTypeUri, "Andrew"));
			response.AddAttribute(new AttributeValues(EmailTypeUri, "a@a.com", "b@b.com"));

			ExtensionTestUtilities.Roundtrip(Protocol.Default, new[] { request }, new[] { response });
		}

		[TestMethod]
		public void Store() {
			var request = new StoreRequest();
			var newAttribute = new AttributeValues(
				IncrementingAttribute,
				"val" + (incrementingAttributeValue++).ToString(),
				"val" + (incrementingAttributeValue++).ToString());
			request.AddAttribute(newAttribute);

			var successResponse = new StoreResponse();
			successResponse.Succeeded = true;

			ExtensionTestUtilities.Roundtrip(Protocol.Default, new[] { request }, new[] { successResponse });

			var failureResponse = new StoreResponse();
			failureResponse.Succeeded = false;
			failureResponse.FailureReason = "Some error";

			ExtensionTestUtilities.Roundtrip(Protocol.Default, new[] { request }, new[] { failureResponse });
		}
	}
}
