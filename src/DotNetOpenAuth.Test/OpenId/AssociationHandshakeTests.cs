//-----------------------------------------------------------------------
// <copyright file="AssociationHandshakeTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
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
		public async Task AssociateUnencrypted() {
			await this.ParameterizedAssociationTestAsync(OPUriSsl);
		}

		[Test]
		public async Task AssociateDiffieHellmanOverHttp() {
			await this.ParameterizedAssociationTestAsync(OPUri);
		}

		/// <summary>
		/// Verifies that the Provider can do Diffie-Hellman over HTTPS.
		/// </summary>
		/// <remarks>
		/// Some OPs out there flatly refuse to do this, and the spec doesn't forbid
		/// putting the two together, so we verify that DNOI can handle it.
		/// </remarks>
		[Test]
		public async Task AssociateDiffieHellmanOverHttps() {
			Protocol protocol = Protocol.V20;
			this.RegisterAutoProvider();
			var rp = this.CreateRelyingParty();

			// We have to formulate the associate request manually,
			// since the DNOI RP won't voluntarily use DH on HTTPS.
			var request = new AssociateDiffieHellmanRequest(protocol.Version, OPUri) {
				AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA256,
				SessionType = protocol.Args.SessionType.DH_SHA256
			};
			request.InitializeRequest();
			var response = await rp.Channel.RequestAsync<AssociateSuccessfulResponse>(request, CancellationToken.None);
			Assert.IsNotNull(response);
			Assert.AreEqual(request.AssociationType, response.AssociationType);
			Assert.AreEqual(request.SessionType, response.SessionType);
		}

		/// <summary>
		/// Verifies that the RP and OP can renegotiate an association type if the RP's
		/// initial request for an association is for a type the OP doesn't support.
		/// </summary>
		[Test]
		public async Task AssociateRenegotiateBitLength() {
			Protocol protocol = Protocol.V20;

			// The strategy is to make a simple request of the RP to establish an association,
			// and to more carefully observe the Provider-side of things to make sure that both
			// the OP and RP are behaving as expected.
			int providerAttemptCount = 0;
			HandleProvider(
				async (op, request) => {
					op.SecuritySettings.MaximumHashBitLength = 160; // Force OP to reject HMAC-SHA256

					switch (++providerAttemptCount) {
						case 1:
							// Receive initial request for an HMAC-SHA256 association.
							var req = (AutoResponsiveRequest)await op.GetRequestAsync(request);
							var associateRequest = (AssociateRequest)req.RequestMessage;
							Assert.That(associateRequest, Is.Not.Null);
							Assert.AreEqual(protocol.Args.SignatureAlgorithm.HMAC_SHA256, associateRequest.AssociationType);

							// Ensure that the response is a suggestion that the RP try again with HMAC-SHA1
							var renegotiateResponse =
								(AssociateUnsuccessfulResponse)await req.GetResponseMessageAsyncTestHook(CancellationToken.None);
							Assert.AreEqual(protocol.Args.SignatureAlgorithm.HMAC_SHA1, renegotiateResponse.AssociationType);
							return await op.PrepareResponseAsync(req);

						case 2:
							// Receive second attempt request for an HMAC-SHA1 association.
							req = (AutoResponsiveRequest)await op.GetRequestAsync(request);
							associateRequest = (AssociateRequest)req.RequestMessage;
							Assert.AreEqual(protocol.Args.SignatureAlgorithm.HMAC_SHA1, associateRequest.AssociationType);

							// Ensure that the response is a success response.
							var successResponse =
								(AssociateSuccessfulResponse)await req.GetResponseMessageAsyncTestHook(CancellationToken.None);
							Assert.AreEqual(protocol.Args.SignatureAlgorithm.HMAC_SHA1, successResponse.AssociationType);
							return await op.PrepareResponseAsync(req);

						default:
							throw Assumes.NotReachable();
					}
				});
			var rp = this.CreateRelyingParty();
			var opDescription = new ProviderEndpointDescription(OPUri, protocol.Version);
			Association association = await rp.AssociationManager.GetOrCreateAssociationAsync(opDescription, CancellationToken.None);
			Assert.IsNotNull(association, "Association failed to be created.");
			Assert.AreEqual(protocol.Args.SignatureAlgorithm.HMAC_SHA1, association.GetAssociationType(protocol));
		}

		/// <summary>
		/// Verifies that the OP rejects an associate request that has no encryption (transport or DH).
		/// </summary>
		/// <remarks>
		/// Verifies OP's compliance with OpenID 2.0 section 8.4.1.
		/// </remarks>
		[Test]
		public async Task OPRejectsHttpNoEncryptionAssociateRequests() {
			Protocol protocol = Protocol.V20;
			this.RegisterAutoProvider();
			var rp = this.CreateRelyingParty();

			// We have to formulate the associate request manually,
			// since the DNOA RP won't voluntarily suggest no encryption at all.
			var request = new AssociateUnencryptedRequestNoSslCheck(protocol.Version, OPUri);
			request.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA256;
			request.SessionType = protocol.Args.SessionType.NoEncryption;
			var response = await rp.Channel.RequestAsync<DirectErrorResponse>(request, CancellationToken.None);
			Assert.IsNotNull(response);
		}

		/// <summary>
		/// Verifies that the OP rejects an associate request
		/// when the HMAC and DH bit lengths do not match.
		/// </summary>
		[Test]
		public async Task OPRejectsMismatchingAssociationAndSessionTypes() {
			Protocol protocol = Protocol.V20;
			this.RegisterAutoProvider();
			var rp = this.CreateRelyingParty();

			// We have to formulate the associate request manually,
			// since the DNOI RP won't voluntarily mismatch the association and session types.
			var request = new AssociateDiffieHellmanRequest(protocol.Version, OPUri);
			request.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA256;
			request.SessionType = protocol.Args.SessionType.DH_SHA1;
			request.InitializeRequest();
			var response = await rp.Channel.RequestAsync<AssociateUnsuccessfulResponse>(request, CancellationToken.None);
			Assert.IsNotNull(response);
			Assert.AreEqual(protocol.Args.SignatureAlgorithm.HMAC_SHA1, response.AssociationType);
			Assert.AreEqual(protocol.Args.SessionType.DH_SHA1, response.SessionType);
		}

		/// <summary>
		/// Verifies that the RP quietly rejects an OP that suggests an unknown association type.
		/// </summary>
		[Test]
		public async Task RPRejectsUnrecognizedAssociationType() {
			Protocol protocol = Protocol.V20;
			HandleProvider(
				async (op, req) => {
					// Receive initial request.
					var request = await op.Channel.ReadFromRequestAsync<AssociateRequest>(req, CancellationToken.None);

					// Send a response that suggests a foreign association type.
					var renegotiateResponse = new AssociateUnsuccessfulResponse(request.Version, request);
					renegotiateResponse.AssociationType = "HMAC-UNKNOWN";
					renegotiateResponse.SessionType = "DH-UNKNOWN";
					return await op.Channel.PrepareResponseAsync(renegotiateResponse);
				});
			var rp = this.CreateRelyingParty();
			var association = await rp.AssociationManager.GetOrCreateAssociationAsync(new ProviderEndpointDescription(OPUri, protocol.Version), CancellationToken.None);
			Assert.IsNull(association, "The RP should quietly give up when the OP misbehaves.");
		}

		/// <summary>
		/// Verifies that the RP quietly rejects an OP that suggests an no encryption over an HTTP channel.
		/// </summary>
		/// <remarks>
		/// Verifies RP's compliance with OpenID 2.0 section 8.4.1.
		/// </remarks>
		[Test]
		public async Task RPRejectsUnencryptedSuggestion() {
			Protocol protocol = Protocol.V20;
			this.HandleProvider(
				async (op, req) => {
					// Receive initial request.
					var request = await op.Channel.ReadFromRequestAsync<AssociateRequest>(req, CancellationToken.None);

					// Send a response that suggests a no encryption.
					var renegotiateResponse = new AssociateUnsuccessfulResponse(request.Version, request);
					renegotiateResponse.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA1;
					renegotiateResponse.SessionType = protocol.Args.SessionType.NoEncryption;
					return await op.Channel.PrepareResponseAsync(renegotiateResponse);
				});

			var rp = this.CreateRelyingParty();
			var association = await rp.AssociationManager.GetOrCreateAssociationAsync(new ProviderEndpointDescription(OPUri, protocol.Version), CancellationToken.None);
			Assert.IsNull(association, "The RP should quietly give up when the OP misbehaves.");
		}

		/// <summary>
		/// Verifies that the RP rejects an associate renegotiate request 
		/// when the HMAC and DH bit lengths do not match.
		/// </summary>
		[Test]
		public async Task RPRejectsMismatchingAssociationAndSessionBitLengths() {
			Protocol protocol = Protocol.V20;
			this.HandleProvider(
				async (op, req) => {
					// Receive initial request.
					var request = await op.Channel.ReadFromRequestAsync<AssociateRequest>(req, CancellationToken.None);

					// Send a mismatched response
					AssociateUnsuccessfulResponse renegotiateResponse = new AssociateUnsuccessfulResponse(request.Version, request);
					renegotiateResponse.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA1;
					renegotiateResponse.SessionType = protocol.Args.SessionType.DH_SHA256;
					return await op.Channel.PrepareResponseAsync(renegotiateResponse);
				});
			var rp = this.CreateRelyingParty();
			var association = await rp.AssociationManager.GetOrCreateAssociationAsync(new ProviderEndpointDescription(OPUri, protocol.Version), CancellationToken.None);
			Assert.IsNull(association, "The RP should quietly give up when the OP misbehaves.");
		}

		/// <summary>
		/// Verifies that the RP cannot get caught in an infinite loop if a bad OP
		/// keeps sending it association retry messages.
		/// </summary>
		[Test]
		public async Task RPOnlyRenegotiatesOnce() {
			Protocol protocol = Protocol.V20;
			int opStep = 0;
			HandleProvider(
				async (op, req) => {
					switch (++opStep) {
						case 1:
							// Receive initial request.
							var request = await op.Channel.ReadFromRequestAsync<AssociateRequest>(req, CancellationToken.None);

							// Send a renegotiate response
							var renegotiateResponse = new AssociateUnsuccessfulResponse(request.Version, request);
							renegotiateResponse.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA1;
							renegotiateResponse.SessionType = protocol.Args.SessionType.DH_SHA1;
							return await op.Channel.PrepareResponseAsync(renegotiateResponse, CancellationToken.None);

						case 2:
							// Receive second-try
							request = await op.Channel.ReadFromRequestAsync<AssociateRequest>(req, CancellationToken.None);

							// Send ANOTHER renegotiate response, at which point the DNOI RP should give up.
							renegotiateResponse = new AssociateUnsuccessfulResponse(request.Version, request);
							renegotiateResponse.AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA256;
							renegotiateResponse.SessionType = protocol.Args.SessionType.DH_SHA256;
							return await op.Channel.PrepareResponseAsync(renegotiateResponse, CancellationToken.None);

						default:
							throw Assumes.NotReachable();
					}
				});
			var rp = this.CreateRelyingParty();
			var association = await rp.AssociationManager.GetOrCreateAssociationAsync(new ProviderEndpointDescription(OPUri, protocol.Version), CancellationToken.None);
			Assert.IsNull(association, "The RP should quietly give up when the OP misbehaves.");
		}

		/// <summary>
		/// Verifies security settings limit RP's acceptance of OP's counter-suggestion
		/// </summary>
		[Test]
		public async Task AssociateRenegotiateLimitedByRPSecuritySettings() {
			Protocol protocol = Protocol.V20;
			HandleProvider(
				async (op, req) => {
					op.SecuritySettings.MaximumHashBitLength = 160;
					return await AutoProviderActionAsync(op, req, CancellationToken.None);
				});
			var rp = this.CreateRelyingParty();
			rp.SecuritySettings.MinimumHashBitLength = 256;
			var association = await rp.AssociationManager.GetOrCreateAssociationAsync(new ProviderEndpointDescription(OPUri, protocol.Version), CancellationToken.None);
			Assert.IsNull(association, "No association should have been created when RP and OP could not agree on association strength.");
		}

		/// <summary>
		/// Verifies that the RP can recover from an invalid or non-existent 
		/// response from the OP, for example in the HTTP timeout case.
		/// </summary>
		[Test]
		public async Task AssociateQuietlyFailsAfterHttpError() {
			// Without wiring up a mock HTTP handler, the RP will get a 404 Not Found error.
			var rp = this.CreateRelyingParty();
			var association = await rp.AssociationManager.GetOrCreateAssociationAsync(new ProviderEndpointDescription(OPUri, Protocol.V20.Version), CancellationToken.None);
			Assert.IsNull(association);
		}

		/// <summary>
		/// Runs a parameterized association flow test using all supported OpenID versions.
		/// </summary>
		/// <param name="opEndpoint">The OP endpoint to simulate using.</param>
		private async Task ParameterizedAssociationTestAsync(Uri opEndpoint) {
			foreach (Protocol protocol in Protocol.AllPracticalVersions) {
				var endpoint = new ProviderEndpointDescription(opEndpoint, protocol.Version);
				var associationType = protocol.Version.Major < 2 ? protocol.Args.SignatureAlgorithm.HMAC_SHA1 : protocol.Args.SignatureAlgorithm.HMAC_SHA256;
				await this.ParameterizedAssociationTestAsync(endpoint, associationType);
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
		private async Task ParameterizedAssociationTestAsync(
			ProviderEndpointDescription opDescription,
			string expectedAssociationType) {
			Protocol protocol = Protocol.Lookup(Protocol.Lookup(opDescription.Version).ProtocolVersion);
			bool expectSuccess = expectedAssociationType != null;
			bool expectDiffieHellman = !opDescription.Uri.IsTransportSecure();
			Association rpAssociation = null, opAssociation;
			AssociateSuccessfulResponse associateSuccessfulResponse = null;
			AssociateUnsuccessfulResponse associateUnsuccessfulResponse = null;
			var relyingParty = new OpenIdRelyingParty(new MemoryCryptoKeyAndNonceStore(), this.HostFactories);
			var provider = new OpenIdProvider(new MemoryCryptoKeyAndNonceStore(), this.HostFactories) {
				SecuritySettings = this.ProviderSecuritySettings
			};
			Handle(opDescription.Uri).By(
				async (request, ct) => {
					IRequest req = await provider.GetRequestAsync(request, ct);
					Assert.IsNotNull(req, "Expected incoming request but did not receive it.");
					Assert.IsTrue(req.IsResponseReady);
					return await provider.PrepareResponseAsync(req, ct);
				});
			relyingParty.Channel.IncomingMessageFilter = message => {
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
			relyingParty.Channel.OutgoingMessageFilter = message => {
				Assert.AreEqual(opDescription.Version, message.Version, "The message was for version {0} but was expected to be for {1}.", message.Version, opDescription.Version);
			};

			relyingParty.SecuritySettings = this.RelyingPartySecuritySettings;
			rpAssociation = await relyingParty.AssociationManager.GetOrCreateAssociationAsync(opDescription, CancellationToken.None);

			if (expectSuccess) {
				Assert.IsNotNull(rpAssociation);
				Association actual = relyingParty.AssociationManager.AssociationStoreTestHook.GetAssociation(opDescription.Uri, rpAssociation.Handle);
				Assert.AreEqual(rpAssociation, actual);
				opAssociation = provider.AssociationStore.Deserialize(new TestSignedDirectedMessage(), false, rpAssociation.Handle);
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
				Assert.IsNull(relyingParty.AssociationManager.AssociationStoreTestHook.GetAssociation(opDescription.Uri, new RelyingPartySecuritySettings()));
			}
		}
	}
}
