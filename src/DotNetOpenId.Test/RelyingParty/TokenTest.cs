using DotNetOpenId.RelyingParty;
using NUnit.Framework;

namespace DotNetOpenId.Test.RelyingParty {
	[TestFixture]
	public class TokenTest {
		static ServiceEndpoint getServiceEndpoint(TestSupport.Scenarios scenario, ProtocolVersion version) {
			Protocol protocol = Protocol.Lookup(version);
			ServiceEndpoint ep = new ServiceEndpoint(
				TestSupport.GetIdentityUrl(scenario, version),
				TestSupport.GetFullUrl(TestSupport.ProviderPage),
				TestSupport.GetDelegateUrl(scenario),
				new[] { protocol.ClaimedIdentifierServiceTypeURI },
				10,
				10
				);
			return ep;
		}

		/// <summary>
		/// Tests token creation, serialization, and conditional nonce serialization.
		/// </summary>
		void tokenBasics(ProtocolVersion version) {
			ServiceEndpoint ep = getServiceEndpoint(TestSupport.Scenarios.AutoApproval, version);
			Token token = new Token(ep);
			Assert.AreSame(ep, token.Endpoint);
			Assert.IsNotNull(token.Nonce);

			INonceStore store = new ApplicationMemoryStore();
			string serializedToken = token.Serialize(store);

			Token token2 = Token.Deserialize(serializedToken, store);

			Assert.AreEqual(token.Endpoint, token2.Endpoint);
			if (ep.Protocol.Version.Major < 2) {
				Assert.AreEqual(token.Nonce, token2.Nonce);
				Assert.IsNotNull(token2.Nonce);
			} else {
				Assert.IsNull(token2.Nonce);
			}
		}
		[Test]
		public void TokenBasics11() {
			tokenBasics(ProtocolVersion.V11);
		}
		[Test]
		public void TokenBasics20() {
			tokenBasics(ProtocolVersion.V20);
		}

		void replayAttackPrevention(ProtocolVersion version) {
			ServiceEndpoint ep = getServiceEndpoint(TestSupport.Scenarios.AutoApproval, version);
			Token token = new Token(ep);

			INonceStore store = new ApplicationMemoryStore();
			string serializedToken = token.Serialize(store);
			Token.Deserialize(serializedToken, store);
			Token.Deserialize(serializedToken, store);
		}
		[Test, ExpectedException(typeof(OpenIdException))]
		public void ReplayAttackPrevention() {
			replayAttackPrevention(ProtocolVersion.V11);
			// We don't test on V2.0 because tokens are not used for replay attack prevention in OpenID 2.0.
		}

		[Test]
		public void EqualsTest() {
			ServiceEndpoint ep1 = getServiceEndpoint(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
			Token token1a = new Token(ep1);
			Token token1b = new Token(ep1);
			Assert.AreEqual(token1a, token1a, "It's the same object!");
			Assert.AreNotEqual(token1a, token1b, "Two tokens generated for the same service endpoint should have unique nonces.");

			ServiceEndpoint ep2 = getServiceEndpoint(TestSupport.Scenarios.AlwaysDeny, ProtocolVersion.V20);
			Token token2 = new Token(ep2);
			Assert.AreNotEqual(token1a, token2);
		}
	}
}
