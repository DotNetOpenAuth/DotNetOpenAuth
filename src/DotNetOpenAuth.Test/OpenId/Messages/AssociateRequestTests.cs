//-----------------------------------------------------------------------
// <copyright file="AssociateRequestTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AssociateRequestTests : OpenIdTestBase {
		private readonly Protocol protocol = Protocol.V20;
		private Uri secureRecipient = new Uri("https://hi");
		private Uri insecureRecipient = new Uri("http://hi");
		private AssociateRequest request;

		[TestInitialize]
		public void Setup() {
			this.request = new AssociateUnencryptedRequest(this.protocol.Version, this.secureRecipient);
		}

		[TestMethod]
		public void ConstructorTest() {
			Assert.AreEqual(this.secureRecipient, this.request.Recipient);
		}

		[TestMethod]
		public void Mode() {
			Assert.AreEqual(this.protocol.Args.Mode.associate, this.request.Mode);
		}

		[TestMethod]
		public void MessagePartsTest() {
			this.request.AssociationType = this.protocol.Args.SignatureAlgorithm.HMAC_SHA1;
			this.request.SessionType = this.protocol.Args.SessionType.NoEncryption;

			Assert.AreEqual(this.protocol.Args.Mode.associate, this.request.Mode);
			Assert.AreEqual(this.protocol.Args.SignatureAlgorithm.HMAC_SHA1, this.request.AssociationType);
			Assert.AreEqual(this.protocol.Args.SessionType.NoEncryption, this.request.SessionType);

			var dict = this.MessageDescriptions.GetAccessor(this.request);
			Assert.AreEqual(Protocol.OpenId2Namespace, dict[this.protocol.openid.ns]);
			Assert.AreEqual(this.protocol.Args.Mode.associate, dict[this.protocol.openid.mode]);
			Assert.AreEqual(this.protocol.Args.SignatureAlgorithm.HMAC_SHA1, dict[this.protocol.openid.assoc_type]);
			Assert.AreEqual(this.protocol.Args.SessionType.NoEncryption, dict[this.protocol.openid.session_type]);
		}

		[TestMethod]
		public void ValidMessageTest() {
			this.request = new AssociateUnencryptedRequest(Protocol.V20.Version, this.secureRecipient);
			this.request.AssociationType = this.protocol.Args.SignatureAlgorithm.HMAC_SHA1;
			this.request.EnsureValidMessage();
		}

		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void InvalidMessageTest() {
			this.request = new AssociateUnencryptedRequest(Protocol.V20.Version, this.insecureRecipient);
			this.request.AssociationType = this.protocol.Args.SignatureAlgorithm.HMAC_SHA1;
			this.request.EnsureValidMessage(); // no-encryption only allowed for secure channels.
		}

		[TestMethod]
		public void RequiredProtection() {
			Assert.AreEqual(MessageProtections.None, this.request.RequiredProtection);
		}

		[TestMethod]
		public void Transport() {
			Assert.AreEqual(MessageTransport.Direct, this.request.Transport);
		}

		/// <summary>
		/// Verifies security settings limit RP's initial associate request
		/// </summary>
		[TestMethod]
		public void AssociateRequestDeterminedBySecuritySettings() {
			Protocol protocol = Protocol.V20;
			SecuritySettings securitySettings = new RelyingPartySecuritySettings();
			securitySettings.MinimumHashBitLength = 160;
			securitySettings.MaximumHashBitLength = 160;
			ProviderEndpointDescription provider = new ProviderEndpointDescription(OPUri, protocol.Version);
			Assert.AreEqual(AssociateRequest.Create(securitySettings, provider).AssociationType, protocol.Args.SignatureAlgorithm.HMAC_SHA1);

			securitySettings.MinimumHashBitLength = 384;
			securitySettings.MaximumHashBitLength = 384;
			Assert.AreEqual(AssociateRequest.Create(securitySettings, provider).AssociationType, protocol.Args.SignatureAlgorithm.HMAC_SHA384);
		}
	}
}
