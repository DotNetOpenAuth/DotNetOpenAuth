//-----------------------------------------------------------------------
// <copyright file="AssociationHandshakeTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AssociationHandshakeTests : OpenIdTestBase {
		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
		}

		[TestMethod]
		public void DHv2() {
			Protocol protocol = Protocol.V20;
			var opDescription = new ProviderEndpointDescription(new Uri("http://host"), protocol.Version);
			this.ParameterizedAssociationTest(
				opDescription,
				protocol.Args.SignatureAlgorithm.HMAC_SHA256);
		}

		[TestMethod]
		public void DHv1() {
			Protocol protocol = Protocol.V11;
			var opDescription = new ProviderEndpointDescription(new Uri("http://host"), protocol.Version);
			this.ParameterizedAssociationTest(
				opDescription,
				protocol.Args.SignatureAlgorithm.HMAC_SHA1);
		}

		[TestMethod]
		public void PTv2() {
			Protocol protocol = Protocol.V20;
			var opDescription = new ProviderEndpointDescription(new Uri("https://host"), protocol.Version);
			this.ParameterizedAssociationTest(
				opDescription,
				protocol.Args.SignatureAlgorithm.HMAC_SHA256);
		}

		[TestMethod]
		public void PTv1() {
			Protocol protocol = Protocol.V11;
			var opDescription = new ProviderEndpointDescription(new Uri("https://host"), protocol.Version);
			this.ParameterizedAssociationTest(
				opDescription,
				protocol.Args.SignatureAlgorithm.HMAC_SHA1);
		}

		/// <summary>
		/// Runs a parameterized association flow test.
		/// </summary>
		/// <param name="opDescription">
		/// The description of the Provider that the relying party uses to formulate the request.  
		/// The specific host is not used, but the scheme is significant.
		/// </param>
		/// <param name="expectedAssociationType">
		/// The value of the openid.assoc_type parameter expected,
		/// or null if a failure is anticipated.
		/// </param>
		private void ParameterizedAssociationTest(
			ProviderEndpointDescription opDescription,
			string expectedAssociationType) {
			Protocol protocol = Protocol.Lookup(opDescription.ProtocolVersion);
			bool expectSuccess = expectedAssociationType != null;
			bool expectDiffieHellman = !opDescription.Endpoint.IsTransportSecure();
			Association rpAssociation = null, opAssociation;
			AssociateSuccessfulResponse associateSuccessfulResponse = null;
			AssociateUnsuccessfulResponse associateUnsuccessfulResponse = null;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					rp.SecuritySettings = this.RelyingPartySecuritySettings;
					rpAssociation = rp.GetAssociation(opDescription);
				},
				op => {
					op.SecuritySettings = this.ProviderSecuritySettings;
					op.AutoRespond();
				});
			coordinator.IncomingMessageFilter = message => {
				Assert.AreSame(opDescription.ProtocolVersion, message.ProtocolVersion, "The message was recognized as version {0} but was expected to be {1}.", message.ProtocolVersion, opDescription.ProtocolVersion);
				var associateSuccess = message as AssociateSuccessfulResponse;
				var associateFailed = message as AssociateUnsuccessfulResponse;
				if (associateSuccess != null) {
					associateSuccessfulResponse = associateSuccess;
				}
				if (associateFailed != null) {
					associateUnsuccessfulResponse = associateFailed;
				}
			};
			coordinator.OutgoingMessageFilter = message => {
				Assert.AreSame(opDescription.ProtocolVersion, message.ProtocolVersion, "The message was for version {0} but was expected to be for {1}.", message.ProtocolVersion, opDescription.ProtocolVersion);
			};
			coordinator.Run();

			if (expectSuccess) {
				Assert.IsNotNull(rpAssociation);
				Assert.AreSame(rpAssociation, coordinator.RelyingParty.AssociationStore.GetAssociation(opDescription.Endpoint, rpAssociation.Handle));
				opAssociation = coordinator.Provider.AssociationStore.GetAssociation(AssociationRelyingPartyType.Smart, rpAssociation.Handle);
				Assert.IsNotNull(opAssociation, "The Provider should have stored the association.");

				Assert.AreEqual(opAssociation.Handle, rpAssociation.Handle);
				Assert.AreEqual(expectedAssociationType, rpAssociation.GetAssociationType(protocol));
				Assert.AreEqual(expectedAssociationType, opAssociation.GetAssociationType(protocol));
				Assert.IsTrue(Math.Abs(opAssociation.SecondsTillExpiration - rpAssociation.SecondsTillExpiration) < 60);
				Assert.IsTrue(MessagingUtilities.AreEquivalent(opAssociation.SecretKey, rpAssociation.SecretKey));

				if (expectDiffieHellman) {
					Assert.IsInstanceOfType(associateSuccessfulResponse, typeof(AssociateDiffieHellmanResponse));
					var diffieHellmanResponse = (AssociateDiffieHellmanResponse)associateSuccessfulResponse;
					Assert.IsFalse(MessagingUtilities.AreEquivalent(diffieHellmanResponse.EncodedMacKey, rpAssociation.SecretKey), "Key should have been encrypted.");
				} else {
					Assert.IsInstanceOfType(associateSuccessfulResponse, typeof(AssociateUnencryptedResponse));
					var unencryptedResponse = (AssociateUnencryptedResponse)associateSuccessfulResponse;
				}
			} else {
				Assert.IsNull(coordinator.RelyingParty.AssociationStore.GetAssociation(opDescription.Endpoint));
				Assert.IsNull(coordinator.Provider.AssociationStore.GetAssociation(AssociationRelyingPartyType.Smart));
			}
		}
	}
}
