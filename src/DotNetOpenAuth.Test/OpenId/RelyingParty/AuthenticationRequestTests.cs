//-----------------------------------------------------------------------
// <copyright file="AuthenticationRequestTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId;
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
			var iauthRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			Assert.IsFalse(iauthRequest.IsDirectedIdentity);

			iauthRequest = this.CreateAuthenticationRequest(IdentifierSelect, IdentifierSelect);
			Assert.IsTrue(iauthRequest.IsDirectedIdentity);
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
		public void RedirectingResponse() {
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					Identifier id = this.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
					IAuthenticationRequest authRequest = rp.CreateRequest(id, this.realm, this.returnTo);
					var response = authRequest.RedirectingResponse;
					Assert.IsNotNull(response);
					Assert.IsInstanceOfType(response.OriginalMessage, typeof(CheckIdRequest));
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
