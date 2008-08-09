using System;
using DotNetOpenId.Extensions.AttributeExchange;
using DotNetOpenId.Extensions.SimpleRegistration;
using DotNetOpenId.RelyingParty;
using NUnit.Framework;
using DotNetOpenId.Test.Mocks;

namespace DotNetOpenId.Test.RelyingParty {
	[TestFixture]
	public class IProviderEndpointTests {
		IRelyingPartyApplicationStore store;
		Realm realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
		Uri returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);

		[SetUp]
		public void SetUp() {
			store = new ApplicationMemoryStore();
			if (!UntrustedWebRequest.WhitelistHosts.Contains("localhost"))
				UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		[TearDown]
		public void TearDown() {
			Mocks.MockHttpRequest.Reset();
		}

		[Test]
		public void IsExtensionSupportedTest() {
			OpenIdRelyingParty rp = TestSupport.CreateRelyingParty(null);
			Identifier id = MockHttpRequest.RegisterMockXrdsResponse("/Discovery/xrdsdiscovery/xrds20.xml");
			IAuthenticationRequest request = rp.CreateRequest(id, TestSupport.Realm, TestSupport.ReturnTo);
			IProviderEndpoint provider = request.Provider;
			Assert.IsTrue(provider.IsExtensionSupported<ClaimsRequest>());
			Assert.IsTrue(provider.IsExtensionSupported(typeof(ClaimsRequest)));
			Assert.IsFalse(provider.IsExtensionSupported<FetchRequest>());
			Assert.IsFalse(provider.IsExtensionSupported(typeof(FetchRequest)));

			// Test the AdditionalTypeUris list by pulling from an XRDS page with one of the
			// TypeURIs that only shows up in that list.
			id = MockHttpRequest.RegisterMockXrdsResponse("/Discovery/xrdsdiscovery/xrds10.xml");
			request = rp.CreateRequest(id, realm, returnTo);
			Assert.IsTrue(provider.IsExtensionSupported<ClaimsRequest>());
			Assert.IsTrue(provider.IsExtensionSupported(typeof(ClaimsRequest)));
		}

		[Test]
		public void UriTest() {
			OpenIdRelyingParty rp = TestSupport.CreateRelyingParty(null);
			Identifier id = MockHttpRequest.RegisterMockXrdsResponse("/Discovery/xrdsdiscovery/xrds20.xml");
			IAuthenticationRequest request = rp.CreateRequest(id, TestSupport.Realm, TestSupport.ReturnTo);
			IProviderEndpoint provider = request.Provider;
			Assert.AreEqual(new Uri("http://a/b"), provider.Uri);
		}
	}
}
