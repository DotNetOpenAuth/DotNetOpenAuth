using System;
using DotNetOpenId.RelyingParty;
using NUnit.Framework;
using ProviderMemoryStore = DotNetOpenId.AssociationMemoryStore<DotNetOpenId.AssociationRelyingPartyType>;
using System.Web;
using System.Collections.Specialized;

namespace DotNetOpenId.Test.RelyingParty {
	[TestFixture]
	public class OpenIdRelyingPartyTest {
		IRelyingPartyApplicationStore store;
		UriIdentifier simpleOpenId = new UriIdentifier("http://nonexistant.openid.com");
		readonly Realm realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
		readonly Uri returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);
		Uri simpleNonOpenIdRequest = new Uri("http://localhost/hi");

		[SetUp]
		public void Setup() {
			store = new ApplicationMemoryStore();
			if (!UntrustedWebRequest.WhitelistHosts.Contains("localhost"))
				UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DefaultCtorWithoutContext() {
			new OpenIdRelyingParty();
		}

		[Test]
		public void CtorWithNullRequestUri() {
			new OpenIdRelyingParty(store, null, null);
		}

		[Test]
		public void CtorWithNullStore() {
			var consumer = new OpenIdRelyingParty(null, simpleNonOpenIdRequest, new NameValueCollection());
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext1() {
			var consumer = new OpenIdRelyingParty(store, simpleNonOpenIdRequest, new NameValueCollection());
			consumer.CreateRequest(simpleOpenId);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext2() {
			var consumer = new OpenIdRelyingParty(store, simpleNonOpenIdRequest, new NameValueCollection());
			consumer.CreateRequest(simpleOpenId, realm);
		}

		[Test]
		public void AssociationCreationWithStore() {
			var providerStore = new ProviderMemoryStore();

			OpenIdRelyingParty rp = new OpenIdRelyingParty(new ApplicationMemoryStore(), null, null);
			var idUrl = TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);

			DotNetOpenId.RelyingParty.IAuthenticationRequest req;
			bool associationMade = false;
			TestSupport.Interceptor.SigningMessage = m => {
				if (m.EncodedFields.ContainsKey("assoc_handle") && m.EncodedFields.ContainsKey("session_type"))
					associationMade = true;
			};
			req = rp.CreateRequest(idUrl, realm, returnTo);
			TestSupport.Interceptor.SigningMessage = null;
			Assert.IsTrue(associationMade);
		}

		[Test]
		public void NoAssociationRequestWithoutStore() {
			var providerStore = new ProviderMemoryStore();

			OpenIdRelyingParty rp = new OpenIdRelyingParty(null, null, null);
			var idUrl = TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);

			DotNetOpenId.RelyingParty.IAuthenticationRequest req;
			bool associationMade = false;
			TestSupport.Interceptor.SigningMessage = m => {
				if (m.EncodedFields.ContainsKey("assoc_handle") && m.EncodedFields.ContainsKey("session_type"))
					associationMade = true;
			};
			req = rp.CreateRequest(idUrl, realm, returnTo);
			TestSupport.Interceptor.SigningMessage = null;
			Assert.IsFalse(associationMade);
		}

		/// <summary>
		/// Verifies that both the return_to and realm arguments either
		/// both explicitly specify the port number when it can be implied
		/// or both leave the port number out.  
		/// </summary>
		/// <remarks>
		/// Implying or explicitly specifying the port should not make any difference
		/// as long as the port is not different, but some other implementations that
		/// we want to interop with have poor comparison functions and a port on one
		/// and missing on the other can cause unwanted failures.  So we just want to
		/// make sure that we do our best to interop with them.
		/// </remarks>
		[Test]
		public void RealmAndReturnToPortImplicitnessAgreement() {
			UriBuilder builder = new UriBuilder(TestSupport.GetFullUrl(TestSupport.ConsumerPage));
			// set the scheme and port such that the port MAY be implied.
			builder.Port = 80;
			builder.Scheme = "http";
			Uri returnTo = builder.Uri;
			testExplicitPortOnRealmAndReturnTo(returnTo, new Realm(builder));
			// Add wildcard and test again.
			builder.Host = "*." + builder.Host;
			testExplicitPortOnRealmAndReturnTo(returnTo, new Realm(builder));
		}

		private static void testExplicitPortOnRealmAndReturnTo(Uri returnTo, Realm realm) {
			var identityUrl = TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
			var consumer = new OpenIdRelyingParty(null, null, null);
			var request = consumer.CreateRequest(identityUrl, realm, returnTo);
			Protocol protocol = Protocol.Lookup(request.ProviderVersion);
			var nvc = HttpUtility.ParseQueryString(request.RedirectingResponse.ExtractUrl().Query);
			string realmString = nvc[protocol.openid.Realm];
			string returnToString = nvc[protocol.openid.return_to];
			bool realmPortExplicitlyGiven = realmString.Contains(":80");
			bool returnToPortExplicitlyGiven = returnToString.Contains(":80");
			if (realmPortExplicitlyGiven ^ returnToPortExplicitlyGiven) {
				if (realmPortExplicitlyGiven)
					Assert.Fail("Realm port is explicitly specified although it may be implied, and return_to only implies it.");
				else
					Assert.Fail("Return_to port is explicitly specified although it may be implied, and realm only implies it.");
			}
		}

		[Test]
		public void ReturnToUrlEncodingTest() {
			Uri origin = TestSupport.GetFullUrl(TestSupport.ConsumerPage);
			var identityUrl = TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
			var consumer = new OpenIdRelyingParty(null, null, null);
			var request = consumer.CreateRequest(identityUrl, origin, origin);
			Protocol protocol = Protocol.Lookup(request.ProviderVersion);
			request.AddCallbackArguments("a+b", "c+d");
			var requestArgs = HttpUtility.ParseQueryString(request.RedirectingResponse.ExtractUrl().Query);
			var returnToArgs = HttpUtility.ParseQueryString(requestArgs[protocol.openid.return_to]);
			Assert.AreEqual("c+d", returnToArgs["a+b"]);
		}
	}
}
