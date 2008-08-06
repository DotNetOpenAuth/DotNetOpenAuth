using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Test.Mocks;
using NUnit.Framework;

namespace DotNetOpenId.Test.RelyingParty {
	[TestFixture]
	public class AuthenticationResponseTests {
		Realm realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
		Uri returnTo;
		const string returnToRemovableParameter = "a";

		public AuthenticationResponseTests() {
			UriBuilder builder = new UriBuilder(TestSupport.GetFullUrl(TestSupport.ConsumerPage));
			// we add something pointless to the return_to, so some tests have something to remove.
			UriUtil.AppendQueryArgs(builder, new Dictionary<string, string> {
				{returnToRemovableParameter, "b" }});
			returnTo = builder.Uri;
		}

		[SetUp]
		public void SetUp() {
			if (!UntrustedWebRequest.WhitelistHosts.Contains("localhost"))
				UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		[TearDown]
		public void TearDown() {
			MockHttpRequest.Reset();
		}

		Uri getPositiveAssertion(ProtocolVersion version) {
			OpenIdRelyingParty rp = TestSupport.CreateRelyingParty(null);
			Identifier id = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, version);
			var request = rp.CreateRequest(id, realm, returnTo);
			var provider = TestSupport.CreateProviderForRequest(request);
			var opRequest = provider.Request as DotNetOpenId.Provider.IAuthenticationRequest;
			opRequest.IsAuthenticated = true;
			return opRequest.Response.ExtractUrl();
		}
		void removeQueryParameter(ref Uri uri, string parameterToRemove) {
			UriBuilder builder = new UriBuilder(uri);
			NameValueCollection nvc = HttpUtility.ParseQueryString(builder.Query);
			nvc.Remove(parameterToRemove);
			builder.Query = UriUtil.CreateQueryString(nvc);
			uri = builder.Uri;
		}
		void setQueryParameter(ref Uri uri, string parameter, string newValue) {
			UriBuilder builder = new UriBuilder(uri);
			NameValueCollection nvc = HttpUtility.ParseQueryString(builder.Query);
			nvc[parameter] = newValue;
			builder.Query = UriUtil.CreateQueryString(nvc);
			uri = builder.Uri;
		}
		void removeReturnToParameter(ref Uri uri, string parameterToRemove) {
			UriBuilder builder = new UriBuilder(uri);
			var args = Util.NameValueCollectionToDictionary(
				HttpUtility.ParseQueryString(builder.Query));
			Protocol protocol = Protocol.Detect(args);
			UriBuilder return_to = new UriBuilder(protocol.openid.return_to);
			var returnToArgs = Util.NameValueCollectionToDictionary(
				HttpUtility.ParseQueryString(return_to.Query));
			returnToArgs.Remove(parameterToRemove);
			return_to.Query = UriUtil.CreateQueryString(returnToArgs);
			args[protocol.openid.return_to] = return_to.ToString();
			builder.Query = UriUtil.CreateQueryString(args);
			uri = builder.Uri;
		}
		void resign(ref Uri uri) {
			UriBuilder builder = new UriBuilder(uri);
			NameValueCollection nvc = HttpUtility.ParseQueryString(builder.Query);
			TestSupport.Resign(nvc, TestSupport.RelyingPartyStore);
			builder.Query = UriUtil.CreateQueryString(nvc);
			uri = builder.Uri;
		}

		[Test]
		public void ReturnToMismatchDetection() {
			ProtocolVersion version = ProtocolVersion.V20;
			Protocol protocol = Protocol.Lookup(version);
			Uri assertion = getPositiveAssertion(version);
			// Here we remove a parameter from the assertion's query line,
			// which should cause a failure because the return_to argument
			// says that parameter is supposed to be there.
			removeQueryParameter(ref assertion, returnToRemovableParameter);
			var response = TestSupport.CreateRelyingParty(TestSupport.RelyingPartyStore, assertion, HttpUtility.ParseQueryString(assertion.Query)).Response;
			Assert.AreEqual(AuthenticationStatus.Failed, response.Status);
			Assert.IsNotNull(response.Exception);
		}

		/// <summary>
		/// Verifies that the RP rejects signed assertions by an OP that makes up a
		/// claimed Id that was not part of the original request, and that the OP
		/// has no authority to assert positively regarding.
		/// </summary>
		[Test]
		public void SpoofedClaimedIdDetection_20() {
			ProtocolVersion version = ProtocolVersion.V20;
			Protocol protocol = Protocol.Lookup(version);
			Uri assertion = getPositiveAssertion(version);
			// The strategy here is to change the openid.claimed_id parameter to be something totally
			// different that originally requested, but to be a positive assertion.  The OP in question
			// has no authority over this new claimed_id, and so it should be rejected.
			// By tampering with the parameters though, we should trip several alarms in the RP:
			// 1) the token's cache of the claimed id mismatches
			// 2) when we remove the token so there's nothing to mismatch with, the return_to doesn't match
			// 3) when we hack return_to to match, we invalidate the signature
			// 4) even when we resign the message (a contrived OP would lie intentionally and have a valid 
			//    signature) the RP's discovery on the new claimed id reveals a different OP endpoint is 
			//    responsible for it, so it should be rejected.
			
			setQueryParameter(ref assertion, protocol.openid.claimed_id,
				TestSupport.GetIdentityUrl( // set a different identity
				TestSupport.Scenarios.ApproveOnSetup, version)); // "when you tell one lie, it leads to another"
			removeQueryParameter(ref assertion, Token.TokenKey); // "then you tell two lies, to cover each other"
			removeReturnToParameter(ref assertion, Token.TokenKey); // "then you tell three lies and--oh brother..." (it's a song)
			resign(ref assertion); // resign changed URL to simulate a contrived OP for breaking into RPs.

			// (triggers exception) "... you're in trouble up to your ears."
			var response = TestSupport.CreateRelyingParty(TestSupport.RelyingPartyStore, assertion, HttpUtility.ParseQueryString(assertion.Query)).Response;
			Assert.AreEqual(AuthenticationStatus.Failed, response.Status);
			Assert.IsNotNull(response.Exception);
		}

	}
}
