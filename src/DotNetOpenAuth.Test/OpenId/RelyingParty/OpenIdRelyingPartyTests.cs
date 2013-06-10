//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Linq;
	using System.Net.Http;
	using System.Threading.Tasks;
	using System.Web;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;

	using NUnit.Framework;

	[TestFixture]
	public class OpenIdRelyingPartyTests : OpenIdTestBase {
		[SetUp]
		public override void SetUp() {
			base.SetUp();
		}

		[Test]
		public async Task CreateRequestDumbMode() {
			var rp = this.CreateRelyingParty(true);
			Identifier id = this.GetMockIdentifier(ProtocolVersion.V20);
			var authReq = await rp.CreateRequestAsync(id, RPRealmUri, RPUri);
			var httpMessage = await authReq.GetRedirectingResponseAsync();
			var data = HttpUtility.ParseQueryString(httpMessage.GetDirectUriRequest().Query);
			Assert.IsNull(data[Protocol.Default.openid.assoc_handle]);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SecuritySettingsSetNull() {
			var rp = new OpenIdRelyingParty(new MemoryCryptoKeyAndNonceStore());
			rp.SecuritySettings = null;
		}

		[Test]
		public void ExtensionFactories() {
			var rp = new OpenIdRelyingParty(null);
			var factories = rp.ExtensionFactories;
			Assert.IsNotNull(factories);
			Assert.AreEqual(1, factories.Count);
			Assert.IsInstanceOf<StandardOpenIdExtensionFactory>(factories[0]);
		}

		[Test]
		public async Task CreateRequest() {
			var rp = this.CreateRelyingParty();
			StoreAssociation(rp, OPUri, HmacShaAssociation.Create("somehandle", new byte[20], TimeSpan.FromDays(1)));
			Identifier id = Identifier.Parse(GetMockIdentifier(ProtocolVersion.V20));
			var req = await rp.CreateRequestAsync(id, RPRealmUri, RPUri);
			Assert.IsNotNull(req);
		}

		[Test]
		public async Task CreateRequests() {
			var rp = this.CreateRelyingParty();
			StoreAssociation(rp, OPUri, HmacShaAssociation.Create("somehandle", new byte[20], TimeSpan.FromDays(1)));
			Identifier id = Identifier.Parse(GetMockIdentifier(ProtocolVersion.V20));
			var requests = await rp.CreateRequestsAsync(id, RPRealmUri, RPUri);
			Assert.AreEqual(1, requests.Count());
		}

		[Test]
		public async Task CreateRequestsWithEndpointFilter() {
			var rp = this.CreateRelyingParty();
			StoreAssociation(rp, OPUri, HmacShaAssociation.Create("somehandle", new byte[20], TimeSpan.FromDays(1)));
			Identifier id = Identifier.Parse(GetMockIdentifier(ProtocolVersion.V20));

			rp.EndpointFilter = opendpoint => true;
			var requests = await rp.CreateRequestsAsync(id, RPRealmUri, RPUri);
			Assert.AreEqual(1, requests.Count());

			rp.EndpointFilter = opendpoint => false;
			requests = await rp.CreateRequestsAsync(id, RPRealmUri, RPUri);
			Assert.AreEqual(0, requests.Count());
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public async Task CreateRequestOnNonOpenID() {
			var nonOpenId = new Uri("http://www.microsoft.com/");
			Handle(nonOpenId).By("<html/>", "text/html");
			var rp = this.CreateRelyingParty();
			await rp.CreateRequestAsync(nonOpenId, RPRealmUri, RPUri);
		}

		[Test]
		public async Task CreateRequestsOnNonOpenID() {
			var nonOpenId = new Uri("http://www.microsoft.com/");
			Handle(nonOpenId).By("<html/>", "text/html");
			var rp = this.CreateRelyingParty();
			var requests = await rp.CreateRequestsAsync(nonOpenId, RPRealmUri, RPUri);
			Assert.AreEqual(0, requests.Count());
		}

		/// <summary>
		/// Verifies that incoming positive assertions throw errors if they come from
		/// OPs that are not approved by <see cref="OpenIdRelyingParty.EndpointFilter"/>.
		/// </summary>
		[Test]
		public async Task AssertionWithEndpointFilter() {
			var opStore = new MemoryCryptoKeyAndNonceStore();
			Handle(RPUri).By(
				async req => {
					var rp = new OpenIdRelyingParty(new MemoryCryptoKeyAndNonceStore(), this.HostFactories);

					// Rig it to always deny the incoming OP
					rp.EndpointFilter = op => false;

					// Receive the unsolicited assertion
					var response = await rp.GetResponseAsync(req);
					Assert.That(response, Is.Not.Null);
					Assert.AreEqual(AuthenticationStatus.Failed, response.Status);
					return new HttpResponseMessage();
				});
			this.RegisterAutoProvider();
			{
				var op = new OpenIdProvider(opStore, this.HostFactories);
				Identifier id = GetMockIdentifier(ProtocolVersion.V20);
				var assertion = await op.PrepareUnsolicitedAssertionAsync(OPUri, GetMockRealm(false), id, id);
				using (var httpClient = this.HostFactories.CreateHttpClient()) {
					using (var response = await httpClient.GetAsync(assertion.Headers.Location)) {
						response.EnsureSuccessStatusCode();
					}
				}
			}
		}
	}
}
