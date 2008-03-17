using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Test.RelyingParty {
	[TestFixture]
	public class TokenTest {
		static ServiceEndpoint getServiceEndpoint(TestSupport.Scenarios scenario) {
			ServiceEndpoint ep = new ServiceEndpoint(
				TestSupport.GetIdentityUrl(scenario),
				TestSupport.GetFullUrl(TestSupport.ProviderPage),
				TestSupport.GetDelegateUrl(scenario));
			return ep;
		}

		[Test]
		public void TokenBasics() {
			ServiceEndpoint ep = getServiceEndpoint(TestSupport.Scenarios.AutoApproval);
			Token token = new Token(ep);
			Assert.AreEqual(ep.ClaimedIdentifier, token.ClaimedIdentifier);
			Assert.AreEqual(ep.ProviderLocalIdentifier, token.ProviderLocalIdentifier);
			Assert.AreEqual(ep.ProviderEndpoint, token.ProviderEndpoint);
			Assert.IsNotNull(token.Nonce);

			INonceStore store = new ConsumerApplicationMemoryStore();
			string serializedToken = token.Serialize(store);

			Token token2 = Token.Deserialize(serializedToken, store);

			Assert.AreEqual(token.ClaimedIdentifier, token2.ClaimedIdentifier);
			Assert.AreEqual(token.ProviderLocalIdentifier, token2.ProviderLocalIdentifier);
			Assert.AreEqual(token.ProviderEndpoint, token2.ProviderEndpoint);
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
