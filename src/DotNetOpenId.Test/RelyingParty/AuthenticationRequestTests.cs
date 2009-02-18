using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web;
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

		[Test]
		public void AddCallbackArgumentReplacesExistingArguments() {
			OpenIdRelyingParty rp = new OpenIdRelyingParty(null, null, null);
			Identifier id = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);

			UriBuilder returnToWithParameter = new UriBuilder(returnTo);
			UriUtil.AppendQueryArgs(returnToWithParameter, new Dictionary<string, string> { { "p1", "v1"} });

			IAuthenticationRequest request = rp.CreateRequest(id, realm, returnToWithParameter.Uri);
			request.AddCallbackArguments("p1", "v2");

			Uri redirectUri = new Uri(request.RedirectingResponse.Headers[HttpResponseHeader.Location]);
			NameValueCollection redirectArgs = HttpUtility.ParseQueryString(redirectUri.Query);
			Uri returnToUri = new Uri(redirectArgs[Protocol.Default.openid.return_to]);
			NameValueCollection returnToArgs = HttpUtility.ParseQueryString(returnToUri.Query);
			Assert.AreEqual("v2", returnToArgs["p1"]);
		}
	}
}
