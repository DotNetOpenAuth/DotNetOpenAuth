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
using System.Diagnostics;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class EndToEndProgrammaticTesting {
		readonly Uri providerEndpoint = new Uri("http://localhost:9999/provider");
		const string realm = "http://localhost";
		readonly Uri return_to = new Uri("http://localhost/login");

		[SetUp]
		public void SetUp() {
			Fetcher.GetResponseFromRequest = (req, body) => {
				Trace.TraceInformation("==========================================");
				Trace.TraceInformation("Intercepted HTTP {0}: {1}", req.Method, req.RequestUri);
				if (body != null && body.Length > 0) {
					Trace.TraceInformation(Encoding.UTF8.GetString(body));
				}
				Trace.TraceInformation("------------------------------------------");
				FetchResponse resp = Fetcher.GetResponse(req.RequestUri, (HttpWebResponse)req.GetResponse());
				Trace.TraceInformation("Intercepted response: {0}", resp.FinalUri);
				Trace.TraceInformation(resp.ReadResponseString());
				Trace.TraceInformation("==========================================");
				return resp;
			};
		}

		[TearDown]
		public void TearDown() {
			Fetcher.GetResponseFromRequest = Fetcher.StandardGetResponseFromRequest;
		}

		[Test]
		public void Pass_Setup_AutoApproval_20() {
			var providerStore = new ProviderMemoryStore();

			OpenIdRelyingParty rp = new OpenIdRelyingParty(new ConsumerApplicationMemoryStore(), null);
			var idUrl = TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
			var se = idUrl.Discover();
			DotNetOpenId.RelyingParty.IAuthenticationRequest req;
			bool associationReqSent = true;
			using (var interceptor = new DotNetOpenId.Test.Hosting.InterceptionHost(providerStore,
				providerEndpoint, ireq => {
					if (ireq == null) return null;
					var assocRequest = ireq as DotNetOpenId.Provider.AssociateRequest;
					if (assocRequest != null) {
						associationReqSent = true;
					}
					return ireq.Response;
				})) {
				req = rp.CreateRequest(idUrl, realm, return_to);
			}
			Assert.IsTrue(associationReqSent);

			//OpenIdProvider op = new OpenIdProvider(new ProviderMemoryStore(), se.ProviderEndpoint,
			//    new Uri("http://somerequest"), HttpUtility.ParseQueryString(req.RedirectToProviderUrl.Query));
			//var authReq = (DotNetOpenId.Provider.IAuthenticationRequest)op.Request;
			//authReq.IsAuthenticated = true;
			//Uri authRespUrl = new Uri(authReq.Response.Headers[HttpResponseHeader.Location]);
			//OpenIdRelyingParty rp2 = new OpenIdRelyingParty(null, authRespUrl);
			//Assert.AreEqual(rp2.Response.Status, AuthenticationStatus.Authenticated);
		}
	}
}
