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
		public void AssociateUnencrypted() {
			this.ParameterizedAssociationTest(new Uri("https://host"));
		}

		[TestMethod]
		public void AssociateDiffieHellmanOverHttp() {
			this.ParameterizedAssociationTest(new Uri("http://host"));
		}

		[TestMethod, Ignore]
		public void AssociateDiffieHellmanOverHttps() {
			// TODO: test the RP and OP agreeing to use Diffie-Hellman over HTTPS.
			throw new NotImplementedException();
		}

		/// <summary>
		/// Verifies that the RP and OP can renegotiate an association type if the RP's
		/// initial request for an association is for a type the OP doesn't support.
		/// </summary>
		[TestMethod, Ignore]
		public void AssociateRenegotiateBitLength() {
			// TODO: test where the RP asks for an association type that the OP doesn't support
			throw new NotImplementedException();
		}

		/// <summary>
		/// Verifies that the RP cannot get caught in an infinite loop if a bad OP
		/// keeps sending it association retry messages.
		/// </summary>
		[TestMethod, Ignore]
		public void AssociateRenegotiateBitLengthRPStopsAfterOneRetry() {
			// TODO: code here
			throw new NotImplementedException();
		}

		/// <summary>
		/// Verifies security settings limit RP's initial associate request
		/// </summary>
		[TestMethod, Ignore]
		public void AssociateRequestDeterminedBySecuritySettings() {
			// TODO: Code here
			throw new NotImplementedException();
		}

		/// <summary>
		/// Verifies security settings limit RP's acceptance of OP's counter-suggestion
		/// </summary>
		[TestMethod, Ignore]
		public void AssociateRenegotiateLimitedByRPSecuritySettings() {
			// TODO: Code here
			throw new NotImplementedException();
		}

		/// <summary>
		/// Verifies security settings limit OP's set of acceptable association types.
		/// </summary>
		[TestMethod, Ignore]
		public void AssociateLimitedByOPSecuritySettings() {
			// TODO: Code here
			throw new NotImplementedException();
		}

		/// <summary>
		/// Verifies the RP can recover with no association after receiving an
		/// associate error response from the OP when no suggested association
		/// type is included.
		/// </summary>
		[TestMethod, Ignore]
		public void AssociateContinueAfterOpenIdError() {
			// TODO: Code here
			throw new NotImplementedException();
		}

		/// <summary>
		/// Verifies that the RP can recover from an invalid or non-existent 
		/// response from the OP, for example in the HTTP timeout case.
		/// </summary>
		[TestMethod, Ignore]
		public void AssociateContinueAfterHttpError() {
			// TODO: Code here
			throw new NotImplementedException();
		}

		/// <summary>
		/// Runs a parameterized association flow test using all supported OpenID versions.
		/// </summary>
		/// <param name="opEndpoint">The OP endpoint to simulate using.</param>
		private void ParameterizedAssociationTest(Uri opEndpoint) {
			foreach (Protocol protocol in Protocol.AllPracticalVersions) {
				var endpoint = new ProviderEndpointDescription(opEndpoint, protocol.Version);
				var associationType = protocol.Version.Major < 2 ? protocol.Args.SignatureAlgorithm.HMAC_SHA1 : protocol.Args.SignatureAlgorithm.HMAC_SHA256;
				this.ParameterizedAssociationTest(endpoint, associationType);
			}
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
					rpAssociation = rp.GetOrCreateAssociation(opDescription);
				},
				op => {
					op.SecuritySettings = this.ProviderSecuritySettings;
					op.AutoRespond();
				});
			coordinator.IncomingMessageFilter = message => {
				Assert.AreSame(opDescription.ProtocolVersion, message.Version, "The message was recognized as version {0} but was expected to be {1}.", message.Version, opDescription.ProtocolVersion);
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
				Assert.AreSame(opDescription.ProtocolVersion, message.Version, "The message was for version {0} but was expected to be for {1}.", message.Version, opDescription.ProtocolVersion);
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
