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
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AuthenticationRequestTests : OpenIdTestBase {
		private readonly Realm realm = new Realm("http://localhost/rp.aspx");
		private readonly Identifier claimedId = "http://claimedId";
		private readonly Identifier delegatedLocalId = "http://localId";
		private readonly Protocol protocol = Protocol.Default;
		private Uri returnTo;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
			this.returnTo = new Uri("http://localhost/rp.aspx");
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
			Assert.AreEqual(this.protocol.Version, authRequest.endpoint.Protocol.Version);
		}

		/// <summary>
		/// Verifies RedirectingResponse.
		/// </summary>
		[TestMethod]
		public void CreateRequestMessage() {
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					Identifier id = this.GetMockIdentifier(ProtocolVersion.V20);
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
				AutoProvider);
			coordinator.Run();
		}

		/// <summary>
		/// Verifies that delegating authentication requests are filtered out when configured to do so.
		/// </summary>
		[TestMethod]
		public void CreateFiltersDelegatingIdentifiers() {
			Identifier id = GetMockIdentifier(ProtocolVersion.V20, false, true);
			var rp = CreateRelyingParty();

			// First verify that delegating identifiers work
			Assert.IsTrue(AuthenticationRequest.Create(id, rp, this.realm, this.returnTo, false).Any(), "The delegating identifier should have not generated any results.");

			// Now disable them and try again.
			rp.SecuritySettings.RejectDelegatingIdentifiers = true;
			Assert.IsFalse(AuthenticationRequest.Create(id, rp, this.realm, this.returnTo, false).Any(), "The delegating identifier should have not generated any results.");
		}

		/// <summary>
		/// Verifies the Provider property returns non-null.
		/// </summary>
		[TestMethod]
		public void Provider() {
			IAuthenticationRequest_Accessor authRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			Assert.IsNotNull(authRequest.Provider);
			Assert.AreEqual(OPUri, authRequest.Provider.Uri);
			Assert.AreEqual(this.protocol.Version, authRequest.Provider.Version);
		}

		/// <summary>
		/// Verifies that AddCallbackArguments adds query arguments to the return_to URL of the message.
		/// </summary>
		[TestMethod]
		public void AddCallbackArgument() {
			IAuthenticationRequest_Accessor authRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			Assert.AreEqual(this.returnTo, authRequest.ReturnToUrl);
			authRequest.AddCallbackArguments("p1", "v1");
			var req = (SignedResponseRequest)authRequest.RedirectingResponse.OriginalMessage;
			NameValueCollection query = HttpUtility.ParseQueryString(req.ReturnTo.Query);
			Assert.AreEqual("v1", query["p1"]);
		}

		/// <summary>
		/// Verifies that AddCallbackArguments replaces pre-existing parameter values 
		/// rather than appending them.
		/// </summary>
		[TestMethod]
		public void AddCallbackArgumentClearsPreviousArgument() {
			UriBuilder returnToWithArgs = new UriBuilder(this.returnTo);
			returnToWithArgs.AppendQueryArgs(new Dictionary<string, string> { { "p1", "v1" } });
			this.returnTo = returnToWithArgs.Uri;
			IAuthenticationRequest_Accessor authRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			authRequest.AddCallbackArguments("p1", "v2");
			var req = (SignedResponseRequest)authRequest.RedirectingResponse.OriginalMessage;
			NameValueCollection query = HttpUtility.ParseQueryString(req.ReturnTo.Query);
			Assert.AreEqual("v2", query["p1"]);
		}

		/// <summary>
		/// Verifies identity-less checkid_* request behavior.
		/// </summary>
		[TestMethod]
		public void NonIdentityRequest() {
			IAuthenticationRequest_Accessor authRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			authRequest.IsExtensionOnly = true;
			Assert.IsTrue(authRequest.IsExtensionOnly);
			var req = (SignedResponseRequest)authRequest.RedirectingResponse.OriginalMessage;
			Assert.IsNotInstanceOfType(req, typeof(CheckIdRequest), "An unexpected SignedResponseRequest derived type was generated.");
		}

		/// <summary>
		/// Verifies that authentication requests are generated first for OPs that respond
		/// to authentication requests.
		/// </summary>
		[TestMethod, Ignore]
		public void UnresponsiveProvidersComeLast() {
			// TODO: code here
			Assert.Inconclusive("Not yet implemented.");
		}

		private AuthenticationRequest_Accessor CreateAuthenticationRequest(Identifier claimedIdentifier, Identifier providerLocalIdentifier) {
			ProviderEndpointDescription providerEndpoint = new ProviderEndpointDescription(OPUri, this.protocol.Version);
			ServiceEndpoint endpoint = ServiceEndpoint.CreateForClaimedIdentifier(claimedIdentifier, providerLocalIdentifier, providerEndpoint, 10, 5);
			ServiceEndpoint_Accessor endpointAccessor = ServiceEndpoint_Accessor.AttachShadow(endpoint);
			OpenIdRelyingParty rp = this.CreateRelyingParty();
			AuthenticationRequest_Accessor authRequest = new AuthenticationRequest_Accessor(endpointAccessor, this.realm, this.returnTo, rp);
			return authRequest;
		}
	}
}
