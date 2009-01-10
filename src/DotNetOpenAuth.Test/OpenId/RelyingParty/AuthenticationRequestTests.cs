//-----------------------------------------------------------------------
// <copyright file="AuthenticationRequestTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AuthenticationRequestTests : OpenIdTestBase {
		private readonly Realm realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
		private readonly Uri returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);
		private readonly Identifier claimedId = "http://claimedId";
		private readonly Identifier delegatedLocalId = "http://localId";
		private readonly Protocol protocol = Protocol.Default;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
		}

		/// <summary>
		/// Verifies IsDirectedIdentity returns true when appropriate.
		/// </summary>
		[TestMethod]
		public void IsDirectedIdentity() {
			IAuthenticationRequest_Accessor iauthRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			Assert.IsFalse(iauthRequest.IsDirectedIdentity);

			iauthRequest = this.CreateAuthenticationRequest(IdentifierSelect, IdentifierSelect);
			Assert.IsTrue(iauthRequest.IsDirectedIdentity);
		}

		/// <summary>
		/// Verifies ClaimedIdentifier behavior.
		/// </summary>
		[TestMethod]
		public void ClaimedIdentifier() {
			IAuthenticationRequest_Accessor iauthRequest = this.CreateAuthenticationRequest(this.claimedId, this.delegatedLocalId);
			Assert.AreEqual(this.claimedId, iauthRequest.ClaimedIdentifier);

			iauthRequest = this.CreateAuthenticationRequest(IdentifierSelect, IdentifierSelect);
			Assert.IsNull(iauthRequest.ClaimedIdentifier, "In directed identity mode, the ClaimedIdentifier should be null.");
		}

		/// <summary>
		/// Verifies ProviderVersion behavior.
		/// </summary>
		[TestMethod]
		public void ProviderVersion() {
			var authRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			Assert.AreEqual(this.protocol.Version, authRequest.ProviderVersion);
		}

		/// <summary>
		/// Verifies RedirectingResponse.
		/// </summary>
		[TestMethod]
		public void CreateRequestMessage() {
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					Identifier id = this.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
					IAuthenticationRequest authRequest = rp.CreateRequest(id, this.realm, this.returnTo);

					// Add some callback arguments
					authRequest.AddCallbackArguments("a", "b");
					authRequest.AddCallbackArguments(new Dictionary<string, string> { { "c", "d" }, { "e", "f" } });

					// Assembly an extension request.
					ClaimsRequest sregRequest = new ClaimsRequest();
					sregRequest.Nickname = DemandLevel.Request;
					authRequest.AddExtension(sregRequest);

					// Construct the actual authentication request message.
					var authRequestAccessor = AuthenticationRequest_Accessor.AttachShadow(authRequest);
					var req = authRequestAccessor.CreateRequestMessage();
					Assert.IsNotNull(req);

					// Verify that callback arguments were included.
					NameValueCollection callbackArguments = HttpUtility.ParseQueryString(req.ReturnTo.Query);
					Assert.AreEqual("b", callbackArguments["a"]);
					Assert.AreEqual("d", callbackArguments["c"]);
					Assert.AreEqual("f", callbackArguments["e"]);

					// Verify that extensions were included.
					Assert.AreEqual(1, req.Extensions.Count);
					Assert.IsTrue(req.Extensions.Contains(sregRequest));
				},
				TestSupport.AutoProvider);
			coordinator.Run();
		}

		/// <summary>
		/// Verifies the Provider property returns non-null.
		/// </summary>
		[TestMethod]
		public void Provider() {
			IAuthenticationRequest_Accessor authRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			Assert.IsNotNull(authRequest.Provider);
			Assert.AreEqual(ProviderUri, authRequest.Provider.Uri);
			Assert.AreEqual(this.protocol.Version, authRequest.Provider.Version);
		}

		private AuthenticationRequest_Accessor CreateAuthenticationRequest(Identifier claimedIdentifier, Identifier providerLocalIdentifier) {
			ProviderEndpointDescription providerEndpoint = new ProviderEndpointDescription(ProviderUri, this.protocol.Version);
			ServiceEndpoint endpoint = ServiceEndpoint.CreateForClaimedIdentifier(claimedIdentifier, providerLocalIdentifier, providerEndpoint, 10, 5);
			ServiceEndpoint_Accessor endpointAccessor = ServiceEndpoint_Accessor.AttachShadow(endpoint);
			OpenIdRelyingParty rp = this.CreateRelyingParty();
			AuthenticationRequest_Accessor authRequest = new AuthenticationRequest_Accessor(endpointAccessor, this.realm, this.returnTo, rp);
			return authRequest;
		}
	}
}
