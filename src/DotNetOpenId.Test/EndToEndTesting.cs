using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using DotNetOpenId.RelyingParty;
using NUnit.Framework;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class EndToEndTesting {

		[SetUp]
		public void Setup() {
			if (!UntrustedWebRequest.WhitelistHosts.Contains("localhost"))
				UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		void parameterizedTest(TestSupport.Scenarios scenario, ProtocolVersion version,
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult) {
			Identifier userSuppliedIdentifier = TestSupport.GetMockIdentifier(scenario, version);
			Identifier claimedId = userSuppliedIdentifier;
			parameterizedProgrammaticTest(scenario, version, claimedId, requestMode, expectedResult, true);
			parameterizedProgrammaticTest(scenario, version, claimedId, requestMode, expectedResult, false);
			parameterizedWebClientTest(userSuppliedIdentifier, requestMode, expectedResult);
		}
		void parameterizedOPIdentifierTest(TestSupport.Scenarios scenario,
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult) {
			ProtocolVersion version = ProtocolVersion.V20; // only this version supports directed identity
			Identifier opIdentifier = TestSupport.GetMockOPIdentifier(TestSupport.Scenarios.ApproveOnSetup);
			Identifier claimedIdentifier = TestSupport.GetDirectedIdentityUrl(TestSupport.Scenarios.ApproveOnSetup, version);
			parameterizedProgrammaticOPIdentifierTest(opIdentifier, version, claimedIdentifier, requestMode, expectedResult, true);
			parameterizedProgrammaticOPIdentifierTest(opIdentifier, version, claimedIdentifier, requestMode, expectedResult, false);
			parameterizedWebClientTest(opIdentifier, requestMode, expectedResult);
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
		void parameterizedWebClientTest(Identifier identityUrl,
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult) {

			Uri redirectToProviderUrl;
			HttpWebRequest rpRequest = (HttpWebRequest)WebRequest.Create(TestSupport.GetFullUrl(TestSupport.ConsumerPage));
			NameValueCollection query = new NameValueCollection();
			using (HttpWebResponse response = (HttpWebResponse)rpRequest.GetResponse()) {
				using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
					Regex regex = new Regex(@"\<input\b.*\bname=""(\w+)"".*\bvalue=""([^""]+)""", RegexOptions.IgnoreCase);
					while (!sr.EndOfStream) {
						string line = sr.ReadLine();
						Match m = regex.Match(line);
						if (m.Success) {
							query[m.Groups[1].Value] = m.Groups[2].Value;
						}
					}
				}
			}
			query["OpenIdTextBox1$wrappedTextBox"] = identityUrl;
			rpRequest = (HttpWebRequest)WebRequest.Create(TestSupport.GetFullUrl(TestSupport.ConsumerPage));
			rpRequest.Method = "POST";
			rpRequest.AllowAutoRedirect = false;
			string queryString = UriUtil.CreateQueryString(query);
			rpRequest.ContentLength = queryString.Length;
			rpRequest.ContentType = "application/x-www-form-urlencoded";
			using (StreamWriter sw = new StreamWriter(rpRequest.GetRequestStream())) {
				sw.Write(queryString);
			}
			using (HttpWebResponse response = (HttpWebResponse)rpRequest.GetResponse()) {
				using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
					string doc = sr.ReadToEnd();
					Debug.WriteLine(doc);
				}
				redirectToProviderUrl = new Uri(response.Headers[HttpResponseHeader.Location]);
			}

			HttpWebRequest providerRequest = (HttpWebRequest)WebRequest.Create(redirectToProviderUrl);
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
			rpRequest = (HttpWebRequest)WebRequest.Create(redirectUrl);
			rpRequest.AllowAutoRedirect = false;
			using (HttpWebResponse response = (HttpWebResponse)rpRequest.GetResponse()) {
				Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode); // redirect on login
			}

			// Try replay attack
			if (expectedResult == AuthenticationStatus.Authenticated) {
				// This simulates a network sniffing user who caught the 
				// authenticating query en route to either the user agent or
				// the consumer, and tries the same query to the consumer in an
				// attempt to spoof the identity of the authenticating user.
				rpRequest = (HttpWebRequest)WebRequest.Create(redirectUrl);
				rpRequest.AllowAutoRedirect = false;
				using (HttpWebResponse response = (HttpWebResponse)rpRequest.GetResponse()) {
					Assert.AreEqual(HttpStatusCode.OK, response.StatusCode); // error message
				}
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
