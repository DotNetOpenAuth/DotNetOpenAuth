//-----------------------------------------------------------------------
// <copyright file="OpenIdScenarioTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class OpenIdScenarioTests {
		private readonly Protocol Protocol = Protocol.V20;

		[TestMethod]
		public void AssociateDiffieHellmanMessages() {
			Association rpAssociation = null, opAssociation;
			AssociateDiffieHellmanResponse associateResponse = null;
			var opDescription = new ProviderEndpointDescription(new Uri("http://host"), this.Protocol);
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					rpAssociation = rp.GetAssociation(opDescription);
				},
				op => {
					op.AutoRespond();
				});
			coordinator.IncomingMessageFilter = (message) => {
				var associateResponseMessage = message as AssociateDiffieHellmanResponse;
				if (associateResponseMessage != null) {
					// capture this message so we can analyze it later
					associateResponse = associateResponseMessage;
				}
			};
			coordinator.Run();
			Assert.IsNotNull(rpAssociation);
			Assert.AreSame(rpAssociation, coordinator.RelyingParty.AssociationStore.GetAssociation(opDescription.Endpoint, rpAssociation.Handle));
			opAssociation = coordinator.Provider.AssociationStore.GetAssociation(AssociationRelyingPartyType.Smart, rpAssociation.Handle);
			Assert.IsNotNull(opAssociation, "The Provider should have stored the association.");

			Assert.AreEqual(opAssociation.Handle, rpAssociation.Handle);
			Assert.IsFalse(MessagingUtilities.AreEquivalent(associateResponse.EncodedMacKey, rpAssociation.SecretKey), "Key should have been encrypted.");
			Assert.IsTrue(Math.Abs(opAssociation.SecondsTillExpiration - rpAssociation.SecondsTillExpiration) < 60);
			Assert.IsTrue(MessagingUtilities.AreEquivalent(opAssociation.SecretKey, rpAssociation.SecretKey));
		}

		[TestMethod]
		public void AssociateUnencryptedMessages() {
			Association rpAssociation = null, opAssociation;
			AssociateUnencryptedResponse associateResponse = null;
			bool unencryptedRequestSent = false;
			var opDescription = new ProviderEndpointDescription(new Uri("https://host"), this.Protocol);
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					rpAssociation = rp.GetAssociation(opDescription);
				},
				op => {
					op.AutoRespond();
				});
			coordinator.IncomingMessageFilter = message => {
				var associateResponseMessage = message as AssociateUnencryptedResponse;
				if (associateResponseMessage != null) {
					// capture this message as it comes into the RP so we can analyze it later
					associateResponse = associateResponseMessage;
				}
			};
			coordinator.OutgoingMessageFilter = message => {
				// we want to check that the RP is sending a request that doesn't require DH
				unencryptedRequestSent |= message is AssociateUnencryptedRequest;
			};
			coordinator.Run();
			Assert.IsNotNull(rpAssociation);
			Assert.AreSame(rpAssociation, coordinator.RelyingParty.AssociationStore.GetAssociation(opDescription.Endpoint, rpAssociation.Handle));
			opAssociation = coordinator.Provider.AssociationStore.GetAssociation(AssociationRelyingPartyType.Smart, rpAssociation.Handle);
			Assert.IsNotNull(opAssociation, "The Provider should have stored the association.");

			Assert.IsTrue(unencryptedRequestSent, "An unencrypted association request should have been used since HTTPS was the transport.");
			Assert.AreEqual(opAssociation.Handle, rpAssociation.Handle);
			Assert.IsTrue(Math.Abs(opAssociation.SecondsTillExpiration - rpAssociation.SecondsTillExpiration) < 60);
			Assert.IsTrue(MessagingUtilities.AreEquivalent(opAssociation.SecretKey, rpAssociation.SecretKey));
		}
	}
}
