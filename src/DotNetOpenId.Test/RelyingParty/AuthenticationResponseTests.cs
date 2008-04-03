using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.RelyingParty;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace DotNetOpenId.Test.RelyingParty {
	[TestFixture]
	public class AuthenticationResponseTests {
		Realm realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
		Uri returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);

		/// <summary>
		/// Verifies that the RP rejects signed assertions by an OP that makes up a
		/// claimed Id that was not part of the original request, and that the OP
		/// has no authority to assert positively regarding.
		/// </summary>
		[Test, ExpectedException(typeof(OpenIdException))]
		public void SpoofedClaimedIdDetection() {
			ApplicationMemoryStore store = new ApplicationMemoryStore();
			ProtocolVersion version = ProtocolVersion.V20;
			Protocol protocol = Protocol.Lookup(version);
			OpenIdRelyingParty rp = new OpenIdRelyingParty(store, null);
			Identifier id = TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, version);
			var request = rp.CreateRequest(id, realm, returnTo);
			HttpWebRequest providerRequest = (HttpWebRequest)WebRequest.Create(request.RedirectToProviderUrl);
			providerRequest.AllowAutoRedirect = false;
			Uri redirectUrl;
			try {
				using (HttpWebResponse providerResponse = (HttpWebResponse)providerRequest.GetResponse()) {
					Assert.AreEqual(HttpStatusCode.Redirect, providerResponse.StatusCode);
					redirectUrl = new Uri(providerResponse.Headers[HttpResponseHeader.Location]);
				}
			} catch (WebException ex) {
				Trace.WriteLine(ex);
				if (ex.Response != null) {
					using (StreamReader sr = new StreamReader(ex.Response.GetResponseStream())) {
						Trace.WriteLine(sr.ReadToEnd());
					}
				}
				throw;
			}
			// The strategy here is to change the openid.claimed_id parameter to be something totally
			// different that originally requested, but to be a positive assertion.  The OP in question
			// has no authority over this new claimed_id, and so it should be rejected.
			// By tampering with the parameters though, we should trip several alarms in the RP:
			// 1) we invalidate the signature (although a contrived OP would lie intentionally and have a valid signature)
			// 2) the token's cache of the claimed id mismatches
			// 3) when we remove the token so there's nothing to mismatch with, the return_to doesn't match
			// 4) even when we hack return_to to match, the RP's discovery on the new claimed id should indicate
			//    a different OP endpoint is responsible for it, so it should be rejected.
			var nvc = HttpUtility.ParseQueryString(redirectUrl.Query);
			nvc[protocol.openid.claimed_id] = "http://blog.nerdbank.net"; // when you tell one lie, it leads to another
			nvc.Remove(Token.TokenKey); // then you tell two lies, to cover each other
			UriBuilder returnToUri = new UriBuilder(nvc[protocol.openid.return_to]);
			var nvcReturnTo = HttpUtility.ParseQueryString(returnToUri.Query.Substring(1));
			nvcReturnTo.Remove(Token.TokenKey); // then you tell three lies and--oh brother... (it's a song)
			returnToUri.Query = UriUtil.CreateQueryString(Util.NameValueCollectionToDictionary(nvcReturnTo));
			nvc[protocol.openid.return_to] = returnToUri.ToString();
			TestSupport.Resign(nvc, store); // resign changed URL to simulate a contrived OP for breaking into RPs.
			var hackedRedirectUrl = new UriBuilder(redirectUrl);
			hackedRedirectUrl.Query = UriUtil.CreateQueryString(Util.NameValueCollectionToDictionary(nvc));
			var rp2 = new OpenIdRelyingParty(store, hackedRedirectUrl.Uri);
			var result = rp2.Response.Status; // (triggers exception) ... you're in trouble up to your ears...
		}
	}
}
