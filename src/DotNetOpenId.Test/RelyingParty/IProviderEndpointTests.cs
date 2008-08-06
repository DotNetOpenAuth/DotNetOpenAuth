using System;
using DotNetOpenId.Extensions.AttributeExchange;
using DotNetOpenId.Extensions.SimpleRegistration;
using DotNetOpenId.RelyingParty;
using NUnit.Framework;

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

		[Test]
		public void IsExtensionSupportedTest() {
			OpenIdRelyingParty rp = new OpenIdRelyingParty(store, null, null);
			Identifier id = TestSupport.GetFullUrl("xrdsdiscovery/xrds20.aspx");
			IAuthenticationRequest request = rp.CreateRequest(id, realm, returnTo);
			IProviderEndpoint provider = request.Provider;
			Assert.IsTrue(provider.IsExtensionSupported<ClaimsRequest>());
			Assert.IsTrue(provider.IsExtensionSupported(typeof(ClaimsRequest)));
			Assert.IsFalse(provider.IsExtensionSupported<FetchRequest>());
			Assert.IsFalse(provider.IsExtensionSupported(typeof(FetchRequest)));

			// Test the AdditionalTypeUris list by pulling from an XRDS page with one of the
			// TypeURIs that only shows up in that list.
			id = TestSupport.GetFullUrl("xrdsdiscovery/xrds10.aspx");
			request = rp.CreateRequest(id, realm, returnTo);
			Assert.IsTrue(provider.IsExtensionSupported<ClaimsRequest>());
			Assert.IsTrue(provider.IsExtensionSupported(typeof(ClaimsRequest)));
		}

		[Test]
		public void UriTest() {
			OpenIdRelyingParty rp = new OpenIdRelyingParty(store, null, null);
			Identifier id = TestSupport.GetFullUrl("xrdsdiscovery/xrds20.aspx");
			IAuthenticationRequest request = rp.CreateRequest(id, realm, returnTo);
			IProviderEndpoint provider = request.Provider;
			Assert.AreEqual(new Uri("http://a/b"), provider.Uri);
		}
	}
}
