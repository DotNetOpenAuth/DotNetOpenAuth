using System;
using DotNetOpenId.RelyingParty;
using NUnit.Framework;
using ProviderMemoryStore = DotNetOpenId.AssociationMemoryStore<DotNetOpenId.AssociationRelyingPartyType>;

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
			store = new ConsumerApplicationMemoryStore();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DefaultCtorWithoutContext() {
			new OpenIdRelyingParty();
		}

		[Test]
		public void CtorWithNullRequestUri() {
			new OpenIdRelyingParty(store, null);
		}

		[Test]
		public void CtorWithNullStore() {
			var consumer = new OpenIdRelyingParty(null, simpleNonOpenIdRequest);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext1() {
			var consumer = new OpenIdRelyingParty(store, simpleNonOpenIdRequest);
			consumer.CreateRequest(simpleOpenId);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext2() {
			var consumer = new OpenIdRelyingParty(store, simpleNonOpenIdRequest);
			consumer.CreateRequest(simpleOpenId, realm);
		}

		[Test]
		public void AssociationCreationWithStore() {
			var providerStore = new ProviderMemoryStore();

			OpenIdRelyingParty rp = new OpenIdRelyingParty(new ConsumerApplicationMemoryStore(), null);
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

			OpenIdRelyingParty rp = new OpenIdRelyingParty(null, null);
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
	}
}
