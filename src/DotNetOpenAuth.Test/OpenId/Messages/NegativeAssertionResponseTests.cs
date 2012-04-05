//-----------------------------------------------------------------------
// <copyright file="NegativeAssertionResponseTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class NegativeAssertionResponseTests : OpenIdTestBase {
		[SetUp]
		public override void SetUp() {
			base.SetUp();
		}

		[Test]
		public void Mode() {
			var setupRequestV1 = new CheckIdRequest(Protocol.V10.Version, OPUri, AuthenticationRequestMode.Setup);
			setupRequestV1.ReturnTo = RPUri;
			var immediateRequestV1 = new CheckIdRequest(Protocol.V10.Version, OPUri, AuthenticationRequestMode.Immediate);
			immediateRequestV1.ReturnTo = RPUri;

			var setupRequestV2 = new CheckIdRequest(Protocol.V20.Version, OPUri, AuthenticationRequestMode.Setup);
			setupRequestV2.ReturnTo = RPUri;
			var immediateRequestV2 = new CheckIdRequest(Protocol.V20.Version, OPUri, AuthenticationRequestMode.Immediate);
			immediateRequestV2.ReturnTo = RPUri;

			Assert.AreEqual("id_res", new NegativeAssertionResponse(immediateRequestV1).Mode);
			Assert.AreEqual("cancel", new NegativeAssertionResponse(setupRequestV1).Mode);
			Assert.AreEqual("setup_needed", new NegativeAssertionResponse(immediateRequestV2).Mode);
			Assert.AreEqual("cancel", new NegativeAssertionResponse(setupRequestV2).Mode);

			Assert.IsTrue(new NegativeAssertionResponse(immediateRequestV1).Immediate);
			Assert.IsFalse(new NegativeAssertionResponse(setupRequestV1).Immediate);
			Assert.IsTrue(new NegativeAssertionResponse(immediateRequestV2).Immediate);
			Assert.IsFalse(new NegativeAssertionResponse(setupRequestV2).Immediate);
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public void UserSetupUrlRequiredInV1Immediate() {
			var immediateRequestV1 = new CheckIdRequest(Protocol.V10.Version, OPUri, AuthenticationRequestMode.Immediate);
			immediateRequestV1.ReturnTo = RPUri;
			new NegativeAssertionResponse(immediateRequestV1).EnsureValidMessage();
		}

		[Test]
		public void UserSetupUrlSetForV1Immediate() {
			var immediateRequestV1 = new CheckIdRequest(Protocol.V10.Version, OPUri, AuthenticationRequestMode.Immediate);
			immediateRequestV1.ReturnTo = RPUri;
			var response = new NegativeAssertionResponse(immediateRequestV1);
			response.UserSetupUrl = new Uri("http://usersetup");
			response.EnsureValidMessage();
		}

		[Test]
		public void UserSetupUrlNotRequiredInV1SetupOrV2() {
			var setupRequestV1 = new CheckIdRequest(Protocol.V10.Version, OPUri, AuthenticationRequestMode.Setup);
			setupRequestV1.ReturnTo = RPUri;
			new NegativeAssertionResponse(setupRequestV1).EnsureValidMessage();

			var setupRequestV2 = new CheckIdRequest(Protocol.V20.Version, OPUri, AuthenticationRequestMode.Setup);
			setupRequestV2.ReturnTo = RPUri;
			new NegativeAssertionResponse(setupRequestV2).EnsureValidMessage();

			var immediateRequestV2 = new CheckIdRequest(Protocol.V20.Version, OPUri, AuthenticationRequestMode.Immediate);
			immediateRequestV2.ReturnTo = RPUri;
			new NegativeAssertionResponse(immediateRequestV2).EnsureValidMessage();
		}
	}
}
