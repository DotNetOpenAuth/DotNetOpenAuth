using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Test.Mocks;
using NUnit.Framework;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class EndToEndTesting {

		[SetUp]
		public void Setup() {
			if (!UntrustedWebRequest.WhitelistHosts.Contains("localhost"))
				UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		[TearDown]
		public void TearDown() {
			MockHttpRequest.Reset();
		}

		void parameterizedTest(TestSupport.Scenarios scenario, ProtocolVersion version,
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult) {
			Identifier claimedId = TestSupport.GetMockIdentifier(scenario, version);
			parameterizedProgrammaticTest(scenario, version, claimedId, requestMode, expectedResult, true);
			parameterizedProgrammaticTest(scenario, version, claimedId, requestMode, expectedResult, false);
		}
		void parameterizedOPIdentifierTest(TestSupport.Scenarios scenario,
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult) {
			ProtocolVersion version = ProtocolVersion.V20; // only this version supports directed identity
			UriIdentifier claimedIdentifier = TestSupport.GetDirectedIdentityUrl(TestSupport.Scenarios.ApproveOnSetup, version);
			Identifier opIdentifier = TestSupport.GetMockOPIdentifier(TestSupport.Scenarios.ApproveOnSetup, claimedIdentifier);
			parameterizedProgrammaticOPIdentifierTest(opIdentifier, version, claimedIdentifier, requestMode, expectedResult, true);
			parameterizedProgrammaticOPIdentifierTest(opIdentifier, version, claimedIdentifier, requestMode, expectedResult, false);
		}
		void parameterizedProgrammaticTest(TestSupport.Scenarios scenario, ProtocolVersion version, 
			Identifier claimedUrl, AuthenticationRequestMode requestMode, 
			AuthenticationStatus expectedResult, bool provideStore) {

			var request = TestSupport.CreateRelyingPartyRequest(!provideStore, scenario, version);
			request.Mode = requestMode;

			var rpResponse = TestSupport.CreateRelyingPartyResponseThroughProvider(request, 
				opReq => opReq.IsAuthenticated = expectedResult == AuthenticationStatus.Authenticated);
			Assert.AreEqual(expectedResult, rpResponse.Status);
			Assert.AreEqual(claimedUrl, rpResponse.ClaimedIdentifier);
		}
		void parameterizedProgrammaticOPIdentifierTest(Identifier opIdentifier, ProtocolVersion version,
			Identifier claimedUrl, AuthenticationRequestMode requestMode,
			AuthenticationStatus expectedResult, bool provideStore) {

			var rp = TestSupport.CreateRelyingParty(provideStore ? TestSupport.RelyingPartyStore : null, null, null);

			var returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);
			var realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
			var request = rp.CreateRequest(opIdentifier, realm, returnTo);
			request.Mode = requestMode;

			var rpResponse = TestSupport.CreateRelyingPartyResponseThroughProvider(request,
				opReq => {
					opReq.ClaimedIdentifier = claimedUrl;
					opReq.IsAuthenticated = expectedResult == AuthenticationStatus.Authenticated;
				});
			Assert.AreEqual(expectedResult, rpResponse.Status);
			if (rpResponse.Status == AuthenticationStatus.Authenticated) {
				Assert.AreEqual(claimedUrl, rpResponse.ClaimedIdentifier);
			}
		}
		[Test]
		public void Pass_Setup_AutoApproval_11() {
			parameterizedTest(
				TestSupport.Scenarios.AutoApproval, ProtocolVersion.V11,
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated
			);
		}
		[Test]
		public void Pass_Setup_AutoApproval_20() {
			parameterizedTest(
				TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20,
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated
			);
		}

		[Test]
		public void Pass_Immediate_AutoApproval_11() {
			parameterizedTest(
				TestSupport.Scenarios.AutoApproval, ProtocolVersion.V11,
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.Authenticated
			);
		}
		[Test]
		public void Pass_Immediate_AutoApproval_20() {
			parameterizedTest(
				TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20,
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.Authenticated
			);
		}

		[Test]
		public void Fail_Immediate_ApproveOnSetup_11() {
			parameterizedTest(
				TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V11,
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.SetupRequired
			);
		}
		[Test]
		public void Fail_Immediate_ApproveOnSetup_20() {
			parameterizedTest(
				TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V20,
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.SetupRequired
			);
		}

		[Test]
		public void Pass_Setup_ApproveOnSetup_11() {
			parameterizedTest(
				TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V11,
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated
			);
		}
		[Test]
		public void Pass_Setup_ApproveOnSetup_20() {
			parameterizedTest(
				TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V20,
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated
			);
		}

		[Test]
		public void Pass_Immediate_AutoApproval_DirectedIdentity_20() {
			parameterizedOPIdentifierTest(
				TestSupport.Scenarios.AutoApproval,
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.Authenticated);
		}

		[Test]
		public void Pass_Setup_ApproveOnSetup_DirectedIdentity_20() {
			parameterizedOPIdentifierTest(
				TestSupport.Scenarios.ApproveOnSetup,
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated);
		}

		[Test]
		public void Fail_Immediate_ApproveOnSetup_DirectedIdentity_20() {
			parameterizedOPIdentifierTest(
				TestSupport.Scenarios.ApproveOnSetup,
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.SetupRequired);
		}

		[Test]
		public void ProviderAddedFragmentRemainsInClaimedIdentifier() {
			Identifier userSuppliedIdentifier = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApprovalAddFragment, ProtocolVersion.V20);
			UriBuilder claimedIdentifier = new UriBuilder(userSuppliedIdentifier);
			claimedIdentifier.Fragment = "frag";
			parameterizedProgrammaticTest(
				TestSupport.Scenarios.AutoApprovalAddFragment, ProtocolVersion.V20,
				claimedIdentifier.Uri,
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated,
				true
			);
		}

		[Test]
		public void SampleScriptedTest() {
			var rpReq = TestSupport.CreateRelyingPartyRequest(false, TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
			var rpResp = TestSupport.CreateRelyingPartyResponseThroughProvider(rpReq, opReq => opReq.IsAuthenticated = true);
			Assert.AreEqual(AuthenticationStatus.Authenticated, rpResp.Status);
		}
	}
}
