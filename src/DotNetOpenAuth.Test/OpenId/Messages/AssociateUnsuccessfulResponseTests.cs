//-----------------------------------------------------------------------
// <copyright file="AssociateUnsuccessfulResponseTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using NUnit.Framework;

	[TestFixture]
	public class AssociateUnsuccessfulResponseTests : OpenIdTestBase {
		private AssociateUnsuccessfulResponse response;

		[SetUp]
		public override void SetUp() {
			base.SetUp();
			var request = new AssociateUnencryptedRequest(Protocol.V20.Version, new Uri("http://host"));
			this.response = new AssociateUnsuccessfulResponse(request.Version, request);
		}

		[Test]
		public void ParameterNames() {
			this.response.ErrorMessage = "Some Error";
			this.response.AssociationType = "HMAC-SHA1";
			this.response.SessionType = "DH-SHA1";

			var fields = this.MessageDescriptions.GetAccessor(this.response).Serialize();
			Assert.AreEqual(Protocol.OpenId2Namespace, fields["ns"]);
			Assert.AreEqual("unsupported-type", fields["error_code"]);
			Assert.AreEqual("Some Error", fields["error"]);
			Assert.AreEqual("HMAC-SHA1", fields["assoc_type"]);
			Assert.AreEqual("DH-SHA1", fields["session_type"]);
		}
	}
}