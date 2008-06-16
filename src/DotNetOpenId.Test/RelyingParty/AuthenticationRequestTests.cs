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
	public class AuthenticationRequestTests {
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
		public void Provider() {
			OpenIdRelyingParty rp = new OpenIdRelyingParty(store, null, null);
			Identifier id = TestSupport.GetFullUrl("xrdsdiscovery/xrds20.aspx");
			IAuthenticationRequest request = rp.CreateRequest(id, realm, returnTo);
			Assert.IsNotNull(request.Provider);
		}
	}
}
