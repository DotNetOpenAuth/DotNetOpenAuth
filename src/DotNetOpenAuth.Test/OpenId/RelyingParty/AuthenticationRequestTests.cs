//-----------------------------------------------------------------------
// <copyright file="AuthenticationRequestTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class AuthenticationRequestTests : OpenIdTestBase {
		private readonly Realm realm = new Realm("http://localhost/rp.aspx");
		private readonly Identifier claimedId = "http://claimedId";
		private readonly Identifier delegatedLocalId = "http://localId";
		private readonly Protocol protocol = Protocol.Default;
		private Uri returnTo;

		[SetUp]
		public override void SetUp() {
			base.SetUp();
			this.returnTo = new Uri("http://localhost/rp.aspx");
		}

		/// <summary>
		/// Verifies IsDirectedIdentity returns true when appropriate.
		/// </summary>
		[Test]
		public void IsDirectedIdentity() {
			var iauthRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			Assert.IsFalse(iauthRequest.IsDirectedIdentity);

			iauthRequest = this.CreateAuthenticationRequest(IdentifierSelect, IdentifierSelect);
			Assert.IsTrue(iauthRequest.IsDirectedIdentity);
		}

		/// <summary>
		/// Verifies ClaimedIdentifier behavior.
		/// </summary>
		[Test]
		public void ClaimedIdentifier() {
			var iauthRequest = this.CreateAuthenticationRequest(this.claimedId, this.delegatedLocalId);
			Assert.AreEqual(this.claimedId, iauthRequest.ClaimedIdentifier);

			iauthRequest = this.CreateAuthenticationRequest(IdentifierSelect, IdentifierSelect);
			Assert.IsNull(iauthRequest.ClaimedIdentifier, "In directed identity mode, the ClaimedIdentifier should be null.");
		}

		/// <summary>
		/// Verifies ProviderVersion behavior.
		/// </summary>
		[Test]
		public void ProviderVersion() {
			var authRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			Assert.AreEqual(this.protocol.Version, authRequest.DiscoveryResult.Version);
		}

		/// <summary>
		/// Verifies RedirectingResponse.
		/// </summary>
		[Test]
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
					var authRequestAccessor = (AuthenticationRequest)authRequest;
					var req = authRequestAccessor.CreateRequestMessageTestHook();
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
		[Test]
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
		[Test]
		public void Provider() {
			var authRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			Assert.IsNotNull(authRequest.Provider);
			Assert.AreEqual(OPUri, authRequest.Provider.Uri);
			Assert.AreEqual(this.protocol.Version, authRequest.Provider.Version);
		}

		/// <summary>
		/// Verifies that AddCallbackArguments adds query arguments to the return_to URL of the message.
		/// </summary>
		[Test]
		public void AddCallbackArgument() {
			var authRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
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
		[Test]
		public void AddCallbackArgumentClearsPreviousArgument() {
			UriBuilder returnToWithArgs = new UriBuilder(this.returnTo);
			returnToWithArgs.AppendQueryArgs(new Dictionary<string, string> { { "p1", "v1" } });
			this.returnTo = returnToWithArgs.Uri;
			var authRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			authRequest.AddCallbackArguments("p1", "v2");
			var req = (SignedResponseRequest)authRequest.RedirectingResponse.OriginalMessage;
			NameValueCollection query = HttpUtility.ParseQueryString(req.ReturnTo.Query);
			Assert.AreEqual("v2", query["p1"]);
		}

		/// <summary>
		/// Verifies identity-less checkid_* request behavior.
		/// </summary>
		[Test]
		public void NonIdentityRequest() {
			var authRequest = this.CreateAuthenticationRequest(this.claimedId, this.claimedId);
			authRequest.IsExtensionOnly = true;
			Assert.IsTrue(authRequest.IsExtensionOnly);
			var req = (SignedResponseRequest)authRequest.RedirectingResponse.OriginalMessage;
			Assert.IsNotInstanceOf<CheckIdRequest>(req, "An unexpected SignedResponseRequest derived type was generated.");
		}

		/// <summary>
		/// Verifies that discovery on identifiers that serve as OP identifiers and claimed identifiers
		/// only generate OP Identifier auth requests.
		/// </summary>
		[Test]
		public void DualIdentifierUsedOnlyAsOPIdentifierForAuthRequest() {
			var rp = this.CreateRelyingParty(true);
			var results = AuthenticationRequest.Create(GetMockDualIdentifier(), rp, this.realm, this.returnTo, false).ToList();
			Assert.AreEqual(1, results.Count);
			Assert.IsTrue(results[0].IsDirectedIdentity);

			// Also test when dual identiifer support is turned on.
			rp.SecuritySettings.AllowDualPurposeIdentifiers = true;
			results = AuthenticationRequest.Create(GetMockDualIdentifier(), rp, this.realm, this.returnTo, false).ToList();
			Assert.AreEqual(1, results.Count);
			Assert.IsTrue(results[0].IsDirectedIdentity);
		}

		/// <summary>
		/// Verifies that authentication requests are generated first for OPs that respond
		/// to authentication requests.
		/// </summary>
		[Test, Ignore("Not yet implemented")]
		public void UnresponsiveProvidersComeLast() {
			// TODO: code here
			Assert.Inconclusive("Not yet implemented.");
		}

		private AuthenticationRequest CreateAuthenticationRequest(Identifier claimedIdentifier, Identifier providerLocalIdentifier) {
			ProviderEndpointDescription providerEndpoint = new ProviderEndpointDescription(OPUri, this.protocol.Version);
			IdentifierDiscoveryResult endpoint = IdentifierDiscoveryResult.CreateForClaimedIdentifier(claimedIdentifier, providerLocalIdentifier, providerEndpoint, 10, 5);
			OpenIdRelyingParty rp = this.CreateRelyingParty();
			return AuthenticationRequest.CreateForTest(endpoint, this.realm, this.returnTo, rp);
		}
	}
}
