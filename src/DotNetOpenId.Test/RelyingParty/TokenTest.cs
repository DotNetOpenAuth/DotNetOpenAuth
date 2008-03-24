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
				TestSupport.GetDelegateUrl(scenario),
				new[] { Protocol.v11.ClaimedIdentifierServiceTypeURI }
				);
			return ep;
		}

		[Test]
		public void TokenBasics() {
			ServiceEndpoint ep = getServiceEndpoint(TestSupport.Scenarios.AutoApproval);
			Token token = new Token(ep);
			Assert.AreSame(ep, token.Endpoint);
			Assert.IsNotNull(token.Nonce);

			INonceStore store = new ConsumerApplicationMemoryStore();
			string serializedToken = token.Serialize(store);

			Token token2 = Token.Deserialize(serializedToken, store);

			Assert.AreEqual(token.Endpoint, token2.Endpoint);
			Assert.AreEqual(token.Nonce, token2.Nonce);
			if (ep.Protocol.Version.Major < 2)
				Assert.IsNotNull(token2.Nonce);
			else
				Assert.IsNull(token2.Nonce);
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
