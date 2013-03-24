//-----------------------------------------------------------------------
// <copyright file="PositiveAuthenticationResponseTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class PositiveAuthenticationResponseTests : OpenIdTestBase {
		private readonly Realm realm = new Realm("http://localhost/rp.aspx");
		private readonly Uri returnTo = new Uri("http://localhost/rp.aspx");

		[SetUp]
		public override void SetUp() {
			base.SetUp();
		}

		/// <summary>
		/// Verifies good, positive assertions are accepted.
		/// </summary>
		[Test]
		public async Task Valid() {
			PositiveAssertionResponse assertion = this.GetPositiveAssertion();
			ClaimsResponse extension = new ClaimsResponse();
			assertion.Extensions.Add(extension);
			var rp = CreateRelyingParty();
			var authResponse = await PositiveAuthenticationResponse.CreateAsync(assertion, rp, CancellationToken.None);
			Assert.AreEqual(AuthenticationStatus.Authenticated, authResponse.Status);
			Assert.IsNull(authResponse.Exception);
			Assert.AreEqual((string)assertion.ClaimedIdentifier, (string)authResponse.ClaimedIdentifier);
			Assert.AreEqual(authResponse.Endpoint.FriendlyIdentifierForDisplay, authResponse.FriendlyIdentifierForDisplay);
			Assert.AreSame(extension, authResponse.GetUntrustedExtension(typeof(ClaimsResponse)));
			Assert.AreSame(extension, authResponse.GetUntrustedExtension<ClaimsResponse>());
			Assert.IsNull(authResponse.GetCallbackArgument("a"));
			Assert.AreEqual(0, authResponse.GetCallbackArguments().Count);
		}

		/// <summary>
		/// Verifies that discovery verification of a positive assertion can match a dual identifier.
		/// </summary>
		[Test]
		public async Task DualIdentifierMatchesInAssertionVerification() {
			PositiveAssertionResponse assertion = this.GetPositiveAssertion(true);
			ClaimsResponse extension = new ClaimsResponse();
			assertion.Extensions.Add(extension);
			var rp = CreateRelyingParty();
			rp.SecuritySettings.AllowDualPurposeIdentifiers = true;
			await PositiveAuthenticationResponse.CreateAsync(assertion, rp, CancellationToken.None); // this will throw if it fails to find a match
		}

		/// <summary>
		/// Verifies that discovery verification of a positive assertion cannot match a dual identifier
		/// if the default settings are in place.
		/// </summary>
		[Test, ExpectedException(typeof(ProtocolException))]
		public async Task DualIdentifierNoMatchInAssertionVerificationByDefault() {
			PositiveAssertionResponse assertion = this.GetPositiveAssertion(true);
			ClaimsResponse extension = new ClaimsResponse();
			assertion.Extensions.Add(extension);
			var rp = CreateRelyingParty();
			await PositiveAuthenticationResponse.CreateAsync(assertion, rp, CancellationToken.None); // this will throw if it fails to find a match
		}

		/// <summary>
		/// Verifies that the RP rejects signed solicited assertions by an OP that
		/// makes up a claimed Id that was not part of the original request, and 
		/// that the OP has no authority to assert positively regarding.
		/// </summary>
		[Test, ExpectedException(typeof(ProtocolException))]
		public async Task SpoofedClaimedIdDetectionSolicited() {
			PositiveAssertionResponse assertion = this.GetPositiveAssertion();
			assertion.ProviderEndpoint = new Uri("http://rogueOP");
			var rp = CreateRelyingParty();
			var authResponse = await PositiveAuthenticationResponse.CreateAsync(assertion, rp, CancellationToken.None);
			Assert.AreEqual(AuthenticationStatus.Failed, authResponse.Status);
		}

		/// <summary>
		/// Verifies that the RP rejects positive assertions with HTTP Claimed
		/// Cdentifiers when RequireSsl is set to true.
		/// </summary>
		[Test, ExpectedException(typeof(ProtocolException))]
		public async Task InsecureIdentifiersRejectedWithRequireSsl() {
			PositiveAssertionResponse assertion = this.GetPositiveAssertion();
			var rp = CreateRelyingParty();
			rp.SecuritySettings.RequireSsl = true;
			var authResponse = await PositiveAuthenticationResponse.CreateAsync(assertion, rp, CancellationToken.None);
		}

		[Test]
		public async Task GetCallbackArguments() {
			PositiveAssertionResponse assertion = this.GetPositiveAssertion();
			var rp = CreateRelyingParty();

			UriBuilder returnToBuilder = new UriBuilder(assertion.ReturnTo);
			returnToBuilder.AppendQueryArgs(new Dictionary<string, string> { { "a", "b" } });
			assertion.ReturnTo = returnToBuilder.Uri;
			var authResponse = await PositiveAuthenticationResponse.CreateAsync(assertion, rp, CancellationToken.None);

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

		/// <summary>
		/// Verifies that certain problematic claimed identifiers pass through to the RP response correctly.
		/// </summary>
		[Test]
		public async Task ProblematicClaimedId() {
			var providerEndpoint = new ProviderEndpointDescription(OpenIdTestBase.OPUri, Protocol.Default.Version);
			string claimed_id = BaseMockUri + "a./b.";
			var se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(claimed_id, claimed_id, providerEndpoint, null, null);
			var identityUri = (UriIdentifier)se.ClaimedIdentifier;
			this.RegisterMockXrdsResponse(se);

			var rp = this.CreateRelyingParty();
			var positiveAssertion = this.GetPositiveAssertion();
			positiveAssertion.ClaimedIdentifier = claimed_id;
			positiveAssertion.LocalIdentifier = claimed_id;
			var authResponse = await PositiveAuthenticationResponse.CreateAsync(positiveAssertion, rp, CancellationToken.None);
			Assert.AreEqual(AuthenticationStatus.Authenticated, authResponse.Status);
			Assert.AreEqual(claimed_id, authResponse.ClaimedIdentifier.ToString());
		}

		private PositiveAssertionResponse GetPositiveAssertion() {
			return this.GetPositiveAssertion(false);
		}

		private PositiveAssertionResponse GetPositiveAssertion(bool dualIdentifier) {
			Protocol protocol = Protocol.Default;
			PositiveAssertionResponse assertion = new PositiveAssertionResponse(protocol.Version, this.returnTo);
			assertion.ClaimedIdentifier = dualIdentifier ? this.GetMockDualIdentifier() : this.GetMockIdentifier(protocol.ProtocolVersion, false);
			assertion.LocalIdentifier = OPLocalIdentifiers[0];
			assertion.ReturnTo = this.returnTo;
			assertion.ProviderEndpoint = OPUri;
			return assertion;
		}
	}
}
