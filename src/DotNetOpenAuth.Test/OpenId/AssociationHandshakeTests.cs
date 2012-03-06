//-----------------------------------------------------------------------
// <copyright file="AssociationHandshakeTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class AssociationHandshakeTests : OpenIdTestBase {
		[SetUp]
		public override void SetUp() {
			base.SetUp();
		}

		[Test]
		public void AssociateUnencrypted() {
			this.ParameterizedAssociationTest(new Uri("https://host"));
		}

		[Test]
		public void AssociateDiffieHellmanOverHttp() {
			this.ParameterizedAssociationTest(new Uri("http://host"));
		}

		/// <summary>
		/// Verifies that the Provider can do Diffie-Hellman over HTTPS.
		/// </summary>
		/// <remarks>
		/// Some OPs out there flatly refuse to do this, and the spec doesn't forbid
		/// putting the two together, so we verify that DNOI can handle it.
		/// </remarks>
		[Test]
		public void AssociateDiffieHellmanOverHttps() {
			Protocol protocol = Protocol.V20;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					// We have to formulate the associate request manually,
					// since the DNOI RP won't voluntarily use DH on HTTPS.
					AssociateDiffieHellmanRequest request = new AssociateDiffieHellmanRequest(protocol.Version, new Uri("https://Provider"));
					request.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA256;
					request.SessionType = protocol.Args.SessionType.DH_SHA256;
					request.InitializeRequest();
					var response = rp.Channel.Request<AssociateSuccessfulResponse>(request);
					Assert.IsNotNull(response);
					Assert.AreEqual(request.AssociationType, response.AssociationType);
					Assert.AreEqual(request.SessionType, response.SessionType);
				},
				AutoProvider);
			coordinator.Run();
		}

		/// <summary>
		/// Verifies that the RP and OP can renegotiate an association type if the RP's
		/// initial request for an association is for a type the OP doesn't support.
		/// </summary>
		[Test]
		public void AssociateRenegotiateBitLength() {
			Protocol protocol = Protocol.V20;

			// The strategy is to make a simple request of the RP to establish an association,
			// and to more carefully observe the Provider-side of things to make sure that both
			// the OP and RP are behaving as expected.
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					var opDescription = new ProviderEndpointDescription(OPUri, protocol.Version);
					Association association = rp.AssociationManager.GetOrCreateAssociation(opDescription);
					Assert.IsNotNull(association, "Association failed to be created.");
					Assert.AreEqual(protocol.Args.SignatureAlgorithm.HMAC_SHA1, association.GetAssociationType(protocol));
				},
				op => {
					op.SecuritySettings.MaximumHashBitLength = 160; // Force OP to reject HMAC-SHA256

					// Receive initial request for an HMAC-SHA256 association.
					AutoResponsiveRequest req = (AutoResponsiveRequest)op.GetRequest();
					AssociateRequest associateRequest = (AssociateRequest)req.RequestMessage;
					Assert.That(associateRequest, Is.Not.Null);
					Assert.AreEqual(protocol.Args.SignatureAlgorithm.HMAC_SHA256, associateRequest.AssociationType);

					// Ensure that the response is a suggestion that the RP try again with HMAC-SHA1
					AssociateUnsuccessfulResponse renegotiateResponse = (AssociateUnsuccessfulResponse)req.ResponseMessageTestHook;
					Assert.AreEqual(protocol.Args.SignatureAlgorithm.HMAC_SHA1, renegotiateResponse.AssociationType);
					op.Respond(req);

					// Receive second attempt request for an HMAC-SHA1 association.
					req = (AutoResponsiveRequest)op.GetRequest();
					associateRequest = (AssociateRequest)req.RequestMessage;
					Assert.AreEqual(protocol.Args.SignatureAlgorithm.HMAC_SHA1, associateRequest.AssociationType);

					// Ensure that the response is a success response.
					AssociateSuccessfulResponse successResponse = (AssociateSuccessfulResponse)req.ResponseMessageTestHook;
					Assert.AreEqual(protocol.Args.SignatureAlgorithm.HMAC_SHA1, successResponse.AssociationType);
					op.Respond(req);
				});
			coordinator.Run();
		}

		/// <summary>
		/// Verifies that the OP rejects an associate request that has no encryption (transport or DH).
		/// </summary>
		/// <remarks>
		/// Verifies OP's compliance with OpenID 2.0 section 8.4.1.
		/// </remarks>
		[Test]
		public void OPRejectsHttpNoEncryptionAssociateRequests() {
			Protocol protocol = Protocol.V20;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					// We have to formulate the associate request manually,
					// since the DNOI RP won't voluntarily suggest no encryption at all.
					var request = new AssociateUnencryptedRequestNoSslCheck(protocol.Version, OPUri);
					request.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA256;
					request.SessionType = protocol.Args.SessionType.NoEncryption;
					var response = rp.Channel.Request<DirectErrorResponse>(request);
					Assert.IsNotNull(response);
				},
				AutoProvider);
			coordinator.Run();
		}

		/// <summary>
		/// Verifies that the OP rejects an associate request
		/// when the HMAC and DH bit lengths do not match.
		/// </summary>
		[Test]
		public void OPRejectsMismatchingAssociationAndSessionTypes() {
			Protocol protocol = Protocol.V20;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					// We have to formulate the associate request manually,
					// since the DNOI RP won't voluntarily mismatch the association and session types.
					AssociateDiffieHellmanRequest request = new AssociateDiffieHellmanRequest(protocol.Version, new Uri("https://Provider"));
					request.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA256;
					request.SessionType = protocol.Args.SessionType.DH_SHA1;
					request.InitializeRequest();
					var response = rp.Channel.Request<AssociateUnsuccessfulResponse>(request);
					Assert.IsNotNull(response);
					Assert.AreEqual(protocol.Args.SignatureAlgorithm.HMAC_SHA1, response.AssociationType);
					Assert.AreEqual(protocol.Args.SessionType.DH_SHA1, response.SessionType);
				},
				AutoProvider);
			coordinator.Run();
		}

		/// <summary>
		/// Verifies that the RP quietly rejects an OP that suggests an unknown association type.
		/// </summary>
		[Test]
		public void RPRejectsUnrecognizedAssociationType() {
			Protocol protocol = Protocol.V20;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					var association = rp.AssociationManager.GetOrCreateAssociation(new ProviderEndpointDescription(OPUri, protocol.Version));
					Assert.IsNull(association, "The RP should quietly give up when the OP misbehaves.");
				},
				op => {
					// Receive initial request.
					var request = op.Channel.ReadFromRequest<AssociateRequest>();

					// Send a response that suggests a foreign association type.
					AssociateUnsuccessfulResponse renegotiateResponse = new AssociateUnsuccessfulResponse(request.Version, request);
					renegotiateResponse.AssociationType = "HMAC-UNKNOWN";
					renegotiateResponse.SessionType = "DH-UNKNOWN";
					op.Channel.Respond(renegotiateResponse);
				});
			coordinator.Run();
		}

		/// <summary>
		/// Verifies that the RP quietly rejects an OP that suggests an no encryption over an HTTP channel.
		/// </summary>
		/// <remarks>
		/// Verifies RP's compliance with OpenID 2.0 section 8.4.1.
		/// </remarks>
		[Test]
		public void RPRejectsUnencryptedSuggestion() {
			Protocol protocol = Protocol.V20;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					var association = rp.AssociationManager.GetOrCreateAssociation(new ProviderEndpointDescription(OPUri, protocol.Version));
					Assert.IsNull(association, "The RP should quietly give up when the OP misbehaves.");
				},
				op => {
					// Receive initial request.
					var request = op.Channel.ReadFromRequest<AssociateRequest>();

					// Send a response that suggests a no encryption.
					AssociateUnsuccessfulResponse renegotiateResponse = new AssociateUnsuccessfulResponse(request.Version, request);
					renegotiateResponse.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA1;
					renegotiateResponse.SessionType = protocol.Args.SessionType.NoEncryption;
					op.Channel.Respond(renegotiateResponse);
				});
			coordinator.Run();
		}

		/// <summary>
		/// Verifies that the RP rejects an associate renegotiate request 
		/// when the HMAC and DH bit lengths do not match.
		/// </summary>
		[Test]
		public void RPRejectsMismatchingAssociationAndSessionBitLengths() {
			Protocol protocol = Protocol.V20;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					var association = rp.AssociationManager.GetOrCreateAssociation(new ProviderEndpointDescription(OPUri, protocol.Version));
					Assert.IsNull(association, "The RP should quietly give up when the OP misbehaves.");
				},
				op => {
					// Receive initial request.
					var request = op.Channel.ReadFromRequest<AssociateRequest>();

					// Send a mismatched response
					AssociateUnsuccessfulResponse renegotiateResponse = new AssociateUnsuccessfulResponse(request.Version, request);
					renegotiateResponse.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA1;
					renegotiateResponse.SessionType = protocol.Args.SessionType.DH_SHA256;
					op.Channel.Respond(renegotiateResponse);
				});
			coordinator.Run();
		}

		/// <summary>
		/// Verifies that the RP cannot get caught in an infinite loop if a bad OP
		/// keeps sending it association retry messages.
		/// </summary>
		[Test]
		public void RPOnlyRenegotiatesOnce() {
			Protocol protocol = Protocol.V20;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					var association = rp.AssociationManager.GetOrCreateAssociation(new ProviderEndpointDescription(OPUri, protocol.Version));
					Assert.IsNull(association, "The RP should quietly give up when the OP misbehaves.");
				},
				op => {
					// Receive initial request.
					var request = op.Channel.ReadFromRequest<AssociateRequest>();

					// Send a renegotiate response
					AssociateUnsuccessfulResponse renegotiateResponse = new AssociateUnsuccessfulResponse(request.Version, request);
					renegotiateResponse.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA1;
					renegotiateResponse.SessionType = protocol.Args.SessionType.DH_SHA1;
					op.Channel.Respond(renegotiateResponse);

					// Receive second-try
					request = op.Channel.ReadFromRequest<AssociateRequest>();

					// Send ANOTHER renegotiate response, at which point the DNOI RP should give up.
					renegotiateResponse = new AssociateUnsuccessfulResponse(request.Version, request);
					renegotiateResponse.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA256;
					renegotiateResponse.SessionType = protocol.Args.SessionType.DH_SHA256;
					op.Channel.Respond(renegotiateResponse);
				});
			coordinator.Run();
		}

		/// <summary>
		/// Verifies security settings limit RP's acceptance of OP's counter-suggestion
		/// </summary>
		[Test]
		public void AssociateRenegotiateLimitedByRPSecuritySettings() {
			Protocol protocol = Protocol.V20;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					rp.SecuritySettings.MinimumHashBitLength = 256;
					var association = rp.AssociationManager.GetOrCreateAssociation(new ProviderEndpointDescription(OPUri, protocol.Version));
					Assert.IsNull(association, "No association should have been created when RP and OP could not agree on association strength.");
				},
				op => {
					op.SecuritySettings.MaximumHashBitLength = 160;
					AutoProvider(op);
				});
			coordinator.Run();
		}

		/// <summary>
		/// Verifies that the RP can recover from an invalid or non-existent 
		/// response from the OP, for example in the HTTP timeout case.
		/// </summary>
		[Test]
		public void AssociateQuietlyFailsAfterHttpError() {
			this.MockResponder.RegisterMockNotFound(OPUri);
			var rp = this.CreateRelyingParty();
			var association = rp.AssociationManager.GetOrCreateAssociation(new ProviderEndpointDescription(OPUri, Protocol.V20.Version));
			Assert.IsNull(association);
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
			Protocol protocol = Protocol.Lookup(Protocol.Lookup(opDescription.Version).ProtocolVersion);
			bool expectSuccess = expectedAssociationType != null;
			bool expectDiffieHellman = !opDescription.Uri.IsTransportSecure();
			Association rpAssociation = null, opAssociation;
			AssociateSuccessfulResponse associateSuccessfulResponse = null;
			AssociateUnsuccessfulResponse associateUnsuccessfulResponse = null;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					rp.SecuritySettings = this.RelyingPartySecuritySettings;
					rpAssociation = rp.AssociationManager.GetOrCreateAssociation(opDescription);
				},
				op => {
					op.SecuritySettings = this.ProviderSecuritySettings;
					IRequest req = op.GetRequest();
					Assert.IsNotNull(req, "Expected incoming request but did not receive it.");
					Assert.IsTrue(req.IsResponseReady);
					op.Respond(req);
				});
			coordinator.IncomingMessageFilter = message => {
				Assert.AreSame(opDescription.Version, message.Version, "The message was recognized as version {0} but was expected to be {1}.", message.Version, Protocol.Lookup(opDescription.Version).ProtocolVersion);
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
				Assert.AreEqual(opDescription.Version, message.Version, "The message was for version {0} but was expected to be for {1}.", message.Version, opDescription.Version);
			};
			coordinator.Run();

			if (expectSuccess) {
				Assert.IsNotNull(rpAssociation);
				Association actual = coordinator.RelyingParty.AssociationManager.AssociationStoreTestHook.GetAssociation(opDescription.Uri, rpAssociation.Handle);
				Assert.AreEqual(rpAssociation, actual);
				opAssociation = coordinator.Provider.AssociationStore.Deserialize(new TestSignedDirectedMessage(), false, rpAssociation.Handle);
				Assert.IsNotNull(opAssociation, "The Provider could not decode the association handle.");

				Assert.AreEqual(opAssociation.Handle, rpAssociation.Handle);
				Assert.AreEqual(expectedAssociationType, rpAssociation.GetAssociationType(protocol));
				Assert.AreEqual(expectedAssociationType, opAssociation.GetAssociationType(protocol));
				Assert.IsTrue(Math.Abs(opAssociation.SecondsTillExpiration - rpAssociation.SecondsTillExpiration) < 60);
				Assert.IsTrue(MessagingUtilities.AreEquivalent(opAssociation.SecretKey, rpAssociation.SecretKey));

				if (expectDiffieHellman) {
					Assert.IsInstanceOf<AssociateDiffieHellmanResponse>(associateSuccessfulResponse);
					var diffieHellmanResponse = (AssociateDiffieHellmanResponse)associateSuccessfulResponse;
					Assert.IsFalse(MessagingUtilities.AreEquivalent(diffieHellmanResponse.EncodedMacKey, rpAssociation.SecretKey), "Key should have been encrypted.");
				} else {
					Assert.IsInstanceOf<AssociateUnencryptedResponse>(associateSuccessfulResponse);
					var unencryptedResponse = (AssociateUnencryptedResponse)associateSuccessfulResponse;
				}
			} else {
				Assert.IsNull(coordinator.RelyingParty.AssociationManager.AssociationStoreTestHook.GetAssociation(opDescription.Uri, new RelyingPartySecuritySettings()));
			}
		}
	}
}
