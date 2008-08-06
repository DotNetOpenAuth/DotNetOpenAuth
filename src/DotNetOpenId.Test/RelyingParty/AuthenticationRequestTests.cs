using System;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Test.Mocks;
using NUnit.Framework;

namespace DotNetOpenId.Test.RelyingParty {
	[TestFixture]
	public class AuthenticationRequestTests {
		Realm realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
		Uri returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);

		[SetUp]
		public void SetUp() {
			if (!UntrustedWebRequest.WhitelistHosts.Contains("localhost"))
				UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		[TearDown]
		public void TearDown() {
			MockHttpRequest.Reset();
		}

		[Test]
		public void Provider() {
			OpenIdRelyingParty rp = new OpenIdRelyingParty(null, null, null);
			Identifier id = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
			IAuthenticationRequest request = rp.CreateRequest(id, realm, returnTo);
			Assert.IsNotNull(request.Provider);
		}
	}
}
