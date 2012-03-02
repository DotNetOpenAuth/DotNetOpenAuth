//-----------------------------------------------------------------------
// <copyright file="AttributeExchangeRoundtripTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using NUnit.Framework;

	[TestFixture]
	public class AttributeExchangeRoundtripTests : OpenIdTestBase {
		private const string NicknameTypeUri = WellKnownAttributes.Name.Alias;
		private const string EmailTypeUri = WellKnownAttributes.Contact.Email;
		private const string IncrementingAttribute = "http://incatt";
		private int incrementingAttributeValue = 1;

		[Test]
		public void Fetch() {
			var request = new FetchRequest();
			request.Attributes.Add(new AttributeRequest(NicknameTypeUri));
			request.Attributes.Add(new AttributeRequest(EmailTypeUri, false, int.MaxValue));

			var response = new FetchResponse();
			response.Attributes.Add(new AttributeValues(NicknameTypeUri, "Andrew"));
			response.Attributes.Add(new AttributeValues(EmailTypeUri, "a@a.com", "b@b.com"));

			ExtensionTestUtilities.Roundtrip(Protocol.Default, new[] { request }, new[] { response });
		}

		[Test]
		public void Store() {
			var request = new StoreRequest();
			var newAttribute = new AttributeValues(
				IncrementingAttribute,
				"val" + (this.incrementingAttributeValue++).ToString(),
				"val" + (this.incrementingAttributeValue++).ToString());
			request.Attributes.Add(newAttribute);

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
