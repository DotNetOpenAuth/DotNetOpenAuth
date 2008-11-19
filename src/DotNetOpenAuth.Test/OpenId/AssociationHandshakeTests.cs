//-----------------------------------------------------------------------
// <copyright file="AssociationHandshakeTests.cs" company="Andrew Arnott">
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
	public class AssociationHandshakeTests {
		[TestMethod]
		public void AssociateDiffieHellmanMessages() {
			var opDescription = new ProviderEndpointDescription(new Uri("http://host"), Protocol.V20);
			ParameterizedAssociationTest(opDescription, true, Protocol.V20.Args.SignatureAlgorithm.HMAC_SHA1);
		}

		[TestMethod]
		public void AssociateUnencryptedMessages() {
			var opDescription = new ProviderEndpointDescription(new Uri("https://host"), Protocol.V20);
			ParameterizedAssociationTest(opDescription, false, Protocol.V20.Args.SignatureAlgorithm.HMAC_SHA1);
		}

		/// <summary>
		/// Runs a parameterized association flow test.
		/// </summary>
		/// <param name="opDescription">
		/// The description of the Provider that the relying party uses to formulate the request.  
		/// The specific host is not used, but the scheme is significant.
		/// </param>
		/// <param name="expectDiffieHellman">True if a DH session is expected to be used.</param>
		/// <param name="expectedAssociationType">
		/// The value of the openid.assoc_type parameter expected,
		/// or null if a failure is anticipated.
		/// </param>
		private void ParameterizedAssociationTest(
			ProviderEndpointDescription opDescription,
			bool expectDiffieHellman,
			string expectedAssociationType) {
			bool expectSuccess = expectedAssociationType != null;
			Association rpAssociation = null, opAssociation;
			AssociateSuccessfulResponse associateSuccessfulResponse = null;
			AssociateUnsuccessfulResponse associateUnsuccessfulResponse = null;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					rpAssociation = rp.GetAssociation(opDescription);
				},
				op => {
					op.AutoRespond();
				});
			coordinator.IncomingMessageFilter = (message) => {
				var associateSuccess = message as AssociateSuccessfulResponse;
				var associateFailed = message as AssociateUnsuccessfulResponse;
				if (associateSuccess != null) {
					associateSuccessfulResponse = associateSuccess;
				}
				if (associateFailed != null) {
					associateUnsuccessfulResponse = associateFailed;
				}
			};
			coordinator.Run();

			if (expectSuccess) {
				Assert.IsNotNull(rpAssociation);
				Assert.AreSame(rpAssociation, coordinator.RelyingParty.AssociationStore.GetAssociation(opDescription.Endpoint, rpAssociation.Handle));
				opAssociation = coordinator.Provider.AssociationStore.GetAssociation(AssociationRelyingPartyType.Smart, rpAssociation.Handle);
				Assert.IsNotNull(opAssociation, "The Provider should have stored the association.");

				Assert.AreEqual(opAssociation.Handle, rpAssociation.Handle);
				Assert.AreEqual(expectedAssociationType, rpAssociation.GetAssociationType(opDescription.Protocol));
				Assert.AreEqual(expectedAssociationType, opAssociation.GetAssociationType(opDescription.Protocol));
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
