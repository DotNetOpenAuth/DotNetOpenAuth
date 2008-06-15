using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Extensions.SimpleRegistration;
using DotNetOpenId.Extensions.AttributeExchange;

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
		public void IsExtensionAdvertisedAsSupportedTest() {
			OpenIdRelyingParty rp = new OpenIdRelyingParty(store, null, null);
			Identifier id = TestSupport.GetFullUrl("xrdsdiscovery/xrds20.aspx");
			IAuthenticationRequest request = rp.CreateRequest(id, realm, returnTo);
			IProviderEndpoint provider = request.Provider;
			Assert.IsTrue(provider.IsExtensionAdvertisedAsSupported<ClaimsRequest>());
			Assert.IsTrue(provider.IsExtensionAdvertisedAsSupported(typeof(ClaimsRequest)));
			Assert.IsFalse(provider.IsExtensionAdvertisedAsSupported<FetchRequest>());
			Assert.IsFalse(provider.IsExtensionAdvertisedAsSupported(typeof(FetchRequest)));

			// Test the AdditionalTypeUris list by pulling from an XRDS page with one of the
			// TypeURIs that only shows up in that list.
			id = TestSupport.GetFullUrl("xrdsdiscovery/xrds10.aspx");
			request = rp.CreateRequest(id, realm, returnTo);
			Assert.IsTrue(provider.IsExtensionAdvertisedAsSupported<ClaimsRequest>());
			Assert.IsTrue(provider.IsExtensionAdvertisedAsSupported(typeof(ClaimsRequest)));
		}

	}
}
