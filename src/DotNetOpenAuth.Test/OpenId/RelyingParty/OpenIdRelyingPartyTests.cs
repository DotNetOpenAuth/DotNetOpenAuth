//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
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
			Identifier id = this.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
			var authReq = rp.CreateRequest(id, TestSupport.Realm, TestSupport.ReturnTo);
			CheckIdRequest requestMessage = (CheckIdRequest)authReq.RedirectingResponse.OriginalMessage;
			Assert.IsNull(requestMessage.AssociationHandle);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SecuritySettingsSetNull() {
			var rp = new OpenIdRelyingParty(new StandardRelyingPartyApplicationStore());
			rp.SecuritySettings = null;
		}

		[TestMethod]
		public void CreateRequest() {
			var rp = this.CreateRelyingParty();
			rp.AssociationStore.StoreAssociation(TestSupport.GetFullUrl("/" + TestSupport.ProviderPage), HmacShaAssociation.Create("somehandle", new byte[20], TimeSpan.FromDays(1)));
			Identifier id = Identifier.Parse(GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20));
			var req = rp.CreateRequest(id, TestSupport.Realm, TestSupport.ReturnTo);
			Assert.IsNotNull(req);
		}

		[TestMethod]
		public void CreateRequests() {
			var rp = this.CreateRelyingParty();
			rp.AssociationStore.StoreAssociation(TestSupport.GetFullUrl("/" + TestSupport.ProviderPage), HmacShaAssociation.Create("somehandle", new byte[20], TimeSpan.FromDays(1)));
			Identifier id = Identifier.Parse(GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20));
			var requests = rp.CreateRequests(id, TestSupport.Realm, TestSupport.ReturnTo);
			Assert.AreEqual(1, requests.Count());
		}

		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void CreateRequestOnNonOpenID() {
			Uri nonOpenId = new Uri("http://www.microsoft.com/");
			var rp = this.CreateRelyingParty();
			this.MockResponder.RegisterMockResponse(nonOpenId, "text/html", "<html/>");
			rp.CreateRequest(nonOpenId, TestSupport.Realm, TestSupport.ReturnTo);
		}

		[TestMethod]
		public void CreateRequestsOnNonOpenID() {
			Uri nonOpenId = new Uri("http://www.microsoft.com/");
			var rp = this.CreateRelyingParty();
			this.MockResponder.RegisterMockResponse(nonOpenId, "text/html", "<html/>");
			var requests = rp.CreateRequests(nonOpenId, TestSupport.Realm, TestSupport.ReturnTo);
			Assert.AreEqual(0, requests.Count());
		}
	}
}
