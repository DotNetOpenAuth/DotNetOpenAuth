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

		[Test, ExpectedException(typeof(WebException), UserMessage = "OP should throw WebException when return URL is unverifiable.")]
		public void UnverifiableReturnUrl() {
			Uri returnTo;
			Realm realm;
			getUnverifiableRP(out returnTo, out realm);
			var consumer = new OpenIdRelyingParty(new ApplicationMemoryStore(), null);
			var request = consumer.CreateRequest(TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20), realm, returnTo);
			WebRequest.Create(request.RedirectToProviderUrl).GetResponse(); // the OP should return 500, causing exception here.
		}

		static void getUnverifiableRP(out Uri returnTo, out Realm realm) {
			var disableDiscovery = new Dictionary<string, string> {
				{"AllowRPDiscovery", "false"},
			};
			returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage, disableDiscovery);
			realm = new Realm(returnTo);
		}
	}
}
