using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.RelyingParty;
using System.Collections.Specialized;
using System.Web;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class EndToEndTesting {
		IRelyingPartyApplicationStore appStore;

		[SetUp]
		public void Setup() {
			appStore = new ApplicationMemoryStore();
			if (!UntrustedWebRequest.WhitelistHosts.Contains("localhost"))
				UntrustedWebRequest.WhitelistHosts.Add("localhost");
		}

		void parameterizedTest(UriIdentifier identityUrl,
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult,
			bool tryReplayAttack, bool provideStore) {
			parameterizedProgrammaticTest(identityUrl, requestMode, expectedResult, tryReplayAttack, provideStore);
			parameterizedWebClientTest(identityUrl, requestMode, expectedResult, tryReplayAttack, provideStore);
		}
		void parameterizedProgrammaticTest(UriIdentifier identityUrl,
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult,
			bool tryReplayAttack, bool provideStore) {
			var store = provideStore ? appStore : null;

			Uri redirectToProviderUrl;
			var returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);
			var realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
			var consumer = new OpenIdRelyingParty(store, null);
			Assert.IsNull(consumer.Response);
			var request = consumer.CreateRequest(identityUrl, realm, returnTo);
			Protocol protocol = Protocol.Lookup(request.ProviderVersion);

			// Test properties and defaults
			Assert.AreEqual(AuthenticationRequestMode.Setup, request.Mode);
			Assert.AreEqual(returnTo, request.ReturnToUrl);
			Assert.AreEqual(realm, request.Realm);

			request.Mode = requestMode;

			// Verify the redirect URL
			Assert.IsNotNull(request.RedirectToProviderUrl);
			var consumerToProviderQuery = HttpUtility.ParseQueryString(request.RedirectToProviderUrl.Query);
			Assert.IsTrue(consumerToProviderQuery[protocol.openid.return_to].StartsWith(returnTo.AbsoluteUri, StringComparison.Ordinal));
			Assert.AreEqual(realm.ToString(), consumerToProviderQuery[protocol.openid.Realm]);
			redirectToProviderUrl = request.RedirectToProviderUrl;

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
			consumer = new OpenIdRelyingParty(store, redirectUrl);
			Assert.AreEqual(expectedResult, consumer.Response.Status);
			Assert.AreEqual(identityUrl, consumer.Response.ClaimedIdentifier);

			// Try replay attack
			if (tryReplayAttack) {
				// This simulates a network sniffing user who caught the 
				// authenticating query en route to either the user agent or
				// the consumer, and tries the same query to the consumer in an
				// attempt to spoof the identity of the authenticating user.
				try {
					var replayAttackConsumer = new OpenIdRelyingParty(store, redirectUrl);
					Assert.AreNotEqual(AuthenticationStatus.Authenticated, replayAttackConsumer.Response.Status, "Replay attack");
				} catch (OpenIdException) { // nonce already used
					// another way to pass
				}
			}
		}
		void parameterizedWebClientTest(UriIdentifier identityUrl,
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult,
			bool tryReplayAttack, bool provideStore) {
			var store = provideStore ? appStore : null;

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
			if (tryReplayAttack) {
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
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V11),
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated,
				true,
				true
			);
		}
		[Test]
		public void Pass_Setup_AutoApproval_20() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20),
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated,
				true,
				true
			);
		}

		[Test]
		public void Pass_Immediate_AutoApproval_11() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V11),
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.Authenticated,
				true,
				true
			);
		}
		[Test]
		public void Pass_Immediate_AutoApproval_20() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20),
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.Authenticated,
				true,
				true
			);
		}

		[Test]
		public void Fail_Immediate_ApproveOnSetup_11() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V11),
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.SetupRequired,
				false,
				true
			);
		}
		[Test]
		public void Fail_Immediate_ApproveOnSetup_20() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V20),
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.SetupRequired,
				false,
				true
			);
		}

		[Test]
		public void Pass_Setup_ApproveOnSetup_11() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V11),
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated,
				true,
				true
			);
		}
		[Test]
		public void Pass_Setup_ApproveOnSetup_20() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V20),
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated,
				true,
				true
			);
		}

		[Test]
		public void Pass_NoStore_AutoApproval_11() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V11),
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated,
				true,
				false
			);
		}
		[Test]
		public void Pass_NoStore_AutoApproval_20() {
			parameterizedTest(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V20),
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated,
				true,
				false
			);
		}
	}
}
