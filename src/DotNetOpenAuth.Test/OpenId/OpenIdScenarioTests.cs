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
			var opDescription = new ProviderEndpointDescription(new Uri("http://host"), Protocol);
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					rpAssociation = rp.GetAssociation(opDescription);
					Assert.IsNotNull(rpAssociation);
					Assert.IsFalse(MessagingUtilities.AreEquivalent(associateResponse.EncodedMacKey, rpAssociation.SecretKey), "Key should have been encrypted.");
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
			Assert.AreSame(rpAssociation, coordinator.RelyingParty.AssociationStore.GetAssociation(opDescription.Endpoint, rpAssociation.Handle));
			opAssociation = coordinator.Provider.AssociationStore.GetAssociation(AssociationRelyingPartyType.Smart, rpAssociation.Handle);
			Assert.IsNotNull(opAssociation, "The Provider should have stored the association.");
			Assert.AreEqual(opAssociation.Handle, rpAssociation.Handle);
			Assert.IsTrue(Math.Abs(opAssociation.SecondsTillExpiration - rpAssociation.SecondsTillExpiration) < 60);
			Assert.IsTrue(MessagingUtilities.AreEquivalent(opAssociation.SecretKey, rpAssociation.SecretKey));
		}
	}
}
