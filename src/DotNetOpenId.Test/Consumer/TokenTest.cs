using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Consumer;

namespace DotNetOpenId.Test.Consumer {
	[TestFixture]
	public class TokenTest {
		static ServiceEndpoint getServiceEndpoint(TestSupport.Scenarios scenario) {
			ServiceEndpoint ep = new ServiceEndpoint(
				TestSupport.GetIdentityUrl(scenario),
				TestSupport.GetFullUrl(TestSupport.ProviderPage),
				null, TestSupport.GetDelegateUrl(scenario), false);
			return ep;
		}

		[Test]
		public void TokenBasics() {
			ServiceEndpoint ep = getServiceEndpoint(TestSupport.Scenarios.AutoApproval);
			Token token = new Token(ep);
			Assert.AreEqual(ep.IdentityUrl, token.IdentityUrl);
			Assert.AreEqual(ep.DelegateUrl, token.ServerId);
			Assert.AreEqual(ep.ServerUrl, token.ServerUrl);
			Assert.IsNotNull(token.Nonce);

			INonceStore store = new ConsumerApplicationMemoryStore();
			string serializedToken = token.Serialize(store);

			Token token2 = Token.Deserialize(serializedToken, store);

			Assert.AreEqual(token.IdentityUrl, token2.IdentityUrl);
			Assert.AreEqual(token.ServerId, token2.ServerId);
			Assert.AreEqual(token.ServerUrl, token2.ServerUrl);
			Assert.IsNotNull(token2.Nonce);
		}

		[Test, ExpectedException(typeof(OpenIdException))]
		public void ReplayAttackPrevention() {
			ServiceEndpoint ep = getServiceEndpoint(TestSupport.Scenarios.AutoApproval);
			Token token = new Token(ep);

			INonceStore store = new ConsumerApplicationMemoryStore();
			string serializedToken = token.Serialize(store);
			Token.Deserialize(serializedToken, store);
			Token.Deserialize(serializedToken, store);
		}

		[Test]
		public void EqualsTest() {
			ServiceEndpoint ep1 = getServiceEndpoint(TestSupport.Scenarios.AutoApproval);
			Token token1a = new Token(ep1);
			Token token1b = new Token(ep1);
			Assert.AreEqual(token1a, token1a, "It's the same object!");
			Assert.AreNotEqual(token1a, token1b, "Two tokens generated for the same service endpoint should have unique nonces.");

			ServiceEndpoint ep2 = getServiceEndpoint(TestSupport.Scenarios.AlwaysDeny);
			Token token2 = new Token(ep2);
			Assert.AreNotEqual(token1a, token2);
		}
	}
}
