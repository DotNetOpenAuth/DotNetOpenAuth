using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Test.Mocks;
using NUnit.Framework;
using OpenIdProvider = DotNetOpenId.Provider.OpenIdProvider;

namespace DotNetOpenId.Test.RelyingParty {
	[TestFixture]
	public class OpenIdRelyingPartyTest {
		UriIdentifier simpleOpenId = new UriIdentifier("http://nonexistant.openid.com");
		readonly Realm realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
		readonly Uri returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);
		Uri simpleNonOpenIdRequest = new Uri("http://localhost/hi");

		[SetUp]
		public void Setup() {
			if (!UntrustedWebRequest.WhitelistHosts.Contains("localhost"))
				UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		[TearDown]
		public void TearDown() {
			MockHttpRequest.Reset();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DefaultCtorWithoutContext() {
			new OpenIdRelyingParty();
		}

		[Test]
		public void CtorWithNullRequestUri() {
			new OpenIdRelyingParty(new ApplicationMemoryStore(), null, null);
		}

		[Test]
		public void CtorWithNullStore() {
			var consumer = new OpenIdRelyingParty(null, simpleNonOpenIdRequest, new NameValueCollection());
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext1() {
			var consumer = new OpenIdRelyingParty(new ApplicationMemoryStore(), simpleNonOpenIdRequest, new NameValueCollection());
			consumer.CreateRequest(simpleOpenId);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext2() {
			var consumer = new OpenIdRelyingParty(new ApplicationMemoryStore(), simpleNonOpenIdRequest, new NameValueCollection());
			consumer.CreateRequest(simpleOpenId, realm);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateRequestNullIdentifier() {
			var consumer = TestSupport.CreateRelyingParty(null);
			consumer.CreateRequest(null, realm, returnTo);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateRequestNullRealm() {
			var consumer = TestSupport.CreateRelyingParty(null);
			consumer.CreateRequest("=someEndpoint", null, returnTo);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateRequestNullReturnTo() {
			var consumer = TestSupport.CreateRelyingParty(null);
			consumer.CreateRequest("=someEndpoint", realm, null);
		}

		[Test]
		public void CreateRequestStripsFragment() {
			var consumer = TestSupport.CreateRelyingParty(null);
			UriBuilder userSuppliedIdentifier = new UriBuilder((Uri)TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20));
			userSuppliedIdentifier.Fragment = "c";
			Identifier mockIdentifer = new MockIdentifier(userSuppliedIdentifier.Uri,
				TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20).Discover());
			Assert.IsTrue(mockIdentifer.ToString().EndsWith("#c"), "Test broken");
			IAuthenticationRequest request = consumer.CreateRequest(mockIdentifer, TestSupport.Realm, TestSupport.ReturnTo);
			Assert.AreEqual(0, new Uri(request.ClaimedIdentifier).Fragment.Length);
		}

		[Test]
		public void AssociationCreationWithStore() {
			TestSupport.ResetStores(); // get rid of existing associations so a new one is created

			OpenIdRelyingParty rp = TestSupport.CreateRelyingParty(null);
			var directMessageSniffer = new DirectMessageSniffWrapper(rp.DirectMessageChannel);
			rp.DirectMessageChannel = directMessageSniffer;
			var idUrl = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);

			DotNetOpenId.RelyingParty.IAuthenticationRequest req;
			bool associationMade = false;
			directMessageSniffer.Receiving += (provider, fields) => {
				if (fields.ContainsKey("assoc_handle") && fields.ContainsKey("session_type"))
					associationMade = true;
			};
			req = rp.CreateRequest(idUrl, realm, returnTo);
			Assert.IsTrue(associationMade);
		}

		[Test]
		public void NoAssociationRequestWithoutStore() {
			TestSupport.ResetStores(); // get rid of existing associations so a new one is created

			OpenIdRelyingParty rp = TestSupport.CreateRelyingParty(null, null);
			var directMessageSniffer = new DirectMessageSniffWrapper(rp.DirectMessageChannel);
			rp.DirectMessageChannel = directMessageSniffer;
			var idUrl = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);

			DotNetOpenId.RelyingParty.IAuthenticationRequest req;
			bool associationMade = false;
			directMessageSniffer.Receiving += (provider, fields) => {
				if (fields.ContainsKey("assoc_handle") && fields.ContainsKey("session_type"))
					associationMade = true;
			};
			req = rp.CreateRequest(idUrl, realm, returnTo);
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
			var request = TestSupport.CreateRelyingPartyRequest(true, TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
			Protocol protocol = Protocol.Lookup(request.Provider.Version);
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
			var request = TestSupport.CreateRelyingPartyRequest(true, TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
			Protocol protocol = Protocol.Lookup(request.Provider.Version);
			request.AddCallbackArguments("a+b", "c+d");
			var requestArgs = HttpUtility.ParseQueryString(request.RedirectingResponse.ExtractUrl().Query);
			var returnToArgs = HttpUtility.ParseQueryString(requestArgs[protocol.openid.return_to]);
			Assert.AreEqual("c+d", returnToArgs["a+b"]);
		}

		static ServiceEndpoint getServiceEndpoint(int? servicePriority, int? uriPriority) {
			Protocol protocol = Protocol.v20;
			ServiceEndpoint ep = ServiceEndpoint.CreateForClaimedIdentifier(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20),
				TestSupport.GetDelegateUrl(TestSupport.Scenarios.AutoApproval),
				TestSupport.GetFullUrl(TestSupport.ProviderPage),
				new[] { protocol.ClaimedIdentifierServiceTypeURI },
				servicePriority,
				uriPriority
				);
			return ep;
		}

		[Test]
		public void DefaultEndpointOrder() {
			var consumer = new OpenIdRelyingParty(null, null, null);
			Assert.AreSame(OpenIdRelyingParty.DefaultEndpointOrder, consumer.EndpointOrder);
			var defaultEndpointOrder = OpenIdRelyingParty.DefaultEndpointOrder;
			// Test service priority ordering
			Assert.AreEqual(-1, defaultEndpointOrder(getServiceEndpoint(10, null), getServiceEndpoint(20, null)));
			Assert.AreEqual(1, defaultEndpointOrder(getServiceEndpoint(20, null), getServiceEndpoint(10, null)));
			Assert.AreEqual(0, defaultEndpointOrder(getServiceEndpoint(10, null), getServiceEndpoint(10, null)));
			Assert.AreEqual(-1, defaultEndpointOrder(getServiceEndpoint(20, null), getServiceEndpoint(null, null)));
			Assert.AreEqual(1, defaultEndpointOrder(getServiceEndpoint(null, null), getServiceEndpoint(10, null)));
			Assert.AreEqual(0, defaultEndpointOrder(getServiceEndpoint(null, null), getServiceEndpoint(null, null)));
			// Test secondary type uri ordering
			Assert.AreEqual(-1, defaultEndpointOrder(getServiceEndpoint(10, 10), getServiceEndpoint(10, 20)));
			Assert.AreEqual(1, defaultEndpointOrder(getServiceEndpoint(10, 20), getServiceEndpoint(10, 10)));
			Assert.AreEqual(0, defaultEndpointOrder(getServiceEndpoint(10, 5), getServiceEndpoint(10, 5)));
			// test that it is secondary...
			Assert.AreEqual(1, defaultEndpointOrder(getServiceEndpoint(20, 10), getServiceEndpoint(10, 20)));
			Assert.AreEqual(-1, defaultEndpointOrder(getServiceEndpoint(null, 10), getServiceEndpoint(null, 20)));
			Assert.AreEqual(1, defaultEndpointOrder(getServiceEndpoint(null, 20), getServiceEndpoint(null, 10)));
			Assert.AreEqual(0, defaultEndpointOrder(getServiceEndpoint(null, 10), getServiceEndpoint(null, 10)));
		}

		[Test]
		public void DefaultFilter() {
			var consumer = new OpenIdRelyingParty(null, null, null);
			Assert.IsNull(consumer.EndpointFilter);
		}

		[Test]
		public void MultipleServiceEndpoints() {
			string xrds = @"<?xml version='1.0' encoding='UTF-8'?>
<XRD xmlns='xri://$xrd*($v*2.0)'>
 <Query>=MultipleEndpoint</Query>
 <Status cid='verified' code='100' />
 <ProviderID>=!91F2.8153.F600.AE24</ProviderID>
 <CanonicalID>=!91F2.8153.F600.AE24</CanonicalID>
 <Service>
  <ProviderID>@!7F6F.F50.A4E4.1133</ProviderID>
  <Type select='true'>xri://+i-service*(+contact)*($v*1.0)</Type>
  <Type match='null'/>
  <Path select='true'>(+contact)</Path>
  <Path match='null'/>
  <MediaType match='default'/>
  <URI append='qxri'>http://contact.freexri.com/contact/</URI>
 </Service>
 <Service priority='20'>
  <ProviderID>@!7F6F.F50.A4E4.1133</ProviderID>
  <Type select='true'>http://openid.net/signon/1.0</Type>
  <Path select='true'>(+login)</Path>
  <Path match='default'/>
  <MediaType match='default'/>
  <URI append='none' priority='2'>http://authn.freexri.com/auth10/</URI>
  <URI append='none' priority='1'>https://authn.freexri.com/auth10/</URI>
 </Service>
 <Service priority='10'>
  <ProviderID>@!7F6F.F50.A4E4.1133</ProviderID>
  <Type select='true'>http://specs.openid.net/auth/2.0/signon</Type>
  <Path select='true'>(+login)</Path>
  <Path match='default'/>
  <MediaType match='default'/>
  <URI append='none' priority='2'>http://authn.freexri.com/auth20/</URI>
  <URI append='none' priority='1'>https://authn.freexri.com/auth20/</URI>
 </Service>
 <ServedBy>OpenXRI</ServedBy>
</XRD>";
			MockHttpRequest.RegisterMockXrdsResponses(new Dictionary<string, string> {
				{"https://xri.net/=MultipleEndpoint?_xrd_r=application/xrd%2Bxml;sep=false", xrds},
			});
			OpenIdRelyingParty rp = new OpenIdRelyingParty(null, null, null);
			Realm realm = new Realm("http://somerealm");
			Uri return_to = new Uri("http://somerealm/return_to");
			IAuthenticationRequest request = rp.CreateRequest("=MultipleEndpoint", realm, return_to);
			Assert.AreEqual("https://authn.freexri.com/auth20/", request.Provider.Uri.AbsoluteUri);
			rp.EndpointOrder = (se1, se2) => -se1.ServicePriority.Value.CompareTo(se2.ServicePriority.Value);
			request = rp.CreateRequest("=MultipleEndpoint", realm, return_to);
			Assert.AreEqual("https://authn.freexri.com/auth10/", request.Provider.Uri.AbsoluteUri);

			// Now test the filter.  Auth20 would come out on top, if we didn't select it out with the filter.
			rp.EndpointOrder = OpenIdRelyingParty.DefaultEndpointOrder;
			rp.EndpointFilter = (se) => se.Uri.AbsoluteUri == "https://authn.freexri.com/auth10/";
			request = rp.CreateRequest("=MultipleEndpoint", realm, return_to);
			Assert.AreEqual("https://authn.freexri.com/auth10/", request.Provider.Uri.AbsoluteUri);
		}

		private string stripScheme(string identifier) {
			return identifier.Substring(identifier.IndexOf("://") + 3);
		}

		[Test]
		public void RequireSslPrependsHttpsScheme() {
			MockHttpRequest.Reset();
			OpenIdRelyingParty rp = TestSupport.CreateRelyingParty(null);
			rp.Settings.RequireSsl = true;
			Identifier mockId = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20, true);
			string noSchemeId = stripScheme(mockId);
			var request = rp.CreateRequest(noSchemeId, TestSupport.Realm, TestSupport.ReturnTo);
			Assert.IsTrue(request.ClaimedIdentifier.ToString().StartsWith("https://", StringComparison.OrdinalIgnoreCase));
		}

		[Test]
		public void DirectedIdentityWithRequireSslSucceeds() {
			Uri claimedId = TestSupport.GetFullUrl("/secureClaimedId", null, true);
			Identifier opIdentifier = TestSupport.GetMockOPIdentifier(TestSupport.Scenarios.AutoApproval, claimedId, true, true);
			var rp = TestSupport.CreateRelyingParty(null);
			rp.Settings.RequireSsl = true;
			var rpRequest = rp.CreateRequest(opIdentifier, TestSupport.Realm, TestSupport.ReturnTo);
			var rpResponse = TestSupport.CreateRelyingPartyResponseThroughProvider(rpRequest, opRequest => {
				opRequest.IsAuthenticated = true;
				opRequest.ClaimedIdentifier = claimedId;
			});
			Assert.AreEqual(AuthenticationStatus.Authenticated, rpResponse.Status);
		}

		[Test]
		public void DirectedIdentityWithRequireSslFailsWithoutSecureIdentity() {
			Uri claimedId = TestSupport.GetFullUrl("/insecureClaimedId", null, false);
			Identifier opIdentifier = TestSupport.GetMockOPIdentifier(TestSupport.Scenarios.AutoApproval, claimedId, true, true);
			var rp = TestSupport.CreateRelyingParty(null);
			rp.Settings.RequireSsl = true;
			var rpRequest = rp.CreateRequest(opIdentifier, TestSupport.Realm, TestSupport.ReturnTo);
			var rpResponse = TestSupport.CreateRelyingPartyResponseThroughProvider(rpRequest, opRequest => {
				opRequest.IsAuthenticated = true;
				opRequest.ClaimedIdentifier = claimedId;
			});
			Assert.AreEqual(AuthenticationStatus.Failed, rpResponse.Status);
		}

		[Test]
		public void DirectedIdentityWithRequireSslFailsWithoutSecureProviderEndpoint() {
			Uri claimedId = TestSupport.GetFullUrl("/secureClaimedId", null, true);
			// We want to generate an OP Identifier that itself is secure, but whose
			// XRDS doc describes an insecure provider endpoint.
			Identifier opIdentifier = TestSupport.GetMockOPIdentifier(TestSupport.Scenarios.AutoApproval, claimedId, true, false);
			var rp = TestSupport.CreateRelyingParty(null);
			rp.Settings.RequireSsl = true;
			var rpRequest = rp.CreateRequest(opIdentifier, TestSupport.Realm, TestSupport.ReturnTo);
			var rpResponse = TestSupport.CreateRelyingPartyResponseThroughProvider(rpRequest, opRequest => {
				opRequest.IsAuthenticated = true;
				opRequest.ClaimedIdentifier = claimedId;
			});
			Assert.AreEqual(AuthenticationStatus.Failed, rpResponse.Status);
		}

		[Test]
		public void UnsolicitedAssertionWithRequireSsl() {
			MockHttpRequest.Reset();
			Mocks.MockHttpRequest.RegisterMockRPDiscovery();
			TestSupport.Scenarios scenario = TestSupport.Scenarios.AutoApproval;
			Identifier claimedId = TestSupport.GetMockIdentifier(scenario, ProtocolVersion.V20, true);
			Identifier localId = TestSupport.GetDelegateUrl(scenario, true);

			OpenIdProvider op = TestSupport.CreateProvider(null, true);
			IResponse assertion = op.PrepareUnsolicitedAssertion(TestSupport.Realm, claimedId, localId);

			var opAuthWebResponse = (Response)assertion;
			var opAuthResponse = (DotNetOpenId.Provider.EncodableResponse)opAuthWebResponse.EncodableMessage;
			var rp = TestSupport.CreateRelyingParty(TestSupport.RelyingPartyStore, opAuthResponse.RedirectUrl,
				opAuthResponse.EncodedFields.ToNameValueCollection());
			rp.Settings.RequireSsl = true;

			Assert.AreEqual(AuthenticationStatus.Authenticated, rp.Response.Status);
			Assert.AreEqual(claimedId, rp.Response.ClaimedIdentifier);
		}

		[Test]
		public void UnsolicitedAssertionWithRequireSslWithoutSecureIdentityUrl() {
			MockHttpRequest.Reset();
			Mocks.MockHttpRequest.RegisterMockRPDiscovery();
			TestSupport.Scenarios scenario = TestSupport.Scenarios.AutoApproval;
			Identifier claimedId = TestSupport.GetMockIdentifier(scenario, ProtocolVersion.V20);
			Identifier localId = TestSupport.GetDelegateUrl(scenario);

			OpenIdProvider op = TestSupport.CreateProvider(null);
			IResponse assertion = op.PrepareUnsolicitedAssertion(TestSupport.Realm, claimedId, localId);

			var opAuthWebResponse = (Response)assertion;
			var opAuthResponse = (DotNetOpenId.Provider.EncodableResponse)opAuthWebResponse.EncodableMessage;
			var rp = TestSupport.CreateRelyingParty(TestSupport.RelyingPartyStore, opAuthResponse.RedirectUrl,
				opAuthResponse.EncodedFields.ToNameValueCollection());
			rp.Settings.RequireSsl = true;

			Assert.AreEqual(AuthenticationStatus.Failed, rp.Response.Status);
			Assert.IsNull(rp.Response.ClaimedIdentifier);
		}

		[Test]
		public void UnsolicitedAssertionWithRequireSslWithSecureIdentityButInsecureProviderEndpoint() {
			MockHttpRequest.Reset();
			Mocks.MockHttpRequest.RegisterMockRPDiscovery();
			TestSupport.Scenarios scenario = TestSupport.Scenarios.AutoApproval;
			ProtocolVersion version = ProtocolVersion.V20;
			ServiceEndpoint providerEndpoint = TestSupport.GetServiceEndpoint(scenario, version, 10, false);
			Identifier claimedId = new MockIdentifier(TestSupport.GetIdentityUrl(scenario, version, true), 
				new ServiceEndpoint[] { providerEndpoint });
			Identifier localId = TestSupport.GetDelegateUrl(scenario, true);

			OpenIdProvider op = TestSupport.CreateProvider(null, false);
			IResponse assertion = op.PrepareUnsolicitedAssertion(TestSupport.Realm, claimedId, localId);

			var opAuthWebResponse = (Response)assertion;
			var opAuthResponse = (DotNetOpenId.Provider.EncodableResponse)opAuthWebResponse.EncodableMessage;
			var rp = TestSupport.CreateRelyingParty(TestSupport.RelyingPartyStore, opAuthResponse.RedirectUrl,
				opAuthResponse.EncodedFields.ToNameValueCollection());
			rp.Settings.RequireSsl = true;

			Assert.AreEqual(AuthenticationStatus.Failed, rp.Response.Status);
			Assert.IsNull(rp.Response.ClaimedIdentifier);
		}

		/// <summary>
		/// Verifies that an RP will not "discover" endpoints below OpenID 2.0 when appropriate.
		/// </summary>
		[Test, ExpectedException(typeof(OpenIdException))]
		public void MinimumOPVersion20() {
			MockIdentifier id = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V11);

			var rp = TestSupport.CreateRelyingParty(null);
			rp.Settings.MinimumRequiredOpenIdVersion = ProtocolVersion.V20;
			rp.CreateRequest(id, TestSupport.Realm, TestSupport.ReturnTo);
		}

		/// <summary>
		/// Verifies that an RP configured to require 2.0 OPs will fail on communicating with 1.x OPs
		/// that merely advertise 2.0 support but don't really have it.
		/// </summary>
		[Test]
		public void MinimumOPVersion20WithDeceptiveEndpointRealizedAtAuthentication() {
			// Create an identifier that claims to have a 2.0 OP endpoint.
			MockIdentifier id = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);

			var rp = TestSupport.CreateRelyingParty(null, null);

			IAuthenticationRequest req = rp.CreateRequest(id, TestSupport.Realm, TestSupport.ReturnTo);
			IResponse providerResponse = TestSupport.CreateProviderResponseToRequest(req, opReq => {
				opReq.IsAuthenticated = true;
			});

			var opAuthWebResponse = (Response)providerResponse;
			var opAuthResponse = (DotNetOpenId.Provider.EncodableResponse)opAuthWebResponse.EncodableMessage;
			var rp2 =TestSupport. CreateRelyingParty(null, opAuthResponse.RedirectUrl,
				opAuthResponse.EncodedFields.ToNameValueCollection());
			rp2.Settings.MinimumRequiredOpenIdVersion = ProtocolVersion.V20;
			// Rig an intercept between the provider and RP to make our own Provider LOOK like a 1.x provider.
			var sniffer = new DirectMessageSniffWrapper(rp2.DirectMessageChannel);
			rp2.DirectMessageChannel = sniffer;
			sniffer.Receiving += (endpoint, fields) => {
				fields.Remove(Protocol.v20.openidnp.ns);
			};
			var resp = rp2.Response;

			Assert.AreEqual(AuthenticationStatus.Failed, resp.Status, "Authentication should have failed since OP is really a 1.x OP masquerading as a 2.0 OP.");
		}
	}
}
