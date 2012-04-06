//-----------------------------------------------------------------------
// <copyright file="SignedResponseRequestTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class SignedResponseRequestTests : OpenIdTestBase {
		private Uri providerEndpoint;
		private SignedResponseRequest immediatev1;
		private SignedResponseRequest setupv1;
		private SignedResponseRequest immediatev2;
		private SignedResponseRequest setupv2;

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			this.providerEndpoint = new Uri("http://host");

			this.immediatev1 = new SignedResponseRequest(Protocol.V11.Version, this.providerEndpoint, AuthenticationRequestMode.Immediate);
			this.setupv1 = new SignedResponseRequest(Protocol.V11.Version, this.providerEndpoint, AuthenticationRequestMode.Setup);

			this.immediatev2 = new SignedResponseRequest(Protocol.V20.Version, this.providerEndpoint, AuthenticationRequestMode.Immediate);
			this.setupv2 = new SignedResponseRequest(Protocol.V20.Version, this.providerEndpoint, AuthenticationRequestMode.Setup);

			// Prepare all message versions so that they SHOULD be valid by default.
			// In particular, V1 messages require ReturnTo.
			this.immediatev1.ReturnTo = new Uri("http://returnto/");
			this.setupv1.ReturnTo = new Uri("http://returnto/");

			try {
				this.immediatev1.EnsureValidMessage();
				this.setupv1.EnsureValidMessage();
				this.immediatev2.EnsureValidMessage();
				this.setupv2.EnsureValidMessage();
			} catch (ProtocolException ex) {
				Assert.Inconclusive("All messages ought to be valid before tests run, but got: {0}", ex.Message);
			}
		}

		/// <summary>
		/// Verifies that the validation check throws if the return_to and the realm
		/// values are not compatible.
		/// </summary>
		/// <remarks>
		/// This test does not test all the realm-return_to matching rules as that is done in the Realm tests.
		/// This test merely checks that the compatibility match occurs at all.
		/// </remarks>
		[Test, ExpectedException(typeof(ProtocolException))]
		public void RealmReturnToMismatchV2() {
			this.setupv2.Realm = "http://somehost/";
			this.setupv2.ReturnTo = new Uri("http://someotherhost/");
			this.setupv2.EnsureValidMessage();
		}

		/// <summary>
		/// Verifies that the validation check throws if the return_to and the realm
		/// values are not compatible.
		/// </summary>
		[Test, ExpectedException(typeof(ProtocolException))]
		public void RealmReturnToMismatchV1() {
			this.setupv1.Realm = "http://somehost/";
			this.setupv1.ReturnTo = new Uri("http://someotherhost/");
			this.setupv1.EnsureValidMessage();
		}
	}
}
