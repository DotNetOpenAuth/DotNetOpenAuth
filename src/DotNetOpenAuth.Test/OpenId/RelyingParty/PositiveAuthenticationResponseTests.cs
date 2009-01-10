//-----------------------------------------------------------------------
// <copyright file="PositiveAuthenticationResponseTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class PositiveAuthenticationResponseTests : OpenIdTestBase {
		private readonly Realm realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
		private readonly Uri returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);

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
			Assert.AreSame(extension, authResponse.GetExtension(typeof(ClaimsResponse)));
			Assert.AreSame(extension, authResponse.GetExtension<ClaimsResponse>());
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

		private PositiveAssertionResponse GetPositiveAssertion() {
			Protocol protocol = Protocol.Default;
			PositiveAssertionResponse assertion = new PositiveAssertionResponse(protocol.Version, this.returnTo);
			assertion.ClaimedIdentifier = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, this.MockResponder, protocol.ProtocolVersion);
			assertion.LocalIdentifier = TestSupport.GetDelegateUrl(TestSupport.Scenarios.AutoApproval);
			assertion.ReturnTo = this.returnTo;
			assertion.ProviderEndpoint = TestSupport.GetFullUrl("/" + TestSupport.ProviderPage, null, false);
			return assertion;
		}
	}
}
