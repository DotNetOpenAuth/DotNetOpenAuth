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

namespace DotNetOpenId.Test {
	[TestFixture]
	public class EndToEndTesting {
		IRelyingPartyApplicationStore appStore;

		[SetUp]
		public void Setup() {
			appStore = new ConsumerApplicationMemoryStore();
		}

		void parameterizedTest(UriIdentifier identityUrl,
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult,
			bool tryReplayAttack, bool provideStore) {
			parameterizedTest(identityUrl,
				new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri),
				TestSupport.GetFullUrl(TestSupport.ConsumerPage),
				requestMode, expectedResult, tryReplayAttack, provideStore);
			parameterizedTest(identityUrl,
				new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri),
				TestSupport.GetFullUrl(TestSupport.ConsumerPage),
				requestMode, expectedResult, tryReplayAttack, provideStore);
		}
		void parameterizedTest(UriIdentifier identityUrl, Realm realm, Uri returnTo,
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult,
			bool tryReplayAttack, bool provideStore) {
			var store = provideStore ? appStore : null;

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
			var consumer2 = new OpenIdRelyingParty(store, redirectUrl);
			Assert.AreEqual(expectedResult, consumer2.Response.Status);
			Assert.AreEqual(identityUrl, consumer2.Response.ClaimedIdentifier);

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
