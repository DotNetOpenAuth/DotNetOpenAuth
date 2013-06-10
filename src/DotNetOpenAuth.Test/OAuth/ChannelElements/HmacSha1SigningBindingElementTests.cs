//-----------------------------------------------------------------------
// <copyright file="HmacSha1SigningBindingElementTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth.ChannelElements {
	using System.Net.Http;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class HmacSha1SigningBindingElementTests : MessagingTestBase {
		[Test]
		public void SignatureTest() {
			UnauthorizedTokenRequest message = SigningBindingElementBaseTests.CreateTestRequestTokenMessage(this.MessageDescriptions, null);

			var hmac = new HmacSha1SigningBindingElement();
			hmac.Channel = new TestChannel(this.MessageDescriptions);
			Assert.AreEqual("kR0LhH8UqylaLfR/esXVVlP4sQI=", hmac.GetSignatureTestHook(message));
		}

		[Test]
		public void LinkedInInteropTest() {
			var endpoint = new MessageReceivingEndpoint("https://api.linkedin.com/v1/people/~:(id,first-name,last-name,headline,industry,summary)", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest);
			var message = new AccessProtectedResourceRequest(endpoint, Protocol.V10.Version);
			message.ConsumerKey = "ub78frzrn0yf";
			message.AccessToken = "852863fd-05da-4d80-a93d-50f64f966de4";
			((ITamperResistantOAuthMessage)message).ConsumerSecret = "ExJXsYl7Or8OfK98";
			((ITamperResistantOAuthMessage)message).TokenSecret = "b197333b-470a-43b3-bcd7-49d6d2229c4c";
			var signedMessage = (ITamperResistantOAuthMessage)message;
			signedMessage.HttpMethod = HttpMethod.Get;
			signedMessage.SignatureMethod = "HMAC-SHA1";
			MessageDictionary dictionary = this.MessageDescriptions.GetAccessor(message);
			dictionary["oauth_timestamp"] = "1353545248";
			dictionary["oauth_nonce"] = "ugEB4bst";

			var hmac = new HmacSha1SigningBindingElement();
			hmac.Channel = new TestChannel(this.MessageDescriptions);
			Assert.That(hmac.GetSignatureTestHook(message), Is.EqualTo("l09yeD9cr4+h1eoUF4WBoGEHrlk="));
		}
	}
}
