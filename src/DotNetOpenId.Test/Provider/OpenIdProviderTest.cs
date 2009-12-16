using System;
using System.Collections.Specialized;
using DotNetOpenId.Provider;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Test.Mocks;
using NUnit.Framework;
using ProviderMemoryStore = DotNetOpenId.AssociationMemoryStore<DotNetOpenId.AssociationRelyingPartyType>;

namespace DotNetOpenId.Test.Provider {
	[TestFixture]
	public class OpenIdProviderTest {
		readonly Uri providerEndpoint = new Uri("http://someendpoint");
		readonly Uri emptyRequestUrl = new Uri("http://someendpoint/request");

		[SetUp]
		public void Setup() {
			if (!UntrustedWebRequest.WhitelistHosts.Contains("localhost"))
				UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		[TearDown]
		public void TearDown() {
			MockHttpRequest.Reset();
		}

		/// <summary>
		/// Verifies that without an ASP.NET context, the default constructor fails.
		/// </summary>
		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void CtorDefault() {
			OpenIdProvider op = new OpenIdProvider();
		}

		[Test]
		public void CtorNonDefault() {
			OpenIdProvider op = new OpenIdProvider(new ProviderMemoryStore(),
				providerEndpoint, emptyRequestUrl, new NameValueCollection());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullStore() {
			OpenIdProvider op = new OpenIdProvider(null, providerEndpoint,
				emptyRequestUrl, new NameValueCollection());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullEndpoint() {
			OpenIdProvider op = new OpenIdProvider(new ProviderMemoryStore(),
				null, emptyRequestUrl, new NameValueCollection());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullRequestUrl() {
			OpenIdProvider op = new OpenIdProvider(new ProviderMemoryStore(),
				providerEndpoint, null, new NameValueCollection());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullQuery() {
			OpenIdProvider op = new OpenIdProvider(new ProviderMemoryStore(),
				providerEndpoint, emptyRequestUrl, null);
		}

		[Test]
		public void RequestNullOnEmptyRequest() {
			OpenIdProvider op = new OpenIdProvider(new ProviderMemoryStore(),
				providerEndpoint, emptyRequestUrl, new NameValueCollection());
			Assert.IsNull(op.Request);
		}

		[Test]
		public void BasicUnsolicitedAssertion() {
			Mocks.MockHttpRequest.RegisterMockRPDiscovery();
			TestSupport.Scenarios scenario = TestSupport.Scenarios.AutoApproval;
			Identifier claimedId = TestSupport.GetMockIdentifier(scenario, ProtocolVersion.V20);
			Identifier localId = TestSupport.GetDelegateUrl(scenario);

			OpenIdProvider op = TestSupport.CreateProvider(null);
			IResponse assertion = op.PrepareUnsolicitedAssertion(TestSupport.Realm, claimedId, localId);
			var rpResponse = TestSupport.CreateRelyingPartyResponse(TestSupport.RelyingPartyStore, assertion);
			Assert.AreEqual(AuthenticationStatus.Authenticated, rpResponse.Status);
			Assert.AreEqual(claimedId, rpResponse.ClaimedIdentifier);
		}

		[Test]
		public void UnsolicitedAssertionWithBadCapitalization() {
			Mocks.MockHttpRequest.RegisterMockRPDiscovery();
			TestSupport.Scenarios scenario = TestSupport.Scenarios.AutoApproval;
			Identifier claimedId = TestSupport.GetMockIdentifier(scenario, ProtocolVersion.V20);
			claimedId = claimedId.ToString().ToUpper(); // make all caps, which is not right
			Identifier localId = TestSupport.GetDelegateUrl(scenario);

			OpenIdProvider op = TestSupport.CreateProvider(null);
			IResponse assertion = op.PrepareUnsolicitedAssertion(TestSupport.Realm, claimedId, localId);
			var rpResponse = TestSupport.CreateRelyingPartyResponse(TestSupport.RelyingPartyStore, assertion);
			Assert.AreEqual(AuthenticationStatus.Failed, rpResponse.Status);
		}

		/// <summary>
		/// Verifies that OP will properly report RP versions in requests.
		/// </summary>
		[Test]
		public void RelyingPartyVersion() {
			Protocol simulatedVersion = Protocol.v11;
			UriIdentifier id = TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, simulatedVersion.ProtocolVersion);

			// make up some OpenID 1.x looking message...
			NameValueCollection rp10Request = new NameValueCollection();
			rp10Request[simulatedVersion.openid.mode] = simulatedVersion.Args.Mode.checkid_immediate;
			rp10Request[simulatedVersion.openid.identity] = id;
			rp10Request[simulatedVersion.openid.return_to] = TestSupport.ReturnTo.AbsoluteUri;
			rp10Request[simulatedVersion.openid.Realm] = TestSupport.Realm;

			OpenIdProvider op = TestSupport.CreateProvider(rp10Request);
			Assert.AreEqual(simulatedVersion.ProtocolVersion,
				((DotNetOpenId.Provider.IAuthenticationRequest)op.Request).RelyingPartyVersion);

			// Verify V2.0 reporting.
			var rp20Request = TestSupport.CreateRelyingPartyRequest(true, TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20, false);
			TestSupport.CreateRelyingPartyResponseThroughProvider(rp20Request, opReq => {
				Assert.AreEqual(ProtocolVersion.V20, opReq.RelyingPartyVersion);
				opReq.IsAuthenticated = true;
			});
		}
	}
}
