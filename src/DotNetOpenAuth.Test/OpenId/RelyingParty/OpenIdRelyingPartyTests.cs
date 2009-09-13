//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class OpenIdRelyingPartyTests : OpenIdTestBase {
		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
		}

		[TestMethod]
		public void CreateRequestDumbMode() {
			var rp = new OpenIdRelyingParty(null);
			Identifier id = this.GetMockIdentifier(ProtocolVersion.V20);
			var authReq = rp.CreateRequest(id, RPRealmUri, RPUri);
			CheckIdRequest requestMessage = (CheckIdRequest)authReq.RedirectingResponse.OriginalMessage;
			Assert.IsNull(requestMessage.AssociationHandle);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SecuritySettingsSetNull() {
			var rp = new OpenIdRelyingParty(new StandardRelyingPartyApplicationStore());
			rp.SecuritySettings = null;
		}

		[TestMethod]
		public void ExtensionFactories() {
			var rp = new OpenIdRelyingParty(null);
			var factories = rp.ExtensionFactories;
			Assert.IsNotNull(factories);
			Assert.AreEqual(1, factories.Count);
			Assert.IsInstanceOfType(factories[0], typeof(StandardOpenIdExtensionFactory));
		}

		[TestMethod]
		public void CreateRequest() {
			var rp = this.CreateRelyingParty();
			StoreAssociation(rp, OPUri, HmacShaAssociation.Create("somehandle", new byte[20], TimeSpan.FromDays(1)));
			Identifier id = Identifier.Parse(GetMockIdentifier(ProtocolVersion.V20));
			var req = rp.CreateRequest(id, RPRealmUri, RPUri);
			Assert.IsNotNull(req);
		}

		[TestMethod]
		public void CreateRequests() {
			var rp = this.CreateRelyingParty();
			StoreAssociation(rp, OPUri, HmacShaAssociation.Create("somehandle", new byte[20], TimeSpan.FromDays(1)));
			Identifier id = Identifier.Parse(GetMockIdentifier(ProtocolVersion.V20));
			var requests = rp.CreateRequests(id, RPRealmUri, RPUri);
			Assert.AreEqual(1, requests.Count());
		}

		[TestMethod]
		public void CreateRequestsWithEndpointFilter() {
			var rp = this.CreateRelyingParty();
			StoreAssociation(rp, OPUri, HmacShaAssociation.Create("somehandle", new byte[20], TimeSpan.FromDays(1)));
			Identifier id = Identifier.Parse(GetMockIdentifier(ProtocolVersion.V20));

			rp.EndpointFilter = opendpoint => true;
			var requests = rp.CreateRequests(id, RPRealmUri, RPUri);
			Assert.AreEqual(1, requests.Count());

			rp.EndpointFilter = opendpoint => false;
			requests = rp.CreateRequests(id, RPRealmUri, RPUri);
			Assert.AreEqual(0, requests.Count());
		}

		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void CreateRequestOnNonOpenID() {
			Uri nonOpenId = new Uri("http://www.microsoft.com/");
			var rp = this.CreateRelyingParty();
			this.MockResponder.RegisterMockResponse(nonOpenId, "text/html", "<html/>");
			rp.CreateRequest(nonOpenId, RPRealmUri, RPUri);
		}

		[TestMethod]
		public void CreateRequestsOnNonOpenID() {
			Uri nonOpenId = new Uri("http://www.microsoft.com/");
			var rp = this.CreateRelyingParty();
			this.MockResponder.RegisterMockResponse(nonOpenId, "text/html", "<html/>");
			var requests = rp.CreateRequests(nonOpenId, RPRealmUri, RPUri);
			Assert.AreEqual(0, requests.Count());
		}

		/// <summary>
		/// Verifies that incoming positive assertions throw errors if they come from
		/// OPs that are not approved by <see cref="OpenIdRelyingParty.EndpointFilter"/>.
		/// </summary>
		[TestMethod]
		public void AssertionWithEndpointFilter() {
			var coordinator = new OpenIdCoordinator(
				rp => {
					// register with RP so that id discovery passes
					rp.Channel.WebRequestHandler = this.MockResponder.MockWebRequestHandler;

					// Rig it to always deny the incoming OP
					rp.EndpointFilter = op => false;

					// Receive the unsolicited assertion
					var response = rp.GetResponse();
					Assert.AreEqual(AuthenticationStatus.Failed, response.Status);
				},
				op => {
					Identifier id = GetMockIdentifier(ProtocolVersion.V20);
					op.SendUnsolicitedAssertion(OPUri, GetMockRealm(false), id, id);
					AutoProvider(op);
				});
			coordinator.Run();
		}
	}
}
