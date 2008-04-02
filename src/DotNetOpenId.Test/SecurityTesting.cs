using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenId.RelyingParty;
using NUnit.Framework;
using DotNetOpenId.Provider;
using ProviderMemoryStore = DotNetOpenId.AssociationMemoryStore<DotNetOpenId.AssociationRelyingPartyType>;
using System.Web;
using System.Net;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class EndToEndProgrammaticTesting {
		const string realm = "http://localhost";
		readonly Uri return_to = new Uri("http://localhost/login");
		internal static ProxyHost SnifferProxy { get; private set; }

		[SetUp]
		public void SetUp() {
			SnifferProxy = new ProxyHost(null);
			WebRequest.DefaultWebProxy = new WebProxy(SnifferProxy.BaseUri);
		}

		[TearDown]
		public void TearDown() {
			WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
		}

		[Test]
		public void SimpleProxyTest() {

		}

		[Test]
		public void Pass_Setup_AutoApproval_20() {
			OpenIdRelyingParty rp = new OpenIdRelyingParty(null, null);
			var idUrl = TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
			var se = idUrl.Discover();
			var req = rp.CreateRequest(idUrl, realm, return_to);
			OpenIdProvider op = new OpenIdProvider(new ProviderMemoryStore(), se.ProviderEndpoint,
				new Uri("http://somerequest"), HttpUtility.ParseQueryString(req.RedirectToProviderUrl.Query));
			var authReq = (DotNetOpenId.Provider.IAuthenticationRequest)op.Request;
			authReq.IsAuthenticated = true;
			Uri authRespUrl = new Uri(authReq.Response.Headers[HttpResponseHeader.Location]);
			OpenIdRelyingParty rp2 = new OpenIdRelyingParty(null, authRespUrl);
			Assert.AreEqual(rp2.Response.Status, AuthenticationStatus.Authenticated);
		}
	}
}
