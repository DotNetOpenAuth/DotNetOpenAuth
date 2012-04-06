//-----------------------------------------------------------------------
// <copyright file="NegativeAuthenticationResponseTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class NegativeAuthenticationResponseTests : OpenIdTestBase {
		private const string UserSuppliedIdentifier = "=arnott";
		private Protocol protocol;
		private NegativeAssertionResponse responseMessage;
		private NegativeAuthenticationResponse response;

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			this.protocol = Protocol.Default;
			this.responseMessage = new NegativeAssertionResponse(this.protocol.Version, RPUri, this.protocol.Args.Mode.cancel);
			this.responseMessage.ExtraData[AuthenticationRequest.UserSuppliedIdentifierParameterName] = UserSuppliedIdentifier;
			this.response = new NegativeAuthenticationResponse(this.responseMessage);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNull() {
			new NegativeAuthenticationResponse(null);
		}

		/// <summary>
		/// Verifies that immediate/setup modes are correctly detected.
		/// </summary>
		[Test]
		public void ImmediateVsSetupModes() {
			this.responseMessage = new NegativeAssertionResponse(this.protocol.Version, RPUri, this.protocol.Args.Mode.cancel);
			this.response = new NegativeAuthenticationResponse(this.responseMessage);
			Assert.AreEqual(AuthenticationStatus.Canceled, this.response.Status);
			try {
				Assert.AreEqual(UserSuppliedIdentifier, this.response.UserSuppliedIdentifier);
				Assert.Fail("Expected InvalidOperationException not thrown.");
			} catch (InvalidOperationException) {
			}

			this.responseMessage = new NegativeAssertionResponse(this.protocol.Version, RPUri, this.protocol.Args.Mode.setup_needed);
			this.responseMessage.ExtraData[AuthenticationRequest.UserSuppliedIdentifierParameterName] = UserSuppliedIdentifier;
			this.response = new NegativeAuthenticationResponse(this.responseMessage);
			Assert.AreEqual(AuthenticationStatus.SetupRequired, this.response.Status);
			Assert.AreEqual(UserSuppliedIdentifier, (string)this.response.UserSuppliedIdentifier);
		}

		[Test]
		public void CommonProperties() {
			Assert.IsNull(this.response.Exception);
			Assert.IsNull(this.response.ClaimedIdentifier);
			Assert.IsNull(this.response.FriendlyIdentifierForDisplay);
		}

		[Test]
		public void CommonMethods() {
			Assert.IsNull(this.response.GetExtension<ClaimsRequest>());
			Assert.IsNull(this.response.GetExtension(typeof(ClaimsRequest)));
			Assert.IsNull(this.response.GetCallbackArgument(AuthenticationRequest.UserSuppliedIdentifierParameterName), "Although the userSuppliedIdentifier parameter is present, failure responses should never return callback args.");
			Assert.AreEqual(0, this.response.GetCallbackArguments().Count);
		}
	}
}
