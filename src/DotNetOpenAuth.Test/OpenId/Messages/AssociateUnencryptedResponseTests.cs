//-----------------------------------------------------------------------
// <copyright file="AssociateUnencryptedResponseTests.cs" company="Outercurve Foundation">
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
	public class AssociateUnencryptedResponseTests : OpenIdTestBase {
		private AssociateUnencryptedResponse response;

		[SetUp]
		public override void SetUp() {
			base.SetUp();
			var request = new AssociateUnencryptedRequest(Protocol.V20.Version, new Uri("http://host"));
			this.response = new AssociateUnencryptedResponse(request.Version, request);
		}

		[Test]
		public void ParameterNames() {
			this.response.AssociationHandle = "HANDLE";
			this.response.AssociationType = "HMAC-SHA1";
			this.response.SessionType = "DH-SHA1";
			this.response.ExpiresIn = 50;

			MessageSerializer serializer = MessageSerializer.Get(this.response.GetType());
			var fields = this.MessageDescriptions.GetAccessor(this.response).Serialize();
			Assert.AreEqual(Protocol.OpenId2Namespace, fields["ns"]);
			Assert.AreEqual("HANDLE", fields["assoc_handle"]);
			Assert.AreEqual("HMAC-SHA1", fields["assoc_type"]);
			Assert.AreEqual("DH-SHA1", fields["session_type"]);
			Assert.AreEqual("50", fields["expires_in"]);
		}

		[Test]
		public void RequiredProtection() {
			Assert.AreEqual(MessageProtections.None, this.response.RequiredProtection);
		}

		[Test]
		public void Transport() {
			Assert.AreEqual(MessageTransport.Direct, this.response.Transport);
		}
	}
}