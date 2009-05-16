//-----------------------------------------------------------------------
// <copyright file="AuthenticationTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AuthenticationTests : OpenIdTestBase {
		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
		}

		[TestMethod]
		public void SharedAssociationPositive() {
			this.ParameterizedAuthenticationTest(true, true, false);
		}

		/// <summary>
		/// Verifies that a shared association protects against tampering.
		/// </summary>
		[TestMethod]
		public void SharedAssociationTampered() {
			this.ParameterizedAuthenticationTest(true, true, true);
		}

		[TestMethod]
		public void SharedAssociationNegative() {
			this.ParameterizedAuthenticationTest(true, false, false);
		}

		[TestMethod]
		public void PrivateAssociationPositive() {
			this.ParameterizedAuthenticationTest(false, true, false);
		}

		/// <summary>
		/// Verifies that a private association protects against tampering.
		/// </summary>
		[TestMethod]
		public void PrivateAssociationTampered() {
			this.ParameterizedAuthenticationTest(false, true, true);
		}

		[TestMethod]
		public void NoAssociationNegative() {
			this.ParameterizedAuthenticationTest(false, false, false);
		}

		[TestMethod]
		public void UnsolicitedAssertion() {
			this.MockResponder.RegisterMockRPDiscovery();
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					rp.Channel.WebRequestHandler = this.MockResponder.MockWebRequestHandler;
					IAuthenticationResponse response = rp.GetResponse();
					Assert.AreEqual(AuthenticationStatus.Authenticated, response.Status);
				},
				op => {
					op.Channel.WebRequestHandler = this.MockResponder.MockWebRequestHandler;
					Identifier id = GetMockIdentifier(ProtocolVersion.V20);
					op.SendUnsolicitedAssertion(OPUri, RPRealmUri, id, OPLocalIdentifiers[0]);
					AutoProvider(op); // handle check_auth
				});
			coordinator.Run();
		}

		[TestMethod]
		public void UnsolicitedAssertionRejected() {
			this.MockResponder.RegisterMockRPDiscovery();
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					rp.Channel.WebRequestHandler = this.MockResponder.MockWebRequestHandler;
					rp.SecuritySettings.RejectUnsolicitedAssertions = true;
					IAuthenticationResponse response = rp.GetResponse();
					Assert.AreEqual(AuthenticationStatus.Failed, response.Status);
				},
				op => {
					op.Channel.WebRequestHandler = this.MockResponder.MockWebRequestHandler;
					Identifier id = GetMockIdentifier(ProtocolVersion.V20);
					op.SendUnsolicitedAssertion(OPUri, RPRealmUri, id, OPLocalIdentifiers[0]);
					AutoProvider(op); // handle check_auth
				});
			coordinator.Run();
		}

		/// <summary>
		/// Verifies that delegating identifiers are rejected in unsolicited assertions
		/// when the appropriate security setting is set.
		/// </summary>
		[TestMethod]
		public void UnsolicitedDelegatingIdentifierRejection() {
			this.MockResponder.RegisterMockRPDiscovery();
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					rp.Channel.WebRequestHandler = this.MockResponder.MockWebRequestHandler;
					rp.SecuritySettings.RejectDelegatingIdentifiers = true;
					IAuthenticationResponse response = rp.GetResponse();
					Assert.AreEqual(AuthenticationStatus.Failed, response.Status);
				},
				op => {
					op.Channel.WebRequestHandler = this.MockResponder.MockWebRequestHandler;
					Identifier id = GetMockIdentifier(ProtocolVersion.V20, false, true);
					op.SendUnsolicitedAssertion(OPUri, RPRealmUri, id, OPLocalIdentifiers[0]);
					AutoProvider(op); // handle check_auth
				});
			coordinator.Run();
		}

		private void ParameterizedAuthenticationTest(bool sharedAssociation, bool positive, bool tamper) {
			foreach (Protocol protocol in Protocol.AllPracticalVersions) {
				foreach (bool statelessRP in new[] { false, true }) {
					if (sharedAssociation && statelessRP) {
						// Skip the invalid combination scenario.
						continue;
					}

					foreach (bool immediate in new[] { false, true }) {
						TestLogger.InfoFormat("Beginning authentication test scenario.  OpenID: {0}, Shared: {1}, positive: {2}, tamper: {3}, stateless: {4}, immediate: {5}", protocol.Version, sharedAssociation, positive, tamper, statelessRP, immediate);
						this.ParameterizedAuthenticationTest(protocol, statelessRP, sharedAssociation, positive, immediate, tamper);
					}
				}
			}
		}

		private void ParameterizedAuthenticationTest(Protocol protocol, bool statelessRP, bool sharedAssociation, bool positive, bool immediate, bool tamper) {
			ErrorUtilities.VerifyArgument(!statelessRP || !sharedAssociation, "The RP cannot be stateless while sharing an association with the OP.");
			ErrorUtilities.VerifyArgument(positive || !tamper, "Cannot tamper with a negative response.");
			ProviderSecuritySettings securitySettings = new ProviderSecuritySettings();
			Association association = sharedAssociation ? HmacShaAssociation.Create(protocol, protocol.Args.SignatureAlgorithm.Best, AssociationRelyingPartyType.Smart, securitySettings) : null;
			var coordinator = new OpenIdCoordinator(
				rp => {
					var request = new CheckIdRequest(protocol.Version, OPUri, immediate ? AuthenticationRequestMode.Immediate : AuthenticationRequestMode.Setup);

					if (association != null) {
						StoreAssociation(rp, OPUri, association);
						request.AssociationHandle = association.Handle;
					}

					request.ClaimedIdentifier = "http://claimedid";
					request.LocalIdentifier = "http://localid";
					request.ReturnTo = RPUri;
					request.Realm = RPUri;
					rp.Channel.Send(request);
					if (positive) {
						if (tamper) {
							try {
								rp.Channel.ReadFromRequest<PositiveAssertionResponse>();
								Assert.Fail("Expected exception {0} not thrown.", typeof(InvalidSignatureException).Name);
							} catch (InvalidSignatureException) {
								TestLogger.InfoFormat("Caught expected {0} exception after tampering with signed data.", typeof(InvalidSignatureException).Name);
							}
						} else {
							var response = rp.Channel.ReadFromRequest<PositiveAssertionResponse>();
							Assert.IsNotNull(response);
							Assert.AreEqual(request.ClaimedIdentifier, response.ClaimedIdentifier);
							Assert.AreEqual(request.LocalIdentifier, response.LocalIdentifier);
							Assert.AreEqual(request.ReturnTo, response.ReturnTo);

							// Attempt to replay the message and verify that it fails.
							// Because in various scenarios and protocol versions different components
							// notice the replay, we can get one of two exceptions thrown.
							// When the OP notices the replay we get a generic InvalidSignatureException.
							// When the RP notices the replay we get a specific ReplayMessageException.
							try {
								CoordinatingChannel channel = (CoordinatingChannel)rp.Channel;
								channel.Replay(response);
								Assert.Fail("Expected ProtocolException was not thrown.");
							} catch (ProtocolException ex) {
								Assert.IsTrue(ex is ReplayedMessageException || ex is InvalidSignatureException, "A {0} exception was thrown instead of the expected {1} or {2}.", ex.GetType(), typeof(ReplayedMessageException).Name, typeof(InvalidSignatureException).Name);
							}
						}
					} else {
						var response = rp.Channel.ReadFromRequest<NegativeAssertionResponse>();
						Assert.IsNotNull(response);
						if (immediate) {
							// Only 1.1 was required to include user_setup_url
							if (protocol.Version.Major < 2) {
								Assert.IsNotNull(response.UserSetupUrl);
							}
						} else {
							Assert.IsNull(response.UserSetupUrl);
						}
					}
				},
				op => {
					if (association != null) {
						op.AssociationStore.StoreAssociation(AssociationRelyingPartyType.Smart, association);
					}

					var request = op.Channel.ReadFromRequest<CheckIdRequest>();
					Assert.IsNotNull(request);
					IProtocolMessage response;
					if (positive) {
						response = new PositiveAssertionResponse(request);
					} else {
						response = new NegativeAssertionResponse(request, op.Channel);
					}
					op.Channel.Send(response);

					if (positive && (statelessRP || !sharedAssociation)) {
						var checkauthRequest = op.Channel.ReadFromRequest<CheckAuthenticationRequest>();
						var checkauthResponse = new CheckAuthenticationResponse(checkauthRequest.Version, checkauthRequest);
						checkauthResponse.IsValid = checkauthRequest.IsValid;
						op.Channel.Send(checkauthResponse);

						if (!tamper) {
							// Respond to the replay attack.
							checkauthRequest = op.Channel.ReadFromRequest<CheckAuthenticationRequest>();
							checkauthResponse = new CheckAuthenticationResponse(checkauthRequest.Version, checkauthRequest);
							checkauthResponse.IsValid = checkauthRequest.IsValid;
							op.Channel.Send(checkauthResponse);
						}
					}
				});
			if (tamper) {
				coordinator.IncomingMessageFilter = message => {
					var assertion = message as PositiveAssertionResponse;
					if (assertion != null) {
						// Alter the Local Identifier between the Provider and the Relying Party.
						// If the signature binding element does its job, this should cause the RP
						// to throw.
						assertion.LocalIdentifier = "http://victim";
					}
				};
			}
			if (statelessRP) {
				coordinator.RelyingParty = new OpenIdRelyingParty(null);
			}

			coordinator.Run();
		}
	}
}
