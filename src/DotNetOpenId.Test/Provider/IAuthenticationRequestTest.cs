using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.RelyingParty;
using System.Net;

namespace DotNetOpenId.Test.Provider {
	[TestFixture]
	public class IAuthenticationRequestTest {
		[SetUp]
		public void Setup() {
			if (!UntrustedWebRequest.WhitelistHosts.Contains("localhost"))
				UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		[TearDown]
		public void TearDown() {
			Mocks.MockHttpRequest.Reset();
		}

		[Test]
		public void UnverifiableReturnUrl() {
			var request = TestSupport.CreateRelyingPartyRequest(true, TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);

			// Clear out the RP discovery information registered by TestSupport
			Mocks.MockHttpRequest.DeleteResponse(TestSupport.Realm.UriWithWildcardChangedToWww);

			bool reachedOP = false;
			var response = TestSupport.CreateRelyingPartyResponseThroughProvider(request, req => {
				Assert.IsFalse(req.IsReturnUrlDiscoverable);
				reachedOP = true;
				req.IsAuthenticated = false;
			});
			Assert.IsTrue(reachedOP);
		}
	}
}
