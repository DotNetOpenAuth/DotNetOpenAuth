//-----------------------------------------------------------------------
// <copyright file="PositiveAuthenticationResponseTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class PositiveAuthenticationResponseTests : OpenIdTestBase {
		private readonly Realm realm = new Realm("http://localhost/rp.aspx");
		private readonly Uri returnTo = new Uri("http://localhost/rp.aspx");

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
		}

		/// <summary>
		/// Verifies good, positive assertions are accepted.
		/// </summary>
		[TestMethod]
		public void Valid() {
			PositiveAssertionResponse assertion = this.GetPositiveAssertion();
			ClaimsResponse extension = new ClaimsResponse();
			assertion.Extensions.Add(extension);
			var rp = CreateRelyingParty();
			var authResponse = new PositiveAuthenticationResponse(assertion, rp);
			var authResponseAccessor = PositiveAuthenticationResponse_Accessor.AttachShadow(authResponse);
			Assert.AreEqual(AuthenticationStatus.Authenticated, authResponse.Status);
			Assert.IsNull(authResponse.Exception);
			Assert.AreEqual<string>(assertion.ClaimedIdentifier, authResponse.ClaimedIdentifier);
			Assert.AreEqual<string>(authResponseAccessor.endpoint.FriendlyIdentifierForDisplay, authResponse.FriendlyIdentifierForDisplay);
			Assert.AreSame(extension, authResponse.GetUntrustedExtension(typeof(ClaimsResponse)));
			Assert.AreSame(extension, authResponse.GetUntrustedExtension<ClaimsResponse>());
			Assert.IsNull(authResponse.GetCallbackArgument("a"));
			Assert.AreEqual(0, authResponse.GetCallbackArguments().Count);
		}

		/// <summary>
		/// Verifies that the RP rejects signed solicited assertions by an OP that
		/// makes up a claimed Id that was not part of the original request, and 
		/// that the OP has no authority to assert positively regarding.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void SpoofedClaimedIdDetectionSolicited() {
			PositiveAssertionResponse assertion = this.GetPositiveAssertion();
			assertion.ProviderEndpoint = new Uri("http://rogueOP");
			var rp = CreateRelyingParty();
			var authResponse = new PositiveAuthenticationResponse(assertion, rp);
			Assert.AreEqual(AuthenticationStatus.Failed, authResponse.Status);
		}

		[TestMethod]
		public void GetCallbackArguments() {
			PositiveAssertionResponse assertion = this.GetPositiveAssertion();
			var rp = CreateRelyingParty();

			UriBuilder returnToBuilder = new UriBuilder(assertion.ReturnTo);
			returnToBuilder.AppendQueryArgs(new Dictionary<string, string> { { "a", "b" } });
			assertion.ReturnTo = returnToBuilder.Uri;
			var authResponse = new PositiveAuthenticationResponse(assertion, rp);

			// First pretend that the return_to args were signed.
			assertion.ReturnToParametersSignatureValidated = true;
			Assert.AreEqual(1, authResponse.GetCallbackArguments().Count);
			Assert.IsTrue(authResponse.GetCallbackArguments().ContainsKey("a"));
			Assert.AreEqual("b", authResponse.GetCallbackArgument("a"));

			// Now simulate them NOT being signed.
			assertion.ReturnToParametersSignatureValidated = false;
			Assert.AreEqual(0, authResponse.GetCallbackArguments().Count);
			Assert.IsFalse(authResponse.GetCallbackArguments().ContainsKey("a"));
			Assert.IsNull(authResponse.GetCallbackArgument("a"));
		}

		private PositiveAssertionResponse GetPositiveAssertion() {
			Protocol protocol = Protocol.Default;
			PositiveAssertionResponse assertion = new PositiveAssertionResponse(protocol.Version, this.returnTo);
			assertion.ClaimedIdentifier = this.GetMockIdentifier(protocol.ProtocolVersion, false);
			assertion.LocalIdentifier = OPLocalIdentifiers[0];
			assertion.ReturnTo = this.returnTo;
			assertion.ProviderEndpoint = OPUri;
			return assertion;
		}
	}
}
